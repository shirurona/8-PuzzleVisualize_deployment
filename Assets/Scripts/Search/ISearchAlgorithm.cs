using System.Collections.Generic;

public interface ISearchAlgorithm
{
    bool Search(Puzzle initialPuzzle, PuzzleState goalPuzzle);
    Dictionary<PuzzleState, PuzzleNodeData> GetSearchDataMap();
} 