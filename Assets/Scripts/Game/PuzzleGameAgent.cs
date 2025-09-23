using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;

public class PuzzleGameAgent : MonoBehaviour
{
    // インスペクタ設定
    [SerializeField] private float executionInterval = 1.0f;
    [SerializeField] private PuzzleFinderView puzzleFinderView;
    [SerializeField] private TMP_Dropdown dropdown;
    
    private Puzzle _puzzle;
    // 状態管理
    private Queue<Puzzle.MoveDirection> moveQueue = new Queue<Puzzle.MoveDirection>();
    public static bool IsAutoSolving { get; private set; } = false;

    [Inject]
    public void Initialize(Puzzle puzzle)
    {
        _puzzle = puzzle;
    }
    
    private Puzzle.MoveDirection GetMoveDirection(PuzzleState from, PuzzleState to)
    {
        BlockPosition emptyPosFrom = from.EmptyBlockPosition;
        BlockPosition emptyPosTo = to.EmptyBlockPosition;
        
        int rowDiff = emptyPosTo.Row - emptyPosFrom.Row;
        int colDiff = emptyPosTo.Column - emptyPosFrom.Column;
        
        // 隣接する位置へのみ移動可能
        if (rowDiff == -1 && colDiff == 0) return Puzzle.MoveDirection.Up;
        if (rowDiff == 1 && colDiff == 0) return Puzzle.MoveDirection.Down;
        if (rowDiff == 0 && colDiff == -1) return Puzzle.MoveDirection.Left;
        if (rowDiff == 0 && colDiff == 1) return Puzzle.MoveDirection.Right;
        
        throw new InvalidOperationException("無効な状態変化です");
    }

    private List<PuzzleState> ReconstructPath(PuzzleState goalState, Dictionary<PuzzleState, PuzzleNodeData> dataMap)
    {
        List<PuzzleState> path = new List<PuzzleState>();
        PuzzleState? current = goalState;
        
        while (current.HasValue)
        {
            path.Add(current.Value);
            current = dataMap[current.Value].Parent;
        }
        
        path.Reverse(); // 初期状態→ゴール状態の順序に変更
        return path;
    }

    private List<Puzzle.MoveDirection> CalculateMovePath(PuzzleState goalState, ISearchAlgorithm algorithm)
    {
        // アルゴリズム実行
        bool found = algorithm.Search(_puzzle, goalState);
        if (!found) return new List<Puzzle.MoveDirection>();
        
        // 経路復元
        var dataMap = algorithm.GetSearchDataMap();
        var statePath = ReconstructPath(goalState, dataMap);
        
        // 移動方向に変換
        return ConvertToMoveDirections(statePath);
    }

    private List<Puzzle.MoveDirection> ConvertToMoveDirections(List<PuzzleState> statePath)
    {
        var moveDirections = new List<Puzzle.MoveDirection>();
        
        for (int i = 0; i < statePath.Count - 1; i++)
        {
            var from = statePath[i];
            var to = statePath[i + 1];
            var direction = GetMoveDirection(from, to);
            moveDirections.Add(direction);
        }
        
        return moveDirections;
    }

    public void StartAutoSolveWithCurrentState()
    {
        ISearchAlgorithm algorithm = null;
        switch (dropdown.value)
        {
            case 0:
                algorithm = new BreadthFirstSearch();
                break;
            case 1:
                algorithm = new DepthFirstSearch();
                break;
        }
        // PuzzleGameの現在のパズル状態を取得
        var goalState = puzzleFinderView.GetCurrentState();
        StartAutoSolve(algorithm, goalState);
    }

    private void StartAutoSolve(ISearchAlgorithm algorithm, PuzzleState goalState)
    {
        if (algorithm == null)
            throw new ArgumentNullException(nameof(algorithm));
            
        if (IsAutoSolving)
            return; // 既に実行中の場合は何もしない
            
        // 経路を計算してキューに格納
        var moveDirections = CalculateMovePath(goalState, algorithm);
        
        moveQueue.Clear();
        foreach (var direction in moveDirections)
        {
            moveQueue.Enqueue(direction);
        }
        
        // 非同期実行を開始
        ExecuteMoveSequenceAsync().Forget();
    }

    public void StopAutoSolve()
    {
        IsAutoSolving = false;
        moveQueue.Clear();
    }

    public async UniTaskVoid ExecuteMoveSequenceAsync()
    {
        IsAutoSolving = true;
        
        // キューが空になるまで、または停止されるまで処理
        while (moveQueue.Count > 0 && IsAutoSolving)
        {
            var direction = moveQueue.Dequeue();
            _puzzle.TryMoveEmpty(direction);
            
            // 指定間隔で待機
            await UniTask.Delay(TimeSpan.FromSeconds(executionInterval));
        }
        
        // 完了時にisAutoSolvingをfalseに設定
        IsAutoSolving = false;
    }
}