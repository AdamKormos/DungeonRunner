using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTile : MonoBehaviour
{
    [HideInInspector] public AudioClip tileSound = default;

    public void OnPlayerEnter()
    {
        MusicPuzzle.OnTileEntered(tileSound);
    }
}
