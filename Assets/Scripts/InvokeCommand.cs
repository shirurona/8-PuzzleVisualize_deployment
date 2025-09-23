using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InvokeCommand
{
    Stack<ICommand> _undoStack = new Stack<ICommand>();
    Stack<ICommand> _redoStack = new Stack<ICommand>();

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        
        _redoStack.Clear();
    }

    public void UndoCommand()
    {
        if (!_undoStack.Any()) return;
        
        ICommand command = _undoStack.Pop();
        _redoStack.Push(command);
        command.Undo();
    }

    public void RedoCommand()
    {
        if (!_redoStack.Any()) return;
        
        ICommand command = _redoStack.Pop();
        _undoStack.Push(command);
        command.Execute();
    }

    public HashSet<PuzzleState> GetVisitedRoute()
    {
        HashSet<PuzzleState> visitedRoute = new HashSet<PuzzleState>();
        foreach (var command in _undoStack)
        {
            visitedRoute.Add(command.GetBoardState());
        }
        return visitedRoute;
    }
}
