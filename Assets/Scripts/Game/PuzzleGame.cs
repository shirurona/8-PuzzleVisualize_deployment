using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class PuzzleGame : MonoBehaviour
{
    private Puzzle _puzzle;
    [SerializeField] private Toggle reverseToggle;
    [SerializeField] private Button undoButton;
    [SerializeField] private Button redoButton;

    private void Awake()
    {
        undoButton.onClick.AddListener(Undo);
        redoButton.onClick.AddListener(Redo);
        
        BlockView.OnBlockClicked += HandlePanelClick;
    }

    [Inject]
    public void Initialize(Puzzle puzzle)
    {
        _puzzle = puzzle;
    }
    
    private void OnDestroy()
    {
        BlockView.OnBlockClicked -= HandlePanelClick;
    }
    
    private void HandlePanelClick(BlockNumber clickedNumber)
    {
        // 自動実行中は手動操作を無効化
        if (PuzzleGameAgent.IsAutoSolving) return;
        
        if (clickedNumber.IsZero()) return;

        BlockPosition clickedPanelPos = _puzzle.State.CurrentValue.FindNumberBlockPosition(clickedNumber);
        BlockPosition emptyPos = _puzzle.EmptyBlockPosition;

        // 斜めの移動は許可しない
        int rowDiff = Math.Abs(clickedPanelPos.Row - emptyPos.Row);
        int colDiff = Math.Abs(clickedPanelPos.Column - emptyPos.Column);
        if (rowDiff != 0 && colDiff != 0) return;

        // 縦方向の移動
        if (rowDiff > 0)
        {
            Puzzle.MoveDirection direction = clickedPanelPos.Row > emptyPos.Row ? Puzzle.MoveDirection.Down : Puzzle.MoveDirection.Up;
            ExecuteMultipleMoves(direction, rowDiff);
        }
        // 横方向の移動
        else if (colDiff > 0)
        {
            Puzzle.MoveDirection direction = clickedPanelPos.Column > emptyPos.Column ? Puzzle.MoveDirection.Right : Puzzle.MoveDirection.Left;
            ExecuteMultipleMoves(direction, colDiff);
        }
    }

    // フレーム毎の更新処理
    void Update()
    {
        // 自動実行中は手動操作を無効化
        if (PuzzleGameAgent.IsAutoSolving) return;
        
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            _puzzle.TryMoveEmpty(reverseToggle.isOn ? Puzzle.MoveDirection.Up : Puzzle.MoveDirection.Down);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            _puzzle.TryMoveEmpty(reverseToggle.isOn ? Puzzle.MoveDirection.Down : Puzzle.MoveDirection.Up);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _puzzle.TryMoveEmpty(reverseToggle.isOn ? Puzzle.MoveDirection.Left : Puzzle.MoveDirection.Right);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            _puzzle.TryMoveEmpty(reverseToggle.isOn ? Puzzle.MoveDirection.Right : Puzzle.MoveDirection.Left);
        }
    }
    
    private void ExecuteMultipleMoves(Puzzle.MoveDirection direction, int count)
    {
        for (int i = 0; i < count; i++)
        {
            _puzzle.TryMoveEmpty(direction);
        }
    }
    
    public void Undo()
    {
        // 自動実行中は手動操作を無効化
        if (PuzzleGameAgent.IsAutoSolving) return;
        
        _puzzle.UndoCommand();
    }
    
    public void Redo()
    {
        // 自動実行中は手動操作を無効化
        if (PuzzleGameAgent.IsAutoSolving) return;
        
        _puzzle.RedoCommand();
    }
}
