using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterTile : MonoBehaviour
{
    static int letterSelectionPerTile = 5;
    public char[] letterSelection { get; private set; }
    int currentLetterIndex = 0;
    TextMesh textMesh = default;
    
    // Start is called before the first frame update
    public void Init()
    {
        textMesh = GetComponent<TextMesh>();
        GenerateLetterSelectionAndAssignCurrent();
    }

    /// <summary>
    /// Creates a random set of letters that can be picked on this tile and also picks one to be initially displayed.
    /// </summary>
    private void GenerateLetterSelectionAndAssignCurrent()
    {
        letterSelection = new char[letterSelectionPerTile];
        for(int i = 0; i < letterSelection.Length; i++)
        {
            letterSelection[i] = (char)Random.Range(65, 91);
        }

        OnLetterChanged(Random.Range(0, letterSelectionPerTile));
    }

    /// <summary>
    /// Called when this tile's letter is changed, either by the player (by using the arrows) or inside code.
    /// </summary>
    /// <param name="amountItChangedBy"></param>
    private void OnLetterChanged(int amountItChangedBy)
    {
        currentLetterIndex += amountItChangedBy;
        if (currentLetterIndex < 0) currentLetterIndex = letterSelectionPerTile - 1;
        else if (currentLetterIndex >= letterSelectionPerTile) currentLetterIndex = 0;

        textMesh.text = letterSelection[currentLetterIndex].ToString();
    }

    /// <summary>
    /// Called when the player enters one of the tile's arrows.
    /// </summary>
    /// <param name="isUpArrow"></param>
    public void OnPlayerEnteredOnArrow(bool isUpArrow)
    {
        if(isUpArrow)
        {
            OnLetterChanged(1);
        }
        else
        {
            OnLetterChanged(-1);
        }
    }

    /// <summary>
    /// Returns the character that is displayed by this tile.
    /// </summary>
    /// <returns></returns>
    public char GetCurrentlyDisplayedChar()
    {
        return letterSelection[currentLetterIndex];
    }
}
