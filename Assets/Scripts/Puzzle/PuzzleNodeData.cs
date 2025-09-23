using System.Collections.Generic;

public class PuzzleNodeData
{
    public bool IsVisited { get; private set; }
    public List<PuzzleState> AdjacentStates { get; }
    public PuzzleState? Parent { get; private set; }
    public int Depth { get; set; }

    public PuzzleNodeData()
    {
        IsVisited = false;
        AdjacentStates = new List<PuzzleState>(4); // Ensure capacity
        Parent = null;
        Depth = 0;
    }
    
    public void MarkAsVisited()
    {
        IsVisited = true;
    }

    public void AddAdjacentState(PuzzleState puzzle)
    {
        AdjacentStates.Add(puzzle);
    }
    
    public void SetParent(PuzzleState parent)
    {
        Parent = parent;
    }

    public void IncrementDepth()
    {
        Depth++;
    }
}