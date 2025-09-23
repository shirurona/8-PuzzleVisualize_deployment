# 8パズル自動解決機能 仕様書

## 要件

### 動機
現在の8パズルアプリケーションは手動プレイのみサポートしており、搭載されている探索アルゴリズム（BFS、DFS、CompleteSpaceExplorer）の教育的価値を十分に活用できていない。各アルゴリズムの動作パターンと効率性を視覚的に理解するため、パズルを自動で解く機能が必要である。

### 目的
- 様々な探索アルゴリズムを用いて8パズルを自動解決する機能の追加
- 現在のパズル状態から指定した目標状態まで、選択したアルゴリズムによる最適経路での自動操作
- アルゴリズムの動作過程を段階的に可視化し、教育的価値を向上させる
- 既存の3D可視化システムとの統合により、状態空間探索の理解を深める

### 背景
- 既存システムには`ISearchAlgorithm`インターフェースを実装した3つの探索アルゴリズムが存在
- `PuzzleGame.cs`による手動操作システムが確立済み
- `PuzzleFinderView.cs`による目標状態設定機能が利用可能
- コマンドパターンによるundo/redo機能とアニメーションシステムが構築済み

## 仕様

### 主要機能

#### 1. PuzzleGameAgent.cs の新規作成
新しい自動解決エージェントクラスを`Assets/Scripts/Game/PuzzleGameAgent.cs`に作成する。

#### 2. 自動解決機能
- **入力**: 現在のパズル状態、目標状態、選択された探索アルゴリズム（ISearchAlgorithm）
- **処理**: 指定アルゴリズムによる最適経路の計算と実行手順の生成
- **出力**: 段階的な自動操作実行

#### 3. 操作履歴管理
- 計算された移動手順をQueue構造に格納
- インスペクタから指定可能な実行間隔での順次操作
- 実行中の停止・キャンセル機能

#### 4. UIとの統合
- **アルゴリズム選択**: TMPドロップダウンでBFS、DFS、CompleteSpaceExplorerを選択
- **実行制御**: 実行ボタン（実行時はキャンセルボタンに変化）
- **設定**: インスペクタで実行間隔を設定（秒単位）

#### 5. モード制限
- **動作環境**: 2Dパズルゲームモードでのみ動作
- **3D連携**: 3D可視化の状態表示も自動更新

### 技術要件

#### 利用する既存システム
- **探索アルゴリズム**: `Scripts/Search/`内のISearchAlgorithm実装クラス
  - `DepthFirstSearch`: 深さ優先探索
  - `BreadthFirstSearch`: 幅優先探索  
  - `CompleteSpaceExplorer`: 完全空間探索
- **パズル操作**: `PuzzleGame.ExecuteMove()`メソッド
- **状態管理**: `Puzzle`クラスと`PuzzleState`構造体
- **目標状態**: `PuzzleFinderView.GetCurrentState()`

#### データ構造
- **移動手順キュー**: `Queue<Puzzle.MoveDirection>`
- **実行状態**: PuzzleGame内でboolフラグによる状態管理
- **参照管理**: SerializeFieldによる直接参照

#### 外部操作制御
- **手動操作制限**: 自動実行中は手動操作をreturnで無効化
- **状態同期**: 自動実行の停止により手動操作を再有効化

## 設計

### アーキテクチャ概要

#### クラス設計: PuzzleGameAgent
```csharp
public class PuzzleGameAgent : MonoBehaviour
{
    // インスペクタ設定
    [SerializeField] private float executionInterval = 1.0f;
    [SerializeField] private PuzzleGame puzzleGame;
    [SerializeField] private PuzzleFinderView puzzleFinderView;
    
    // 状態管理
    private Queue<Puzzle.MoveDirection> moveQueue;
    private bool isAutoSolving;
    
    // 主要メソッド
    public void StartAutoSolve(ISearchAlgorithm algorithm);
    public void StopAutoSolve();
    private async UniTaskVoid ExecuteMoveSequenceAsync();
    private List<Puzzle.MoveDirection> CalculateMovePath(PuzzleState currentState, PuzzleState goalState, ISearchAlgorithm algorithm);
}
```

### UniTask統合設計

#### 1. アニメーション待機システム
```csharp
// PuzzleGame.ExecuteMove()をUniTask対応に変更
public async UniTask ExecuteMove(Puzzle.MoveDirection direction)
{
    // 移動実行
    if (puzzle.TryMoveEmpty(direction))
    {
        // アニメーション実行と完了待機
        await puzzleView.AnimateMove();
        
        // 3D可視化更新
        UpdateVisualization();
    }
}
```

#### 2. 順次実行システム
```csharp
private async UniTaskVoid ExecuteMoveSequenceAsync()
{
    isAutoSolving = true;
    
    while (moveQueue.Count > 0 && isAutoSolving)
    {
        var direction = moveQueue.Dequeue();
        
        // 移動実行とアニメーション完了待機
        await puzzleGame.ExecuteMove(direction);
        
        // 指定間隔で待機
        await UniTask.Delay(TimeSpan.FromSeconds(executionInterval));
    }
    
    isAutoSolving = false;
}
```

### コンポーネント間の連携

#### 1. 探索アルゴリズムとの統合
```csharp
private List<Puzzle.MoveDirection> CalculateMovePath(PuzzleState currentState, PuzzleState goalState, ISearchAlgorithm algorithm)
{
    // 初期パズル作成
    var initialPuzzle = new Puzzle(currentState);
    
    // アルゴリズム実行
    bool found = algorithm.Search(initialPuzzle, goalState);
    if (!found) return new List<Puzzle.MoveDirection>();
    
    // 経路復元
    var dataMap = algorithm.GetSearchDataMap();
    var statePath = ReconstructPath(goalState, dataMap);
    
    // 移動方向に変換
    return ConvertToMoveDirections(statePath);
}
```

#### 2. 経路復元アルゴリズム
```csharp
private List<PuzzleState> ReconstructPath(PuzzleState goalState, Dictionary<PuzzleState, PuzzleNodeData> dataMap)
{
    List<PuzzleState> path = new List<PuzzleState>();
    PuzzleState? current = goalState;
    
    while (current.HasValue)
    {
        path.Add(current.Value);
        current = dataMap[current.Value].Parent;
    }
    
    path.Reverse(); // 初期状態→ゴール状態の順序に変更
    return path;
}
```

#### 3. 移動方向の特定
```csharp
private Puzzle.MoveDirection GetMoveDirection(PuzzleState from, PuzzleState to)
{
    BlockPosition emptyPosFrom = from.FindEmptyBlockPosition();
    BlockPosition emptyPosTo = to.FindEmptyBlockPosition();
    
    if (emptyPosTo.Row < emptyPosFrom.Row) return Puzzle.MoveDirection.Up;
    if (emptyPosTo.Row > emptyPosFrom.Row) return Puzzle.MoveDirection.Down;
    if (emptyPosTo.Column < emptyPosFrom.Column) return Puzzle.MoveDirection.Left;
    if (emptyPosTo.Column > emptyPosFrom.Column) return Puzzle.MoveDirection.Right;
    
    throw new InvalidOperationException("無効な状態変化です");
}
```

### 実行制御とライフサイクル

#### 1. 初期化フロー
- `PuzzleGameAgent.Start()`: 自動初期化、参照確認
- SerializeFieldによる依存コンポーネントの設定確認
- 2Dパズルモードでのみ有効化

#### 2. 実行フロー
1. **アルゴリズム選択**: TMPドロップダウンから選択
2. **目標状態取得**: `puzzleFinderView.GetCurrentState()`
3. **経路計算**: 選択アルゴリズムで最適経路を計算
4. **キュー構築**: 移動手順を`Queue<Puzzle.MoveDirection>`に格納
5. **順次実行**: `ExecuteMoveSequenceAsync()`で段階的実行
6. **完了処理**: キュー空時またはキャンセル時に終了

#### 3. 外部操作制御
```csharp
// PuzzleGame内での手動操作制限
public bool TryMoveEmpty(MoveDirection direction)
{
    // 自動実行中は手動操作を無効化
    if (isAutoSolving) return false;
    
    // 通常の移動処理
    return ExecuteMoveInternal(direction);
}
```

### 簡素化された設計方針

#### 除外した機能
- **進行状況表示**: 現在ステップ数/総ステップ数は不要
- **エラーハンドリング**: 解決不可能なケースは考慮せず
- **パフォーマンス最適化**: 特別な最適化は不要
- **デバッグ機能**: 専用のデバッグ支援機能は不要
- **ステップ実行モード**: 実装レベルでのステップ実行のみ

#### UI実装の委譲
- **配置とレイアウト**: ユーザーが後で実装
- **ドロップダウン設定**: ユーザーが選択肢を設定
- **ボタンイベント**: StartAutoSolve()とStopAutoSolve()の呼び出しのみ

### 実装順序

1. **PuzzleGameAgent.cs基本構造**: クラス作成とSerializeField設定
2. **UniTask対応**: PuzzleGame.ExecuteMove()をUniTask化
3. **経路計算ロジック**: アルゴリズム統合と経路復元実装
4. **順次実行システム**: ExecuteMoveSequenceAsync()実装
5. **外部操作制御**: 実行中フラグによる手動操作制限
6. **統合テスト**: 既存システムとの動作確認

## テストケース

### GetMoveDirection() メソッドのテスト

#### TC001: 上方向移動の検証
- **前提条件**: 初期状態で空きブロックが(1,1)、目標状態で空きブロックが(0,1)
- **実行**: GetMoveDirection(初期状態, 目標状態)を呼び出し
- **期待結果**: Puzzle.MoveDirection.Upを返すことを検証

#### TC003: 下方向移動の検証
- **前提条件**: 初期状態で空きブロックが(1,1)、目標状態で空きブロックが(2,1)
- **実行**: GetMoveDirection(初期状態, 目標状態)を呼び出し
- **期待結果**: Puzzle.MoveDirection.Downを返すことを検証

#### TC007: 左方向移動の検証
- **前提条件**: 初期状態で空きブロックが(1,1)、目標状態で空きブロックが(1,0)
- **実行**: GetMoveDirection(初期状態, 目標状態)を呼び出し
- **期待結果**: Puzzle.MoveDirection.Leftを返すことを検証

#### TC011: 右方向移動の検証
- **前提条件**: 初期状態で空きブロックが(1,1)、目標状態で空きブロックが(1,2)
- **実行**: GetMoveDirection(初期状態, 目標状態)を呼び出し
- **期待結果**: Puzzle.MoveDirection.Rightを返すことを検証

#### TC013: 無効な状態変化の例外検証
- **前提条件**: 初期状態で空きブロックが(0,0)、目標状態で空きブロックが(2,2)（隣接していない）
- **実行**: GetMoveDirection(初期状態, 目標状態)を呼び出し
- **期待結果**: InvalidOperationExceptionが発生することを検証

### ReconstructPath() メソッドのテスト

#### TC017: 正常な経路復元の検証
- **前提条件**: 有効なdataMapとgoalStateが与えられ、初期状態への親子関係が構築済み
- **実行**: ReconstructPath(goalState, dataMap)を呼び出し
- **期待結果**: 初期状態からゴール状態までの順序で並んだPuzzleStateリストを返すことを検証

#### TC019: 単一状態の経路復元検証
- **前提条件**: goalStateのParentがnullのdataMapが与えられる
- **実行**: ReconstructPath(goalState, dataMap)を呼び出し
- **期待結果**: goalStateのみを含む単一要素リストを返すことを検証

#### TC023: 複数ステップ経路の順序検証
- **前提条件**: 3ステップの経路を持つdataMap（初期→中間→ゴール）が与えられる
- **実行**: ReconstructPath(goalState, dataMap)を呼び出し
- **期待結果**: [初期状態, 中間状態, ゴール状態]の順序で並んだリストを返すことを検証

### CalculateMovePath() メソッドのテスト

#### TC029: BFSアルゴリズムによる経路計算検証
- **前提条件**: 解決可能なcurrentStateとgoalState、BreadthFirstSearchアルゴリズムが与えられる
- **実行**: CalculateMovePath(currentState, goalState, bfsAlgorithm)を呼び出し
- **期待結果**: 有効な移動方向リストを返し、リストの各要素がPuzzle.MoveDirectionの値であることを検証

#### TC031: DFSアルゴリズムによる経路計算検証
- **前提条件**: 解決可能なcurrentStateとgoalState、DepthFirstSearchアルゴリズムが与えられる
- **実行**: CalculateMovePath(currentState, goalState, dfsAlgorithm)を呼び出し
- **期待結果**: 有効な移動方向リストを返し、リストの各要素がPuzzle.MoveDirectionの値であることを検証

#### TC037: 解決不可能な場合の検証
- **前提条件**: アルゴリズムのSearch()がfalseを返すモックアルゴリズムが与えられる
- **実行**: CalculateMovePath(currentState, goalState, mockAlgorithm)を呼び出し
- **期待結果**: 空のList<Puzzle.MoveDirection>を返すことを検証

### StartAutoSolve() メソッドのテスト

#### TC041: 自動解決開始の状態変化検証
- **前提条件**: isAutoSolvingがfalse、有効なISearchAlgorithmが与えられる
- **実行**: StartAutoSolve(algorithm)を呼び出し
- **期待結果**: isAutoSolvingがtrueに変更され、moveQueueに移動手順が格納されることを検証

#### TC043: 実行中の重複呼び出し制御検証
- **前提条件**: isAutoSolvingがtrue（既に実行中）
- **実行**: StartAutoSolve(algorithm)を呼び出し
- **期待結果**: 新しい自動解決が開始されず、既存のキューが保持されることを検証

#### TC047: nullアルゴリズムの例外検証
- **前提条件**: nullのISearchAlgorithmが与えられる
- **実行**: StartAutoSolve(null)を呼び出し
- **期待結果**: ArgumentNullExceptionが発生することを検証

### StopAutoSolve() メソッドのテスト

#### TC051: 自動解決停止の状態変化検証
- **前提条件**: isAutoSolvingがtrue（実行中）
- **実行**: StopAutoSolve()を呼び出し
- **期待結果**: isAutoSolvingがfalseに変更され、moveQueueがクリアされることを検証

#### TC053: 非実行中の停止呼び出し検証
- **前提条件**: isAutoSolvingがfalse（実行中でない）
- **実行**: StopAutoSolve()を呼び出し
- **期待結果**: 例外が発生せず、状態が変更されないことを検証

### ExecuteMoveSequenceAsync() メソッドのテスト

#### TC059: 正常な順次実行完了検証
- **前提条件**: moveQueueに有効な移動手順が格納済み、executionInterval = 0.1f
- **実行**: ExecuteMoveSequenceAsync()を呼び出し
- **期待結果**: 全ての移動が順次実行され、完了時にisAutoSolvingがfalseになることを検証

#### TC061: 実行中キャンセルの検証
- **前提条件**: moveQueueに複数の移動手順が格納済み
- **実行**: ExecuteMoveSequenceAsync()実行中にStopAutoSolve()を呼び出し
- **期待結果**: 実行が中断され、残りの移動が実行されずにisAutoSolvingがfalseになることを検証

#### TC067: 空キューでの実行検証
- **前提条件**: 空のmoveQueue
- **実行**: ExecuteMoveSequenceAsync()を呼び出し
- **期待結果**: 即座に完了し、isAutoSolvingがfalseになることを検証

### 統合テスト

#### TC071: BFSアルゴリズム完全実行フローの検証
- **前提条件**: 初期状態、目標状態、BreadthFirstSearchアルゴリズムが準備済み
- **実行**: StartAutoSolve(bfs)を呼び出し、ExecuteMoveSequenceAsync()の完了まで待機
- **期待結果**: パズルが目標状態に到達し、isAutoSolvingがfalseになることを検証

#### TC073: DFSアルゴリズム完全実行フローの検証
- **前提条件**: 初期状態、目標状態、DepthFirstSearchアルゴリズムが準備済み
- **実行**: StartAutoSolve(dfs)を呼び出し、ExecuteMoveSequenceAsync()の完了まで待機
- **期待結果**: パズルが目標状態に到達し、isAutoSolvingがfalseになることを検証

#### TC079: 手動操作制限の統合検証
- **前提条件**: 自動解決実行中（isAutoSolving = true）
- **実行**: PuzzleGame.TryMoveEmpty()を手動で呼び出し
- **期待結果**: falseを返し、パズル状態が変更されないことを検証

---

**作成日**: 2025年1月29日  
**対象システム**: Unity 6000.0.26f1 8パズル可視化アプリケーション  
**関連ファイル**: `Assets/Scripts/Game/PuzzleGameAgent.cs`（新規作成予定）