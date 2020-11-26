using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jigsaw : MonoBehaviour
{
    bool[] insertedPieces = new bool[4];

    private void OnBecameVisible()
    {
        foreach (JigsawPieceHolder jigsawPieceHolder in GetComponentsInChildren<JigsawPieceHolder>(true))
        {
            foreach (JigsawPiece jigsawPiece in Player.collectedPieces)
            {
                if(jigsawPieceHolder.jigsawPosition == jigsawPiece.jigsawPosition && !insertedPieces[(int)jigsawPiece.jigsawPosition])
                {
                    Vector2 distance = jigsawPieceHolder.transform.position - jigsawPiece.transform.position;

                    //Debug.Log("Go");
                    StartCoroutine(InsertPiece(jigsawPiece, distance));
                    insertedPieces[(int)jigsawPiece.jigsawPosition] = true;
                }
            }
        }
    }

    private IEnumerator InsertPiece(JigsawPiece jigsawPiece, Vector2 distance)
    {
        jigsawPiece.transform.parent = this.transform;
        jigsawPiece.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        jigsawPiece.GetComponent<SpriteRenderer>().enabled = true;
        
        Vector3 distancePerTick = distance / 100f;
        Vector3 scalePerTick = (new Vector3(1f, 1f) - jigsawPiece.transform.localScale) / 100f;

        for (int i = 0; i < 250; i++)
        {
            jigsawPiece.transform.position += distancePerTick;
            jigsawPiece.transform.localScale += scalePerTick;
            yield return new WaitForEndOfFrame();
        }
    }
}
