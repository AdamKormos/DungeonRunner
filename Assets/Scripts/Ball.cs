using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody2D rigidbody = default;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(transform.localScale.x / transform.parent.lossyScale.x, transform.localScale.y / transform.parent.lossyScale.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Door>())
        {
            Door door = collision.GetComponent<Door>();
            Vector2 movementAfterDoor = new Vector2();

            switch(door.doorDirection)
            {
                case Direction.Down:
                    movementAfterDoor = new Vector2(0f, -4.5f);
                    break;
                case Direction.Left:
                    movementAfterDoor = new Vector2(-4.5f, 0f);
                    break;
                case Direction.Up:
                    movementAfterDoor = new Vector2(0f, 4.5f);
                    break;
                case Direction.Right:
                    movementAfterDoor = new Vector2(4.5f, 0f);
                    break;
            }

            rigidbody.Sleep();
            transform.position += (Vector3)movementAfterDoor;
            rigidbody.WakeUp();
            rigidbody.velocity = Vector2.zero;
        }
    }
}
