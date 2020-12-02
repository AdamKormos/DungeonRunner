using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteAlways]
public class Room : MonoBehaviour
{
    [SerializeField] public int blockCount = 1;
    public bool containsPuzzle = false;

    /// <summary>
    /// up, down, right, left
    /// </summary>
    public bool[] walls = new bool[4];
    
    /// <summary>
    /// up, down, right, left
    /// </summary>
    public bool[] doors = new bool[4];

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if(!Application.isPlaying && Selection.activeGameObject != null && Selection.activeGameObject == this.gameObject)
        {
            Vector2 roomOffsets = new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize) * 2f;

            transform.position = new Vector3(
                transform.position.x - (transform.position.x % roomOffsets.x),
                transform.position.y - (transform.position.y % roomOffsets.y),
                transform.position.z);
        }
#endif
    }

    /// <summary>
    /// Does this room have any doors?
    /// </summary>
    /// <returns></returns>
    public bool HasAnyDoors() { return doors[0] || doors[1] || doors[2] || doors[3]; }

    public int GetDoorCount()
    {
        int count = 0;
        foreach (bool door in doors) if (door) count++;
        return count;
    }

    /// <summary>
    /// Same as GetDoorCount() but includes sides where walls don't exist.
    /// </summary>
    /// <returns></returns>
    public int GetEntranceCount()
    {
        int count = 0;
        foreach (bool door in doors) if (door) count++;
        foreach (bool wall in walls) if (!wall) count++;
        return count;
    }

    /// <summary>
    /// Gets the door of the given direction.
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public Door GetDoorOfDirection(Direction d)
    {
        foreach(Door door in GetComponentsInChildren<Door>(true))
        {
            if (door.doorDirection == d) return door;
        }
        return null;
    }

    /// <summary>
    /// Removes a door from the given direction.
    /// </summary>
    /// <param name="d"></param>
    public void RemoveDoor(Direction d)
    {
        Destroy(GetDoorOfDirection(d).gameObject);
        doors[(int)d] = false;
    }

    /// <summary>
    /// Sets multiwalls where needed.
    /// </summary>
    /// <param name="multiSprite"></param>
    public void SetMultiWalls(Sprite multiSprite)
    {
        foreach (Wall w in GetComponentsInChildren<Wall>(true))
        {
            switch(w.direction)
            {
                case Direction.Up:
                    if (!walls[2]) w.GetComponent<SpriteRenderer>().sprite = multiSprite;
                    else if (!walls[3])
                    {
                        w.GetComponent<SpriteRenderer>().sprite = multiSprite;
                        w.transform.Rotate(0f, 180f, 0f);
                    }
                    break;
                case Direction.Down:
                    if (!walls[3]) w.GetComponent<SpriteRenderer>().sprite = multiSprite;
                    else if (!walls[2])
                    {
                        w.GetComponent<SpriteRenderer>().sprite = multiSprite;
                        w.transform.Rotate(0f, 180f, 0f);
                    }
                    break;
                case Direction.Right:
                    if (!walls[1]) w.GetComponent<SpriteRenderer>().sprite = multiSprite;
                    else if (!walls[0])
                    {
                        w.GetComponent<SpriteRenderer>().sprite = multiSprite;
                        w.transform.Rotate(0f, 180f, 0f);
                    }
                    break;
                case Direction.Left:
                    if (!walls[0]) w.GetComponent<SpriteRenderer>().sprite = multiSprite;
                    else if (!walls[1])
                    {
                        w.GetComponent<SpriteRenderer>().sprite = multiSprite;
                        w.transform.Rotate(0f, 180f, 0f);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Determines whether the given two rooms are adjacent or not.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool AreNeighbours(Room a, Room b)
    {
        return MapManager.GetDistanceBetweenRoomsByPosition(a, b) == 1;
    }
}
