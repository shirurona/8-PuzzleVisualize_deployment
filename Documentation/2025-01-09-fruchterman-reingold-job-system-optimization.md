# FruchtermanReingoldVisualizer Job System 最適化仕様書

## 動機

8-PuzzleVisualize プロジェクトにおける FruchtermanReingoldVisualizer.cs は、Barnes-Hut 法を使用したグラフ可視化アルゴリズムを実装しています。しかし、現在の実装は単一スレッドでの逐次処理のため、大規模な状態空間（数千のパズル状態）を扱う際にパフォーマンスのボトルネックとなっています。

### 現在の問題点
- **単一スレッド処理**: 斥力計算と引力計算が逐次実行され、CPUリソースが効率的に活用されていない
- **メモリ効率**: Dictionary や List ベースのデータ構造により、メモリアクセスパターンが最適化されていない
- **スケーラビリティ**: ノード数の増加に対して処理時間が大幅に増加

### パフォーマンス改善の必要性
- 全 8-puzzle 状態空間（約 181,440 状態）の可視化
- リアルタイムでの状態空間探索可視化
- 複数のアルゴリズムの並列比較

## 仕様

### 基本要件
Unity Job System と Burst コンパイラを活用して、FruchtermanReingoldVisualizer の以下の処理を並列化する：

1. **Barnes-Hut 法による斥力計算**
2. **隣接関係に基づく引力計算**
3. **位置更新処理**

### 機能要件

#### 1. 並列処理対応
- `IJobParallelFor` を使用した並列処理の実装
- 各計算段階（斥力、引力、位置更新）の独立した Job 実装
- Job 間の依存関係管理

#### 2. データ構造最適化
- `NativeArray<float3>` を使用した位置データの効率的な格納
- `NativeArray<float3>` を使用した力データの管理
- PuzzleState と int インデックスの効率的なマッピング

#### 3. メモリ管理
- `IDisposable` パターンによる NativeArray の適切な解放
- `JobHandle` による非同期処理の制御
- メモリリークの防止

#### 4. パフォーマンス最適化
- `[BurstCompile]` 属性による高速化
- `Unity.Mathematics` の使用による SIMD 最適化
- キャッシュ効率の向上

### 非機能要件

#### パフォーマンス
- **目標**: 現在の実装に対して 3-5 倍のパフォーマンス向上
- **スケーラビリティ**: 10,000 ノード以上での安定した処理
- **メモリ使用量**: 現在の実装と同等またはそれ以下

#### 互換性
- 既存の IVisualizeStrategy インターフェースとの互換性維持
- 現在の FruchtermanReingoldVisualizer との機能的同等性
- Unity 6000.0.26f1 での動作保証

## 設計

### 設計制約

#### 既存システムの保護
本最適化では、既存のクラス構造とシステムアーキテクチャを最大限保護します：

**🔒 変更禁止対象（保護すべきシステム）**
- **探索アルゴリズム**: `ISearchAlgorithm`、`DepthFirstSearch`、`BreadthFirstSearch`、`CompleteSpaceExplorer`
- **ゲームシステム**: `PuzzleGame`、`PuzzleView`、`PuzzleCreator`
- **パズル核心システム**: `Puzzle`、`PuzzleState`、`PuzzleNodeData`
- **ブロック管理**: `BlockNumber`、`BlockPosition`、`BlockView`
- **コマンドシステム**: `ICommand`、`MoveCommand`、`InvokeCommand`
- **レンダリングシステム**: `InstancedMeshRenderer`、`InstancedMeshInfo`、`PuzzleVisualizer`

**✅ 変更可能対象**
- **FruchtermanReingoldVisualizer** とその関連クラス
- **IVisualizeStrategy** インターフェース実装（新規実装のみ）
- **新規データクラス** の作成

#### 実装方針
1. **新規クラス作成**: 必要なデータクラスや補助クラスは新規作成
2. **インターフェース準拠**: 既存の `IVisualizeStrategy` インターフェースを実装
3. **非侵入的統合**: 既存システムへの影響を最小限に抑制
4. **後方互換性**: 既存の FruchtermanReingoldVisualizer も保持

### アーキテクチャ概要

```
JobSystemOptimizedFruchtermanReingoldVisualizer
├── データ変換層 (PuzzleState ↔ int mapping)
├── Job 実行層 (IJobParallelFor implementations)
├── メモリ管理層 (NativeArray management)
└── 統合層 (IVisualizeStrategy implementation)
```

### 主要コンポーネント

#### 1. データ構造

```csharp
// メインデータ構造
private NativeArray<float3> positions;      // ノード位置
private NativeArray<float3> forces;         // 計算された力
private NativeArray<int> adjacencyData;     // 隣接関係データ
private NativeArray<int> adjacencyOffsets;  // 隣接関係オフセット

// マッピング システム
private Dictionary<PuzzleState, int> stateToIndex;
private List<PuzzleState> indexToState;
```

#### 2. Job 実装

##### A. 斥力計算 Job
```csharp
[BurstCompile]
public struct RepulsiveForcesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> positions;
    [ReadOnly] public float k;
    [ReadOnly] public float theta;
    [NativeDisableParallelForRestriction] 
    public NativeArray<float3> forces;
    
    public void Execute(int index)
    {
        // Barnes-Hut 法による斥力計算
        // OctTree を使用した効率的な近似計算
    }
}
```

##### B. 引力計算 Job
```csharp
[BurstCompile]
public struct AttractiveForcesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> positions;
    [ReadOnly] public NativeArray<int> adjacencyData;
    [ReadOnly] public NativeArray<int> adjacencyOffsets;
    [ReadOnly] public float k;
    [NativeDisableParallelForRestriction] 
    public NativeArray<float3> forces;
    
    public void Execute(int index)
    {
        // 隣接ノード間の引力計算
        // バネモデルによる力の算出
    }
}
```

##### C. 位置更新 Job
```csharp
[BurstCompile]
public struct UpdatePositionsJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> forces;
    [ReadOnly] public float temperature;
    public NativeArray<float3> positions;
    
    public void Execute(int index)
    {
        // 温度に基づく位置更新
        // 収束判定とクランプ処理
    }
}
```

#### 3. OctTree 最適化

```csharp
// Native-friendly な OctTree 実装
public struct NativeOctreeNode
{
    public float3 centerOfMass;
    public float totalMass;
    public float3 boundsMin;
    public float3 boundsMax;
    public int childIndex;     // 子ノードのインデックス (-1 if leaf)
    public int nodeIndex;      // ノードのインデックス (-1 if internal)
}

private NativeArray<NativeOctreeNode> octreeNodes;
```

#### 4. 実行フロー

```csharp
public Dictionary<PuzzleState, Vector3> VisualizeSearchSpace(
    Dictionary<PuzzleState, PuzzleNodeData> searchDataMap, 
    Puzzle initialPuzzle)
{
    // 1. データ変換
    ConvertToNativeArrays(searchDataMap);
    
    // 2. 繰り返し処理
    for (int iteration = 0; iteration < maxIterations; iteration++)
    {
        // 3. Job スケジューリング
        JobHandle repulsiveHandle = new RepulsiveForcesJob(...).Schedule(nodeCount, 64);
        JobHandle attractiveHandle = new AttractiveForcesJob(...).Schedule(nodeCount, 64, repulsiveHandle);
        JobHandle updateHandle = new UpdatePositionsJob(...).Schedule(nodeCount, 64, attractiveHandle);
        
        // 4. 完了待機
        updateHandle.Complete();
        
        // 5. 収束判定
        if (HasConverged()) break;
    }
    
    // 6. 結果変換
    return ConvertToUnityVectors();
}
```

### パッケージ依存関係

#### 必要なパッケージ
- `com.unity.burst: 1.8.18` (既存)
- `com.unity.collections: 2.5.1` (既存)
- `com.unity.mathematics: 1.3.2` (既存)

#### アセンブリ定義更新
```json
{
    "name": "Puzzle",
    "references": [
        "Unity.Mathematics",
        "Unity.Collections",
        "Unity.Burst"
    ],
    "allowUnsafeCode": true
}
```

### メモリ管理戦略

#### 1. ライフサイクル管理
```csharp
public class JobSystemOptimizedFruchtermanReingoldVisualizer : IDisposable
{
    private void InitializeNativeArrays(int nodeCount)
    {
        positions = new NativeArray<float3>(nodeCount, Allocator.Persistent);
        forces = new NativeArray<float3>(nodeCount, Allocator.Persistent);
        // ...
    }
    
    public void Dispose()
    {
        if (positions.IsCreated) positions.Dispose();
        if (forces.IsCreated) forces.Dispose();
        // ...
    }
}
```

#### 2. Job 調整
```csharp
private void ExecuteJobs()
{
    // バッチサイズの最適化
    int batchSize = Mathf.Max(1, nodeCount / (SystemInfo.processorCount * 2));
    
    // 非同期実行
    var repulsiveJob = new RepulsiveForcesJob(...);
    var repulsiveHandle = repulsiveJob.Schedule(nodeCount, batchSize);
    
    // 依存関係チェーン
    var attractiveJob = new AttractiveForcesJob(...);
    var attractiveHandle = attractiveJob.Schedule(nodeCount, batchSize, repulsiveHandle);
    
    // 完了待機
    attractiveHandle.Complete();
}
```

### パフォーマンス最適化

#### 1. Burst 最適化
- 数学関数の最適化: `math.distance()`, `math.normalize()` 使用
- ループの最適化: 配列境界チェックの削除
- SIMD 命令の活用: `float3` ベクトル演算

#### 2. キャッシュ効率
- 連続メモリアクセス: `NativeArray` の線形走査
- データ局所性: 関連データの近接配置
- プリフェッチ最適化: 予測可能なアクセスパターン

#### 3. 並列度調整
- バッチサイズの動的調整
- ワーカースレッド数の最適化
- False sharing の回避

### 実装段階

#### Phase 1: 基盤構築
1. アセンブリ定義の更新（既存システムに影響なし）
2. 新規データクラスの作成（`JobSystemOptimizedFruchtermanReingoldVisualizer`）
3. 基本的な NativeArray データ構造の実装
4. PuzzleState-Index マッピングシステム（新規ユーティリティクラス）

#### Phase 2: Job 実装
1. 位置更新 Job の実装（最も単純）
2. 引力計算 Job の実装
3. 斥力計算 Job の実装（最も複雑）

#### Phase 3: 統合とテスト
1. 既存インターフェースとの統合
2. メモリ管理システムの実装
3. パフォーマンステストと調整

#### Phase 4: 最適化
1. Burst 最適化の適用
2. バッチサイズの調整
3. 最終的なパフォーマンス検証

### 期待効果

#### パフォーマンス改善
- **処理速度**: 3-5 倍の高速化
- **メモリ効率**: 20-30% の使用量削減
- **スケーラビリティ**: 10,000+ ノードでの安定動作

#### 開発効率
- **保守性**: 明確なコンポーネント分離
- **テスト性**: 各 Job の独立テスト
- **拡張性**: 新しい力計算アルゴリズムの追加

### リスク評価

#### 技術リスク
- **複雑性**: Job System の学習コスト
- **デバッグ**: 並列処理のデバッグ困難
- **互換性**: Unity バージョン依存

#### 軽減策
- **段階的実装**: 小さな変更から始める
- **テスト戦略**: 単体テストの充実
- **フォールバック**: 従来実装の保持
- **システム保護**: 既存の探索アルゴリズムやゲームシステムへの影響を排除
- **分離実装**: 新規クラスでの実装により既存システムとの依存関係を最小化

## 結論

この仕様書に基づいて FruchtermanReingoldVisualizer を Job System と Burst で最適化することで、8-PuzzleVisualize プロジェクトの可視化パフォーマンスを大幅に向上させることができます。特に大規模な状態空間での処理能力向上により、より複雑な検索アルゴリズムの比較や、リアルタイムでの可視化が可能になります。

実装は段階的に進めることで、リスクを最小限に抑えながら確実にパフォーマンス改善を実現できます。