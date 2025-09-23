using System.Collections.Generic;
using UnityEngine;

public interface IVisualizeStrategy
{
    public Dictionary<PuzzleState, Vector3> VisualizeSearchSpace(Dictionary<PuzzleState, PuzzleNodeData> searchDataMap, PuzzleState initialPuzzleState);
}
