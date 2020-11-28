using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTile : MonoBehaviour
{
    [HideInInspector] public AudioClip tileSound = default;

    /// <summary>
    /// Called when the player enters this tile. Passes its own sound to the music puzzle's method.
    /// </summary>
    public void OnPlayerEnter()
    {
        MusicPuzzle.OnTileEntered(tileSound);
    }
}
