using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SwitchPuzzle : Puzzle
{
    bool isPlayerInRange = false;
    bool isObstacleActive = true;
    public static Tuple<int, int> gridPos { get; private set; }

    private void Start()
    {
        if (jigsawPieceDict.ContainsKey(this))
        {
            gridPos = MapManager.PositionToGridPosition(transform.position);
            // components[0] = jigsaw piece
            components[0].objectToSpawn.SetActive(isObstacleActive);

            JigsawPiece jigsawObject = components[0].objectToSpawn.GetComponentInChildren<JigsawPiece>(true);
            JigsawPosition jigsawPosition = jigsawPieceDict[this];
            jigsawObject.jigsawPosition = jigsawPosition;
            jigsawObject.GetComponentInChildren<SpriteRenderer>(true).sprite = MapManager.s_jigsawPieceSprites[(int)jigsawPosition];

            jigsawObject.transform.parent = this.transform;
            jigsawObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            jigsawObject.GetComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.6f);

            GetComponentInChildren<TextMesh>(true).gameObject.SetActive(false);
        }
        else
        {
            components[0].objectToSpawn.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Return)) OnPlayerUse(); 
    }

    public void OnPlayerUse()
    {
        isObstacleActive = !isObstacleActive;
        components[0].objectToSpawn.SetActive(isObstacleActive);
        transform.Rotate(0f, 180f, 0f);

        GameObject textMeshObject = GetComponentInChildren<TextMesh>(true).gameObject;
        textMeshObject.transform.position = new Vector3(textMeshObject.transform.position.x * -1, textMeshObject.transform.position.y, textMeshObject.transform.position.z);
        textMeshObject.transform.Rotate(0f, 180, 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>())
        {
            GetComponentInChildren<TextMesh>(true).gameObject.SetActive(true);
            isPlayerInRange = true;
            //Debug.Log("P");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>())
        {
            GetComponentInChildren<TextMesh>(true).gameObject.SetActive(false);
            isPlayerInRange = false;
        }
    }
}
