using System;
using R3;
using UnityEngine;
using VContainer;

public class PuzzleDragger : MonoBehaviour
{
    [SerializeField] private PuzzleGameBlockCreator _blockCreator;
    private Puzzle _puzzle;
    private RectTransform _rectTransform;
    private Subject<Puzzle.MoveDirection?> _subject = new Subject<Puzzle.MoveDirection?>();

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        BlockView.OnBlockEndDrag += OnEndDrag;
        BlockView.OnBlockDragging += OnDrag;
    }

    [Inject]
    private void Initialize(Puzzle puzzle)
    {
        _puzzle = puzzle;
        _subject
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .Where(_ => !PuzzleGameAgent.IsAutoSolving)
            .Subscribe(x => puzzle.TryMoveEmpty(x))
            .AddTo(this);
    }
    
    private void OnDrag(Vector2 position)
    {
        Puzzle.MoveDirection? dragDirection = GetDragDirection(position);
        _subject.OnNext(dragDirection);
    }
    
    private Puzzle.MoveDirection? GetDragDirection(Vector2 position)
    {
        for (int row = 0; row < PuzzleState.RowCount; row++)
        {
            for (int column = 0; column < PuzzleState.ColumnCount; column++)
            {
                BlockPosition blockPos = new BlockPosition(row, column);
                BlockNumber number = _puzzle.State.CurrentValue[blockPos];
                if (number.IsZero())
                {
                    continue;
                }

                Vector2 size = _blockCreator.GetBlockRect(number).sizeDelta;
                Vector2 pos = (Vector2)_rectTransform.position + _blockCreator.GetLocalPosition(blockPos) - size / 2;
                Rect blockRect = new Rect(pos, size);
                
                if (blockRect.Contains(position))
                {
                    BlockPosition emptyBlockPos = _puzzle.EmptyBlockPosition;
                    int rowDiff = blockPos.Row - emptyBlockPos.Row;
                    int colDiff = blockPos.Column - emptyBlockPos.Column;
                    
                    if (rowDiff == 1 && colDiff == 0) return Puzzle.MoveDirection.Down;
                    if (rowDiff == -1 && colDiff == 0) return Puzzle.MoveDirection.Up;
                    if (rowDiff == 0 && colDiff == 1) return Puzzle.MoveDirection.Right;
                    if (rowDiff == 0 && colDiff == -1) return Puzzle.MoveDirection.Left;
                }
            }
        }
        return null;
    }

    private void OnEndDrag()
    {
        BlockPosition emptyPosition = _puzzle.EmptyBlockPosition;
        _blockCreator.GetBlockRect(BlockNumber.Zero()).anchoredPosition = _blockCreator.GetLocalPosition(emptyPosition);
    }

    private void OnDestroy()
    {
        BlockView.OnBlockEndDrag -= OnEndDrag;
        BlockView.OnBlockDragging -= OnDrag;
    }
}
