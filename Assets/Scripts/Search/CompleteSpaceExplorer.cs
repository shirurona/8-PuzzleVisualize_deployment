using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 8-puzzleの完全な探索空間を収集するクラス
/// 深度制限付きで全ての可能な状態とその隣接関係を構築
/// </summary>
public class CompleteSpaceExplorer : ISearchAlgorithm
{
    private Dictionary<PuzzleState, PuzzleNodeData> _puzzleDataMap = new Dictionary<PuzzleState, PuzzleNodeData>();
    private bool _goalFound = false;

    public bool Search(Puzzle initialPuzzle, PuzzleState goalPuzzle)
    {
        _puzzleDataMap.Clear();
        _goalFound = false;
        
        Queue<Puzzle> queue = new Queue<Puzzle>();
        HashSet<PuzzleState> visited = new HashSet<PuzzleState>();

        // 初期状態の設定
        PuzzleNodeData initialNodeData = GetOrCreateNodeData(initialPuzzle.State.CurrentValue);
        initialNodeData.Depth = 0;
        queue.Enqueue(initialPuzzle);
        visited.Add(initialPuzzle.State.CurrentValue);

        while (queue.Any())
        {
            Puzzle currentPuzzle = queue.Dequeue();
            PuzzleNodeData currentNodeData = _puzzleDataMap[currentPuzzle.State.CurrentValue];

            // ゴール発見の記録（探索は継続）
            if (currentPuzzle.State.Equals(goalPuzzle))
            {
                _goalFound = true;
                Debug.Log($"ゴール発見: 深度={currentNodeData.Depth}");
            }

            // 全ての可能な移動を試行
            var adjacentPuzzles = GenerateAllAdjacentPuzzles(currentPuzzle);
            
            foreach (var nextPuzzle in adjacentPuzzles)
            {
                // 隣接関係を双方向で構築（常に実行）
                BuildBidirectionalAdjacency(currentPuzzle.State.CurrentValue, nextPuzzle.State.CurrentValue, currentNodeData);
                
                // 未訪問の場合のみキューに追加
                if (!visited.Contains(nextPuzzle.State.CurrentValue))
                {
                    PuzzleNodeData nextNodeData = GetOrCreateNodeData(nextPuzzle.State.CurrentValue);
                    nextNodeData.Depth = currentNodeData.Depth + 1;
                    
                    // 親設定（BFS最短経路を保持）
                    nextNodeData.SetParent(currentPuzzle.State.CurrentValue);
                    
                    queue.Enqueue(nextPuzzle);
                    visited.Add(nextPuzzle.State.CurrentValue);
                }
            }
        }

        Debug.Log($"完全探索完了: 発見状態数={_puzzleDataMap.Count}, ゴール発見={_goalFound}");
        
        return _goalFound;
    }

    /// <summary>
    /// 指定パズルから可能な全ての隣接パズルを生成
    /// </summary>
    private List<Puzzle> GenerateAllAdjacentPuzzles(Puzzle puzzle)
    {
        var adjacentPuzzles = new List<Puzzle>();
        BlockPosition emptyPos = puzzle.EmptyBlockPosition;
        
        // 右移動
        var rightPuzzle = puzzle.Clone();
        if (rightPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Right))
        {
            adjacentPuzzles.Add(rightPuzzle);
        }

        // 左移動
        var leftPuzzle = puzzle.Clone();
        if (leftPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Left))
        {
            adjacentPuzzles.Add(leftPuzzle);
        }

        // 下移動
        var downPuzzle = puzzle.Clone();
        if (downPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Down))
        {
            adjacentPuzzles.Add(downPuzzle);
        }

        // 上移動
        var upPuzzle = puzzle.Clone();
        if (upPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Up))
        {
            adjacentPuzzles.Add(upPuzzle);
        }

        return adjacentPuzzles;
    }

    /// <summary>
    /// 双方向の隣接関係を構築
    /// </summary>
    private void BuildBidirectionalAdjacency(PuzzleState puzzleStateA, PuzzleState puzzleStateB, PuzzleNodeData nodeDataA)
    {
        PuzzleNodeData nodeDataB = GetOrCreateNodeData(puzzleStateB);

        // A → B の隣接関係
        if (!nodeDataA.AdjacentStates.Contains(puzzleStateB))
        {
            nodeDataA.AddAdjacentState(puzzleStateB);
        }

        // B → A の隣接関係
        if (!nodeDataB.AdjacentStates.Contains(puzzleStateA))
        {
            nodeDataB.AddAdjacentState(puzzleStateA);
        }
    }

    /// <summary>
    /// ノードデータの取得または作成
    /// </summary>
    private PuzzleNodeData GetOrCreateNodeData(PuzzleState puzzleState)
    {
        if (!_puzzleDataMap.ContainsKey(puzzleState))
        {
            _puzzleDataMap[puzzleState] = new PuzzleNodeData();
        }
        return _puzzleDataMap[puzzleState];
    }

    /// <summary>
    /// 探索結果のデータマップを取得
    /// </summary>
    public Dictionary<PuzzleState, PuzzleNodeData> GetSearchDataMap()
    {
        return _puzzleDataMap;
    }
}