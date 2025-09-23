using System;
using System.Collections.Generic;
using System.Linq;

public class DepthFirstSearch : ISearchAlgorithm
{
    private Dictionary<PuzzleState, PuzzleNodeData> _puzzleDataMap = new Dictionary<PuzzleState, PuzzleNodeData>();

    private PuzzleNodeData GetOrCreateNodeData(PuzzleState puzzle)
    {
        if (!_puzzleDataMap.ContainsKey(puzzle))
        {
            _puzzleDataMap[puzzle] = new PuzzleNodeData();
        }
        return _puzzleDataMap[puzzle];
    }

    public bool Search(Puzzle initialPuzzle, PuzzleState goalPuzzle)
    {
        _puzzleDataMap.Clear();
        Stack<Puzzle> stack = new Stack<Puzzle>();

        PuzzleNodeData initialNodeData = GetOrCreateNodeData(initialPuzzle.State.CurrentValue);
        initialNodeData.MarkAsVisited();
        initialNodeData.Depth = 0;

        stack.Push(initialPuzzle);

        while (stack.Any())
        {
            Puzzle currentPuzzle = stack.Pop();
            PuzzleNodeData currentNodeData = _puzzleDataMap[currentPuzzle.State.CurrentValue];

            if (currentPuzzle.State.Equals(goalPuzzle))
            {
                UnityEngine.Debug.Log("ゴールに到達しました！ (DFS)");
                return true;
            }

            // 上へ移動可能な場合
            var upPuzzle = currentPuzzle.Clone();
            if (upPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Up))
            {
                UpdateAndStackPuzzle(upPuzzle, currentPuzzle.State.CurrentValue, currentNodeData, stack);
            }

            // 下へ移動可能な場合
            var downPuzzle = currentPuzzle.Clone();
            if (downPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Down))
            {
                UpdateAndStackPuzzle(downPuzzle, currentPuzzle.State.CurrentValue, currentNodeData, stack);
            }

            // 左へ移動可能な場合
            var leftPuzzle = currentPuzzle.Clone();
            if (leftPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Left))
            {
                UpdateAndStackPuzzle(leftPuzzle, currentPuzzle.State.CurrentValue, currentNodeData, stack);
            }

            // 右へ移動可能な場合
            var rightPuzzle = currentPuzzle.Clone();
            if (rightPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Right))
            {
                UpdateAndStackPuzzle(rightPuzzle, currentPuzzle.State.CurrentValue, currentNodeData, stack);
            }
        }
        return false;
    }

    private bool IsPuzzleUnvisited(PuzzleState puzzle)
    {
        return !_puzzleDataMap.ContainsKey(puzzle) || !_puzzleDataMap[puzzle].IsVisited;
    }

    private void UpdateAndStackPuzzle(Puzzle nextPuzzle, PuzzleState currentPuzzleState, PuzzleNodeData currentNodeData, Stack<Puzzle> stack)
    {
        if (!IsPuzzleUnvisited(nextPuzzle.State.CurrentValue))
            return;
            
        PuzzleNodeData nextNodeData = GetOrCreateNodeData(nextPuzzle.State.CurrentValue);
        nextNodeData.MarkAsVisited(); // 訪問済みにマーク
        stack.Push(nextPuzzle); // スタックに追加
        currentNodeData.AddAdjacentState(nextPuzzle.State.CurrentValue); // 隣接リストに追加
        nextNodeData.SetParent(currentPuzzleState); // 親を設定
        nextNodeData.Depth = currentNodeData.Depth; // 深さを設定
        nextNodeData.IncrementDepth(); 
    }

    public Dictionary<PuzzleState, PuzzleNodeData> GetSearchDataMap()
    {
        return _puzzleDataMap;
    }
}

