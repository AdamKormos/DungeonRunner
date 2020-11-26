using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
struct PuzzleComponent
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
    [SerializeField] PuzzleComponent mainComponent = default;
    [SerializeField] PuzzleComponent[] otherComponents = default;
    public static Dictionary<Puzzle, JigsawPosition> jigsawPieceDict = new Dictionary<Puzzle, JigsawPosition>();

    public void SpawnPuzzleComponents()
    {
        GameObject mainComponentGO = Instantiate(mainComponent.objectToSpawn, transform.position, Quaternion.identity, this.transform);
        mainComponentGO.transform.position = transform.position;

        foreach(PuzzleComponent component in otherComponents)
        {
            GameObject puzzleComponent = Instantiate(component.objectToSpawn, transform.position, Quaternion.identity, this.transform);
            puzzleComponent.transform.position = MapManager.AssignPuzzleToGrid();
            
            //puzzleComponent.transform.position = mainComponentGO.transform.position + 
        }
    }
}
