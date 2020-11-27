using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicPuzzle : Puzzle
{
    [SerializeField] MusicTile sampleTileObject = default;
    [SerializeField] Transform jigsawPieceTransform = default;
    [SerializeField] AudioClip[] sounds = default;
    AudioClip[] solutionList = default;
    static List<AudioClip> playerInputList = default;
    static AudioSource audioSource = default;
    Vector2 offsetBetweenTiles = new Vector2();
    static bool isCompleted = false;

    private void OnBecameInvisible()
    {
        audioSource.Stop();
    }

    // Start is called before the first frame update
    void Start()
    {
        playerInputList = new List<AudioClip>();
        audioSource = GetComponent<AudioSource>();
        offsetBetweenTiles = sampleTileObject.GetComponent<SpriteRenderer>().bounds.size;
        GenerateGrid();
        CreateSolution();

        //OnCorrectAnswer();
    }

    private void Update()
    {
        if (Player.enteredMemoryGridPuzzle && !isCompleted) OnAnswerSubmitted();
    }

    private void CreateSolution()
    {
        int solutionRange = Random.Range(3, 8);
        solutionList = new AudioClip[solutionRange];

        for(int i = 0; i < solutionList.Length; i++)
        {
            solutionList[i] = sounds[Random.Range(0, sounds.Length)];
        }
    }

    private void GenerateGrid()
    {
        for (int i = 0; i < sounds.Length; i++) // The amount of sounds also represents the number of possible playable sounds.
        {
            GameObject musicTile = Instantiate(
                sampleTileObject.gameObject,
                transform.position + new Vector3(i * offsetBetweenTiles.x, 0f),
                Quaternion.identity,
                this.transform);

            musicTile.GetComponent<MusicTile>().tileSound = sounds[i];
        }
    }

    public static void OnTileEntered(AudioClip soundOfTtile)
    {
        playerInputList.Add(soundOfTtile);
        audioSource.clip = soundOfTtile;
        audioSource.Play();
    }

    protected override void OnAnswerSubmitted()
    {
        if(solutionList.Length != playerInputList.Count)
        {
            playerInputList.Clear();
            return;
        }

        for(int i = 0; i < solutionList.Length; i++)
        {
            if (!solutionList[i].Equals(playerInputList[i])) return;
        }

        OnCorrectAnswer();
    }

    protected override void OnCorrectAnswer()
    {
        isCompleted = true;

        JigsawPosition jigsawPosition = Puzzle.jigsawPieceDict[this];
        JigsawPiece jigsawPiece = jigsawPieceTransform.gameObject.AddComponent<JigsawPiece>();

        BoxCollider2D boxCollider = jigsawPieceTransform.gameObject.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector2(0.8f, 0.8f);

        jigsawPieceTransform.GetComponent<SpriteRenderer>().sprite = MapManager.s_jigsawPieceSprites[(int)jigsawPosition];
        jigsawPiece.jigsawPosition = jigsawPosition;
    }
}
