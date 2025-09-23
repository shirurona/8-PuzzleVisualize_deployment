using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

public class Puzzle
{
    public enum MoveDirection
    {
        Up,
        Down,
        Right,
        Left
    }
    
    public static readonly Vector2Int[] DirectionVectors = new Vector2Int[4]
    {
        Vector2Int.down,
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.left
    };
    
    public static Vector2Int GetDirectionVector(MoveDirection direction)
    {
        return DirectionVectors[(int)direction];
    }
    
    private ReactiveProperty<PuzzleState> _state;
    public ReadOnlyReactiveProperty<PuzzleState> State => _state;
    
    private InvokeCommand _invokeCommand = new InvokeCommand();
    
    public Puzzle(PuzzleState puzzleState)
    {
        _state = new ReactiveProperty<PuzzleState>(puzzleState);
    }
    
    public BlockPosition EmptyBlockPosition => _state.CurrentValue.EmptyBlockPosition;
    
    public BlockNumber this[BlockPosition position] => _state.CurrentValue[position];

    public void SetPuzzle(PuzzleState puzzleState)
    {
        _state.Value = puzzleState;
    }

    public bool TryMoveEmpty(MoveDirection direction)
    {
        return TryMoveEmpty(GetDirectionVector(direction));
    }
    
    public bool TryMoveEmpty(Vector2Int direction)
    {
        var targetPos = new BlockPosition(EmptyBlockPosition.Row + direction.y, EmptyBlockPosition.Column + direction.x);
        if (!_state.CurrentValue.CanSwap(targetPos)) return false;
        
        var command = new MoveCommand(_state.CurrentValue, direction, TryMoveEmptyDirect); 
        _state.Value = command.GetBoardState();
        ExecuteCommand(command);
        return true;
    }
    
    private void TryMoveEmptyDirect(Vector2Int direction)
    {
        var targetPos = new BlockPosition(EmptyBlockPosition.Row + direction.y, EmptyBlockPosition.Column + direction.x);
        _state.Value = _state.CurrentValue.Swap(targetPos);
    }
    
    public Puzzle Clone()
    {
        return new Puzzle(_state.CurrentValue);
    }
    
    public void ExecuteCommand(ICommand command)
    {
        _invokeCommand.ExecuteCommand(command);
    }
    
    public void UndoCommand()
    {
        _invokeCommand.UndoCommand();
    }
    
    public void RedoCommand()
    {
        _invokeCommand.RedoCommand();
    }
    
    public HashSet<PuzzleState> GetVisitedRoute()
    {
        var visited = new HashSet<PuzzleState>(_invokeCommand.GetVisitedRoute());
        visited.Add(State.CurrentValue);
        return visited;
    }
}
