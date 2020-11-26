using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JigsawPosition { BottomLeft, BottomRight, TopLeft, TopRight };

public class JigsawPiece : MonoBehaviour
{
    [SerializeField] public JigsawPosition jigsawPosition;
}
