using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapManager : MonoBehaviour
{
    [SerializeField] Vector2Int gridSize = new Vector2Int(4, 4);
    [SerializeField] GameObject roomSample = default;
    [SerializeField] GameObject doorSample = default;
    [SerializeField] GameObject wallSample = default;
    [SerializeField] Sprite singleWall = default;
    [SerializeField] Sprite multipleWall = default;
    public static Room[][] roomGrid { get; private set; }
    public static Room currentRoom { get; private set; } // = roomGrid[Player.currentRoomI][Player.currentRoomJ]
    public static Vector2 roomOffsets { get; private set; }
    //static Vector2 doorOffsetFromRoomCenter = new Vector2(0.0763f, 0.0664f);
    static Vector2 doorOffsetFromRoomCenter = new Vector2(8.08f, 4.4f);
    static Vector3 localVerticalWallScale = new Vector3(0.1973228f, 0.04432031f, 1f);
    public static Vector2Int s_gridSize { get; private set; }

    private void Start()
    {
        s_gridSize = gridSize;
        roomOffsets = new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize) * 2f;

        LayoutInit();

        do
        {
            Player.currentRoomI = Random.Range(0, gridSize.y);
            Player.currentRoomJ = Random.Range(0, gridSize.x);
            currentRoom = roomGrid[Player.currentRoomI][Player.currentRoomJ];
        } while (currentRoom == null);
    }

    private void Update()
    {
        currentRoom = roomGrid[Player.currentRoomI][Player.currentRoomJ];
    }

    private void LayoutInit()
    {
        inaccessibleCoords.Clear();
        GenerateLayout(gridSize.y, gridSize.x);

        Player.currentRoomI = Random.Range(0, gridSize.y);
        Player.currentRoomJ = Random.Range(0, gridSize.x);
        currentRoom = roomGrid[Player.currentRoomI][Player.currentRoomJ];

        TraverseMap();
        if (activeRoomCount < (gridSize.y * gridSize.x) / 2)
        {
            DestroyGridRooms();
            LayoutInit();
        }
        else
        {
            LookForMerge();

            for(int i = 0; i < inaccessibleCoords.Count; i++)
            {
                int roomI = inaccessibleCoords[i].Item1, roomJ = inaccessibleCoords[i].Item2;
                Room currentInaccessible = roomGrid[roomI][roomJ];

                if (currentInaccessible != null)
                {
                    if (currentInaccessible.doors[0]) roomGrid[roomI + 1][roomJ].RemoveDoor(Direction.Down);
                    if (currentInaccessible.doors[1]) roomGrid[roomI - 1][roomJ].RemoveDoor(Direction.Up);
                    if (currentInaccessible.doors[2]) roomGrid[roomI][roomJ + 1].RemoveDoor(Direction.Left);
                    if (currentInaccessible.doors[3]) roomGrid[roomI][roomJ - 1].RemoveDoor(Direction.Right);

                    Destroy(currentInaccessible.gameObject);
                    roomGrid[roomI][roomJ] = null;
                }
            }

            for (int i = 0; i < gridSize.y; i++)
            {
                for (int j = 0; j < gridSize.x; j++)
                {
                    if (roomGrid[i][j] != null)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            if(roomGrid[i][j].walls[k]) AddWallToRoom(i, j, (Direction)k);
                        }
                    }
                }
            }
        }
    }

    private void LookForMerge()
    {
        for (int i = 0; i < gridSize.y; i++)
        {
            for (int j = 1; j < gridSize.x; j++)
            {
                SeekForMerge(i, j);
            }
        }
    }

    private void SeekForMerge(int i, int j)
    {
        if (!inaccessibleCoords.Contains(new Tuple<int, int>(i, j))) // If room is accessible
        {
            Direction d = (Random.Range(0, 2) == 0 ? Direction.Down : Direction.Left);
            Tuple<int, int> mergeAttemptRoomCoords = default;

            if(d == Direction.Down)
            {
                mergeAttemptRoomCoords = new Tuple<int, int>(i - 1, j);
            }
            else if(d == Direction.Left)
            {
                mergeAttemptRoomCoords = new Tuple<int, int>(i, j - 1);
            }

            if (mergeAttemptRoomCoords.Item1 < 0 || mergeAttemptRoomCoords.Item2 < 0 || mergeAttemptRoomCoords.Item1 >= gridSize.y || mergeAttemptRoomCoords.Item2 >= gridSize.x) return;
            
            Room mergeAttemptRoom = roomGrid[mergeAttemptRoomCoords.Item1][mergeAttemptRoomCoords.Item2];

            if (mergeAttemptRoom != null)
            {
                int mergeChance = (mergeAttemptRoom.HasAnyDoors() ? 20 : 40);
                bool requiredWallExists = (d == Direction.Down ? mergeAttemptRoom.walls[0] : mergeAttemptRoom.walls[2]);

                if (Random.Range(0, 100) < mergeChance && requiredWallExists) // On Success
                {
                    if (d == Direction.Down)
                    {
                        roomGrid[i][j].walls[1] = false;
                        roomGrid[i][j].doors[1] = false;

                        mergeAttemptRoom.walls[0] = false;
                        if (mergeAttemptRoom.doors[0]) Destroy(mergeAttemptRoom.GetDoorOfDirection(Direction.Up).gameObject);
                        mergeAttemptRoom.doors[0] = false;
                    }
                    else if (d == Direction.Left)
                    {
                        roomGrid[i][j].walls[3] = false;
                        roomGrid[i][j].doors[3] = false;

                        mergeAttemptRoom.walls[2] = false;
                        if (mergeAttemptRoom.doors[2]) Destroy(mergeAttemptRoom.GetDoorOfDirection(Direction.Right).gameObject);
                        mergeAttemptRoom.doors[2] = false;
                    }

                    if (roomGrid[i][j].GetDoorOfDirection(d) != null) Destroy(roomGrid[i][j].GetDoorOfDirection(d).gameObject);

                    roomGrid[i][j].blockCount++;
                    mergeAttemptRoom.blockCount++;

                    inaccessibleCoords.Remove(new Tuple<int, int>(mergeAttemptRoomCoords.Item1, mergeAttemptRoomCoords.Item2));

                    if (roomGrid[i][j].blockCount < 3 && mergeAttemptRoom.blockCount == 1 && Random.Range(0, 100) < 50)
                    {
                        SeekForMerge(mergeAttemptRoomCoords.Item1, mergeAttemptRoomCoords.Item2);
                    }
                }
            }
        }
    }

    private void DestroyGridRooms()
    {
        for (int i = 0; i < gridSize.y; i++)
        {
            for (int j = 0; j < gridSize.x; j++)
            {
                if (roomGrid[i][j] != null)
                {
                    Destroy(roomGrid[i][j].gameObject);
                    roomGrid[i][j] = null;
                }
            }
        }
    }

    static bool[][] keepRooms = default;
    static int activeRoomCount = 0;
    static List<Tuple<int, int>> inaccessibleCoords = new List<Tuple<int, int>>();

    private void TraverseMap()
    {
        keepRooms = new bool[gridSize.y][];
        for (int i = 0; i < keepRooms.Length; i++) keepRooms[i] = new bool[gridSize.x];

        TraverseAdjacents(Player.currentRoomI, Player.currentRoomJ); // Start from the current (first) room
        MarkInaccessibleRooms();
    }

    private static void MarkInaccessibleRooms()
    {
        activeRoomCount = 0;
        for (int i = 0; i < roomGrid.Length; i++)
        {
            for (int j = 0; j < roomGrid[i].Length; j++)
            {
                if (!keepRooms[i][j])
                {
                    inaccessibleCoords.Add(new Tuple<int, int>(i, j)); // Instead of despawning, mark them for potential room merging.
                    //Destroy(roomGrid[i][j].gameObject);
                    //roomGrid[i][j] = null;
                }
                else activeRoomCount++;
            }
        }
    }

    private void TraverseAdjacents(int roomI, int roomJ)
    {
        keepRooms[roomI][roomJ] = true;

        if(roomGrid[roomI][roomJ].doors[1] && !keepRooms[roomI-1][roomJ])
        {
            TraverseAdjacents(roomI - 1, roomJ);
        }

        if (roomGrid[roomI][roomJ].doors[0] && !keepRooms[roomI + 1][roomJ])
        {
            TraverseAdjacents(roomI + 1, roomJ);
        }

        if (roomGrid[roomI][roomJ].doors[3] && !keepRooms[roomI][roomJ - 1])
        {
            TraverseAdjacents(roomI, roomJ - 1);
        }

        if (roomGrid[roomI][roomJ].doors[2] && !keepRooms[roomI][roomJ + 1])
        {
            TraverseAdjacents(roomI, roomJ + 1);
        }
    }

    private void GenerateLayout(int height, int width)
    {
        roomGrid = new Room[height][];

        for (int i = 0; i < roomGrid.Length; i++)
        {
            roomGrid[i] = new Room[width];

            for (int j = 0; j < width; j++)
            {
                SetRandomDoors(i, j);
            }
        }
    }

    private void SetRandomDoors(int i, int j)
    {
        GameObject roomGO = Instantiate(roomSample, 
                new Vector3(
                    j * roomOffsets.x,
                    i * roomOffsets.y,
                    roomSample.transform.position.z), 
                Quaternion.identity);

        Room room = roomGO.GetComponent<Room>();
        roomGrid[i][j] = room;
        roomGrid[i][j].walls = new bool[]{ true, true, true, true};

        if (i > 0)
        {
            room.doors[1] = (Random.Range(0, 2) > 0);
            if (room.doors[1])
            {
                AddDoorToRoom(i, j, Direction.Down);
                roomGrid[i - 1][j].doors[0] = true;
                AddDoorToRoom(i - 1, j, Direction.Up);
            }
            //Debug.Log(room.doorLeft + ", position in grid is (" + i + ", " + j + ")");
        }

        if (j > 0)
        {
            room.doors[3] = (Random.Range(0, 2) > 0);
            if (room.doors[3])
            {
                AddDoorToRoom(i, j, Direction.Left);
                roomGrid[i][j - 1].doors[2] = true;
                AddDoorToRoom(i, j - 1, Direction.Right);
            }
            //Debug.Log(room.doorLeft + ", position in grid is (" + i + ", " + j + ")");
        }
    }

    public void AddDoorToRoom(int i, int j, Direction direction)
    {
        GameObject door = Instantiate(doorSample, roomGrid[i][j].transform.position, Quaternion.identity, roomGrid[i][j].transform);
        door.GetComponent<Door>().doorDirection = direction;
        door.transform.position += new Vector3(0f, 0f, -2f);

        switch (direction)
        {
            case Direction.Down:
                door.transform.position += new Vector3(0f, -doorOffsetFromRoomCenter.y, 0f);
                door.transform.Rotate(0f, 0f, 180f);
                break;
            case Direction.Left:
                door.transform.position += new Vector3(-doorOffsetFromRoomCenter.x, 0f, 0f);
                door.transform.Rotate(0f, 0f, 270f);
                break;
            case Direction.Up:
                door.transform.position += new Vector3(0f, doorOffsetFromRoomCenter.y, 0f);
                break;
            case Direction.Right:
                door.transform.position += new Vector3(doorOffsetFromRoomCenter.x, 0f, 0f);
                door.transform.Rotate(0f, 0f, 90f);
                break;
        }
    }

    float wallOffsetFromDoor = 0.05f;

    public void AddWallToRoom(int i, int j, Direction direction)
    {
        GameObject wall = Instantiate(wallSample, roomGrid[i][j].transform.position, Quaternion.identity, roomGrid[i][j].transform);
        //wall.transform.localPosition += new Vector3(0f, 0f, -1f);

        switch (direction)
        {
            case Direction.Down:
                wall.transform.localPosition = new Vector3(0.0061f, -0.0677f, -1f);// new Vector3(0.0044f, -doorOffsetFromRoomCenter.y - wallOffsetFromDoor, 0f);
                wall.transform.Rotate(0f, 0f, 180f);
                break;
            case Direction.Left:
                wall.transform.localPosition = new Vector3(-0.0745f, -0.0035f, -1f);//new Vector3(-doorOffsetFromRoomCenter.x - wallOffsetFromDoor, 0.0042f, 0f);
                wall.transform.Rotate(0f, 0f, 90f);
                wall.transform.localScale = localVerticalWallScale;
                break;
            case Direction.Up:
                wall.transform.localPosition = new Vector3(-0.0044f, 0.0691f, -1f);//doorOffsetFromRoomCenter.y + wallOffsetFromDoor, 0f);
                break;
            case Direction.Right:
                wall.transform.localPosition = new Vector3(0.0745f, 0.0055f, -1f);//+= new Vector3(doorOffsetFromRoomCenter.x + wallOffsetFromDoor, 0.0042f, 0f);
                wall.transform.Rotate(0f, 0f, 270);
                wall.transform.localScale = localVerticalWallScale;
                break;
        }
    }

    public static Vector2 movementAfterDoor;
    static float unitOnDoorEntrance = 3f;

    public static void OnRoomChange(Direction d)
    {
        if(d == Direction.Down)
        {
            movementAfterDoor = new Vector2(0f, -unitOnDoorEntrance);
        }
        else if(d == Direction.Up)
        {
            movementAfterDoor = new Vector2(0f, unitOnDoorEntrance);
        }
        else if(d == Direction.Left)
        {
            movementAfterDoor = new Vector2(-unitOnDoorEntrance, 0f);
        }
        else if(d == Direction.Right)
        {
            movementAfterDoor = new Vector2(unitOnDoorEntrance, 0f);
        }

        //currentRoom = roomGrid[Player.currentRoomI][Player.currentRoomJ];
    }

    public static void ApplyCurrentRoomIndices()
    {
        currentRoom = roomGrid[Player.currentRoomI][Player.currentRoomJ];
    }
}
