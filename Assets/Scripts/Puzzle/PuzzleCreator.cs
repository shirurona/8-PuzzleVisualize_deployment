using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class PuzzleCreator : MonoBehaviour
{
    [SerializeField] private PuzzleVisualizer puzzleVisualizer;
    private Dictionary<PuzzleState, Vector3> _puzzleViewMap;
    private Dictionary<PuzzleState, PuzzleNodeData> _searchDataMap;
    private Puzzle _puzzle;

    [Inject]
    public void Initialize(Puzzle puzzle)
    {
        int[,] goalNumbers = new [,]
        {
            /*
            {8, 6, 7},
            {2, 5, 4},
            {3, 0, 1}
            */
            {5, 4, 3},
            {1, 0, 7},
            {2, 8, 6}
        };
        var goalPuzzle = PuzzleState.Create(goalNumbers);

        var searchAlgorithm = new BreadthFirstSearch();
        _searchDataMap = VisualizeSearchSpace(searchAlgorithm, puzzle, goalPuzzle);
        IVisualizeStrategy visualizeStrategy = GetComponent<JobSystemOptimizedFruchtermanReingoldVisualizer>();
        _puzzleViewMap = visualizeStrategy.VisualizeSearchSpace(_searchDataMap, puzzle.State.CurrentValue);
        Debug.Log(_puzzleViewMap.Count);
        puzzleVisualizer.Initialize();
        puzzleVisualizer.AddAdjacencyEdges(_searchDataMap, _puzzleViewMap, new HashSet<PuzzleState>(0));
        puzzleVisualizer.AddPuzzleInstances(_puzzleViewMap);
        puzzleVisualizer.ApplyAllMatrixData();
    }

    public void ShowRoutes()
    {
        puzzleVisualizer.ClearEdgeMatrices();
        Debug.Log(_puzzleViewMap.Count);
        HashSet<PuzzleState> routes = _puzzle.GetVisitedRoute();
        Debug.Log(routes.Count);
        foreach (var v in routes)
        {
            Debug.Log(v.ToString());
        }
        puzzleVisualizer.AddAdjacencyEdges(_searchDataMap, _puzzleViewMap, routes); // 重要。ここだけが変わる
        puzzleVisualizer.ApplyEdgeMatrixData();
    }
    
    private Dictionary<PuzzleState, PuzzleNodeData> VisualizeSearchSpace(ISearchAlgorithm searchAlgorithm, Puzzle initialPuzzle, PuzzleState goalPuzzle)
    {
        bool searchSuccess = searchAlgorithm.Search(initialPuzzle, goalPuzzle);
        Dictionary<PuzzleState, PuzzleNodeData> searchDataMap = searchAlgorithm.GetSearchDataMap();

        if (searchSuccess)
        {
            int visitedCount = searchDataMap.Count(kvp => kvp.Value.IsVisited);
            Debug.Log($"探索完了。訪問状態数: {visitedCount}");
        }
        else
        {
            Debug.Log("ゴールに到達できませんでした");
        }

        return searchDataMap;
    }
    
    public Vector3 GetBoardStatePosition(PuzzleState puzzleState)
    {
        if (_puzzleViewMap.TryGetValue(puzzleState, out Vector3 puzzlePosition))
        {
            return puzzlePosition;
        }
        Debug.LogWarning("Puzzle not found");
        return Vector3.zero;
    }
}
