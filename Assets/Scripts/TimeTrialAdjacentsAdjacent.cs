using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialAdjacentsAdjacent : Puzzle
{
    [SerializeField] GameObject sampleDirectionPointerArrowObject = default;
    [SerializeField] Bound jigsawSpawnDistanceBound = default;
    [SerializeField] float activationDuration = 20f;
    bool isCompleted = false, isTimeTrialGoing = false, holdsJigsawPiece = false;
    JigsawPiece jigsawObject;
    Tuple<int, int> jigsawPieceRoomCoords = default;

    // Start is called before the first frame update
    void Start()
    {
        holdsJigsawPiece = jigsawPieceDict.ContainsKey(this);

        if (holdsJigsawPiece)
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

            jigsawPieceRoomCoords = MapManager.PositionToGridPosition(jigsawObject.transform.position);
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
        if (Player.startedTimeTrialAA && !isCompleted) OnPuzzleStarted();
        else if(!isCompleted && holdsJigsawPiece && jigsawPieceRoomCoords.Item1 == Player.currentRoomI && jigsawPieceRoomCoords.Item2 == Player.currentRoomJ)
        {
            isCompleted = true;
        }
    }

    Room jigsawPieceRoom = default;

    /// <summary>
    /// Called when the player steps on the tile that activates this puzzle. @Optimization Everything that happens in this method can be heavily optimized.
    /// </summary>
    private void OnPuzzleStarted()
    {
        Tuple<int, int> thisRoomCoords = MapManager.PositionToGridPosition(transform.position);
        StartCoroutine(GetOptimalRoomForJigsawPiece(thisRoomCoords.Item1, thisRoomCoords.Item2));
    }

    /// <summary>
    /// Gets an optimal room for the jigsaw piece. Optimal means that it's as far from the puzzle's room as it's supposed to be (= requiredJigsawPieceDistance).
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetOptimalRoomForJigsawPiece(int currentI, int currentJ)
    {
        Room thisRoom = MapManager.roomGrid[currentI][currentJ];

        int attemptCounter = 0;
        List<Room> roomsToNotEnter = new List<Room>();

        do
        {
            yield return new WaitForSeconds(0.2f);
            attemptCounter++;
            Debug.Log("Starting attempt " + attemptCounter);

            int[] doorIndices = new int[] { 0, 1, 2, 3 };
            //for(int i = 0; i < 3; i++)
            //{
            //    for(int j = i+1; j < 4; j++)
            //    {
            //        if(UnityEngine.Random.Range(0, 100) < 50)
            //        {
            //            int tmp = doorIndices[j];
            //            doorIndices[j] = doorIndices[i];
            //            doorIndices[i] = tmp;
            //        }
            //    }
            //}

            int jigsawRoomI = currentI, jigsawRoomJ = currentJ;

            Direction recentDirection = default;
            for (int i = 0; i < thisRoom.doors.Length; i++)
            {
                if (thisRoom.doors[doorIndices[i]] || !thisRoom.walls[doorIndices[i]])
                {
                    int testI = jigsawRoomI, testJ = jigsawRoomJ;

                    recentDirection = (Direction)(doorIndices[i]);
                    if (recentDirection == Direction.Up) testI++;
                    if (recentDirection == Direction.Down) testI--;
                    if (recentDirection == Direction.Right) testJ++;
                    if (recentDirection == Direction.Left) testJ--;

                    if (MapManager.roomGrid[testI][testJ] == null || MapManager.roomGrid[testI][testJ].GetEntranceCount() < 2) continue;
                    else
                    {
                        jigsawRoomI = testI;
                        jigsawRoomJ = testJ;
                        break;
                    }
                }
            }

            Debug.Log("Attempt " + attemptCounter + ": Gone to direction " + recentDirection + " on first move.");

            jigsawPieceRoom = MapManager.roomGrid[jigsawRoomI][jigsawRoomJ];

            //for (int i = 0; i < 3; i++)
            //{
            //    for (int j = i + 1; j < 4; j++)
            //    {
            //        if (UnityEngine.Random.Range(0, 100) < 50)
            //        {
            //            int tmp = doorIndices[j];
            //            doorIndices[j] = doorIndices[i];
            //            doorIndices[i] = tmp;
            //        }
            //    }
            //}

            for (int i = 0; i < jigsawPieceRoom.doors.Length; i++)
            {
                if ((jigsawPieceRoom.doors[doorIndices[i]] || !jigsawPieceRoom.walls[doorIndices[i]]))
                {
                    int testI = jigsawRoomI, testJ = jigsawRoomJ;

                    recentDirection = (Direction)(doorIndices[i]);
                    if (recentDirection == Direction.Up) testI++;
                    if (recentDirection == Direction.Down) testI--;
                    if (recentDirection == Direction.Right) testJ++;
                    if (recentDirection == Direction.Left) testJ--;

                    if (MapManager.roomGrid[testI][testJ] == null || MapManager.roomGrid[testI][testJ].transform.position == thisRoom.transform.position) continue;
                    else
                    {
                        jigsawRoomI = testI;
                        jigsawRoomJ = testJ;
                        break;
                    }
                }
            }

            Debug.Log("Attempt " + attemptCounter + ": Gone to direction " + recentDirection + " on second move.");
            jigsawPieceRoom = MapManager.roomGrid[jigsawRoomI][jigsawRoomJ];
        } while (MapManager.GetDistanceBetweenRoomsByPosition(jigsawPieceRoom, thisRoom) < 2);

        StartCoroutine(PlaceAndDisplayJigsawPiece());
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

        if(!isCompleted) jigsawObject.gameObject.SetActive(false);
    }
}