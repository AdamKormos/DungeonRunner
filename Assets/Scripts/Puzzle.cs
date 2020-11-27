using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PuzzleComponent
{
    [SerializeField] public GameObject objectToSpawn;
    [SerializeField] int minimumDistanceFromPuzzle, maximumDistanceFromPuzzle;
    [SerializeField] int minimumDistanceFromComponents, maximumDistanceFromComponents;
    Bound2D spawnBounds;



    public Tuple<int, int> GetPotentialRoomOffsetFromMain()
    {
        return new Tuple<int, int>(0, 0);
    }
}

public class Puzzle : MonoBehaviour
{
    [SerializeField] protected PuzzleComponent[] components = default;
    public static Dictionary<Puzzle, JigsawPosition> jigsawPieceDict = new Dictionary<Puzzle, JigsawPosition>();

    /// <summary>
    /// Called after the puzzle was assigned to the grid.
    /// </summary>
    public void SpawnPuzzleComponents()
    {
        for(int i = 0; i < components.Length; i++)
        {
            GameObject puzzleComponent = Instantiate(components[i].objectToSpawn, transform.position, Quaternion.identity, this.transform);
            puzzleComponent.transform.position = MapManager.AssignPuzzleToGrid();
            components[i].objectToSpawn = puzzleComponent; // So that we can refer to the instantiated, in-game version of the object and modify it.

            //puzzleComponent.transform.position = mainComponentGO.transform.position + 
        }
    }

    protected virtual void OnAnswerSubmitted() { }
    protected virtual void OnCorrectAnswer() { }
}
