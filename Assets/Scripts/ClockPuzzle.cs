using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class ClockPuzzle : Puzzle
{
    [SerializeField] float bigHandRotationDegreePerSec = 10f;
    [SerializeField] Sprite handSprite = default;
    GameObject littleHand = default, bigHand = default;
    JigsawPiece jigsawObject = default;
    public static int puzzleActivityHour { get; private set; }
    public static Tuple<int, int> gridPos { get; private set; }
    public static Tuple<int, int> jigsawSpawnGridPos { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        if (jigsawPieceDict.ContainsKey(this))
        {
            jigsawObject = components[0].objectToSpawn.GetComponent<JigsawPiece>();
            JigsawPosition jigsawPosition = jigsawPieceDict[this];
            jigsawObject.jigsawPosition = jigsawPosition;
            jigsawObject.GetComponentInChildren<SpriteRenderer>(true).sprite = MapManager.s_jigsawPieceSprites[(int)jigsawPosition];

            jigsawObject.transform.parent = this.transform;
            jigsawObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            jigsawObject.GetComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.6f);
            jigsawObject.gameObject.SetActive(false);

            gridPos = MapManager.PositionToGridPosition(transform.position);
            jigsawSpawnGridPos = MapManager.PositionToGridPosition(jigsawObject.transform.position);
        }
        else components[0].objectToSpawn.SetActive(false);

        {
            littleHand = Instantiate(new GameObject(), this.transform);
            littleHand.transform.position += new Vector3(0f, 0f, -0.25f);
            littleHand.transform.localScale += new Vector3(0f, -0.5f, 0f);
            littleHand.name = "LittleHand";
            SpriteRenderer spriteRenderer = littleHand.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = handSprite;
        }

        {
            bigHand = Instantiate(new GameObject(), this.transform);
            bigHand.transform.position += new Vector3(0f, 0f, -0.25f);
            bigHand.name = "BigHand";
            SpriteRenderer spriteRenderer = bigHand.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = handSprite;
        }

        puzzleActivityHour = Random.Range(0, 12);
        StartCoroutine(TickClock());
        StartCoroutine(SetPuzzleActivity());
    }

    private IEnumerator TickClock()
    {
        yield return new WaitForEndOfFrame();
        transform.position += new Vector3(0f, Player.cameraResolutionBounds.y / 3f, 0f);

        while (true)
        {
            bigHand.transform.Rotate(0, 0, -bigHandRotationDegreePerSec);
            littleHand.transform.Rotate(0, 0, -bigHandRotationDegreePerSec / 12f);
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator SetPuzzleActivity()
    {
        if (!jigsawPieceDict.ContainsKey(this)) yield break;

        int hour = -1;
        float secsToCompleteHour = (360f / bigHandRotationDegreePerSec);
        //Debug.Log(secsToCompleteHour);

        while(true)
        {
            yield return new WaitForSeconds(secsToCompleteHour);
            hour++;
            if (hour == 12) hour = 0;

            if (hour == puzzleActivityHour)
            {
                jigsawObject.gameObject.SetActive(true);
            }
            else jigsawObject.gameObject.SetActive(false);
        }
    }
}
