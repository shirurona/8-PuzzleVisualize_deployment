# PuzzleVisualizer Transform拡張仕様書

**作成日**: 2025-01-12  
**対象システム**: 8-PuzzleVisualize  
**対象ファイル**: `PuzzleVisualizer.cs`
**関連ファイル**: `InstancedMeshRenderer.cs`, `PuzzleNodeData.cs`

## 要件・問題・動機

### 現状の問題点

現在の`PuzzleVisualizer`では、パズル状態の3D表示において以下の問題が存在している：

1. **パズル単位操作の欠如**
   - 個別パズル状態のTransform操作ができない
   - パズル全体（9ブロック + 番号テキスト）の一括操作が不可能
   - ゲームオブジェクト感覚での直感的操作が実現できない

2. **高レベルAPIの不足**
   - 低レベルのMatrix4x4操作のみで使いにくい
   - パズルドメインに特化した操作メソッドがない
   - ブロック単位での細かい制御ができない

3. **インデックス管理の複雑さ**
   - PuzzleStateとレンダリングインデックスの対応が不明確
   - ブロックやテキストの個別操作時のインデックス計算が複雑
   - エラーが発生しやすい手動インデックス管理

4. **エッジ・ルート表示との連携不足**
   - パズル位置変更時にエッジが自動で追従しない
   - 経路可視化とTransform操作が分離している
   - 動的な可視化効果が実現できない

### 改善の必要性

以下の機能を実現するため、`PuzzleVisualizer`の高レベルAPI実装が必要：

- **パズル単位操作**: PuzzleStateを指定した直感的なTransform操作
- **ブロック単位操作**: 個別ブロックの独立したTransform制御
- **インデックス管理**: 自動的なレンダリングインデックス対応
- **エッジ連携**: Transform変更時の自動エッジ更新

## 仕様

### 機能要件

#### 1. パズル単位Transform操作
- **一括設定**: パズル全体のposition、rotation、scaleの同時設定
- **成分別操作**: 各Transform成分の個別制御
- **相対操作**: 現在値からのオフセット適用
- **取得操作**: 現在のTransform状態の取得

#### 2. ブロック単位操作
- **個別制御**: 特定ブロックのTransform操作
- **相対位置**: パズル全体Transformに対する相対Transform
- **成分合成**: パズル全体とブロック個別Transformの適切な合成

#### 3. インスタンス管理
- **動的追加**: 新しいパズルインスタンスの追加
- **安全削除**: パズルインスタンスの完全な削除
- **自動インデックス**: レンダリングインデックスの自動管理

#### 4. エッジ・ルート表示連携
- **自動更新**: パズルTransform変更時のエッジ自動更新
- **経路追従**: プレイヤー経路のリアルタイム可視化
- **動的レイアウト**: Transform変更に応じた配置調整

#### 5. ユーティリティ機能
- **Reset操作**: パズルTransformのIdentity状態復元
- **Copy操作**: パズル間のTransform複製
- **全体操作**: 全パズルに対する一括Transform適用

### 非機能要件

#### 1. パフォーマンス
- **効率的インデックス**: Dictionary<PuzzleState, PuzzleRenderInfo>による高速検索
- **キャッシュ活用**: Transform情報の効率的キャッシュ
- **最小更新**: 変更されたパズルのみの更新

#### 2. 安全性
- **PuzzleState検証**: 存在しないパズル状態への操作を防止
- **BlockPosition検証**: 有効なブロック位置の確認
- **例外処理**: 明確なエラーメッセージによる問題特定

#### 3. 拡張性
- **アニメーション対応**: 将来のアニメーション機能への拡張
- **カスタム効果**: 特殊な視覚効果の追加対応
- **レイアウト戦略**: 新しい配置アルゴリズムとの統合

## 設計

### アーキテクチャ概要

#### データフロー
```
パズル操作要求 → PuzzleVisualizer → インデックス計算 → InstancedMeshRenderer
                      ↓                                        ↓
              エッジ更新処理 ← Matrix4x4更新 ← GPU Instancing
```

#### 主要コンポーネント

1. **高レベルAPI**: パズルドメイン特化の操作メソッド
2. **インデックス管理システム**: PuzzleStateとレンダリングインデックスの対応
3. **Transform合成システム**: パズル全体とブロック個別Transformの合成
4. **エッジ更新システム**: Transform変更時の自動エッジ調整

### 詳細設計

#### 1. 高レベルAPI設計

**パズル単位操作メソッド**:

```csharp
// 完全なTransform操作
public void SetPuzzleTransform(PuzzleState puzzleState, Vector3 position, Quaternion rotation, Vector3 scale)
public void UpdatePuzzlePosition(PuzzleState puzzleState, Vector3 position)
public void UpdatePuzzleRotation(PuzzleState puzzleState, Quaternion rotation)
public void UpdatePuzzleScale(PuzzleState puzzleState, Vector3 scale)

// 相対操作
public void MovePuzzle(PuzzleState puzzleState, Vector3 offset)
public void RotatePuzzle(PuzzleState puzzleState, Vector3 eulerAngles)
public void ScalePuzzle(PuzzleState puzzleState, float uniformScale)
public void ScalePuzzle(PuzzleState puzzleState, Vector3 scale)

// 取得操作
public Matrix4x4 GetPuzzleMatrix(PuzzleState puzzleState)
public Vector3 GetPuzzlePosition(PuzzleState puzzleState)
public Quaternion GetPuzzleRotation(PuzzleState puzzleState)
public Vector3 GetPuzzleScale(PuzzleState puzzleState)

// パズル管理
public void AddSinglePuzzleInstance(PuzzleState puzzleState, Vector3 position)
public void AddSinglePuzzleInstance(PuzzleState puzzleState, Matrix4x4 matrix)
public void RemovePuzzleInstance(PuzzleState puzzleState)
```

**ブロック単位操作メソッド**:

```csharp
public void UpdateBlockMatrix(PuzzleState puzzleState, BlockPosition blockPosition, Matrix4x4 matrix)
public void UpdateBlockPosition(PuzzleState puzzleState, BlockPosition blockPosition, Vector3 offset)
public void RotateBlock(PuzzleState puzzleState, BlockPosition blockPosition, Quaternion rotation)
public void ScaleBlock(PuzzleState puzzleState, BlockPosition blockPosition, Vector3 scale)
public Matrix4x4 GetBlockMatrix(PuzzleState puzzleState, BlockPosition blockPosition)
```

**ユーティリティメソッド**:

```csharp
public void ResetTransform(PuzzleState puzzleState)
public void CopyTransform(PuzzleState fromPuzzle, PuzzleState toPuzzle)
public void ApplyUniformScale(float scale)
public void ApplyGlobalRotation(Quaternion rotation)
```

#### 2. インデックス管理システム

**PuzzleRenderInfo構造体**:

```csharp
private struct PuzzleRenderInfo
{
    public int startIndex;           // ブロック配列の開始インデックス
    public int blockCount;           // ブロック数（通常9）
    public int textStartIndex;       // テキスト配列の開始インデックス
    public Matrix4x4 baseMatrix;    // パズル全体のベースMatrix4x4
    public Dictionary<BlockPosition, Matrix4x4> blockOffsets; // ブロック個別オフセット
}
```

**インデックス管理**:

```csharp
private Dictionary<PuzzleState, PuzzleRenderInfo> _puzzleRenderMap;

private PuzzleRenderInfo GetPuzzleRenderInfo(PuzzleState puzzleState)
{
    if (!_puzzleRenderMap.TryGetValue(puzzleState, out var renderInfo))
    {
        throw new ArgumentException($"指定されたパズル状態は登録されていません: {puzzleState}");
    }
    return renderInfo;
}

private int GetBlockRenderIndex(PuzzleState puzzleState, BlockPosition blockPosition)
{
    var renderInfo = GetPuzzleRenderInfo(puzzleState);
    return renderInfo.startIndex + blockPosition.Row * 3 + blockPosition.Column;
}

private int GetTextRenderIndex(PuzzleState puzzleState, BlockPosition blockPosition)
{
    var renderInfo = GetPuzzleRenderInfo(puzzleState);
    return renderInfo.textStartIndex + blockPosition.Row * 3 + blockPosition.Column;
}
```

#### 3. Transform合成システム

**Transform合成計算**:

```csharp
private Matrix4x4 CalculateBlockMatrix(PuzzleState puzzleState, BlockPosition blockPosition)
{
    var renderInfo = GetPuzzleRenderInfo(puzzleState);
    Matrix4x4 puzzleMatrix = renderInfo.baseMatrix;
    
    // ブロック個別オフセットを取得
    Matrix4x4 blockOffset = Matrix4x4.identity;
    if (renderInfo.blockOffsets.TryGetValue(blockPosition, out var offset))
    {
        blockOffset = offset;
    }
    
    // パズル全体Transform × ブロック個別Transform
    return puzzleMatrix * blockOffset;
}

private void UpdateBlockInternalMatrix(PuzzleState puzzleState, BlockPosition blockPosition, Matrix4x4 newMatrix)
{
    var renderInfo = GetPuzzleRenderInfo(puzzleState);
    
    // ブロックの最終Matrix4x4を計算
    Matrix4x4 finalMatrix = CalculateBlockMatrix(puzzleState, blockPosition);
    
    // InstancedMeshRendererに反映
    int blockIndex = GetBlockRenderIndex(puzzleState, blockPosition);
    int textIndex = GetTextRenderIndex(puzzleState, blockPosition);
    
    _blockRenderer.UpdateMatrix(blockIndex, finalMatrix);
    _textRenderer.UpdateMatrix(textIndex, finalMatrix);
}
```

#### 4. エッジ更新システム

**エッジMatrix4x4計算**:

```csharp
private Matrix4x4 CalculateEdgeMatrix(PuzzleState fromPuzzle, PuzzleState toPuzzle)
{
    Vector3 fromPos = GetPuzzlePosition(fromPuzzle);
    Vector3 toPos = GetPuzzlePosition(toPuzzle);
    
    // Transform適用済みの位置計算
    Vector3 position = (fromPos + toPos) / 2f;
    Vector3 direction = toPos - fromPos;
    Quaternion rotation = Quaternion.identity;
    
    if (direction != Vector3.zero)
    {
        rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
    }
    
    float length = direction.magnitude;
    Vector3 scale = new Vector3(0.1f, length / 2f, 0.1f);
    
    return Matrix4x4.TRS(position, rotation, scale);
}

private void UpdateAllEdges()
{
    if (_searchSpaceVisualizer == null) return;
    
    // 隣接関係のエッジを更新
    foreach (var puzzleData in _searchSpaceVisualizer.GetPuzzleDataMap())
    {
        var puzzleState = puzzleData.Key;
        var nodeData = puzzleData.Value;
        
        foreach (var adjacentState in nodeData.AdjacentStates)
        {
            var edgeMatrix = CalculateEdgeMatrix(puzzleState, adjacentState);
            // エッジレンダラーに適用（実装は別途検討）
        }
    }
}
```

#### 5. キャッシュシステム

**Transform情報キャッシュ**:

```csharp
private Dictionary<PuzzleState, Matrix4x4> _matrixCache;
private Dictionary<(PuzzleState, BlockPosition), Matrix4x4> _blockMatrixCache;

private void InvalidateCache(PuzzleState puzzleState)
{
    _matrixCache.Remove(puzzleState);
    
    // ブロックキャッシュのクリア
    var keysToRemove = _blockMatrixCache.Keys
        .Where(key => key.Item1.Equals(puzzleState))
        .ToList();
    
    foreach (var key in keysToRemove)
    {
        _blockMatrixCache.Remove(key);
    }
}
```

### 実装例

#### 基本的な操作例

```csharp
// パズルの位置変更
visualizer.UpdatePuzzlePosition(targetPuzzle, new Vector3(10, 5, 0));

// パズルの回転（Y軸45度回転）
visualizer.UpdatePuzzleRotation(targetPuzzle, Quaternion.Euler(0, 45, 0));

// パズルの拡大（2倍）
visualizer.UpdatePuzzleScale(targetPuzzle, Vector3.one * 2f);

// 一括設定
visualizer.SetPuzzleTransform(targetPuzzle, 
    new Vector3(10, 5, 0),       // position
    Quaternion.Euler(0, 45, 0),  // rotation
    Vector3.one * 1.5f           // scale
);

// 相対操作
visualizer.MovePuzzle(targetPuzzle, Vector3.up * 2f);
visualizer.RotatePuzzle(targetPuzzle, new Vector3(0, 90, 0));
visualizer.ScalePuzzle(targetPuzzle, 1.2f);
```

#### ブロック単位操作例

```csharp
var blockPos = new BlockPosition(1, 1);

// 個別ブロックの回転
visualizer.RotateBlock(targetPuzzle, blockPos, Quaternion.Euler(0, 0, 30));

// 個別ブロックのスケール変更
visualizer.ScaleBlock(targetPuzzle, blockPos, new Vector3(1.2f, 1.2f, 1f));

// 個別ブロックの位置オフセット
visualizer.UpdateBlockPosition(targetPuzzle, blockPos, Vector3.forward * 0.5f);
```

#### インスタンス管理例

```csharp
// 新しいパズルインスタンスを追加
var newPuzzleState = PuzzleState.Create(new int[,] { ... });
visualizer.AddSinglePuzzleInstance(newPuzzleState, new Vector3(20, 0, 0));

// パズルインスタンスを削除
visualizer.RemovePuzzleInstance(targetPuzzle);

// Transform情報をコピー
visualizer.CopyTransform(sourcePuzzle, targetPuzzle);
```

### エラーハンドリング

#### 検証メソッド

```csharp
private void ValidatePuzzleState(PuzzleState puzzleState)
{
    if (!_puzzleRenderMap.ContainsKey(puzzleState))
    {
        throw new ArgumentException($"指定されたパズル状態は登録されていません。", nameof(puzzleState));
    }
}

private void ValidateBlockPosition(BlockPosition blockPosition)
{
    if (blockPosition.Row < 0 || blockPosition.Row >= 3 || 
        blockPosition.Column < 0 || blockPosition.Column >= 3)
    {
        throw new ArgumentOutOfRangeException(nameof(blockPosition), 
            $"ブロック位置が有効範囲外です。有効範囲: (0,0)-(2,2)、指定値: ({blockPosition.Row},{blockPosition.Column})");
    }
}
```

## テストケース

### PlayModeテスト

#### PV-001: パズルTransform基本操作
**検証内容**: パズル単位でのTransform操作確認
- SetPuzzleTransform()での一括設定
- UpdatePuzzlePosition/Rotation/Scale()での成分別更新
- 設定値の正確な適用と取得

```csharp
[UnityTest]
public IEnumerator SetPuzzleTransform_UpdatesAllComponents()
{
    // Arrange
    var puzzleState = CreateTestPuzzleState();
    var position = new Vector3(10, 5, 0);
    var rotation = Quaternion.Euler(0, 45, 0);
    var scale = Vector3.one * 2f;
    
    // Act
    _visualizer.SetPuzzleTransform(puzzleState, position, rotation, scale);
    yield return null; // 1フレーム待機
    
    // Assert
    Assert.AreEqual(position, _visualizer.GetPuzzlePosition(puzzleState));
    Assert.AreEqual(rotation, _visualizer.GetPuzzleRotation(puzzleState));
    Assert.AreEqual(scale, _visualizer.GetPuzzleScale(puzzleState));
}
```

#### PV-005: パズル相対操作
**検証内容**: 現在値からの相対的なTransform変更確認
- MovePuzzle()での位置オフセット適用
- RotatePuzzle()での回転加算
- ScalePuzzle()での均等・非均等スケール適用

```csharp
[UnityTest]
public IEnumerator MovePuzzle_AppliesOffset()
{
    // Arrange
    var puzzleState = CreateTestPuzzleState();
    var initialPosition = new Vector3(5, 0, 0);
    var offset = new Vector3(10, 5, 3);
    _visualizer.UpdatePuzzlePosition(puzzleState, initialPosition);
    
    // Act
    _visualizer.MovePuzzle(puzzleState, offset);
    yield return null;
    
    // Assert
    Vector3 expectedPosition = initialPosition + offset;
    Assert.AreEqual(expectedPosition, _visualizer.GetPuzzlePosition(puzzleState));
}
```

#### PV-010: パズルインスタンス管理
**検証内容**: パズルインスタンスの追加・削除操作確認
- AddSinglePuzzleInstance()での正確な追加
- RemovePuzzleInstance()での完全な削除
- インデックス管理の整合性維持

```csharp
[UnityTest]
public IEnumerator AddSinglePuzzleInstance_AddsCorrectly()
{
    // Arrange
    var newPuzzleState = CreateTestPuzzleState();
    var position = new Vector3(20, 0, 0);
    
    // Act
    _visualizer.AddSinglePuzzleInstance(newPuzzleState, position);
    yield return null;
    
    // Assert
    Assert.AreEqual(position, _visualizer.GetPuzzlePosition(newPuzzleState));
    // レンダリングインデックスの整合性確認
}
```

#### PV-015: ブロック個別操作
**検証内容**: 個別ブロックのTransform操作確認
- UpdateBlockMatrix()での対象ブロックのみの変更
- GetBlockMatrix()での個別Transform取得
- パズル全体Transformと個別Transformの正確な合成

```csharp
[UnityTest]
public IEnumerator RotateBlock_OnlyAffectsTargetBlock()
{
    // Arrange
    var puzzleState = CreateTestPuzzleState();
    var targetBlock = new BlockPosition(1, 1);
    var otherBlock = new BlockPosition(0, 0);
    var rotation = Quaternion.Euler(0, 0, 45);
    
    var originalOtherMatrix = _visualizer.GetBlockMatrix(puzzleState, otherBlock);
    
    // Act
    _visualizer.RotateBlock(puzzleState, targetBlock, rotation);
    yield return null;
    
    // Assert
    Assert.AreEqual(originalOtherMatrix, _visualizer.GetBlockMatrix(puzzleState, otherBlock));
    // ターゲットブロックの回転が適用されていることを確認
}
```

#### PV-020: ユーティリティ操作
**検証内容**: Transform操作の便利機能確認
- ResetTransform()でのIdentity状態復元
- CopyTransform()での正確なTransform複製
- ApplyUniformScale/ApplyGlobalRotation()での全体操作

```csharp
[UnityTest]
public IEnumerator CopyTransform_CopiesAccurately()
{
    // Arrange
    var sourcePuzzle = CreateTestPuzzleState();
    var targetPuzzle = CreateTestPuzzleState();
    var sourceMatrix = Matrix4x4.TRS(Vector3.one, Quaternion.Euler(45, 90, 135), Vector3.one * 2f);
    _visualizer.SetPuzzleMatrix(sourcePuzzle, sourceMatrix);
    
    // Act
    _visualizer.CopyTransform(sourcePuzzle, targetPuzzle);
    yield return null;
    
    // Assert
    Assert.AreEqual(_visualizer.GetPuzzleMatrix(sourcePuzzle), 
                   _visualizer.GetPuzzleMatrix(targetPuzzle));
}
```

#### PV-025: 例外処理
**検証内容**: 無効な操作に対する適切な例外発生確認
- 存在しないPuzzleStateでのArgumentException
- 無効なBlockPositionでのArgumentOutOfRangeException

```csharp
[Test]
public void UpdatePuzzlePosition_NonExistentPuzzle_ThrowsException()
{
    // Arrange
    var nonExistentPuzzle = CreateTestPuzzleState(); // 未登録
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() => 
        _visualizer.UpdatePuzzlePosition(nonExistentPuzzle, Vector3.zero));
}
```

### エッジレンダリング統合テスト

#### ER-001: エッジMatrix4x4計算・更新
**検証内容**: パズル間エッジの計算と更新動作確認
- CalculateEdgeMatrix()での位置・回転・スケール計算
- パズルTransform変更後のエッジ追従更新

```csharp
[UnityTest]
public IEnumerator UpdatePuzzlePosition_UpdatesConnectedEdges()
{
    // Arrange
    var puzzle1 = CreateTestPuzzleState();
    var puzzle2 = CreateTestPuzzleState();
    
    // 隣接関係を設定
    SetupAdjacentPuzzles(puzzle1, puzzle2);
    
    // Act
    _visualizer.UpdatePuzzlePosition(puzzle1, new Vector3(10, 0, 0));
    yield return null;
    
    // Assert
    // エッジの位置と回転が更新されていることを確認
}
```

### 統合テスト

#### INT-001: 大量パズル操作テスト
**検証内容**: 複数パズルの同時Transform操作確認

```csharp
[UnityTest]
public IEnumerator MultiPuzzleTransform_HandlesLargeDataset()
{
    // 100個のパズルに対する同時Transform操作テスト
}
```

---

この仕様書に基づき、`PuzzleVisualizer`のパズルドメイン特化の高レベルTransform操作機能を実現し、直感的で効率的なパズル可視化システムを構築する。