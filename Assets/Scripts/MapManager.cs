using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public struct EnvironmentObject
{
    [SerializeField] public GameObject gameObject;
    [SerializeField] public bool isPositionRoomCenter;
}

/// <summary>
/// @EarlyExec Executes earlier than supposed to by -20.
/// </summary>
public class MapManager : MonoBehaviour
{
    [SerializeField] Vector2Int gridSize = new Vector2Int(4, 4);
    [SerializeField] GameObject roomSample = default;
    [SerializeField] GameObject doorSample = default;
    [SerializeField] GameObject wallSample = default;
    [SerializeField] Sprite[] singleWalls = default;
    [SerializeField] Sprite[] multipleWalls = default;
    [Space(20f)]
    [SerializeField] Bound environmentObjectSpawnAmountBound = default;
    [SerializeField] EnvironmentObject[] environmentObjects = default;
    [Space(20f)]
    [SerializeField] int puzzleAmountToSpawn = 6;
    [SerializeField] Puzzle jigsaw = default;
    [SerializeField] Puzzle[] puzzles = default;
    [SerializeField] Sprite[] jigsawPieceSprites = default;
    public static Room[][] roomGrid { get; private set; }
    public static Room currentRoom { get; private set; } // = roomGrid[Player.currentRoomI][Player.currentRoomJ]
    public static Vector2 roomOffsets { get; private set; }
    //static Vector2 doorOffsetFromRoomCenter = new Vector2(0.0763f, 0.0664f);
    static Vector2 doorOffsetFromRoomCenter = new Vector2(8.14f, 4.4f);
    static Vector3 localVerticalWallScale = new Vector3(0.2047224f, 0.04432031f, 1f);
    public static Vector2Int s_gridSize { get; private set; }
    public static Sprite[] s_jigsawPieceSprites { get; private set; }

    private void Start()
    {
        s_jigsawPieceSprites = jigsawPieceSprites;
        s_gridSize = gridSize;
        roomOffsets = new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize) * 2f;

        LayoutInit();
    }

    private void Update()
    {
        currentRoom = roomGrid[Player.currentRoomI][Player.currentRoomJ]; // Set z to -8f
    }

    /// <summary>
    /// The main init method of the map. Covers the layout of rooms, merged rooms and puzzles.
    /// </summary>
    private void LayoutInit()
    {
        inaccessibleCoords.Clear();
        GenerateLayout(gridSize.y, gridSize.x);

        Player.currentRoomI = Random.Range(0, gridSize.y);
        Player.currentRoomJ = Random.Range(0, gridSize.x);
        currentRoom = roomGrid[Player.currentRoomI][Player.currentRoomJ];

        TraverseMap();
        if (activeRoomCount < (gridSize.y * gridSize.x) * 0.5f)
        {
            DestroyGridRooms();
            LayoutInit();
        }
        else
        {
            FinalizeGrid();
            SetInitialPlayerPosition();
            ShufflePuzzleSelection();
            GeneratePuzzles();
            SpawnEnvironmentObjects();
        }
    }

    /// <summary>
    /// Randomly spawns the environment objects in any room with no restriction.
    /// </summary>
    private void SpawnEnvironmentObjects()
    {
        foreach(EnvironmentObject environmentObject in environmentObjects)
        {
            int spawnAmount = Random.Range(environmentObjectSpawnAmountBound.min, environmentObjectSpawnAmountBound.max + 1);
            for (int i = 0; i < spawnAmount; i++)
            {
                Room room = GetRandomRoom();
                GameObject gameObject = Instantiate(environmentObject.gameObject, room.transform.position, Quaternion.identity, room.transform);

                if (!environmentObject.isPositionRoomCenter)
                {
                    Vector2 offsetFromScreenEdge = gameObject.GetComponent<SpriteRenderer>().bounds.size * 1.5f;
                    gameObject.transform.position = new Vector3(
                        Random.Range(
                            room.transform.position.x - Player.cameraResolutionBounds.x + offsetFromScreenEdge.x,
                            room.transform.position.x + Player.cameraResolutionBounds.x - offsetFromScreenEdge.x),
                        Random.Range(
                            room.transform.position.y - Player.cameraResolutionBounds.y + offsetFromScreenEdge.y,
                            room.transform.position.y + Player.cameraResolutionBounds.y - offsetFromScreenEdge.y),
                        gameObject.transform.position.z);
                }

                gameObject.transform.localPosition += new Vector3(0f, 0f, -1.5f);
                gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x / room.transform.lossyScale.x, gameObject.transform.localScale.y / room.transform.lossyScale.y);
            }
        }
    }

    /// <summary>
    /// Defines where the player will spawn. It must be done before GeneratePuzzles, because for example, a MemoryTile throws Nullptrexp if the player spawns on it, since OnPlayerEnter()
    /// gets triggered and the tile has a component that gets referenced at Start().
    /// </summary>
    private void SetInitialPlayerPosition()
    {
        do
        {
            Player.currentRoomI = Random.Range(0, gridSize.y);
            Player.currentRoomJ = Random.Range(0, gridSize.x);
            currentRoom = roomGrid[Player.currentRoomI][Player.currentRoomJ];
        } while (currentRoom == null);

        roomGrid[Player.currentRoomI][Player.currentRoomJ].containsPuzzle = true;
    }

    /// <summary>
    /// Shuffles the puzzle list so that out of all, not the first four will always give jigsaw pieces.
    /// </summary>
    private void ShufflePuzzleSelection()
    {
        Dictionary<int, Puzzle> puzzleIndexDict = new Dictionary<int, Puzzle>(puzzles.Length);
        int[] puzzleIndexArr = new int[puzzles.Length];

        for (int i = 0; i < puzzles.Length; i++)
        {
            puzzleIndexDict.Add(i, puzzles[i]);
            puzzleIndexArr[i] = i;
        }

        for(int i = 0; i < puzzles.Length-1; i++)
        {
            for(int j = i+1; j < puzzles.Length; j++)
            {
                if (Random.Range(0, 100) < 50)
                {
                    int tmp = puzzleIndexArr[j];
                    puzzleIndexArr[j] = puzzleIndexArr[i];
                    puzzleIndexArr[i] = tmp;
                }
            }
        }

        for(int i = 0; i < puzzles.Length; i++)
        {
            puzzles[i] = puzzleIndexDict[puzzleIndexArr[i]];
        }
    }

    /// <summary>
    /// Creates the puzzles and spawns their components as well.
    /// </summary>
    private void GeneratePuzzles()
    {
        jigsaw.transform.position = AssignPuzzleToGrid();
        jigsaw.SpawnPuzzleComponents();

        // Spawn ALL puzzles
        for (int i = 0; i < puzzleAmountToSpawn && i < puzzles.Length; i++)
        {
            puzzles[i].transform.position = AssignPuzzleToGrid();
            puzzles[i].SpawnPuzzleComponents();
        }

        // Set puzzles that give jigsaw pieces:
        for(int i = 0; i < 4 && i < puzzles.Length; i++)
        {
            Puzzle.jigsawPieceDict[puzzles[i]] = (JigsawPosition)(i);
        }
    }

    /// <summary>
    /// Reserves a position in the grid to be used by a certain puzzle.
    /// </summary>
    /// <returns>The reserved room's, and therefore the puzzle's/puzzle component's new position</returns>
    public static Vector3 AssignPuzzleToGrid()
    {
        Vector2 newTilePos = new Vector2();
        int randI, randJ;
        do
        {
            randI = Random.Range(0, s_gridSize.y);
            randJ = Random.Range(0, s_gridSize.x);
            if(roomGrid[randI][randJ] != null) newTilePos = roomGrid[randI][randJ].transform.position;
        } while (roomGrid[randI][randJ] == null || (roomGrid[randI][randJ] != null && roomGrid[randI][randJ].containsPuzzle));

        PositionToRoom(newTilePos).containsPuzzle = true;
        //Debug.Log("Successfully placed puzzle");
        return new Vector3(newTilePos.x, newTilePos.y, -8.5f);
    }

    /// <summary>
    /// Called once the existing rooms' count is bigger than half of the grid size (when the size is acceptable). Starts merging and once it finished, it destroys the remaining
    /// inaccessible rooms and according to that, any adjacent room doors that may lead to them also get deleted.
    /// </summary>
    private void FinalizeGrid()
    {
        LookForMerge();

        for (int i = 0; i < inaccessibleCoords.Count; i++)
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
                        if (roomGrid[i][j].walls[k]) AddWallToRoom(i, j, (Direction)k);
                    }
                    roomGrid[i][j].SetMultiWalls(multipleWalls[Random.Range(0, multipleWalls.Length)]);
                }
            }
        }
    }

    /// <summary>
    /// Iterates through the grid. Driver of SeekForMerge.
    /// </summary>
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

    /// <summary>
    /// If the room found on the given coordinates can be accessed from the player's first position, the room might seek for merge. It can happen in two directions, down or left so that
    /// we don't iterate through every direction twice. Once the direction is picked randomly, the method returns, if by any chance, the room we want to merge with is not in the grid.
    /// If the method continues, we get the merge chance (higher for inaccessible rooms) and if whether the room at grid[i, j] has the existing walls or not. If so, and if the generated
    /// number is good, we destroy the two rooms' walls that should be gone by merging, merge, and continue looking for merging if the current room size is < 3.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
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

            // Against IndexOORException:
            if (mergeAttemptRoomCoords.Item1 < 0 || mergeAttemptRoomCoords.Item2 < 0 || mergeAttemptRoomCoords.Item1 >= gridSize.y || mergeAttemptRoomCoords.Item2 >= gridSize.x) return;

            Room mergeAttemptRoom = roomGrid[mergeAttemptRoomCoords.Item1][mergeAttemptRoomCoords.Item2];

            // Restriction of amount:
            if (roomGrid[i][j].blockCount + mergeAttemptRoom.blockCount > 3) return;
            
            //Debug.Log(roomGrid[i][j].blockCount + " " + mergeAttemptRoom.blockCount);

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

                    recentlyMergedRoomCoords.Clear();
                    CollectRecentlyMergedRooms(i, j);
                    foreach (Tuple<int, int> rCoords in recentlyMergedRoomCoords) roomGrid[rCoords.Item1][rCoords.Item2].blockCount++;

                    //Debug.Log(recentlyMergedRoomCoords.Count);

                    inaccessibleCoords.Remove(new Tuple<int, int>(mergeAttemptRoomCoords.Item1, mergeAttemptRoomCoords.Item2));

                    if (Random.Range(0, 100) < 50)
                    {
                        SeekForMerge(mergeAttemptRoomCoords.Item1, mergeAttemptRoomCoords.Item2);
                    }
                }
            }
        }
    }

    List<Tuple<int, int>> recentlyMergedRoomCoords = new List<Tuple<int, int>>();

    /// <summary>
    /// Finds all rooms that can be visited from the room the merge started in (and of course, from the room the merge goes to, due to the recursion).
    /// </summary>
    /// <param name="fromI"></param>
    /// <param name="fromJ"></param>
    private void CollectRecentlyMergedRooms(int fromI, int fromJ)
    {
        recentlyMergedRoomCoords.Add(new Tuple<int, int>(fromI, fromJ));
        if (!roomGrid[fromI][fromJ].walls[0] && !recentlyMergedRoomCoords.Contains(new Tuple<int, int>(fromI + 1, fromJ))) CollectRecentlyMergedRooms(fromI + 1, fromJ);
        if (!roomGrid[fromI][fromJ].walls[1] && !recentlyMergedRoomCoords.Contains(new Tuple<int, int>(fromI - 1, fromJ))) CollectRecentlyMergedRooms(fromI - 1, fromJ);
        if (!roomGrid[fromI][fromJ].walls[2] && !recentlyMergedRoomCoords.Contains(new Tuple<int, int>(fromI, fromJ + 1))) CollectRecentlyMergedRooms(fromI, fromJ + 1);
        if (!roomGrid[fromI][fromJ].walls[3] && !recentlyMergedRoomCoords.Contains(new Tuple<int, int>(fromI, fromJ - 1))) CollectRecentlyMergedRooms(fromI, fromJ - 1);
    }

    /// <summary>
    /// Destroys every room in the grid and clears the roomGrid list. Used when the generated map is too small.
    /// </summary>
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

    /// <summary>
    /// Goes through the whole map and selects the rooms that are inaccessible.
    /// </summary>
    private void TraverseMap()
    {
        keepRooms = new bool[gridSize.y][];
        for (int i = 0; i < keepRooms.Length; i++) keepRooms[i] = new bool[gridSize.x];

        TraverseAdjacents(Player.currentRoomI, Player.currentRoomJ); // Start from the current (first) room
        MarkInaccessibleRooms();
    }

    /// <summary>
    /// Adds inaccessible rooms (the ones that couldn't be reached) to the list.
    /// </summary>
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

    /// <summary>
    /// Visits the given room's neighbours and tells to keep the given room, as it was found.
    /// </summary>
    /// <param name="roomI"></param>
    /// <param name="roomJ"></param>
    private void TraverseAdjacents(int roomI, int roomJ)
    {
        keepRooms[roomI][roomJ] = true;

        if(roomGrid[roomI][roomJ].doors[1] && !keepRooms[roomI-1][roomJ]) TraverseAdjacents(roomI - 1, roomJ);
        if (roomGrid[roomI][roomJ].doors[0] && !keepRooms[roomI + 1][roomJ]) TraverseAdjacents(roomI + 1, roomJ);
        if (roomGrid[roomI][roomJ].doors[3] && !keepRooms[roomI][roomJ - 1]) TraverseAdjacents(roomI, roomJ - 1);
        if (roomGrid[roomI][roomJ].doors[2] && !keepRooms[roomI][roomJ + 1]) TraverseAdjacents(roomI, roomJ + 1);
    }

    /// <summary>
    /// Loops through the grid and fills the roomGrid. Driver for SetRandomDoors.
    /// </summary>
    /// <param name="height"></param>
    /// <param name="width"></param>
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

    /// <summary>
    /// Called on every room at GenerateLayout. Assigns doors in the down and left directions randomly to the given room, and applies changes to the adjacent room's set of doors as well.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
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

    Vector2 doorLocalPositionOn1920x1080 = new Vector2(0.0776f, 0.0665f);
    Vector2 wallLocalPositionOn1920x1080DownUp = new Vector2(0.0061f, 0.0677f);

    /// <summary>
    /// Creates a door object and assigns it to the given room's layout.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="direction"></param>
    public void AddDoorToRoom(int i, int j, Direction direction)
    {
        GameObject door = Instantiate(doorSample, roomGrid[i][j].transform.position, Quaternion.identity, roomGrid[i][j].transform);
        door.GetComponent<Door>().doorDirection = direction;

        switch (direction)
        {
            case Direction.Down:
                door.transform.localPosition = new Vector3(0f, -0.0665f, 0f);
                door.transform.Rotate(0f, 0f, 180f);
                break;
            case Direction.Left:
                door.transform.localPosition = new Vector3(-0.0776f, 0f, 0f);
                door.transform.Rotate(0f, 0f, 90f);
                break;
            case Direction.Up:
                door.transform.localPosition = new Vector3(0f, 0.0665f, 0f);
                break;
            case Direction.Right:
                door.transform.localPosition = new Vector3(0.0776f, 0f, 0f);
                door.transform.Rotate(0f, 0f, 270f);
                break;
        }

        door.transform.localPosition += new Vector3(0f, 0f, -1.5f);
    }

    //float wallOffsetFromDoor = 0.05f;

    /// <summary>
    /// Creates a wall object and assigns it to the given room's layout.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="direction"></param>
    public void AddWallToRoom(int i, int j, Direction direction)
    {
        GameObject wall = Instantiate(wallSample, roomGrid[i][j].transform.position, Quaternion.identity, roomGrid[i][j].transform);
        wall.GetComponent<Wall>().direction = direction;
        wall.GetComponent<SpriteRenderer>().sprite = singleWalls[Random.Range(0, singleWalls.Length)];
        //wall.transform.localPosition += new Vector3(0f, 0f, -1f);

        switch (direction)
        {
            case Direction.Down:
                wall.transform.localPosition = new Vector3(0.0061f, -0.0677f, -1f);// new Vector3(0.0044f, -doorOffsetFromRoomCenter.y - wallOffsetFromDoor, 0f);
                wall.transform.Rotate(0f, 0f, 180f);
                break;
            case Direction.Left:
                wall.transform.localPosition = new Vector3(-0.0768f, -0.0035f, -1f);//new Vector3(-doorOffsetFromRoomCenter.x - wallOffsetFromDoor, 0.0042f, 0f);
                wall.transform.Rotate(0f, 0f, 90f);
                wall.transform.localScale = localVerticalWallScale;
                break;
            case Direction.Up:
                wall.transform.localPosition = new Vector3(-0.0061f, 0.0677f, -1f);//doorOffsetFromRoomCenter.y + wallOffsetFromDoor, 0f);
                break;
            case Direction.Right:
                wall.transform.localPosition = new Vector3(0.0768f, 0.0055f, -1f);//+= new Vector3(doorOffsetFromRoomCenter.x + wallOffsetFromDoor, 0.0042f, 0f);
                wall.transform.Rotate(0f, 0f, 270);
                wall.transform.localScale = localVerticalWallScale;
                break;
        }
    }

    public static Vector2 movementAfterDoor;
    static float unitOnDoorEntrance = 3f;

    /// <summary>
    /// Called when entering a door. Gives a vector a certain value that is used in the Player to be applied for transmission between two rooms.
    /// </summary>
    /// <param name="d"></param>
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

    /// <summary>
    /// Gets the GRID DISTANCE between two rooms (using Manhattan distance).
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static int GetDistanceBetweenRoomsByPosition(Room a, Room b)
    {
        Tuple<int, int> aCoords = PositionToGridPosition(a.transform.position);
        Tuple<int, int> bCoords = PositionToGridPosition(b.transform.position);

        return Mathf.Abs(aCoords.Item1 - bCoords.Item1) + Mathf.Abs(aCoords.Item2 - bCoords.Item2);
    }

    /// <summary>
    /// Converts a world position to grid position.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>[i][j]</returns>
    public static Tuple<int, int> PositionToGridPosition(Vector2 pos)
    {
        return new Tuple<int, int>((int)Mathf.Round(pos.y / roomOffsets.y), (int)Mathf.Round(pos.x / roomOffsets.x));
    }

    /// <summary>
    /// Converts a grid position to world position.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    public static Vector2 GridPositionToPosition(int i, int j)
    {
        return new Vector2(j * roomOffsets.x, i * roomOffsets.y);
    }

    /// <summary>
    /// Converts a position to room.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static Room PositionToRoom(Vector2 pos)
    {
        Tuple<int, int> coords = PositionToGridPosition(pos);
        return roomGrid[coords.Item1][coords.Item2];
    }

    /// <summary>
    /// Returns a random room that is on the map and isn't null.
    /// </summary>
    /// <returns></returns>
    public static Room GetRandomRoom()
    {
        int i, j;
        do
        {
            i = Random.Range(0, s_gridSize.y);
            j = Random.Range(0, s_gridSize.x);
        } while (roomGrid[i][j] == null);

        return roomGrid[i][j];
    }
}
