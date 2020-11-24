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
    //[HideInInspector] public int roomID = 0; // To determine which room set this room belongs to

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

    public bool HasAnyDoors() { return doors[0] || doors[1] || doors[2] || doors[3]; }

    public Door GetDoorOfDirection(Direction d)
    {
        foreach(Door door in GetComponentsInChildren<Door>(true))
        {
            if (door.doorDirection == d) return door;
        }
        return null;
    }

    public void RemoveDoor(Direction d)
    {
        Destroy(GetDoorOfDirection(d).gameObject);
        doors[(int)d] = false;
    }

    public void AssignWallSprites(Sprite singleSprite, Sprite multiSprite)
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
}
