using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { Up, Down, Right, Left };

public class Door : MonoBehaviour
{
    [SerializeField] public Direction doorDirection;

    public void OnPlayerEnter()
    {
        Debug.Log("Player entered!");
        //MapManager.OnRoomChange(doorDirection);
    }
}
