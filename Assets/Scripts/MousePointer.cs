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
        if(!GameMenuUI.isUIActive) CalculatePosition();
    }

    /// <summary>
    /// Calculates the position of the mouse pointer object by adding the continuous mouse movement to its position.
    /// </summary>
    private void CalculatePosition()
    {
        float z = transform.position.z;

        transform.position += new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f) * (0.1f * sensitivity);
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
        currentPosition = transform.position;
    }
}
