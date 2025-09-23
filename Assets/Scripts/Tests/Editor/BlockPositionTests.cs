using NUnit.Framework;
using System;
using UnityEngine.TestTools;

public class BlockPositionTests
{
    // Test for valid construction
    [Test]
    public void BlockPosition_ValidConstruction_ReturnsCorrectValues()
    {
        var pos = new BlockPosition(1, 2);
        Assert.AreEqual(1, pos.Row);
        Assert.AreEqual(2, pos.Column);
    }

    // Test for boundary values (no longer throws exceptions)
    [Test]
    public void BlockPosition_BoundaryValues_AllowsAnyValues()
    {
        // Should not throw exceptions anymore
        var pos1 = new BlockPosition(-1, 0);
        var pos2 = new BlockPosition(3, 0);
        var pos3 = new BlockPosition(0, -1);
        var pos4 = new BlockPosition(0, 3);
        
        Assert.AreEqual(-1, pos1.Row);
        Assert.AreEqual(3, pos2.Row);
        Assert.AreEqual(-1, pos3.Column);
        Assert.AreEqual(3, pos4.Column);
    }

    // Test for Up method
    [Test]
    public void BlockPosition_Up_ReturnsCorrectPosition()
    {
        var pos = new BlockPosition(1, 1);
        var newPos = pos.Up();
        Assert.AreEqual(new BlockPosition(0, 1), newPos);
    }

    // Test for Down method
    [Test]
    public void BlockPosition_Down_ReturnsCorrectPosition()
    {
        var pos = new BlockPosition(1, 1);
        var newPos = pos.Down();
        Assert.AreEqual(new BlockPosition(2, 1), newPos);
    }

    // Test for Left method
    [Test]
    public void BlockPosition_Left_ReturnsCorrectPosition()
    {
        var pos = new BlockPosition(1, 1);
        var newPos = pos.Left();
        Assert.AreEqual(new BlockPosition(1, 0), newPos);
    }

    // Test for Right method
    [Test]
    public void BlockPosition_Right_ReturnsCorrectPosition()
    {
        var pos = new BlockPosition(1, 1);
        var newPos = pos.Right();
        Assert.AreEqual(new BlockPosition(1, 2), newPos);
    }

    // Test for CreateFromIndex method
    [Test]
    public void BlockPosition_CreateFromIndex_ReturnsCorrectPosition()
    {
        var pos0 = BlockPosition.CreateFromIndex(0);
        var pos1 = BlockPosition.CreateFromIndex(1);
        var pos4 = BlockPosition.CreateFromIndex(4);
        var pos8 = BlockPosition.CreateFromIndex(8);
        
        Assert.AreEqual(new BlockPosition(0, 0), pos0);
        Assert.AreEqual(new BlockPosition(0, 1), pos1);
        Assert.AreEqual(new BlockPosition(1, 1), pos4);
        Assert.AreEqual(new BlockPosition(2, 2), pos8);
    }

    // Test for implicit operator to int
    [Test]
    public void BlockPosition_ImplicitOperatorToInt_ReturnsCorrectIndex()
    {
        var pos1 = new BlockPosition(0, 0);
        var pos2 = new BlockPosition(0, 1);
        var pos3 = new BlockPosition(1, 1);
        var pos4 = new BlockPosition(2, 2);
        
        int index1 = pos1;
        int index2 = pos2;
        int index3 = pos3;
        int index4 = pos4;
        
        Assert.AreEqual(0, index1);
        Assert.AreEqual(1, index2);
        Assert.AreEqual(4, index3);
        Assert.AreEqual(8, index4);
    }

    // Test for Equals method and equality operators
    [Test]
    public void BlockPosition_EqualityOperators_WorkCorrectly()
    {
        var pos1 = new BlockPosition(1, 1);
        var pos2 = new BlockPosition(1, 1);
        var pos3 = new BlockPosition(1, 2);

        // Test Equals method
        Assert.IsTrue(pos1.Equals(pos2));
        Assert.IsFalse(pos1.Equals(pos3));
        
        // Test == operator
        Assert.IsTrue(pos1 == pos2);
        Assert.IsFalse(pos1 == pos3);
        
        // Test != operator
        Assert.IsFalse(pos1 != pos2);
        Assert.IsTrue(pos1 != pos3);
    }

    // Test for GetHashCode method - only test consistency for equal objects
    [Test]
    public void BlockPosition_GetHashCode_ConsistentForEqualObjects()
    {
        var pos1 = new BlockPosition(1, 1);
        var pos2 = new BlockPosition(1, 1);

        Assert.AreEqual(pos1.GetHashCode(), pos2.GetHashCode());
    }
} 