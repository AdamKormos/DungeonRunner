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
    public static int currentRoomI;
    public static int currentRoomJ;
    bool isReloadingStamina = false;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(MapManager.currentRoom.transform.position.x, MapManager.currentRoom.transform.position.y, -6f);
        rigidbody = GetComponent<Rigidbody2D>();
        cameraResolutionBounds = new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize);
        StartCoroutine(HandleStamina());
    }

    // Update is called once per frame
    void Update()
    {
        currentRoomI = (int)Mathf.Round(transform.position.y / MapManager.roomOffsets.y);
        currentRoomJ = (int)Mathf.Round(transform.position.x / MapManager.roomOffsets.x);

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
            if (!MapManager.currentRoom.walls[0] || !MapManager.currentRoom.walls[1])
            {
                Camera.main.transform.position = new Vector3(MapManager.currentRoom.transform.position.x, MapManager.currentRoom.transform.position.y, Camera.main.transform.position.z);
                cameraPosXBounds = new Vector2(
                    MapManager.currentRoom.walls[3] ? Camera.main.transform.position.x : Mathf.NegativeInfinity,
                    MapManager.currentRoom.walls[2] ? Camera.main.transform.position.x : Mathf.Infinity);

                if (MapManager.currentRoom.walls[0])
                {
                    cameraPosYBounds.y = Camera.main.transform.position.y;
                }
                else if (MapManager.currentRoom.walls[1])
                {
                    cameraPosYBounds.x = Camera.main.transform.position.y;
                }
            }
            else if (!MapManager.currentRoom.walls[2] || !MapManager.currentRoom.walls[3])
            {
                Debug.Log("Z");
                Camera.main.transform.position = new Vector3(MapManager.currentRoom.transform.position.x, MapManager.currentRoom.transform.position.y, Camera.main.transform.position.z);
                cameraPosYBounds = new Vector2(
                    MapManager.currentRoom.walls[1] ? Camera.main.transform.position.y : Mathf.NegativeInfinity,
                    MapManager.currentRoom.walls[0] ? Camera.main.transform.position.y : Mathf.Infinity);

                if (MapManager.currentRoom.walls[2])
                {
                    cameraPosXBounds.y = Camera.main.transform.position.x;
                }
                else if (MapManager.currentRoom.walls[3])
                {
                    cameraPosXBounds.x = Camera.main.transform.position.x;
                }
            }
        }
        else
        {
            cameraPosXBounds = new Vector2(Mathf.NegativeInfinity, Mathf.Infinity);
            cameraPosYBounds = new Vector2(Mathf.NegativeInfinity, Mathf.Infinity);
        }

        if (MapManager.currentRoom.blockCount == 1)
        {
            Camera.main.transform.position = new Vector3(MapManager.currentRoom.transform.position.x, MapManager.currentRoom.transform.position.y, Camera.main.transform.position.z);
        }
        else
        {
            Camera.main.transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, cameraPosXBounds.x, cameraPosXBounds.y), 
                Mathf.Clamp(transform.position.y, cameraPosYBounds.x, cameraPosYBounds.y), 
                Camera.main.transform.position.z);
        }
    }

    private void FollowMouseMovement()
    {
        //Debug.Log(MousePointer.currentPosition);
        transform.LookAt(MousePointer.currentPosition);
        rigidbody.AddForce(transform.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Door>())
        {
            //collision.GetComponent<Door>().OnPlayerEnter();
            MapManager.OnRoomChange(collision.GetComponent<Door>().doorDirection);

            //Debug.Log(MapManager.currentRoom.blockCount);

            rigidbody.Sleep();
            transform.position += (Vector3)MapManager.movementAfterDoor;
            rigidbody.WakeUp();
            rigidbody.velocity = Vector2.zero;

            collision.GetComponent<Door>().OnPlayerEnter();
            MousePointer.instance.transform.position = new Vector3(transform.position.x + transform.forward.x, transform.position.y + transform.forward.y, MousePointer.instance.transform.position.z);
            //StartCoroutine(DoorTriggerCheckCooldown());
        }
        else if(collision.GetComponent<Water>())
        {
            StartCoroutine(ReloadStamina());
            Debug.Log("P");
        }
    }
}
