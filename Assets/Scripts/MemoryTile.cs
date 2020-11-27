using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryTile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer { get; private set; }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer.color != Color.red) spriteRenderer.color = (Random.Range(0, 2) == 0 ? Color.white : Color.black); 
    }

    /// <summary>
    /// Called when the player enters this tile. Based on what color it is right now, it changes to either black or white.
    /// </summary>
    public void OnPlayerEnter()
    {
        if (spriteRenderer.color != Color.red)
        {
            if (spriteRenderer.color == Color.white) spriteRenderer.color = Color.black;
            else spriteRenderer.color = Color.white;
        }
    }
}
