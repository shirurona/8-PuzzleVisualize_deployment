using NUnit.Framework;
// Puzzle クラスが他の名前空間にある場合は、適切なusingディレクティブを追加してください。
// 例: using YourProject.Models;

public class DepthFirstSearchTests
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

        DepthFirstSearch dfs = new DepthFirstSearch();

        // Act
        bool result = dfs.Search(initialPuzzle, goalPuzzle);

        // Assert
        Assert.IsTrue(result, "初期状態がゴール状態の場合、探索は成功 (true) するべきです。");
    }

    [Test]
    public void Search_SimplePathExists_ReturnsTrue()
    {
        // Arrange
        Puzzle initialPuzzle = new Puzzle(PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 0, 5}, // 0 が右に1つ移動すればゴール
            {7, 8, 6}
        }));
        PuzzleState goalPuzzle = PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 5, 0},
            {7, 8, 6}
        });

        DepthFirstSearch dfs = new DepthFirstSearch();

        // Act
        bool result = dfs.Search(initialPuzzle, goalPuzzle);

        // Assert
        Assert.IsTrue(result, "単純なパスでゴールに到達できる場合、探索は成功 (true) するべきです。");
    }

    [Test]
    public void Search_NoPathToGoal_Unsolvable_ReturnsFalse()
    {
        // Arrange
        // 解けないパズルの例 (逆転数が奇数で解けない)
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

        DepthFirstSearch dfs = new DepthFirstSearch();

        // Act
        bool result = dfs.Search(initialPuzzle, goalPuzzle);

        // Assert
        Assert.IsFalse(result, "解けないパズルの場合、探索は失敗 (false) するべきです。");
    }

    [Test]
    public void Search_MultipleMovesPathExists_ReturnsTrue()
    {
        // Arrange
        Puzzle initialPuzzle = new Puzzle(PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {0, 4, 6}, // (1,0)
            {7, 5, 8}
        }));
        PuzzleState goalPuzzle = PuzzleState.Create(new int[,]
        {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 0}  // (2,2)
        });

        DepthFirstSearch dfs = new DepthFirstSearch();

        // Act
        bool result = dfs.Search(initialPuzzle, goalPuzzle);

        // Assert
        Assert.IsTrue(result, "複数回の移動が必要なパスでゴールに到達できる場合、探索は成功 (true) するべきです。");
    }
} 