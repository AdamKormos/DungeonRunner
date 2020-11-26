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
    [SerializeField] Rigidbody2D rigidbody;
    public static Vector2 cameraResolutionBounds { get; private set; }
    public static int currentRoomI, currentRoomJ, previousRoomI, previousRoomJ;
    bool isReloadingStamina = false, checksForDoorTrigger = true;
    public static List<JigsawPiece> collectedPieces { get; private set; }

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
    }

    IEnumerator A()
    {
        yield return new WaitForEndOfFrame();
        JigsawPiece obj = FindObjectOfType<JigsawPiece>();
        collectedPieces.Add(obj);
        obj.transform.parent = this.transform;
        obj.transform.localPosition = Vector2.zero;
        obj.GetComponent<BoxCollider2D>().enabled = false;
        obj.GetComponent<SpriteRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        currentRoomI = (int)Mathf.Round(transform.position.y / MapManager.roomOffsets.y);
        currentRoomJ = (int)Mathf.Round(transform.position.x / MapManager.roomOffsets.x);

        //if (Input.GetKeyDown(KeyCode.Space)) StartCoroutine(A());

        //Debug.Log(MapManager.currentRoom.blockCount);
        FollowMouseMovement();
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

    private void DetermineCameraMovement()
    {
        if (MapManager.currentRoom.blockCount > 1)
        {
            TransferCamera(MapManager.currentRoom.transform.position, 100);

            if (MapManager.currentRoom.blockCount == 2)
            {
                MultiRoomCameraBoundClamp();

                TransferCamera(new Vector3(
                Mathf.Clamp(transform.position.x, cameraPosXBounds.x, cameraPosXBounds.y),
                Mathf.Clamp(transform.position.y, cameraPosYBounds.x, cameraPosYBounds.y)), 100);
            }
            else
            {
                MultiRoomCameraBoundClamp();

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

    private void MultiRoomCameraBoundClamp()
    {
        cameraPosXBounds = new Vector2(
            MapManager.currentRoom.walls[3] ? Camera.main.transform.position.x : Mathf.NegativeInfinity,
            MapManager.currentRoom.walls[2] ? Camera.main.transform.position.x : Mathf.Infinity);
        cameraPosYBounds = new Vector2(
            MapManager.currentRoom.walls[1] ? Camera.main.transform.position.y : Mathf.NegativeInfinity,
            MapManager.currentRoom.walls[0] ? Camera.main.transform.position.y : Mathf.Infinity);
    }

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
            collectedPieces.Add(collision.GetComponent<JigsawPiece>());
            collision.transform.parent = this.transform;
            collision.transform.localPosition = Vector2.zero;
            collision.GetComponent<BoxCollider2D>().enabled = false;
            collision.GetComponent<SpriteRenderer>().enabled = false;
        }
        else if(collision.GetComponent<MemoryTile>())
        {
            collision.GetComponent<MemoryTile>().OnPlayerEnter();
        }
        else if(collision.tag.Equals("MemoryGridSubmitTile"))
        {
            StartCoroutine(TogglePuzzleBool_MemoryGrid());
        }
    }

    public static bool enteredMemoryGridPuzzle { get; private set; }

    private IEnumerator TogglePuzzleBool_MemoryGrid()
    {
        enteredMemoryGridPuzzle = true;
        yield return new WaitForEndOfFrame();
        enteredMemoryGridPuzzle = false;
    }

    private IEnumerator DoorTriggerCheckCooldown()
    {
        checksForDoorTrigger = false;
        yield return new WaitForSeconds(0.5f);
        checksForDoorTrigger = true;
    }

    List<Room> currentRooms = new List<Room>();

    private void OnDoorEnter()
    {
        foreach(Room r in currentRooms)
        {
            r.transform.position = new Vector3(r.transform.position.x, r.transform.position.y, -5f);
        }

        previousRoomI = currentRoomI;
        previousRoomJ = currentRoomJ;
    }

    private void OnDoorLeave()
    {
        SetCurrentRooms();
        //Debug.Log(currentRooms.Count);
        foreach (Room r in currentRooms)
        {
            r.transform.position = new Vector3(r.transform.position.x, r.transform.position.y, -8f);
        }
    }

    private void SetCurrentRooms()
    {
        currentRooms.Clear();
        currentRoomI = (int)Mathf.Round(transform.position.y / MapManager.roomOffsets.y);
        currentRoomJ = (int)Mathf.Round(transform.position.x / MapManager.roomOffsets.x);

        CollectCurrentRooms(currentRoomI, currentRoomJ);
    }

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
