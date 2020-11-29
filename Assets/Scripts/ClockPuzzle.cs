using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockPuzzle : Puzzle
{
    [SerializeField] float littleHandRotationDegreePerSec = 10f;
    [SerializeField] Sprite littleHandSprite = default, bigHandSprite = default;
    GameObject littleHand = default, bigHand = default;
    JigsawPiece jigsawObject = default;
    int puzzleActivityHour;

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
        }
        else Destroy(components[0].objectToSpawn.gameObject);

        {
            littleHand = Instantiate(new GameObject(), this.transform);
            littleHand.transform.position += new Vector3(0f, 0f, -1.5f);
            littleHand.name = "LittleHand";
            SpriteRenderer spriteRenderer = littleHand.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = littleHandSprite;
        }

        {
            bigHand = Instantiate(new GameObject(), this.transform);
            bigHand.transform.position += new Vector3(0f, 0f, -1.5f);
            bigHand.name = "BigHand";
            SpriteRenderer spriteRenderer = bigHand.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = bigHandSprite;
        }

        puzzleActivityHour = Random.Range(0, 12);
        StartCoroutine(TickClock());
        StartCoroutine(SetPuzzleActivity());
    }

    private IEnumerator TickClock()
    {
        while(true)
        {
            littleHand.transform.Rotate(0, 0, -littleHandRotationDegreePerSec);
            bigHand.transform.Rotate(0, 0, -littleHandRotationDegreePerSec / 12f);
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator SetPuzzleActivity()
    {
        if (!jigsawPieceDict.ContainsKey(this)) yield break;

        int hour = -1;
        float secsToCompleteHour = (360f / littleHandRotationDegreePerSec);
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
