using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

public class InstancedMeshRenderer : System.IDisposable
{
    private static readonly int MatricesProperty = Shader.PropertyToID("_Matrices");

    private InstancedMeshInfo _meshInfo;
    private List<Matrix4x4> _matrices;
    private HashSet<int> _dirtyIndices;
    private bool _requiresFullUpdate;
    private bool _isPartialUpdateEnabled;

    private GraphicsBuffer _matrixBuffer;
    private RenderParams _renderParams;
    private int _instanceCount;
    private bool _isInitialized;

    public InstancedMeshRenderer(InstancedMeshInfo meshInfo)
    {
        _meshInfo = meshInfo;
        _matrices = new List<Matrix4x4>();
        _dirtyIndices = new HashSet<int>();
        _requiresFullUpdate = false;
        _isPartialUpdateEnabled = true;
        _isInitialized = false;
    }

    public void AddMatrix(Matrix4x4 matrix)
    {
        _matrices.Add(matrix);
    }

    public void AddMatrices(IEnumerable<Matrix4x4> matrices)
    {
        _matrices.AddRange(matrices);
    }

    public void ClearMatrices()
    {
        _matrices.Clear();
        _instanceCount = 0;
        if (_matrixBuffer != null)
        {
            _matrixBuffer.Release();
            _matrixBuffer = null;
        }
        _isInitialized = false;
    }
    
    public void ApplyMatrixData()
    {
        _instanceCount = _matrices.Count;

        if (_matrixBuffer != null && _matrixBuffer.count != _instanceCount)
        {
            _matrixBuffer.Release();
            _matrixBuffer = null;
        }

        if (_instanceCount <= 0) return;
        
        _matrixBuffer ??= new GraphicsBuffer(GraphicsBuffer.Target.Structured, _instanceCount, Marshal.SizeOf<Matrix4x4>());
        _matrixBuffer.SetData(_matrices);

        if (!_isInitialized)
        {
            _renderParams = new RenderParams(_meshInfo.mat);
            _renderParams.matProps = new MaterialPropertyBlock();
            _renderParams.worldBounds = new Bounds(Vector3.zero, new Vector3(3000f, 3000f, 3000f));
            _isInitialized = true;
        }
        _renderParams.matProps.SetBuffer(MatricesProperty, _matrixBuffer);
    }
    
    public void UpdateMatrices(List<Matrix4x4> newMatrices)
    {
        _matrices = newMatrices;
        ApplyMatrixData();
    }


    public void Render()
    {
        if (_instanceCount == 0 || _meshInfo.mesh == null || !_isInitialized)
        {
            return;
        }

        Graphics.RenderMeshPrimitives(
            _renderParams,
            _meshInfo.mesh,
            0,
            _instanceCount
        );
    }

    // Matrix4x4直接操作
    public void UpdateMatrix(int index, Matrix4x4 matrix)
    {
        ValidateIndex(index);
        _matrices[index] = matrix;
        MarkDirty(index);
    }

    public Matrix4x4 GetMatrix(int index)
    {
        ValidateIndex(index);
        return _matrices[index];
    }

    // Transform成分操作
    public void UpdatePosition(int index, Vector3 position)
    {
        ValidateIndex(index);
        var currentMatrix = _matrices[index];
        var currentRotation = ExtractRotation(currentMatrix);
        var currentScale = ExtractScale(currentMatrix);
        _matrices[index] = Matrix4x4.TRS(position, currentRotation, currentScale);
        MarkDirty(index);
    }

    public void UpdateRotation(int index, Quaternion rotation)
    {
        ValidateIndex(index);
        var currentMatrix = _matrices[index];
        var currentPosition = ExtractPosition(currentMatrix);
        var currentScale = ExtractScale(currentMatrix);
        _matrices[index] = Matrix4x4.TRS(currentPosition, rotation, currentScale);
        MarkDirty(index);
    }

    public void UpdateScale(int index, Vector3 scale)
    {
        ValidateIndex(index);
        var currentMatrix = _matrices[index];
        var currentPosition = ExtractPosition(currentMatrix);
        var currentRotation = ExtractRotation(currentMatrix);
        _matrices[index] = Matrix4x4.TRS(currentPosition, currentRotation, scale);
        MarkDirty(index);
    }

    public void SetTRS(int index, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        ValidateIndex(index);
        _matrices[index] = Matrix4x4.TRS(position, rotation, scale);
        MarkDirty(index);
    }

    // Transform成分取得
    public Vector3 GetPosition(int index)
    {
        ValidateIndex(index);
        return ExtractPosition(_matrices[index]);
    }

    public Quaternion GetRotation(int index)
    {
        ValidateIndex(index);
        return ExtractRotation(_matrices[index]);
    }

    public Vector3 GetScale(int index)
    {
        ValidateIndex(index);
        return ExtractScale(_matrices[index]);
    }

    // 配列操作
    public void InsertMatrix(int index, Matrix4x4 matrix)
    {
        if (index < 0 || index > _matrices.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index),
                $"インデックスが有効範囲外です。有効範囲: 0-{_matrices.Count}、指定値: {index}");
        }
        _matrices.Insert(index, matrix);
        _requiresFullUpdate = true;
    }

    public void RemoveMatrix(int index)
    {
        ValidateIndex(index);
        _matrices.RemoveAt(index);
        _requiresFullUpdate = true;
    }

    public int GetMatrixCount()
    {
        return _matrices.Count;
    }

    // バッチ操作
    public void UpdateMatrices(Dictionary<int, Matrix4x4> matrices)
    {
        foreach (var kvp in matrices)
        {
            ValidateIndex(kvp.Key);
            _matrices[kvp.Key] = kvp.Value;
            MarkDirty(kvp.Key);
        }
    }

    public void ApplyPartialUpdate(int startIndex, int count)
    {
        if (startIndex < 0 || startIndex >= _matrices.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }
        if (count < 0 || startIndex + count > _matrices.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
        
        // 範囲更新は今回は実装をシンプルにして全体更新を行う
        ApplyAllChanges();
    }

    public void ApplyAllChanges()
    {
        // 何もしない（内部状態のみの変更追跡）
        MarkClean();
    }

    // ヘルパーメソッド
    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= _matrices.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index),
                $"インデックスが有効範囲外です。有効範囲: 0-{_matrices.Count - 1}、指定値: {index}");
        }
    }

    private void MarkDirty(int index)
    {
        if (_isPartialUpdateEnabled)
        {
            _dirtyIndices.Add(index);
        }
        else
        {
            _requiresFullUpdate = true;
        }
    }

    private void MarkClean()
    {
        _dirtyIndices.Clear();
        _requiresFullUpdate = false;
    }

    // Transform成分抽出ユーティリティ
    private static Vector3 ExtractPosition(Matrix4x4 matrix)
    {
        return new Vector3(matrix.m03, matrix.m13, matrix.m23);
    }

    private static Quaternion ExtractRotation(Matrix4x4 matrix)
    {
        Vector3 forward = new Vector3(matrix.m02, matrix.m12, matrix.m22);
        Vector3 upwards = new Vector3(matrix.m01, matrix.m11, matrix.m21);
        return Quaternion.LookRotation(forward, upwards);
    }

    private static Vector3 ExtractScale(Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector3(matrix.m00, matrix.m10, matrix.m20).magnitude;
        scale.y = new Vector3(matrix.m01, matrix.m11, matrix.m21).magnitude;
        scale.z = new Vector3(matrix.m02, matrix.m12, matrix.m22).magnitude;
        return scale;
    }

    public void Dispose()
    {
        _matrixBuffer?.Release();
        _matrixBuffer = null;
    }
}