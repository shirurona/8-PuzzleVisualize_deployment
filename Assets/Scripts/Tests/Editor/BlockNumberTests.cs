using NUnit.Framework;
using System;
using UnityEngine.TestTools;

public class BlockNumberTests
{
    // Test for valid construction
    [Test]
    public void BlockNumber_ValidConstruction_ReturnsCorrectValue()
    {
        var blockNum = new BlockNumber(5);
        Assert.AreEqual(5, (int)blockNum);
    }

    // Test for ArgumentOutOfRangeException on invalid value (too low)
    [Test]
    public void BlockNumber_InvalidValueTooLow_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BlockNumber(-1));
    }

    // Test for ArgumentOutOfRangeException on invalid value (too high)
    [Test]
    public void BlockNumber_InvalidValueTooHigh_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BlockNumber(9));
    }

    // Test for implicit operator int
    [Test]
    public void BlockNumber_ImplicitToInt_ReturnsCorrectInt()
    {
        BlockNumber blockNum = new BlockNumber(3);
        int intValue = blockNum;
        Assert.AreEqual(3, intValue);
    }

    // Test for ToString method when value is 0
    [Test]
    public void BlockNumber_ToString_ValueZero_ReturnsEmptyString()
    {
        var blockNum = new BlockNumber(0);
        Assert.AreEqual(string.Empty, blockNum.ToString());
    }

    // Test for ToString method when value is not 0
    [Test]
    public void BlockNumber_ToString_ValueNotZero_ReturnsStringRepresentation()
    {
        var blockNum = new BlockNumber(7);
        Assert.AreEqual("7", blockNum.ToString());
    }

    // Test for Equals method and equality operators
    [Test]
    public void BlockNumber_EqualityOperators_WorkCorrectly()
    {
        var num1 = new BlockNumber(1);
        var num2 = new BlockNumber(1);
        var num3 = new BlockNumber(2);

        // Test Equals method
        Assert.IsTrue(num1.Equals(num2));
        Assert.IsFalse(num1.Equals(num3));
        
        // Test == operator
        Assert.IsTrue(num1 == num2);
        Assert.IsFalse(num1 == num3);
        
        // Test != operator
        Assert.IsFalse(num1 != num2);
        Assert.IsTrue(num1 != num3);
    }

    // Test for GetHashCode method - only test consistency for equal objects
    [Test]
    public void BlockNumber_GetHashCode_ConsistentForEqualObjects()
    {
        var num1 = new BlockNumber(1);
        var num2 = new BlockNumber(1);

        Assert.AreEqual(num1.GetHashCode(), num2.GetHashCode());
    }

    // Test for Parse method with empty string
    [Test]
    public void BlockNumber_Parse_EmptyString_ReturnsZero()
    {
        int result = BlockNumber.Parse("");
        Assert.AreEqual(0, result);
    }

    // Test for Parse method with valid numbers
    [Test]
    public void BlockNumber_Parse_ValidNumbers_ReturnsCorrectValue()
    {
        Assert.AreEqual(1, BlockNumber.Parse("1"));
        Assert.AreEqual(5, BlockNumber.Parse("5"));
        Assert.AreEqual(8, BlockNumber.Parse("8"));
    }

    // Test for Parse method with invalid value
    [Test]
    public void BlockNumber_Parse_InvalidValue_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BlockNumber.Parse("9"));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlockNumber.Parse("a"));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlockNumber.Parse("10"));
    }

    // Test for IsZero method
    [Test]
    public void BlockNumber_IsZero_ReturnsCorrectBoolean()
    {
        var zeroNum = new BlockNumber(0);
        var nonZeroNum = new BlockNumber(5);
        
        Assert.IsTrue(zeroNum.IsZero());
        Assert.IsFalse(nonZeroNum.IsZero());
    }
} 