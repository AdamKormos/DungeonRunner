using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimationPlayer : MonoBehaviour
{
    SpriteRenderer spriteRenderer = default;
    [SerializeField] Sprite[] animationFrames = default;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(PlayAnim());
    }

    private IEnumerator PlayAnim()
    {
        while(true)
        {
            for(int i = 0; i < animationFrames.Length; i++)
            {
                spriteRenderer.sprite = animationFrames[i];
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
