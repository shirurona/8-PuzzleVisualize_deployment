using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BreadthFirstSearchTests
{
    [Test]
    public void Search_InitialStateIsGoal_ReturnsTrue()
    {
        // Arrange
        Puzzle initialPuzzle = new Puzzle(PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 0}
        }));
        PuzzleState goalPuzzle = PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 0}
        });

        BreadthFirstSearch bfs = new BreadthFirstSearch();

        // Act
        bool result = bfs.Search(initialPuzzle, goalPuzzle);

        // Assert
        Assert.IsTrue(result, "初期状態がゴール状態の場合、探索は成功するべきです。");
    }

    [Test]
    public void Search_SimplePathExists_ReturnsTrue()
    {
        // Arrange
        Puzzle initialPuzzle = new Puzzle(PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 5, 0},
            {7, 8, 6}
        }));
        PuzzleState goalPuzzle = PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 0}
        });

        BreadthFirstSearch bfs = new BreadthFirstSearch();

        // Act
        bool result = bfs.Search(initialPuzzle, goalPuzzle);

        // Assert
        Assert.IsTrue(result, "単純なパスでゴールに到達できる場合、探索は成功するべきです。");
    }

    [Test]
    public void Search_ReturnsSearchDataMap()
    {
        // Arrange
        Puzzle initialPuzzle = new Puzzle(PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 5, 0},
            {7, 8, 6}
        }));
        PuzzleState goalPuzzle = PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 0}
        });

        BreadthFirstSearch bfs = new BreadthFirstSearch();

        // Act
        bool result = bfs.Search(initialPuzzle, goalPuzzle);
        var searchData = bfs.GetSearchDataMap();

        // Assert
        Assert.IsTrue(result, "探索は成功するべきです。");
        Assert.IsNotNull(searchData, "探索データマップが返されるべきです。");
        Assert.Greater(searchData.Count, 0, "探索データマップには少なくとも1つの要素があるべきです。");
    }

    [Test]
    public void Search_UnsolvablePuzzle_ReturnsFalse()
    {
        // Arrange
        Puzzle initialPuzzle = new Puzzle(PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 5, 6},
            {8, 7, 0}
        }));
        PuzzleState goalPuzzle = PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 0}
        });

        BreadthFirstSearch bfs = new BreadthFirstSearch();

        // Act
        bool result = bfs.Search(initialPuzzle, goalPuzzle);

        // Assert
        Assert.IsFalse(result, "解けないパズルの場合、探索は失敗するべきです。");
    }
}