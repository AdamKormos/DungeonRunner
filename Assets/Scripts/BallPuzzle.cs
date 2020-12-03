using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BallPuzzle : Puzzle
{
    [SerializeField] Transform jigsawPieceTransform = default;
    bool isCompleted = false;
    public static Tuple<int, int> gridPos { get; private set; }

    private void Start()
    {
        gridPos = MapManager.PositionToGridPosition(transform.position);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Ball>())
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

            Destroy(collision.gameObject);
        }
    }
}
