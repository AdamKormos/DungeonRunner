﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialArrowPuzzle : Puzzle
{
    [SerializeField] GameObject sampleDirectionPointerArrowObject = default;
    [SerializeField] Bound jigsawSpawnDistanceBound = default;
    [SerializeField] float activationDuration = 20f;
    bool isCompleted = false, isTimeTrialGoing = false;
    JigsawPiece jigsawObject;

    // Start is called before the first frame update
    void Start()
    {
        if (jigsawPieceDict.ContainsKey(this))
        {
            // components[0] = jigsaw piece
            components[0].objectToSpawn.SetActive(isTimeTrialGoing);

            jigsawObject = components[0].objectToSpawn.GetComponentInChildren<JigsawPiece>(true);
            JigsawPosition jigsawPosition = jigsawPieceDict[this];
            jigsawObject.jigsawPosition = jigsawPosition;
            jigsawObject.GetComponentInChildren<SpriteRenderer>(true).sprite = MapManager.s_jigsawPieceSprites[(int)jigsawPosition];

            jigsawObject.transform.parent = this.transform;
            jigsawObject.transform.localScale = new Vector3(1f, 1f, 1f);
            jigsawObject.GetComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.6f);
        }
        else
        {
            components[0].objectToSpawn.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.startedTimeTrialArrowPuzzle || Input.GetKeyDown(KeyCode.Space)) OnPuzzleStarted();
    }

    int requiredJigsawPieceDistance = 0, currentDistance = 0;
    GameObject[] arrowObjectArr = default;

    /// <summary>
    /// Called when the player steps on the tile that activates this puzzle. @Optimization Everything that happens in this method can be heavily optimized.
    /// </summary>
    private void OnPuzzleStarted()
    {
        requiredJigsawPieceDistance = Random.Range(jigsawSpawnDistanceBound.min, jigsawSpawnDistanceBound.max + 1);
        System.Tuple<int, int> thisRoomCoords = MapManager.PositionToGridPosition(transform.position);

        //do
        //{
        recentVisitedRooms.Clear();
        goodPath.Clear();
        GetOptimalRoomForJigsawPiece(thisRoomCoords.Item1, thisRoomCoords.Item2);
        //} while (goodPath.Count < jigsawSpawnDistanceBound.min);

        Direction[] directions = TranslateAndVisualizeGoodPathToDirections();
        Debug.Log("Move amount: " + directions.Length);
        for (int i = 0; i < directions.Length; i++)
        {
            Debug.Log((i + 1) + ". move: " + directions[i]);
        }

        StartCoroutine(PlaceAndDisplayJigsawPiece());
    }

    /// <summary>
    /// Takes the good room path and converts it to a Direction list while also spawning arrows that indicate the correct path for the player. The arrows will be stored in an array because
    /// we have to destroy them as soon as the jigsaw piece disappears.
    /// </summary>
    /// <returns></returns>
    private Direction[] TranslateAndVisualizeGoodPathToDirections()
    {
        //Debug.Log("Output array length is: " + (goodPath.Count - 1));
        Direction[] output = new Direction[goodPath.Count - 1];
        arrowObjectArr = new GameObject[output.Length];
        float firstArrowXOffset = sampleDirectionPointerArrowObject.GetComponent<SpriteRenderer>().sprite.bounds.size.x * ((float)arrowObjectArr.Length / 2f);
        float offsetBetweenArrows = sampleDirectionPointerArrowObject.GetComponent<SpriteRenderer>().sprite.bounds.size.x * 5f;

        for (int i = 0; i < goodPath.Count - 1; i++)
        {
            System.Tuple<int, int> currentRoomCoords = MapManager.PositionToGridPosition(goodPath[i].transform.position);
            System.Tuple<int, int> nextRoomCoords = MapManager.PositionToGridPosition(goodPath[i + 1].transform.position);
            Vector2 recentNextCoordsDifference = new Vector2(nextRoomCoords.Item1 - currentRoomCoords.Item1, nextRoomCoords.Item2 - currentRoomCoords.Item2);

            GameObject arrow
                = Instantiate(sampleDirectionPointerArrowObject, transform.position + new Vector3(-firstArrowXOffset + i * offsetBetweenArrows, 1f), Quaternion.identity, this.transform);
            arrowObjectArr[i] = arrow;

            // .x: up, down
            // .y: right, left
            if (recentNextCoordsDifference.x == -1)
            {
                output[i] = Direction.Down;
            }
            else if (recentNextCoordsDifference.x == 1)
            {
                output[i] = Direction.Up;
                arrow.transform.Rotate(0f, 0f, 180f);
            }
            else if (recentNextCoordsDifference.y == -1)
            {
                output[i] = Direction.Left;
                arrow.transform.Rotate(0f, 0f, 270f);
            }
            else if (recentNextCoordsDifference.y == 1)
            {
                output[i] = Direction.Right;
                arrow.transform.Rotate(0f, 0f, 90f);
            }
        }

        return output;
    }

    /// <summary>
    /// Sets the jigsaw piece's position and keeps it active for a certain amount of time. Once the given duration passes, the jigsaw piece disappears and the instruction arrows 
    /// are destroyed.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlaceAndDisplayJigsawPiece()
    {
        jigsawObject.transform.position = jigsawPieceRoom.transform.position + new Vector3(-Player.cameraResolutionBounds.x / 4f, +Player.cameraResolutionBounds.y / 5f, -4.0f);
        jigsawObject.gameObject.SetActive(true);

        yield return new WaitForSeconds(activationDuration);

        jigsawObject.gameObject.SetActive(false);
        foreach (GameObject arrowInstructionObject in arrowObjectArr)
        {
            Destroy(arrowInstructionObject);
        }
    }

    Room jigsawPieceRoom = default;
    List<Room> goodPath = new List<Room>();

    /// <summary>
    /// Gets an optimal room for the jigsaw piece. Optimal means that it's as far from the puzzle's room as it's supposed to be (= requiredJigsawPieceDistance).
    /// </summary>
    /// <returns></returns>
    private void GetOptimalRoomForJigsawPiece(int currentI, int currentJ)
    {
        Room destinationRoom = MapManager.GetRandomRoom();
        Room thisRoom = MapManager.roomGrid[currentI][currentJ];

        while (MapManager.GetDistanceBetweenRoomsByPosition(destinationRoom, thisRoom) < jigsawSpawnDistanceBound.min || destinationRoom.blockCount > 1 || destinationRoom.GetDoorCount() < 2)
        {
            //Debug.Log("RoomFindIteration");
            destinationRoom = MapManager.GetRandomRoom();
        }

        while (goodPath.Count < 2)
        {
            //Debug.Log("PathfindIteration");
            FindRoom(destinationRoom, thisRoom);
        }

        //Debug.Log(MapManager.PositionToGridPosition(destinationRoom.transform.position));


        //Debug.Log("Found at " + MapManager.PositionToGridPosition(destinationRoom.transform.position));
        jigsawPieceRoom = destinationRoom;
    }

    int stepCount = 0;
    List<Room> recentVisitedRooms = new List<Room>();

    private void FindRoom(Room from, Room dest)
    {
        if (goodPath.Count != 0) return;

        if (from.transform.position == dest.transform.position)
        {
            recentVisitedRooms.Add(from);
            //Debug.Log("Found in " + stepCount + " steps.");
            goodPath = new List<Room>(recentVisitedRooms);
            recentVisitedRooms.Clear();
            stepCount = 0;
            return;
        }

        System.Tuple<int, int> fromCoords = MapManager.PositionToGridPosition(from.transform.position);
        bool went = false;

        if ((from.doors[1]) && !recentVisitedRooms.Contains(MapManager.roomGrid[fromCoords.Item1 - 1][fromCoords.Item2]))
        {
            went = true;
            int tmp = stepCount;
            List<Room> tmpVisited = new List<Room>(recentVisitedRooms);
            stepCount++;
            recentVisitedRooms.Add(from);
            FindRoom(MapManager.roomGrid[fromCoords.Item1 - 1][fromCoords.Item2], dest);
            stepCount = tmp;
            recentVisitedRooms = new List<Room>(tmpVisited);
        }
        if ((from.doors[0]) && !recentVisitedRooms.Contains(MapManager.roomGrid[fromCoords.Item1 + 1][fromCoords.Item2]))
        {
            went = true;
            int tmp = stepCount;
            List<Room> tmpVisited = new List<Room>(recentVisitedRooms);
            stepCount++;
            recentVisitedRooms.Add(from);
            FindRoom(MapManager.roomGrid[fromCoords.Item1 + 1][fromCoords.Item2], dest);
            stepCount = tmp;
            recentVisitedRooms = new List<Room>(tmpVisited);
        }
        if ((from.doors[3]) && !recentVisitedRooms.Contains(MapManager.roomGrid[fromCoords.Item1][fromCoords.Item2 - 1]))
        {
            went = true;
            int tmp = stepCount;
            List<Room> tmpVisited = new List<Room>(recentVisitedRooms);
            stepCount++;
            recentVisitedRooms.Add(from);
            FindRoom(MapManager.roomGrid[fromCoords.Item1][fromCoords.Item2 - 1], dest);
            stepCount = tmp;
            recentVisitedRooms = new List<Room>(tmpVisited);
        }
        if ((from.doors[2]) && !recentVisitedRooms.Contains(MapManager.roomGrid[fromCoords.Item1][fromCoords.Item2 + 1]))
        {
            went = true;
            int tmp = stepCount;
            List<Room> tmpVisited = new List<Room>(recentVisitedRooms);
            stepCount++;
            recentVisitedRooms.Add(from);
            FindRoom(MapManager.roomGrid[fromCoords.Item1][fromCoords.Item2 + 1], dest);
            stepCount = tmp;
            recentVisitedRooms = new List<Room>(tmpVisited);
        }

        if(!went)
        {
            recentVisitedRooms.Clear();
            stepCount = 0;
            return;
        }
    }
}