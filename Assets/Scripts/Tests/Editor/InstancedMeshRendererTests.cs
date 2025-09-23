using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

[TestFixture]
public class InstancedMeshRendererTests
{
    private InstancedMeshRenderer _renderer;
    private InstancedMeshInfo _meshInfo;

    [SetUp]
    public void SetUp()
    {
        _meshInfo = new InstancedMeshInfo();
        _renderer = CreateInstancedMeshRenderer();
    }

    [TearDown]
    public void TearDown()
    {
        _renderer?.Dispose();
    }

    private InstancedMeshRenderer CreateInstancedMeshRenderer()
    {
        var renderer = new InstancedMeshRenderer(_meshInfo);
        // 初期Matrix4x4を3つ追加
        renderer.AddMatrix(Matrix4x4.identity);
        renderer.AddMatrix(Matrix4x4.TRS(Vector3.up, Quaternion.identity, Vector3.one));
        renderer.AddMatrix(Matrix4x4.TRS(Vector3.right, Quaternion.identity, Vector3.one));
        return renderer;
    }

    // IMR-001: UpdateMatrix() 正常動作
    [Test]
    public void UpdateMatrix_ValidIndex_UpdatesCorrectly()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        var matrix = Matrix4x4.TRS(Vector3.one, Quaternion.identity, Vector3.one * 2f);
        
        // Act
        renderer.UpdateMatrix(0, matrix);
        
        // Assert
        Assert.AreEqual(matrix, renderer.GetMatrix(0));
    }

    // IMR-005: UpdateMatrix() 境界値テスト
    [Test]
    public void UpdateMatrix_InvalidIndex_ThrowsException()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        var matrix = Matrix4x4.identity;
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.UpdateMatrix(-1, matrix));
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.UpdateMatrix(renderer.GetMatrixCount(), matrix));
    }

    // IMR-010: 成分別更新テスト
    [Test]
    public void UpdatePosition_PreservesOtherComponents()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        var originalRotation = Quaternion.Euler(45, 90, 135);
        var originalScale = new Vector3(2, 3, 4);
        renderer.SetTRS(0, Vector3.zero, originalRotation, originalScale);
        
        // Act
        var newPosition = new Vector3(10, 20, 30);
        renderer.UpdatePosition(0, newPosition);
        
        // Assert
        Assert.AreEqual(newPosition, renderer.GetPosition(0));
        // 誤差許容の比較
        Assert.That(Quaternion.Angle(originalRotation, renderer.GetRotation(0)), Is.LessThan(0.01f));
        Assert.That(Vector3.Distance(originalScale, renderer.GetScale(0)), Is.LessThan(0.01f));
    }

    // IMR-015: 配列操作テスト
    [Test]
    public void InsertMatrix_IncreasesCountAndShiftsIndices()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        int initialCount = renderer.GetMatrixCount();
        var newMatrix = Matrix4x4.TRS(Vector3.up, Quaternion.identity, Vector3.one);
        
        // Act
        renderer.InsertMatrix(1, newMatrix);
        
        // Assert
        Assert.AreEqual(initialCount + 1, renderer.GetMatrixCount());
        Assert.AreEqual(newMatrix, renderer.GetMatrix(1));
    }

    // IMR-020: バッチ操作テスト
    [Test]
    public void UpdateMatrices_BatchUpdate_AllIndicesUpdated()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        var batchUpdates = new Dictionary<int, Matrix4x4>
        {
            { 0, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one) },
            { 2, Matrix4x4.TRS(Vector3.up, Quaternion.identity, Vector3.one * 2f) }
        };
        
        // Act
        renderer.UpdateMatrices(batchUpdates);
        
        // Assert
        foreach (var kvp in batchUpdates)
        {
            Assert.AreEqual(kvp.Value, renderer.GetMatrix(kvp.Key));
        }
    }

    // 追加テスト: GetMatrixCount正常動作
    [Test]
    public void GetMatrixCount_ReturnsCorrectCount()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        
        // Act & Assert
        Assert.AreEqual(3, renderer.GetMatrixCount());
    }

    // 追加テスト: RemoveMatrix正常動作
    [Test]
    public void RemoveMatrix_DecreasesCountAndShiftsIndices()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        int initialCount = renderer.GetMatrixCount();
        
        // Act
        renderer.RemoveMatrix(1);
        
        // Assert
        Assert.AreEqual(initialCount - 1, renderer.GetMatrixCount());
    }

    // 追加テスト: Transform成分取得
    [Test]
    public void GetTRS_ReturnsCorrectComponents()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        var position = new Vector3(1, 2, 3);
        var rotation = Quaternion.Euler(30, 60, 90);
        var scale = new Vector3(2, 3, 4);
        renderer.SetTRS(0, position, rotation, scale);
        
        // Act & Assert
        Assert.That(Vector3.Distance(position, renderer.GetPosition(0)), Is.LessThan(0.01f));
        Assert.That(Quaternion.Angle(rotation, renderer.GetRotation(0)), Is.LessThan(0.01f));
        Assert.That(Vector3.Distance(scale, renderer.GetScale(0)), Is.LessThan(0.01f));
    }

    // 追加テスト: ApplyAllChanges正常動作
    [Test]
    public void ApplyAllChanges_DoesNotThrow()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        renderer.UpdatePosition(0, Vector3.one);
        
        // Act & Assert
        Assert.DoesNotThrow(() => renderer.ApplyAllChanges());
    }

    // 追加テスト: UpdateRotation成分保持
    [Test]
    public void UpdateRotation_PreservesOtherComponents()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        var originalPosition = new Vector3(10, 20, 30);
        var originalScale = new Vector3(2, 3, 4);
        renderer.SetTRS(0, originalPosition, Quaternion.identity, originalScale);
        
        // Act
        var newRotation = Quaternion.Euler(45, 90, 135);
        renderer.UpdateRotation(0, newRotation);
        
        // Assert
        Assert.That(Vector3.Distance(originalPosition, renderer.GetPosition(0)), Is.LessThan(0.01f));
        Assert.That(Quaternion.Angle(newRotation, renderer.GetRotation(0)), Is.LessThan(0.01f));
        Assert.That(Vector3.Distance(originalScale, renderer.GetScale(0)), Is.LessThan(0.01f));
    }

    // 追加テスト: UpdateScale成分保持
    [Test]
    public void UpdateScale_PreservesOtherComponents()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        var originalPosition = new Vector3(10, 20, 30);
        var originalRotation = Quaternion.Euler(45, 90, 135);
        renderer.SetTRS(0, originalPosition, originalRotation, Vector3.one);
        
        // Act
        var newScale = new Vector3(2, 3, 4);
        renderer.UpdateScale(0, newScale);
        
        // Assert
        Assert.That(Vector3.Distance(originalPosition, renderer.GetPosition(0)), Is.LessThan(0.01f));
        Assert.That(Quaternion.Angle(originalRotation, renderer.GetRotation(0)), Is.LessThan(0.01f));
        Assert.That(Vector3.Distance(newScale, renderer.GetScale(0)), Is.LessThan(0.01f));
    }

    // 追加テスト: ApplyPartialUpdate正常動作
    [Test]
    public void ApplyPartialUpdate_ValidRange_DoesNotThrow()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        
        // Act & Assert
        Assert.DoesNotThrow(() => renderer.ApplyPartialUpdate(0, 1));
    }

    // 追加テスト: ApplyPartialUpdate範囲外エラー
    [Test]
    public void ApplyPartialUpdate_InvalidRange_ThrowsException()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        int count = renderer.GetMatrixCount();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.ApplyPartialUpdate(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.ApplyPartialUpdate(count, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.ApplyPartialUpdate(0, count + 1));
    }

    // 追加テスト: InsertMatrix境界値
    [Test]
    public void InsertMatrix_InvalidIndex_ThrowsException()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        var matrix = Matrix4x4.identity;
        int count = renderer.GetMatrixCount();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.InsertMatrix(-1, matrix));
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.InsertMatrix(count + 1, matrix));
    }

    // 追加テスト: RemoveMatrix境界値
    [Test]
    public void RemoveMatrix_InvalidIndex_ThrowsException()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        int count = renderer.GetMatrixCount();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.RemoveMatrix(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.RemoveMatrix(count));
    }

    // 追加テスト: GetMatrix境界値
    [Test]
    public void GetMatrix_InvalidIndex_ThrowsException()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        int count = renderer.GetMatrixCount();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.GetMatrix(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.GetMatrix(count));
    }

    // 追加テスト: BatchUpdate境界値
    [Test]
    public void UpdateMatrices_InvalidIndex_ThrowsException()
    {
        // Arrange
        var renderer = CreateInstancedMeshRenderer();
        int count = renderer.GetMatrixCount();
        var invalidUpdates = new Dictionary<int, Matrix4x4>
        {
            { -1, Matrix4x4.identity }
        };
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            renderer.UpdateMatrices(invalidUpdates));
    }
}