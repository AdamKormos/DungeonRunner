using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] Slider staminaSlider = default;
    [SerializeField] float staminaDecreasePerFrame = 0.005f;
    [SerializeField] float staminaReloadOnWaterDrink = 50f;
    [SerializeField] int staminaReloadFrameAmount = 1000;
    [SerializeField] new Rigidbody2D rigidbody;
    public static Vector2 cameraResolutionBounds { get; private set; }
    public static int currentRoomI, currentRoomJ, previousRoomI, previousRoomJ;
    bool isReloadingStamina = false, checksForDoorTrigger = true;
    public static List<JigsawPiece> collectedPieces { get; private set; }
    List<Room> currentRooms = new List<Room>();

    // Start is called before the first frame update
    void Start()
    {
        //Bound2D b = new Bound2D(-1, 4, 2, 6);
        //b.GetIntersection(new Bound2D(1, 5, -1, 3));
        collectedPieces = new List<JigsawPiece>();
        transform.position = new Vector3(MapManager.currentRoom.transform.position.x, MapManager.currentRoom.transform.position.y, -9f);
        rigidbody = GetComponent<Rigidbody2D>();
        cameraResolutionBounds = new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize);
        StartCoroutine(HandleStamina());
        OnDoorLeave();

        //StartCoroutine(A());
    }

    // Update is called once per frame
    void Update()
    {
        currentRoomI = (int)Mathf.Round(transform.position.y / MapManager.roomOffsets.y);
        currentRoomJ = (int)Mathf.Round(transform.position.x / MapManager.roomOffsets.x);

        //if (Input.GetKeyDown(KeyCode.Space)) StartCoroutine(A());

        //Debug.Log(MapManager.currentRoom.blockCount);
        if(!GameMenuUI.isUIActive) FollowMouseMovement();
        DetermineCameraMovement();
    }

    private IEnumerator HandleStamina()
    {
        staminaSlider.value = staminaSlider.maxValue;

        while(staminaSlider.value >= 0.00f)
        {
            if (!isReloadingStamina)
            {
                staminaSlider.value -= staminaDecreasePerFrame;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator ReloadStamina()
    {
        float toReloadOnOneFrame = staminaReloadOnWaterDrink / staminaReloadFrameAmount;
        int iterationAmount = 0;

        if (staminaSlider.value + staminaReloadOnWaterDrink > staminaSlider.maxValue)
        {
            iterationAmount = (int)((staminaSlider.maxValue - staminaSlider.value) / toReloadOnOneFrame);
        }
        else iterationAmount = staminaReloadFrameAmount;

        isReloadingStamina = true;
        for (int i = 0; i < iterationAmount; i++)
        {
            staminaSlider.value += toReloadOnOneFrame;
            yield return new WaitForEndOfFrame();
        }
        isReloadingStamina = false;
    }

    Vector2 cameraPosXBounds = new Vector2(Mathf.NegativeInfinity, Mathf.Infinity);
    Vector2 cameraPosYBounds = new Vector2(Mathf.NegativeInfinity, Mathf.Infinity);

    /// <summary>
    /// Controls the camera movement. Potentially can be heavily optimized.
    /// </summary>
    private void DetermineCameraMovement()
    {
        if (MapManager.currentRoom.blockCount > 1)
        {
            TransferCamera(MapManager.currentRoom.transform.position, 100);
            MultiRoomCameraBoundClamp();

            if (MapManager.currentRoom.blockCount == 2)
            {
                TransferCamera(new Vector3(
                Mathf.Clamp(transform.position.x, cameraPosXBounds.x, cameraPosXBounds.y),
                Mathf.Clamp(transform.position.y, cameraPosYBounds.x, cameraPosYBounds.y)), 100);
            }
            else
            {
                if((MapManager.currentRoom.walls[0] || MapManager.currentRoom.walls[1]) && !(MapManager.currentRoom.walls[2] && MapManager.currentRoom.walls[3]) && !(MapManager.currentRoom.walls[0] && MapManager.currentRoom.walls[1]))
                {
                    cameraPosXBounds = new Vector2(MapManager.currentRoom.transform.position.x, MapManager.currentRoom.transform.position.x);
                }

                //if (currentRoomJ < previousRoomJ || currentRoomJ > previousRoomJ)
                //{
                //    cameraPosYBounds = new Vector2(Camera.main.transform.position.y, Camera.main.transform.position.y);
                //}
                //else if(currentRoomI != previousRoomI)
                //{
                //    cameraPosXBounds = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.x);
                //}

                TransferCamera(new Vector3(
                Mathf.Clamp(transform.position.x, cameraPosXBounds.x, cameraPosXBounds.y),
                Mathf.Clamp(transform.position.y, cameraPosYBounds.x, cameraPosYBounds.y)), 100);
            }
        }
        else
        {
            Camera.main.transform.position = new Vector3(MapManager.currentRoom.transform.position.x, MapManager.currentRoom.transform.position.y, Camera.main.transform.position.z);
        }
    }

    /// <summary>
    /// Called when the current room contains more than 1 blocks. Clamps the camera according to the existing walls of the current room.
    /// </summary>
    private void MultiRoomCameraBoundClamp()
    {
        cameraPosXBounds = new Vector2(
            MapManager.currentRoom.walls[3] ? Camera.main.transform.position.x : Mathf.NegativeInfinity,
            MapManager.currentRoom.walls[2] ? Camera.main.transform.position.x : Mathf.Infinity);
        cameraPosYBounds = new Vector2(
            MapManager.currentRoom.walls[1] ? Camera.main.transform.position.y : Mathf.NegativeInfinity,
            MapManager.currentRoom.walls[0] ? Camera.main.transform.position.y : Mathf.Infinity);
    }

    /// <summary>
    /// Makes the player go towards the mouse pointer.
    /// </summary>
    private void FollowMouseMovement()
    {
        //Debug.Log(MousePointer.currentPosition);
        transform.LookAt(MousePointer.currentPosition);
        rigidbody.AddForce(transform.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (checksForDoorTrigger && collision.GetComponent<Door>())
        {
            OnDoorEnter();
            MapManager.OnRoomChange(collision.GetComponent<Door>().doorDirection);
            StartCoroutine(DoorTriggerCheckCooldown());

            rigidbody.Sleep();
            transform.position += (Vector3)MapManager.movementAfterDoor;
            rigidbody.WakeUp();
            rigidbody.velocity = Vector2.zero;

            OnDoorLeave();
            MousePointer.instance.transform.position = new Vector3(transform.position.x + transform.forward.x, transform.position.y + transform.forward.y, MousePointer.instance.transform.position.z);
        }
        else if(collision.GetComponent<Water>())
        {
            StartCoroutine(ReloadStamina());
            Debug.Log("P");
        }
        else if(collision.GetComponent<JigsawPiece>())
        {
            Jigsaw.forceAppeared = true;
            collectedPieces.Add(collision.GetComponent<JigsawPiece>());
            collision.transform.parent = this.transform;
            collision.transform.localPosition = Vector2.zero;
            collision.GetComponent<BoxCollider2D>().enabled = false;
            collision.GetComponent<SpriteRenderer>().enabled = false;
        }
        else if (collision.GetComponent<MemoryTile>())
        {
            collision.GetComponent<MemoryTile>().OnPlayerEnter();
        }
        else if (collision.GetComponent<MusicTile>())
        {
            collision.GetComponent<MusicTile>().OnPlayerEnter();
        }
        else if (collision.CompareTag("MemoryGridSubmitTile"))
        {
            StartCoroutine(TogglePuzzleBool_MemoryGridSubmit());
        }
        else if (collision.CompareTag("MusicPuzzleSubmitTile"))
        {
            StartCoroutine(TogglePuzzleBool_MusicPuzzleSubmit());
        }
        else if(collision.CompareTag("LetterPuzzleSubmitTile"))
        {
            StartCoroutine(TogglePuzzleBool_LetterPuzzle());
        }
        else if(collision.CompareTag("LetterPuzzleUpArrow"))
        {
            collision.GetComponentInParent<LetterTile>().OnPlayerEnteredOnArrow(true);
        }
        else if (collision.CompareTag("LetterPuzzleDownArrow"))
        {
            collision.GetComponentInParent<LetterTile>().OnPlayerEnteredOnArrow(false);
        }
        else if (collision.CompareTag("TimeTrialArrowStartTile"))
        {
            StartCoroutine(TogglePuzzleBool_TimeTrialArrow());
        }
        else if (collision.CompareTag("TimeTrialAAStartTile"))
        {
            StartCoroutine(TogglePuzzleBool_TimeTrialAdjacentsAdjacent());
        }
    }

    /// <summary>
    /// Called when the player enters a door. Moves the previous room behind the Z position that should be used for current rooms.
    /// </summary>
    private void OnDoorEnter()
    {
        foreach (Room r in currentRooms)
        {
            r.transform.position = new Vector3(r.transform.position.x, r.transform.position.y, -5f);
        }

        // Uses previous room's coordinates:
        if (MusicPuzzle.musicPlayerRoomCoords != null)
        {
            if (currentRooms.Contains(MapManager.roomGrid[MusicPuzzle.musicPlayerRoomCoords.Item1][MusicPuzzle.musicPlayerRoomCoords.Item2]))
            {
                leftMusicPlayerRoom = true;
                StartCoroutine(TogglePuzzleBool_MusicPlayerRoomLeave());
            }
        }

        previousRoomI = currentRoomI;
        previousRoomJ = currentRoomJ;
    }

    /// <summary>
    /// Called when the player leaves the door (after movementAfterDoor position is applied). Moves the current room to the Z position that should be used for current rooms.
    /// </summary>
    private void OnDoorLeave()
    {
        if (!GameMenuUI.isUIActive)
        {
            UI_Hint.SetHint("");

            SetCurrentRooms();
            //Debug.Log(currentRooms.Count);
            foreach (Room r in currentRooms)
            {
                r.transform.position = new Vector3(r.transform.position.x, r.transform.position.y, -8f);
            }

            // Using new (actually, current) room's coordinates:
            if (MusicPuzzle.musicPlayerRoomCoords != null && currentRoomI == MusicPuzzle.musicPlayerRoomCoords.Item1 && currentRoomJ == MusicPuzzle.musicPlayerRoomCoords.Item2)
            {
                enteredMusicPlayerRoom = true;
                StartCoroutine(TogglePuzzleBool_MusicPlayerRoom());
            }
            else if (MemoryGrid.gridPos != null && currentRoomI == MemoryGrid.gridPos.Item1 && currentRoomJ == MemoryGrid.gridPos.Item2)
            {
                if (!enteredMemoryGridRoomPreviously)
                {
                    enteredMemoryGridRoomPreviously = true;
                    StartCoroutine(UI_Hint.SetHint("Color the tiles in the right way! Red means this puzzle's position, the rest are other rooms nearby. Black means that there is no room, " +
                        "white means that a room exists at the given position. Once you're done, step on the button!", 10f));
                }
            }
            else if (LetterPuzzle.gridPos != null && currentRoomI == LetterPuzzle.gridPos.Item1 && currentRoomJ == LetterPuzzle.gridPos.Item2)
            {
                if (!enteredLetterPuzzleRoomPreviously)
                {
                    enteredLetterPuzzleRoomPreviously = true;
                    StartCoroutine(UI_Hint.SetHint("Find the letters across the map and write them in order! There is a number above each, to make your life easier.", 4f));
                }
            }
            else if (ClockPuzzle.jigsawSpawnGridPos != null && currentRoomI == ClockPuzzle.jigsawSpawnGridPos.Item1 && currentRoomJ == ClockPuzzle.jigsawSpawnGridPos.Item2)
            {
                if (!enteredClockJigsawSpawnRoomPreviously)
                {
                    enteredClockJigsawSpawnRoomPreviously = true;
                    StartCoroutine(UI_Hint.SetHint("A jigsaw piece spawns here when it's " + ClockPuzzle.puzzleActivityHour + " o'clock.", 4f));
                }
            }
            else if (SwitchPuzzle.gridPos != null && currentRoomI == SwitchPuzzle.gridPos.Item1 && currentRoomJ == SwitchPuzzle.gridPos.Item2)
            {
                if (!enteredSwitchPreviously)
                {
                    enteredSwitchPreviously = true;
                    StartCoroutine(UI_Hint.SetHint("Pulling this switch may destroy an obstacle around a jigsaw piece.", 4f));
                }
            }
            else if (BallPuzzle.gridPos != null && currentRoomI == BallPuzzle.gridPos.Item1 && currentRoomJ == BallPuzzle.gridPos.Item2)
            {
                if (!enteredBallPuzzleRoomPreviously)
                {
                    enteredBallPuzzleRoomPreviously = true;
                    StartCoroutine(UI_Hint.SetHint("Find a ball and get it into this hole!", 4f));
                }
            }
            else if (Ball.gridPos != null && currentRoomI == Ball.gridPos.Item1 && currentRoomJ == Ball.gridPos.Item2)
            {
                if (!enteredBallRoomPreviously)
                {
                    enteredBallRoomPreviously = true;
                    StartCoroutine(UI_Hint.SetHint("Find a hole somewhere on the map, and make the ball go into it!", 10f));
                }
            }
        }
    }

    public static bool enteredMemoryGridSubmit { get; private set; }
    public static bool enteredMusicPuzzleSubmit { get; private set; }
    public static bool enteredLetterPuzzleSubmit { get; private set; }
    public static bool leftMusicPlayerRoom { get; private set; }
    public static bool enteredMusicPlayerRoom { get; private set; } // where the sounds for music puzzle get played in the correct order
    public static bool startedTimeTrialArrowPuzzle { get; private set; }
    public static bool startedTimeTrialAA { get; private set; }
    bool enteredMusicPlayerRoomPreviously = false, enteredTTArrowPreviously = false, enteredTTAAPreviously = false, enteredMemoryGridRoomPreviously = false, 
        enteredLetterPuzzleRoomPreviously = false, enteredClockJigsawSpawnRoomPreviously = false, enteredSwitchPreviously = false, 
        enteredBallRoomPreviously = false, enteredBallPuzzleRoomPreviously = false;

    private IEnumerator TogglePuzzleBool_TimeTrialArrow()
    {
        if (!enteredTTArrowPreviously)
        {
            enteredTTArrowPreviously = true;
            StartCoroutine(UI_Hint.SetHint("Follow the arrow instructions and reach the room which contains the jigsaw piece before you run out of time!", 8f));
        }
        startedTimeTrialArrowPuzzle = true;
        yield return new WaitForEndOfFrame();
        startedTimeTrialArrowPuzzle = false;
    }

    private IEnumerator TogglePuzzleBool_TimeTrialAdjacentsAdjacent()
    {
        if (!enteredTTAAPreviously)
        {
            enteredTTAAPreviously = true;
            StartCoroutine(UI_Hint.SetHint("A jigsaw piece has spawned in one of this room's adjacent room's adjacents. Find it, quickly!", 8f));
        }
        startedTimeTrialAA = true;
        yield return new WaitForEndOfFrame();
        startedTimeTrialAA = false;
    }

    private IEnumerator TogglePuzzleBool_MemoryGridSubmit()
    {
        enteredMemoryGridSubmit = true;
        yield return new WaitForEndOfFrame();
        enteredMemoryGridSubmit = false;
    }

    private IEnumerator TogglePuzzleBool_MusicPuzzleSubmit()
    {
        enteredMusicPuzzleSubmit = true;
        yield return new WaitForEndOfFrame();
        enteredMusicPuzzleSubmit = false;
    }

    private IEnumerator TogglePuzzleBool_MusicPlayerRoom()
    {
        if (!enteredMusicPlayerRoomPreviously)
        {
            enteredMusicPlayerRoomPreviously = true;
            StartCoroutine(UI_Hint.SetHint("Listen to the tone being played. Then, find the room with tiles that play similar sounds and repeat the sounds in order!", 8f));
        }
        enteredMusicPlayerRoom = true;
        yield return new WaitForEndOfFrame();
        enteredMusicPlayerRoom = false;
    }

    private IEnumerator TogglePuzzleBool_MusicPlayerRoomLeave()
    {
        leftMusicPlayerRoom = true;
        yield return new WaitForEndOfFrame();
        leftMusicPlayerRoom = false;
    }

    private IEnumerator TogglePuzzleBool_LetterPuzzle()
    {
        enteredLetterPuzzleSubmit = true;
        yield return new WaitForEndOfFrame();
        enteredLetterPuzzleSubmit = false;
    }

    /// <summary>
    /// Called when entering a door in order to avoid continous enter events on two doors. This is a previous bug that's most likely fixed now, but I'd rather keep this and remove it once
    /// I'm done with more important things.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DoorTriggerCheckCooldown()
    {
        checksForDoorTrigger = false;
        yield return new WaitForSeconds(0.3f);
        checksForDoorTrigger = true;
    }

    /// <summary>
    /// Sets the current rooms by casting the player position to grid position.
    /// </summary>
    private void SetCurrentRooms()
    {
        currentRooms.Clear();
        currentRoomI = (int)Mathf.Round(transform.position.y / MapManager.roomOffsets.y);
        currentRoomJ = (int)Mathf.Round(transform.position.x / MapManager.roomOffsets.x);

        CollectCurrentRooms(currentRoomI, currentRoomJ);
    }

    /// <summary>
    /// Finds all rooms that can be visited from the room the player is currently in, without having to enter doors.
    /// </summary>
    /// <param name="fromI"></param>
    /// <param name="fromJ"></param>
    private void CollectCurrentRooms(int fromI, int fromJ)
    {
        currentRooms.Add(MapManager.roomGrid[fromI][fromJ]);
        if (!MapManager.roomGrid[fromI][fromJ].walls[0] && !currentRooms.Contains(MapManager.roomGrid[fromI + 1][fromJ])) CollectCurrentRooms(fromI + 1, fromJ);
        if (!MapManager.roomGrid[fromI][fromJ].walls[1] && !currentRooms.Contains(MapManager.roomGrid[fromI - 1][fromJ])) CollectCurrentRooms(fromI - 1, fromJ);
        if (!MapManager.roomGrid[fromI][fromJ].walls[2] && !currentRooms.Contains(MapManager.roomGrid[fromI][fromJ + 1])) CollectCurrentRooms(fromI, fromJ + 1);
        if (!MapManager.roomGrid[fromI][fromJ].walls[3] && !currentRooms.Contains(MapManager.roomGrid[fromI][fromJ - 1])) CollectCurrentRooms(fromI, fromJ - 1);
    }

    private void TransferCamera(Vector2 destination, int tickAmount)
    {
        Vector2 offset = (Vector3)destination - Camera.main.transform.position;

        for(int i = 0; i < tickAmount; i++)
        {
            Camera.main.transform.position += (Vector3)(offset / tickAmount);
        }
    }
}
