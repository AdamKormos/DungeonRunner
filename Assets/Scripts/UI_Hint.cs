using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Hint : MonoBehaviour
{
    static Text hintText = default;

    // Start is called before the first frame update
    void Start()
    {
        hintText = GetComponent<Text>();
        SetHint("");
    }

    public static void SetHint(string hint)
    {
        hintText.text = hint;
    }

    public static IEnumerator SetHint(string hint, float duration)
    {
        hintText.text = hint;
        yield return new WaitForSeconds(duration);
        hintText.text = "";
    }
}
