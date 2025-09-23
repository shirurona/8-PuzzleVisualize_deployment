# InstancedMeshRenderer Transform拡張仕様書

**作成日**: 2025-01-12  
**対象システム**: 8-PuzzleVisualize  
**対象ファイル**: `InstancedMeshRenderer.cs`
**関連ファイル**: `InstancedMeshInfo.cs`

## 要件・問題・動機

### 現状の問題点

現在の`InstancedMeshRenderer`では、GPU Instancingによる大量描画において以下の問題が存在している：

1. **全体再構築方式の非効率性**
   - Matrix4x4変更時に全てのGPUバッファを一から再作成
   - 部分的な変更でも全体のバッファ転送が発生
   - レンダリングパフォーマンスの低下とGPU帯域の無駄遣い

2. **Transform操作の制限**
   - 初期化時にすべてのMatrix4x4を決定する必要がある
   - 個別インスタンスの動的な操作ができない
   - Matrix4x4の直接操作のみでTransform成分へのアクセス不可

3. **メモリ管理の非効率**
   - 変更追跡機能の欠如
   - 不要なGPUメモリ再確保
   - キャッシュ効率の悪化

### 改善の必要性

以下の機能を実現するため、`InstancedMeshRenderer`の抜本的な改良が必要：

- **個別Matrix4x4操作**: インデックス指定による効率的なTransform操作
- **部分更新システム**: 変更された要素のみのGPUバッファ更新
- **Transform成分分離**: position、rotation、scaleの独立した操作
- **パフォーマンス向上**: 最小限のGPU転送による最適化

## 仕様

### 機能要件

#### 1. Matrix4x4操作API
- **直接操作**: Matrix4x4の完全な設定・取得
- **成分別操作**: position、rotation、scaleの個別制御
- **TRS操作**: Transform、Rotation、Scaleの一括設定

#### 2. 配列管理機能
- **動的サイズ**: Matrix4x4配列の動的な挿入・削除
- **インデックス管理**: 安全なインデックスアクセスと例外処理
- **カウント取得**: 現在のMatrix4x4数の取得

#### 3. バッチ操作
- **複数同時更新**: Dictionary<int, Matrix4x4>による一括更新
- **範囲更新**: 指定範囲の部分的なGPUバッファ転送
- **変更追跡**: ダーティフラグによる効率的な更新管理

#### 4. パフォーマンス最適化
- **部分更新**: 変更されたインデックスのみのGPU転送
- **遅延実行**: ApplyPartialUpdate()による明示的な更新タイミング
- **メモリ効率**: 最小限のメモリ再確保

### 非機能要件

#### 1. パフォーマンス
- **O(1)アクセス**: インデックス指定による高速アクセス
- **最小GPU転送**: 変更部分のみのバッファ更新
- **メモリ効率**: 動的サイズ変更時の最適化

#### 2. 安全性
- **境界チェック**: インデックス範囲の厳密な検証
- **例外処理**: 無効操作に対する明確なエラーメッセージ
- **メモリ安全**: 適切なリソース管理

#### 3. 互換性
- **既存API保持**: 現在のAddMatrix()インターフェースとの互換性
- **段階的移行**: 新旧メソッドの並行動作

## 設計

### アーキテクチャ概要

#### データフロー
```
Transform操作要求 → InstancedMeshRenderer → Matrix4x4配列更新
                                               ↓
                  GPU Instancing ← GPUバッファ部分更新
```

#### 主要コンポーネント

1. **Matrix4x4管理システム**: 効率的なMatrix4x4配列操作
2. **変更追跡システム**: ダーティフラグによる部分更新
3. **Transform抽出ユーティリティ**: Matrix4x4からの成分分離
4. **GPU転送最適化**: 最小限のバッファ更新

### 詳細設計

#### 1. 拡張API設計

**Transform操作メソッド**:

```csharp
// Matrix4x4直接操作
public void UpdateMatrix(int index, Matrix4x4 matrix)
public Matrix4x4 GetMatrix(int index)

// Transform成分操作
public void UpdatePosition(int index, Vector3 position)
public void UpdateRotation(int index, Quaternion rotation)
public void UpdateScale(int index, Vector3 scale)
public void SetTRS(int index, Vector3 position, Quaternion rotation, Vector3 scale)

// Transform成分取得
public Vector3 GetPosition(int index)
public Quaternion GetRotation(int index)
public Vector3 GetScale(int index)

// 配列操作
public void InsertMatrix(int index, Matrix4x4 matrix)
public void RemoveMatrix(int index)
public int GetMatrixCount()

// バッチ操作
public void UpdateMatrices(Dictionary<int, Matrix4x4> matrices)
public void ApplyPartialUpdate(int startIndex, int count)
public void ApplyAllChanges()
```

#### 2. 内部データ管理

**データ構造**:

```csharp
private List<Matrix4x4> _matrices; // 既存のMatrix4x4リスト
private HashSet<int> _dirtyIndices; // 変更されたインデックス
private bool _requiresFullUpdate; // 全体更新フラグ
private bool _isPartialUpdateEnabled; // 部分更新有効フラグ
```

**変更追跡システム**:

```csharp
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
```

#### 3. Transform成分抽出ユーティリティ

**Matrix4x4分解メソッド**:

```csharp
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
    scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
    scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
    scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
    return scale;
}
```

#### 4. 部分更新システム

**効率的なGPU転送**:

```csharp
private void ApplyPartialUpdateInternal()
{
    if (_requiresFullUpdate)
    {
        // 全体更新
        _matrixBuffer.SetData(_matrices.ToArray());
        MarkClean();
        return;
    }

    if (_dirtyIndices.Count == 0)
    {
        return; // 更新不要
    }

    // 部分更新の実装
    if (_dirtyIndices.Count < _matrices.Count * 0.3f) // 30%未満なら部分更新
    {
        foreach (int index in _dirtyIndices)
        {
            _matrixBuffer.SetData(new[] { _matrices[index] }, 0, index, 1);
        }
    }
    else
    {
        // 変更が多い場合は全体更新
        _matrixBuffer.SetData(_matrices.ToArray());
    }

    MarkClean();
}
```

#### 5. エラーハンドリング

**境界チェックと例外処理**:

```csharp
private void ValidateIndex(int index)
{
    if (index < 0 || index >= _matrices.Count)
    {
        throw new ArgumentOutOfRangeException(nameof(index), 
            $"インデックスが有効範囲外です。有効範囲: 0-{_matrices.Count - 1}、指定値: {index}");
    }
}

private void ValidateMatrixList()
{
    if (_matrices == null)
    {
        throw new InvalidOperationException("Matrix4x4リストが初期化されていません。");
    }
}
```

### 実装例

#### 基本的な操作例

```csharp
// Matrix4x4の直接更新
renderer.UpdateMatrix(5, Matrix4x4.TRS(
    new Vector3(10, 0, 5), 
    Quaternion.Euler(0, 45, 0), 
    Vector3.one * 2f
));

// Transform成分の個別更新
renderer.UpdatePosition(3, new Vector3(1, 2, 3));
renderer.UpdateRotation(3, Quaternion.Euler(0, 90, 0));
renderer.UpdateScale(3, new Vector3(1.5f, 1.5f, 1.5f));

// TRS一括設定
renderer.SetTRS(7, 
    new Vector3(0, 5, 0),     // position
    Quaternion.identity,       // rotation
    Vector3.one * 0.8f        // scale
);

// バッチ更新
var batchUpdates = new Dictionary<int, Matrix4x4>
{
    { 0, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one) },
    { 1, Matrix4x4.TRS(Vector3.up, Quaternion.identity, Vector3.one) },
    { 2, Matrix4x4.TRS(Vector3.right, Quaternion.identity, Vector3.one) }
};
renderer.UpdateMatrices(batchUpdates);

// 変更の適用
renderer.ApplyAllChanges();
```

#### 配列操作例

```csharp
// 新しいMatrix4x4を挿入
Matrix4x4 newMatrix = Matrix4x4.TRS(Vector3.forward, Quaternion.identity, Vector3.one);
renderer.InsertMatrix(2, newMatrix); // インデックス2に挿入

// Matrix4x4を削除
renderer.RemoveMatrix(5); // インデックス5を削除

// 現在の数を取得
int count = renderer.GetMatrixCount();
Debug.Log($"現在のMatrix4x4数: {count}");
```

### パフォーマンス最適化

#### 1. 部分更新戦略

- **閾値ベース判定**: 変更数が全体の30%未満なら部分更新
- **連続範囲最適化**: 連続するインデックスの範囲更新
- **遅延実行**: 明示的なApply呼び出しによる更新タイミング制御

#### 2. メモリ管理

- **List<Matrix4x4>容量予約**: 頻繁なリサイズを避ける初期容量設定
- **GPUバッファプール**: バッファ再利用による確保コスト削減
- **ダーティフラグ最適化**: HashSet<int>による効率的な変更追跡

#### 3. CPU-GPU同期

- **非同期更新**: GPUバッファ更新の非同期化
- **フレーム分散**: 大量更新の複数フレーム分散
- **優先度制御**: 重要度に応じた更新順序制御

## テストケース

### EditModeテスト

#### IMR-001: UpdateMatrix() 正常動作
**検証内容**: 指定されたインデックスのMatrix4x4が正しく更新されることを確認
- 有効なインデックス範囲での更新
- 更新後にGetMatrix()で取得した値が設定値と一致

```csharp
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
```

#### IMR-005: UpdateMatrix() 境界値テスト
**検証内容**: インデックス境界値での動作確認
- インデックス0での更新
- 最大インデックス(count-1)での更新
- 無効なインデックス(-1, count以上)で例外が発生

```csharp
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
```

#### IMR-010: 成分別更新テスト
**検証内容**: Transform成分（position/rotation/scale）の個別更新動作確認
- 各成分の個別更新時に他の成分が保持される
- 更新対象成分が指定値に正しく変更される

```csharp
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
    Assert.AreEqual(originalRotation, renderer.GetRotation(0));
    Assert.AreEqual(originalScale, renderer.GetScale(0));
}
```

#### IMR-015: 配列操作テスト
**検証内容**: Matrix4x4配列の挿入・削除・取得操作の確認
- InsertMatrix()でのカウント増加とインデックスシフト
- RemoveMatrix()でのカウント減少とインデックスシフト
- GetMatrixCount()での正確なカウント取得

```csharp
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
```

#### IMR-020: バッチ操作テスト
**検証内容**: 複数Matrix4x4操作の一括処理確認
- UpdateMatrices()での複数インデックス同時更新
- 部分更新時の最小限GPU転送とダーティフラグ管理

```csharp
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
```

### パフォーマンステスト

#### PERF-001: 部分更新効率テスト
**検証内容**: 部分更新vs全体更新の処理時間比較

```csharp
[Test]
public void PartialUpdate_IsFasterThanFullUpdate()
{
    // 大量データでの部分更新と全体更新の時間計測
    // 部分更新が全体更新より高速であることを確認
}
```

### メモリ管理テスト

#### MEM-001: リソース解放テスト
**検証内容**: GPUバッファの適切な解放確認

```csharp
[Test]
public void Dispose_ReleasesGPUResources()
{
    // GPUバッファのリソース解放テスト
}
```

---

この仕様書に基づき、`InstancedMeshRenderer`の効率的なMatrix4x4操作機能を実現し、GPU Instancingによる大量描画のパフォーマンスを大幅に向上させる。