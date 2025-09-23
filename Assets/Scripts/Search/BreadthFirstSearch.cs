using System;
using System.Collections.Generic;
using System.Linq;

public class BreadthFirstSearch : ISearchAlgorithm
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
        Queue<Puzzle> queue = new Queue<Puzzle>();

        PuzzleNodeData initialNodeData = GetOrCreateNodeData(initialPuzzle.State.CurrentValue);
        initialNodeData.MarkAsVisited();
        initialNodeData.Depth = 0;
        
        queue.Enqueue(initialPuzzle);
        
        while (queue.Any())
        {
            Puzzle currentPuzzle = queue.Dequeue();
            PuzzleNodeData currentNodeData = _puzzleDataMap[currentPuzzle.State.CurrentValue];

            /*
            if (currentPuzzle.State.Equals(goalPuzzle))
            {
                UnityEngine.Debug.Log("ゴールに到達しました！ (DFS)");
                return true;
            }
            /*/
            if (currentNodeData.Depth.Equals(16))
            {
                UnityEngine.Debug.Log("ゴールに到達しました！ (DFS)");
                return true;
            }
            //*/

            // BFSでは通常、特定の順序で隣接ノードを処理します（例：右、左、下、上）。
            // この順序を維持します。

            // 右へ移動可能な場合
            var rightPuzzle = currentPuzzle.Clone();
            if (rightPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Right))
            {
                UpdateAndEnqueuePuzzle(rightPuzzle, currentPuzzle.State.CurrentValue, currentNodeData, queue);
            }

            // 左へ移動可能な場合
            var leftPuzzle = currentPuzzle.Clone();
            if (leftPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Left))
            {
                UpdateAndEnqueuePuzzle(leftPuzzle, currentPuzzle.State.CurrentValue, currentNodeData, queue);
            }

            // 下へ移動可能な場合
            var downPuzzle = currentPuzzle.Clone();
            if (downPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Down))
            {
                UpdateAndEnqueuePuzzle(downPuzzle, currentPuzzle.State.CurrentValue, currentNodeData, queue);
            }

            // 上へ移動可能な場合
            var upPuzzle = currentPuzzle.Clone();
            if (upPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Up))
            {
                UpdateAndEnqueuePuzzle(upPuzzle, currentPuzzle.State.CurrentValue, currentNodeData, queue);
            }
        }

        return false;
    }

    private bool IsPuzzleUnvisited(PuzzleState puzzle)
    {
        // このメソッドはDFSと共通なので、将来的に基底クラスなどに移動することも検討可能
        return !_puzzleDataMap.ContainsKey(puzzle) || !_puzzleDataMap[puzzle].IsVisited;
    }

    private void UpdateAndEnqueuePuzzle(Puzzle nextPuzzle, PuzzleState currentPuzzleState, PuzzleNodeData currentNodeData, Queue<Puzzle> queue)
    {
        if (!IsPuzzleUnvisited(nextPuzzle.State.CurrentValue))
            return;
            
        PuzzleNodeData nextNodeData = GetOrCreateNodeData(nextPuzzle.State.CurrentValue);
        nextNodeData.MarkAsVisited(); // 訪問済みにマーク
        queue.Enqueue(nextPuzzle);     // キューに追加
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