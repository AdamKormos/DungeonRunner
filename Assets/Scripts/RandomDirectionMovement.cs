using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomDirectionMovement : MonoBehaviour
{
    [SerializeField] bool rotates = false;
    Vector3 direction = default;

    private void Start()
    {
        if(rotates)
        {
            direction = -transform.right;
        }
        StartCoroutine(PickDirection());
    }

    private IEnumerator PickDirection()
    {
        while(true)
        {
            if(rotates) transform.Rotate(0f, 0f, Random.Range(0f, 360f));
            else direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            yield return new WaitForSeconds(Random.Range(10f, 20f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(rotates)
        {
            direction = -transform.right;  // Has to be updated continuously as transform.right changes every frame.
        }

        transform.position += direction * Time.deltaTime;
    }
}
