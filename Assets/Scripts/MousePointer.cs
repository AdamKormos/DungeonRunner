using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePointer : MonoBehaviour
{
    public static MousePointer instance;
    public static Vector2 currentPosition;
    [SerializeField] float sensitivity = 5f;

    private void Start()
    {
        instance = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        transform.position = new Vector3(MapManager.currentRoom.transform.position.x, MapManager.currentRoom.transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        CalculatePosition();
    }

    private void CalculatePosition()
    {
        float z = transform.position.z;

        #region Using actual mouse position
        //transform.position = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        // Commented to prevent the character from stopping at room edges:
        //currentMousePosition.x = Mathf.Clamp(currentMousePosition.x, 0f, 1f);
        //currentMousePosition.y = Mathf.Clamp(currentMousePosition.y, 0f, 1f);
        //transform.position -= new Vector3(0.5f, 0.5f);
        //transform.position *= 2f;

        #region Previous solutions
        // Attempt 1
        //currentMousePosition.x = (currentMousePosition.x < 0.5f ? cameraResolutionMinBounds.x + (currentMousePosition.x * 2f * cameraResolutionMaxBounds.x) 
        //    : ((currentMousePosition.x - 0.5f) * (cameraResolutionMaxBounds.x * 2f)));

        //currentMousePosition.y = Mathf.Clamp(currentMousePosition.y, 0f, 1f);
        //currentMousePosition.y = (currentMousePosition.y < 0.5f ? cameraResolutionMinBounds.y + (currentMousePosition.y * 2f * cameraResolutionMaxBounds.y)
        //    : ((currentMousePosition.y - 0.5f) * (cameraResolutionMaxBounds.y * 2f)));


        // Attempt 2
        //currentMousePosition.x = (currentMousePosition.x < 0f ? cameraResolutionMinBounds.x + (currentMousePosition.x * 2f * cameraResolutionMaxBounds.x)
        //    : ((currentMousePosition.x) * (cameraResolutionMaxBounds.x * 2f)));

        //currentMousePosition.y = (currentMousePosition.y < 0f ? cameraResolutionMinBounds.y + (currentMousePosition.y * 2f * cameraResolutionMaxBounds.y)
        //    : ((currentMousePosition.y) * (cameraResolutionMaxBounds.y * 2f)));
        #endregion

        //transform.position *= Player.cameraResolutionBounds;
        //transform.position += MapManager.currentRoom.transform.position;
        #endregion

        transform.position += new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f) * (0.1f * sensitivity);
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
        currentPosition = transform.position;
    }
}
