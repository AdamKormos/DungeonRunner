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
    bool isCompleted = false;
    public static Tuple<int, int> musicPlayerRoomCoords { get; private set; }

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
        musicPlayerRoomCoords = MapManager.PositionToGridPosition(components[0].objectToSpawn.transform.position);

        GenerateMusicTileRow();
        CreateSolution();

        //OnCorrectAnswer();
    }

    private void Update()
    {
        if (Player.enteredMusicPuzzleSubmit && !isCompleted) OnAnswerSubmitted();
        else if (Player.enteredMusicPlayerRoom) StartCoroutine(PlaySolution());
    }

    /// <summary>
    /// Plays the solution's tones in the correct order with applied delay so that every sound gets played totally, without interruption.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlaySolution()
    {
        for (int i = 0; i < solutionList.Length; i++)
        {
            audioSource.clip = solutionList[i];
            audioSource.Play();
            yield return new WaitForSeconds(solutionList[i].length);
        }
    }

    /// <summary>
    /// Makes a random list of the existing sounds to be the solution list.
    /// </summary>
    private void CreateSolution()
    {
        int solutionRange = Random.Range(3, 8);
        solutionList = new AudioClip[solutionRange];

        for(int i = 0; i < solutionList.Length; i++)
        {
            //solutionList[i] = sounds[Random.Range(0, sounds.Length)]; @TempRemove
        }
    }

    /// <summary>
    /// Creates the row of music tiles with giving each other a sound that will be put on the player's sound input list which can later be checked if whether or not it's the correct solution.
    /// </summary>
    private void GenerateMusicTileRow()
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

    /// <summary>
    /// Called when the player enters a tile. It adds the tile's sound to the input list and plays the sound.
    /// </summary>
    /// <param name="soundOfTtile"></param>
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

        if (jigsawPieceDict.ContainsKey(this))
        {
            JigsawPosition jigsawPosition = jigsawPieceDict[this];
            JigsawPiece jigsawPiece = jigsawPieceTransform.gameObject.AddComponent<JigsawPiece>();

            BoxCollider2D boxCollider = jigsawPieceTransform.gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(0.8f, 0.8f);

            jigsawPieceTransform.GetComponent<SpriteRenderer>().sprite = MapManager.s_jigsawPieceSprites[(int)jigsawPosition];
            jigsawPiece.jigsawPosition = jigsawPosition;
        }

        foreach(MusicTile musicTile in GetComponentsInChildren<MusicTile>(true))
        {
            musicTile.GetComponent<Collider2D>().enabled = false; // So that the player can't mess around with the tiles anymore
        }
    }
}
