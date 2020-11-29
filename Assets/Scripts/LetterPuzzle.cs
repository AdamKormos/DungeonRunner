using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// @EarlyExec Executes earlier than supposed to by -50.
/// </summary>
public class LetterPuzzle : Puzzle
{
    [SerializeField] Bound letterAmountRange;
    [SerializeField] LetterTile sampleTileObject = default;
    [SerializeField] GameObject sampleLetterObject = default;
    [SerializeField] Transform jigsawPieceTransform = default;
    char[] solution = default;
    bool isCompleted = false;
    int originalComponentsLength = 0;
    float xOffsetBetweenTiles = 0f;
    List<LetterTile> letterTiles = new List<LetterTile>();

    // Start is called before the first frame update
    void Start()
    {
        xOffsetBetweenTiles = sampleTileObject.GetComponent<MeshRenderer>().bounds.size.x * 2f;
        originalComponentsLength = components.Length;
        solution = new char[Random.Range(letterAmountRange.min, letterAmountRange.max+1)];
        components = new PuzzleComponent[components.Length + solution.Length];

        GenerateLetterTileRow();
        CreateSolution();   
    }

    /// <summary>
    /// Creates the interactable letter tiles' row.
    /// </summary>
    private void GenerateLetterTileRow()
    {
        for (int i = 0; i < solution.Length; i++) // The amount of sounds also represents the number of possible playable sounds.
        {
            GameObject letterTile = Instantiate(
                sampleTileObject.gameObject,
                transform.position + new Vector3(i * xOffsetBetweenTiles - 1.5f, 0f),
                Quaternion.identity,
                this.transform);
            letterTiles.Add(letterTile.GetComponent<LetterTile>());
            letterTiles[i].Init();
        }
    }

    /// <summary>
    /// Puts the solution string together by randomly picking a letter from each tile's letter set.
    /// </summary>
    private void CreateSolution()
    {
        for(int i = 0; i < solution.Length; i++)
        {
            solution[i] = letterTiles[i].letterSelection[Random.Range(0, letterTiles[i].letterSelection.Length)];
            AddNthLetterToPuzzleComponentList(i);
        }

        for (int i = 0; i < solution.Length; i++)
        {
            //Debug.Log((i+1) + ". : " + solution[i]);
        }
    }

    /// <summary>
    /// Adds a letter to this puzzle's component list. This is done in the way it is because each letter tile is supposed to be in different rooms, and when
    /// spawning components, that factor is already considered and I didn't want to break the whole structure by spawning this puzzle's components earlier, branching in MapManager's puzzle
    /// spawner, etc.
    /// </summary>
    private void AddNthLetterToPuzzleComponentList(int i)
    {
        GameObject letter = Instantiate(sampleLetterObject);
        TextMesh[] textMeshes = letter.GetComponentsInChildren<TextMesh>(true);
        textMeshes[0].text = solution[i].ToString(); // The letter
        textMeshes[1].text = (i + 1).ToString(); // The number
        components[originalComponentsLength + i].objectToSpawn = letter;
        Destroy(letter);
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.enteredLetterPuzzleSubmit && !isCompleted) OnAnswerSubmitted();
    }

    protected override void OnAnswerSubmitted()
    {
        LetterTile[] letterTiles = GetComponentsInChildren<LetterTile>(true);

        for (int i = 0; i < solution.Length; i++)
        {
            if (solution[i] != letterTiles[i].GetCurrentlyDisplayedChar()) return;
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

        foreach (LetterTile letterTile in GetComponentsInChildren<LetterTile>(true))
        {
            letterTile.GetComponent<Collider2D>().enabled = false; // So that the player can't mess around with the tiles anymore
        }
    }
}

