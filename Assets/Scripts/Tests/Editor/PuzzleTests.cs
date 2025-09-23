using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools;

public class PuzzleTests
{
    private PuzzleState CreateValidPuzzleState()
    {
        return PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 0}
        });
    }

    // Test for valid constructor
    [Test]
    public void Puzzle_ValidConstructor_CreatesPuzzle()
    {
        var puzzleState = CreateValidPuzzleState();
        var puzzle = new Puzzle(puzzleState);
        Assert.IsNotNull(puzzle);
        Assert.AreEqual(puzzleState, puzzle.State.CurrentValue);
    }

    // Test for indexer (get)
    [Test]
    public void Puzzle_IndexerGet_ReturnsCorrectBlockNumber()
    {
        var puzzle = new Puzzle(CreateValidPuzzleState());
        Assert.AreEqual(new BlockNumber(1), puzzle[new BlockPosition(0, 0)]);
        Assert.AreEqual(new BlockNumber(5), puzzle[new BlockPosition(1, 1)]);
        Assert.AreEqual(new BlockNumber(0), puzzle[new BlockPosition(2, 2)]);
    }

    // Test for indexer with out of range position (get) - このテストはPuzzleStateで行うべき
    [Test]
    public void Puzzle_IndexerGet_OutOfRangePosition_ThrowsArgumentOutOfRangeException()
    {
        var puzzle = new Puzzle(CreateValidPuzzleState());
        Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = puzzle[new BlockPosition(-1, 0)]; });
        Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = puzzle[new BlockPosition(3, 0)]; });
    }

    // Test for FindEmptyBlockPosition
    [Test]
    public void Puzzle_FindEmptyBlockPosition_ReturnsCorrectPosition()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        Assert.AreEqual(new BlockPosition(1, 1), puzzle.EmptyBlockPosition);
    }

    // Test for TryMoveEmpty Left
    [Test]
    public void Puzzle_TryMoveEmptyLeft_SwapsBlocksCorrectly()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        bool result = puzzle.TryMoveEmpty(Puzzle.MoveDirection.Left);
        Assert.IsTrue(result);
        Assert.AreEqual(new BlockNumber(0), puzzle[new BlockPosition(1, 0)]);
        Assert.AreEqual(new BlockNumber(4), puzzle[new BlockPosition(1, 1)]);
    }

    // Test for TryMoveEmpty Right
    [Test]
    public void Puzzle_TryMoveEmptyRight_SwapsBlocksCorrectly()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        bool result = puzzle.TryMoveEmpty(Puzzle.MoveDirection.Right);
        Assert.IsTrue(result);
        Assert.AreEqual(new BlockNumber(0), puzzle[new BlockPosition(1, 2)]);
        Assert.AreEqual(new BlockNumber(6), puzzle[new BlockPosition(1, 1)]);
    }

    // Test for TryMoveEmpty Up
    [Test]
    public void Puzzle_TryMoveEmptyUp_SwapsBlocksCorrectly()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        bool result = puzzle.TryMoveEmpty(Puzzle.MoveDirection.Up);
        Assert.IsTrue(result);
        Assert.AreEqual(new BlockNumber(0), puzzle[new BlockPosition(0, 1)]);
        Assert.AreEqual(new BlockNumber(2), puzzle[new BlockPosition(1, 1)]);
    }

    // Test for TryMoveEmpty Down
    [Test]
    public void Puzzle_TryMoveEmptyDown_SwapsBlocksCorrectly()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        bool result = puzzle.TryMoveEmpty(Puzzle.MoveDirection.Down);
        Assert.IsTrue(result);
        Assert.AreEqual(new BlockNumber(0), puzzle[new BlockPosition(2, 1)]);
        Assert.AreEqual(new BlockNumber(8), puzzle[new BlockPosition(1, 1)]);
    }

    // Test for Clone
    [Test]
    public void Puzzle_Clone_CreatesDeepCopy()
    {
        var originalPuzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } }));
        var clonedPuzzle = originalPuzzle.Clone();

        Assert.IsNotNull(clonedPuzzle);
        Assert.AreNotSame(originalPuzzle, clonedPuzzle); // Ensure it's a new instance
        Assert.AreEqual(originalPuzzle.State, clonedPuzzle.State); // Ensure content is the same

        // Modify cloned puzzle to ensure deep copy
        clonedPuzzle.TryMoveEmpty(Puzzle.MoveDirection.Left);
        Assert.AreNotEqual(originalPuzzle.State, clonedPuzzle.State);
    }

    // Test for EmptyBlockPosition after swap operations
    [Test]
    public void Puzzle_EmptyBlockPosition_UpdatesAfterSwap()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        Assert.AreEqual(new BlockPosition(1, 1), puzzle.EmptyBlockPosition);
        
        puzzle.TryMoveEmpty(Puzzle.MoveDirection.Left);
        Assert.AreEqual(new BlockPosition(1, 0), puzzle.EmptyBlockPosition);
        
        puzzle.TryMoveEmpty(Puzzle.MoveDirection.Right); // Back to (1,1)
        Assert.AreEqual(new BlockPosition(1, 1), puzzle.EmptyBlockPosition);
        
        puzzle.TryMoveEmpty(Puzzle.MoveDirection.Up);
        Debug.Log(puzzle.EmptyBlockPosition.Row +" : "+puzzle.EmptyBlockPosition.Column);
        Assert.AreEqual(new BlockPosition(0, 1), puzzle.EmptyBlockPosition);
        
        puzzle.TryMoveEmpty(Puzzle.MoveDirection.Down); // Back to (1,1)
        Assert.AreEqual(new BlockPosition(1, 1), puzzle.EmptyBlockPosition);
    }


    // Test for TryMoveEmpty Left when at left edge
    [Test]
    public void Puzzle_TryMoveEmptyLeft_DoesNotMoveWhenAtLeftEdge()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 } })); // 0 is at (0,0)
        var originalState = puzzle.State.CurrentValue;
        bool result = puzzle.TryMoveEmpty(Puzzle.MoveDirection.Left);
        Assert.IsFalse(result);
        Assert.AreEqual(originalState, puzzle.State.CurrentValue);
    }

    // Test for TryMoveEmpty Right when at right edge
    [Test]
    public void Puzzle_TryMoveEmptyRight_DoesNotMoveWhenAtRightEdge()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 0 }, { 3, 4, 5 }, { 6, 7, 8 } })); // 0 is at (0,2)
        var originalState = puzzle.State.CurrentValue;
        bool result = puzzle.TryMoveEmpty(Puzzle.MoveDirection.Right);
        Assert.IsFalse(result);
        Assert.AreEqual(originalState, puzzle.State.CurrentValue);
    }

    // Test for TryMoveEmpty Up when at top edge
    [Test]
    public void Puzzle_TryMoveEmptyUp_DoesNotMoveWhenAtTopEdge()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 0, 2 }, { 3, 4, 5 }, { 6, 7, 8 } })); // 0 is at (0,1)
        var originalState = puzzle.State.CurrentValue;
        bool result = puzzle.TryMoveEmpty(Puzzle.MoveDirection.Up);
        Assert.IsFalse(result);
        Assert.AreEqual(originalState, puzzle.State.CurrentValue);
    }

    // Test for TryMoveEmpty Down when at bottom edge
    [Test]
    public void Puzzle_TryMoveEmptyDown_DoesNotMoveWhenAtBottomEdge()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 0, 8 } })); // 0 is at (2,1)
        var originalState = puzzle.State.CurrentValue;
        bool result = puzzle.TryMoveEmpty(Puzzle.MoveDirection.Down);
        Assert.IsFalse(result);
        Assert.AreEqual(originalState, puzzle.State.CurrentValue);
    }

    // Test for TryMoveEmpty with Vector2Int parameter
    [Test]
    public void Puzzle_TryMoveEmptyVector2Int_WorksCorrectly()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        
        // Test left movement using Vector2Int
        bool result = puzzle.TryMoveEmpty(Vector2Int.left);
        Assert.IsTrue(result);
        Assert.AreEqual(new BlockNumber(0), puzzle[new BlockPosition(1, 0)]);
        
        // Test right movement using Vector2Int
        puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        result = puzzle.TryMoveEmpty(Vector2Int.right);
        Assert.IsTrue(result);
        Assert.AreEqual(new BlockNumber(0), puzzle[new BlockPosition(1, 2)]);
    }

    // Test for GetDirectionVector static method
    [Test]
    public void Puzzle_GetDirectionVector_ReturnsCorrectVector()
    {
        Assert.AreEqual(Vector2Int.down, Puzzle.GetDirectionVector(Puzzle.MoveDirection.Up));
        Assert.AreEqual(Vector2Int.up, Puzzle.GetDirectionVector(Puzzle.MoveDirection.Down));
        Assert.AreEqual(Vector2Int.right, Puzzle.GetDirectionVector(Puzzle.MoveDirection.Right));
        Assert.AreEqual(Vector2Int.left, Puzzle.GetDirectionVector(Puzzle.MoveDirection.Left));
    }

    // Test for UndoCommand functionality
    [Test]
    public void Puzzle_UndoCommand_RestoresPreviousState()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        var originalState = puzzle.State.CurrentValue;
        
        puzzle.TryMoveEmpty(Puzzle.MoveDirection.Left);
        Assert.AreNotEqual(originalState, puzzle.State.CurrentValue);
        
        puzzle.UndoCommand();
        Assert.AreEqual(originalState, puzzle.State.CurrentValue);
    }

    // Test for RedoCommand functionality
    [Test]
    public void Puzzle_RedoCommand_ReappliesUndoneMove()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        var originalState = puzzle.State.CurrentValue;
        
        puzzle.TryMoveEmpty(Puzzle.MoveDirection.Left);
        var movedState = puzzle.State.CurrentValue;
        
        puzzle.UndoCommand();
        Assert.AreEqual(originalState, puzzle.State.CurrentValue);
        
        puzzle.RedoCommand();
        Assert.AreEqual(movedState, puzzle.State.CurrentValue);
    }

    // Test for GetVisitedRoute functionality
    [Test]
    public void Puzzle_GetVisitedRoute_TracksMovementHistory()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        var originalState = puzzle.State.CurrentValue;
        
        var visitedStates = puzzle.GetVisitedRoute();
        Assert.AreEqual(0, visitedStates.Count);
        
        puzzle.TryMoveEmpty(Puzzle.MoveDirection.Left);
        var stateAfterLeft = puzzle.State.CurrentValue;
        visitedStates = puzzle.GetVisitedRoute();
        Assert.AreEqual(1, visitedStates.Count);
        Assert.IsTrue(visitedStates.Contains(originalState));
        
        puzzle.TryMoveEmpty(Puzzle.MoveDirection.Right);
        visitedStates = puzzle.GetVisitedRoute();
        Assert.AreEqual(2, visitedStates.Count);
        Assert.IsTrue(visitedStates.Contains(originalState));
        Assert.IsTrue(visitedStates.Contains(stateAfterLeft));
    }

    // Test for multiple undo/redo operations
    [Test]
    public void Puzzle_MultipleUndoRedo_WorksCorrectly()
    {
        var puzzle = new Puzzle(PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } }));
        var state0 = puzzle.State.CurrentValue;
        
        puzzle.TryMoveEmpty(Puzzle.MoveDirection.Left);
        var state1 = puzzle.State.CurrentValue;
        
        puzzle.TryMoveEmpty(Puzzle.MoveDirection.Up);
        var state2 = puzzle.State.CurrentValue;
        
        // Undo twice
        puzzle.UndoCommand();
        Assert.AreEqual(state1, puzzle.State.CurrentValue);
        
        puzzle.UndoCommand();
        Assert.AreEqual(state0, puzzle.State.CurrentValue);
        
        // Redo twice
        puzzle.RedoCommand();
        Assert.AreEqual(state1, puzzle.State.CurrentValue);
        
        puzzle.RedoCommand();
        Assert.AreEqual(state2, puzzle.State.CurrentValue);
    }

}