using System.Collections;
using System.Collections.Generic;
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
}
