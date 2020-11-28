using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryGrid : Puzzle
{
    [SerializeField] MemoryTile sampleTileObject = default;
    [SerializeField] Transform jigsawPieceTransform = default;
    [SerializeField] int gridWidth, gridHeight;
    Vector2 offsetBetweenTiles = new Vector2();
    int redTileI, redTileJ;
    static List<Color> solutionList = new List<Color>();
    bool isCompleted = false;

    // Start is called before the first frame update
    void Start()
    {
        offsetBetweenTiles = sampleTileObject.GetComponent<SpriteRenderer>().bounds.size;
        PickRedTileCoords();
        GenerateGrid();

        //OnCorrectAnswer();
    }

    private void Update()
    {
        if (Player.enteredMemoryGridSubmit && !isCompleted) OnAnswerSubmitted();
    }

    private void GenerateGrid()
    {
        Tuple<int, int> puzzleGridPos = MapManager.PositionToGridPosition(transform.position);

        for (int i = 0; i < gridHeight; i++)
        {
            for(int j = 0; j < gridWidth; j++)
            {
                GameObject memoryTile = Instantiate(
                    sampleTileObject.gameObject, 
                    transform.position + new Vector3(j * offsetBetweenTiles.x, i * offsetBetweenTiles.y), 
                    Quaternion.identity, 
                    this.transform);

                Tuple<int, int> distanceFromRedInCoords = new Tuple<int, int>(redTileI - i, redTileJ - j);
                Tuple<int, int> thisTilePos = new Tuple<int, int>(puzzleGridPos.Item1 - distanceFromRedInCoords.Item1, puzzleGridPos.Item2 - distanceFromRedInCoords.Item2);

                //Debug.Log("This tile pos is " + thisTilePos.Item1 + ", " + thisTilePos.Item2);
                if (thisTilePos.Item1 > -1 && thisTilePos.Item1 < MapManager.s_gridSize.y
                    && thisTilePos.Item2 > -1 && thisTilePos.Item2 < MapManager.s_gridSize.x && MapManager.roomGrid[thisTilePos.Item1][thisTilePos.Item2] != null)
                {
                    solutionList.Add(Color.white);
                }
                else solutionList.Add(Color.black);

                //Debug.Log("At " + thisTilePos + ", it's " + solutionTable[thisTilePos]);

                //memoryTile.GetComponent<SpriteRenderer>().color = solutionTable[thisTilePos];

                if (i == redTileI && j == redTileJ) memoryTile.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }

    private void PickRedTileCoords()
    {
        Tuple<int, int> puzzleGridPos = MapManager.PositionToGridPosition(transform.position);
        Bound2D toPickFrom = new Bound2D();

        toPickFrom.minX = Mathf.Clamp(puzzleGridPos.Item2, 0, MapManager.s_gridSize.x - 1);
        toPickFrom.maxX = Mathf.Clamp(puzzleGridPos.Item2 + 1, 0, MapManager.s_gridSize.x - 1);
        if (toPickFrom.maxX - toPickFrom.minX != 1)
        {
            if (toPickFrom.minX == 0) toPickFrom.maxX++;
            else if (toPickFrom.maxX == MapManager.s_gridSize.x - 1) toPickFrom.minX--;
        }

        toPickFrom.minY = Mathf.Clamp(puzzleGridPos.Item1-1, 0, MapManager.s_gridSize.y-1);
        toPickFrom.maxY = Mathf.Clamp(puzzleGridPos.Item1+2, 0, MapManager.s_gridSize.y-1);
        if(toPickFrom.maxY - toPickFrom.minY != 3)
        {
            if (toPickFrom.minY == 0) toPickFrom.maxY += 3 - (toPickFrom.maxY - toPickFrom.minY);
            else if (toPickFrom.maxY == MapManager.s_gridSize.y-1) toPickFrom.minY -= 3 - (toPickFrom.maxY - toPickFrom.minY);
        }

        //Debug.Log("Y: " + "[" + toPickFrom.minY + ", " + toPickFrom.maxY + "]");
        redTileI = UnityEngine.Random.Range(toPickFrom.minY, toPickFrom.maxY) - toPickFrom.minY;
        redTileJ = UnityEngine.Random.Range(toPickFrom.minX, toPickFrom.maxX) - toPickFrom.minX;
        //Debug.Log("(" + redTileI + ", " + redTileJ + ")");
    }

    /// <summary>
    /// Called when an answer was submitted. If the answer matches with the correct solution, OnCorrectAnswer() will be called.
    /// </summary>
    protected override void OnAnswerSubmitted()
    {
        MemoryTile[] tiles = GetComponentsInChildren<MemoryTile>(true);
        for (int i = 0; i < tiles.Length; i++)
        {
            if (solutionList[i] != tiles[i].spriteRenderer.color && tiles[i].spriteRenderer.color != Color.red)
            {
                for (int j = 0; j < tiles.Length; j++)
                {
                    if (tiles[j].spriteRenderer.color != Color.red)
                    {
                        tiles[j].spriteRenderer.color = (UnityEngine.Random.Range(0, 2) == 0 ? Color.white : Color.black);
                    }
                }
                return;
            }
        }

        OnCorrectAnswer();
    }

    /// <summary>
    /// Sets this puzzle completed and spawns the jigsaw piece that was assigned to this puzzle.
    /// </summary>
    protected override void OnCorrectAnswer()
    {
        isCompleted = true;

        JigsawPosition jigsawPosition = jigsawPieceDict[this];
        JigsawPiece jigsawPiece = jigsawPieceTransform.gameObject.AddComponent<JigsawPiece>();

        BoxCollider2D boxCollider = jigsawPieceTransform.gameObject.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector2(0.8f, 0.8f);

        jigsawPieceTransform.GetComponent<SpriteRenderer>().sprite = MapManager.s_jigsawPieceSprites[(int)jigsawPosition];
        jigsawPiece.jigsawPosition = jigsawPosition;

        foreach (MemoryTile memoryTile in GetComponentsInChildren<MemoryTile>(true))
        {
            memoryTile.GetComponent<Collider2D>().enabled = false; // So that the player can't mess around with the tiles anymore
        }
    }
}
