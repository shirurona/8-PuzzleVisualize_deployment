using NUnit.Framework;
using System;
using UnityEngine.TestTools;

public class PuzzleStateTests
{
    private BlockNumber[,] CreateValidBlockNumbers()
    {
        return new BlockNumber[3, 3]
        {
            {new BlockNumber(1), new BlockNumber(2), new BlockNumber(3)},
            {new BlockNumber(4), new BlockNumber(5), new BlockNumber(6)},
            {new BlockNumber(7), new BlockNumber(8), new BlockNumber(0)}
        };
    }

    // Test for valid constructor
    [Test]
    public void PuzzleState_ValidConstructor_CreatesPuzzleState()
    {
        var blockNumbers = CreateValidBlockNumbers();
        var puzzleState = new PuzzleState(blockNumbers);
        Assert.AreEqual(new BlockNumber(1), puzzleState[new BlockPosition(0, 0)]);
    }

    // Test for constructor with null blockNumbers
    [Test]
    public void PuzzleState_Constructor_NullBlockNumbers_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PuzzleState(null));
    }

    // Test for constructor with invalid dimensions
    [Test]
    public void PuzzleState_Constructor_InvalidDimensions_ThrowsArgumentException()
    {
        var invalidBlockNumbers = new BlockNumber[2, 2];
        Assert.Throws<ArgumentException>(() => new PuzzleState(invalidBlockNumbers));
    }

    // Test for Create method with valid input
    [Test]
    public void PuzzleState_Create_ValidInput_CreatesPuzzleState()
    {
        int[,] numbers = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } };
        var puzzleState = PuzzleState.Create(numbers);
        Assert.AreEqual(new BlockNumber(1), puzzleState[new BlockPosition(0, 0)]);
    }

    // Test for Create method with null input
    [Test]
    public void PuzzleState_Create_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => PuzzleState.Create(null));
    }

    // Test for Create method with invalid dimensions
    [Test]
    public void PuzzleState_Create_InvalidDimensions_ThrowsArgumentException()
    {
        int[,] invalidNumbers = { { 1, 2 }, { 3, 4 } };
        Assert.Throws<ArgumentException>(() => PuzzleState.Create(invalidNumbers));
    }

    // Test for indexer (get)
    [Test]
    public void PuzzleState_IndexerGet_ReturnsCorrectBlockNumber()
    {
        var blockNumbers = CreateValidBlockNumbers();
        var puzzleState = new PuzzleState(blockNumbers);
        Assert.AreEqual(new BlockNumber(1), puzzleState[new BlockPosition(0, 0)]);
    }

    // Test for indexer with out of range position (get)
    [Test]
    public void PuzzleState_IndexerGet_OutOfRangePosition_ThrowsArgumentOutOfRangeException()
    {
        var puzzleState = new PuzzleState(CreateValidBlockNumbers());
        Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = puzzleState[new BlockPosition(-1, 0)]; });
        Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = puzzleState[new BlockPosition(0, -1)]; });
    }

    // Test for Equals
    [Test]
    public void PuzzleState_Equals_ReturnsTrueForEqualPuzzleStates()
    {
        var puzzleState1 = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } });
        var puzzleState2 = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } });
        Assert.IsTrue(puzzleState1.Equals(puzzleState2));
    }

    [Test]
    public void PuzzleState_Equals_ReturnsFalseForUnequalPuzzleStates()
    {
        var puzzleState1 = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } });
        var puzzleState2 = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 0 }, { 7, 8, 6 } });
        Assert.IsFalse(puzzleState1.Equals(puzzleState2));
    }

    // Test for GetHashCode
    [Test]
    public void PuzzleState_GetHashCode_ReturnsSameHashCodeForEqualPuzzleStates()
    {
        var puzzleState1 = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } });
        var puzzleState2 = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } });
        Assert.AreEqual(puzzleState1.GetHashCode(), puzzleState2.GetHashCode());
    }

    // Test for constructor with duplicate block numbers
    [Test]
    public void PuzzleState_Constructor_DuplicateBlockNumbers_ThrowsArgumentException()
    {
        var duplicateNumbers = new BlockNumber[3, 3]
        {
            {new BlockNumber(1), new BlockNumber(2), new BlockNumber(3)},
            {new BlockNumber(4), new BlockNumber(5), new BlockNumber(6)},
            {new BlockNumber(7), new BlockNumber(8), new BlockNumber(8)} // Duplicate 8
        };
        Assert.Throws<ArgumentException>(() => new PuzzleState(duplicateNumbers));
    }

    // Test for constructor with missing block number
    [Test]
    public void PuzzleState_Constructor_MissingBlockNumber_ThrowsArgumentException()
    {
        var missingNumber = new BlockNumber[3, 3]
        {
            {new BlockNumber(1), new BlockNumber(2), new BlockNumber(3)},
            {new BlockNumber(4), new BlockNumber(5), new BlockNumber(6)},
            {new BlockNumber(7), new BlockNumber(8), new BlockNumber(7)} // Missing 0, has duplicate 7
        };
        Assert.Throws<ArgumentException>(() => new PuzzleState(missingNumber));
    }

    // Test for Create method with duplicate block numbers
    [Test]
    public void PuzzleState_Create_DuplicateBlockNumbers_ThrowsArgumentException()
    {
        int[,] duplicateNumbers = new int[3, 3]
        {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 8} // Duplicate 8
        };
        Assert.Throws<ArgumentException>(() => PuzzleState.Create(duplicateNumbers));
    }

    // Test for Create method with missing block number
    [Test]
    public void PuzzleState_Create_MissingBlockNumber_ThrowsArgumentException()
    {
        int[,] missingNumber = new int[3, 3]
        {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 7} // Missing 0, has duplicate 7
        };
        Assert.Throws<ArgumentException>(() => PuzzleState.Create(missingNumber));
    }

    // Test for FindEmptyBlockPosition
    [Test]
    public void PuzzleState_FindEmptyBlockPosition_ReturnsCorrectPosition()
    {
        var puzzleState = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } });
        Assert.AreEqual(new BlockPosition(1, 1), puzzleState.EmptyBlockPosition);
    }

    // Test for FindNumberBlockPosition
    [Test]
    public void PuzzleState_FindNumberBlockPosition_ReturnsCorrectPosition()
    {
        var puzzleState = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } });
        Assert.AreEqual(new BlockPosition(0, 0), puzzleState.FindNumberBlockPosition(new BlockNumber(1)));
        Assert.AreEqual(new BlockPosition(1, 1), puzzleState.FindNumberBlockPosition(new BlockNumber(5)));
        Assert.AreEqual(new BlockPosition(2, 2), puzzleState.FindNumberBlockPosition(new BlockNumber(0)));
    }

    // Test for equality operators
    [Test]
    public void PuzzleState_EqualityOperator_WorksCorrectly()
    {
        var puzzleState1 = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } });
        var puzzleState2 = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } });
        var puzzleState3 = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 0 }, { 7, 8, 6 } });

        Assert.IsTrue(puzzleState1 == puzzleState2);
        Assert.IsFalse(puzzleState1 == puzzleState3);
        Assert.IsFalse(puzzleState1 != puzzleState2);
        Assert.IsTrue(puzzleState1 != puzzleState3);
    }

    // Test for ToString
    [Test]
    public void PuzzleState_ToString_ReturnsFormattedString()
    {
        var puzzleState = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } });
        string result = puzzleState.ToString();
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("1"));
        Assert.IsTrue(result.Contains("0"));
    }

    // Test for TrySwap - valid swap with empty block
    [Test]
    public void PuzzleState_TrySwap_ValidSwapWithEmpty_ReturnsTrue()
    {
        var puzzleState = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } });
        var emptyPos = new BlockPosition(1, 1);
        var rightPos = new BlockPosition(1, 2);
        
        var newState = puzzleState.Swap(rightPos);
        
        Assert.IsNotNull(newState);
        Assert.AreEqual(new BlockNumber(0), newState[rightPos]);
        Assert.AreEqual(new BlockNumber(6), newState[emptyPos]);
    }

    // Test for TrySwap - invalid swap with non-adjacent positions
    [Test]
    public void PuzzleState_TrySwap_InvalidSwapNonAdjacent_ReturnsFalse()
    {
        var puzzleState = PuzzleState.Create(new int[,] { { 1, 2, 3 }, { 4, 0, 6 }, { 7, 8, 5 } });
        var farPos = new BlockPosition(0, 0);
        
        var newState = puzzleState.Swap(farPos);
        
        Assert.IsNull(newState);
    }
}