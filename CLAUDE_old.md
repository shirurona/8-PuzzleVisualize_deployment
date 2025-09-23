# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Code of Conduct
This document outlines the basic code of conduct and rules for collaboration with users for AI coding agents (e.g., Cline, Cursor, Junie, etc.). As a software development partner, AI coding agent promotes honest, flexible, and iterative development.

# Basic Principles
Always check any unclear points in the requirements at the planning stage
If there are any ambiguities in the requirements, always check with the user before starting implementation
Never implement based on assumptions, but place importance on a common understanding with the user
Subdivide the functions to meet the requirements, make an implementation plan, and reach an agreement with the user
Proceed iteratively with each subdivided function
Share the deliverables of each iteration with the user to allow for flexible direction corrections
Always refer to the game engine, frameworks, and libraries' official API documentation

## Conversation Guidelines

- 常に日本語で可愛らしく会話する

## LLM操作ガイドライン

### 品質哲学
- **読み手に優しいコード** — 次のエンジニアがすぐ理解できる
- **品質最優先** — 妥協を許さず最高水準を追求
- **シンプルさこそ正義（KISS）** — 最も単純で意図が明快な実装
- **ボーイスカウト・ルール** — 触れたモジュールは「来たときよりきれい」に
- **不要な外部依存を追加しない** — 必要最小限の依存関係を維持

## そのほかの設計に関する方針

以下の書籍の内容を参考にする。
- リーダブルコード
- プリンシプル オブ プログラミング
- ドメイン駆動設計
- 達人プログラマー
- Clean Architechture
- C# 実践開発手法
- リファクタリング
- テスト駆動開発

## プロジェクト概要

このプロジェクトは8-puzzleゲームの可視化アプリケーションです。Unity 6000.0.26f1を使用し、C#で実装されています。プレイヤーは8-puzzleをプレイしながら、検索アルゴリズムによる状態空間の探索を3D空間で可視化できます。

## プロジェクトの機能詳細

### 主要な機能

このアプリケーションは、従来の8-puzzleゲームに**3D状態空間可視化**を組み合わせた革新的な教育ツールです。

#### 1. デュアルモードシステム
- **ゲームモード（右クリックで有効）**: 従来の8-puzzleゲームプレイ
- **探索モード（デフォルト）**: 3D空間を自由に移動して状態空間を探索

#### 2. インタラクティブなパズルゲーム
- **マルチ入力対応**: キーボード（WASD/矢印キー）、マウスクリック、ドラッグ＆ドロップ
- **完全なUndo/Redo機能**: コマンドパターンによる操作履歴管理
- **スムーズアニメーション**: ブロック移動の補間アニメーション
- **視覚的フィードバック**: リアルタイムの状態更新と数値表示

#### 3. 3D状態空間可視化
- **状態ノード表現**: 各パズル状態が3D空間の独立したノードとして表示
- **エッジ接続**: 隣接する状態間（1手で到達可能）の接続線を表示
- **経路ハイライト**: プレイヤーの移動経路を色分けして可視化
- **GPU Instancing**: 数千の状態を効率的に同時レンダリング

#### 4. 高度な検索アルゴリズム
- **幅優先探索（BFS）**: 最短経路保証、レベル別探索パターン
- **深さ優先探索（DFS）**: 深い探索から後戻り、異なる探索パターン
- **完全空間探索**: 到達可能な全状態空間の生成と双方向関係構築

#### 5. 洗練されたレイアウトアルゴリズム
- **階層的レイアウト**: 検索深度に基づく木構造配置
- **Fruchterman-Reingold可視化**: 物理ベースの力指向グラフレイアウト
- **Barnes-Hut最適化**: 大規模グラフのO(n log n)複雑度での処理
- **3D OctTree実装**: 空間的最適化による高速化

#### 6. 3Dナビゲーションシステム
- **一人称コントローラー**: WASD移動、マウスルック
- **複数移動モード**: 歩行、飛行、観察者モード
- **カメラ効果**: FOV変更、揺れ効果、カーソルロック/解除

#### 7. パズル検索・ナビゲーション
- **状態検索インターフェース**: 任意のパズル構成への瞬時移動
- **動的経路可視化**: プレイヤーの移動に応じたリアルタイムハイライト

### 教育的価値

このプロジェクトは、コンピュータサイエンス教育における以下の概念を視覚化します：
- **探索アルゴリズムの比較**: 異なる戦略の行動パターンを並列表示
- **状態空間理論**: 抽象的な概念を3D空間で具体化
- **アルゴリズム複雑度**: 視覚的な効率性比較
- **インタラクティブ学習**: ゲーミフィケーションによる理解促進

### ユーザーワークフロー

1. **開始**: 完全な8-puzzle状態空間が事前生成されて表示
2. **探索**: 一人称視点で3D可視化空間を自由に移動
3. **プレイ**: 右クリックでゲームモードに切り替えてパズル操作
4. **学習**: 各移動が状態空間内の移動にどう対応するかを観察
5. **検索**: 検索インターフェースで特定のパズル構成にジャンプ
6. **分析**: 異なる検索アルゴリズムのパターンと効率性を比較

### テストに関して
@`.claude/documents/testing.md` を参照

### アセンブリ管理
プロジェクトには以下のアセンブリ定義があります：
- `Assets/Scripts/Puzzle.asmdef` - コアパズルロジック
- `Assets/Scripts/Tests/Editor/EditModeTests.asmdef` - EditModeテストアセンブリ（NUnit使用）
- `Assets/Scripts/Tests/PlayModeTests.asmdef` - PlayModeテストアセンブリ（NUnit使用）

## 使用技術スタック

### Unity エンジン & バージョン
- **Unity 6000.0.26f1** (Unity 6.x系列)

### アルゴリズム & 数学ライブラリ
- **検索アルゴリズム**:
  - 深さ優先探索 (DFS)
  - 幅優先探索 (BFS)
  - 完全空間探索
- **グラフ可視化アルゴリズム**:
  - **Fruchterman-Reingold**: 力指向グラフレイアウト
  - **Barnes-Hut**: N体シミュレーション最適化
  - **Octree**: 3D空間分割
- **Unity.Mathematics**: SIMD最適化数学演算

### 設計パターン & アーキテクチャ
- **Assembly Definition**: `.asmdef` モジュラーコンパイル
- **Strategy Pattern**: `ISearchAlgorithm`, `IVisualizeStrategy`
- **Command Pattern**: `ICommand`, `MoveCommand`, `InvokeCommand`
- **Value Object Pattern**: `BlockNumber`, `BlockPosition`
- **GPU Instancing Pattern**: `InstancedMeshRenderer`

### 開発 & ビルドツール
- **Visual Studio/JetBrains Rider**: IDE統合
- **Unity Package Manager**: パッケージ依存関係管理
- **Git**: バージョン管理

### パフォーマンス最適化技術
- **GPU Instancing**: 大量オブジェクトの効率的レンダリング
- **Structured Buffers**: GPUメモリ最適化
- **Barnes-Hut最適化**: O(n log n) 力計算
- **Octree空間分割**: 3D空間最適化
- **Burst Compilation**: 高性能C#コンパイル

## コードベース概要

### 全体的な構造と組織

このUnity 8-puzzleコードベースは、論理的なディレクトリ構造と明確な責任分離で設計されています。

#### 主要コンポーネント間の関係

**データフロー**:
```
ユーザー入力 → PuzzleGame → Puzzle → PuzzleState
                ↓
            MoveCommand → InvokeCommand (undo/redoスタック)
                ↓
            PuzzleView (UI更新)

検索処理:
ISearchAlgorithm → PuzzleNodeData → SearchSpaceVisualizer → PuzzleVisualizer
                                                              ↓
                                                    InstancedMeshRenderer (GPU)
```

#### 採用されている設計パターン

1. **コマンドパターン**: `ICommand`インターフェースによる可逆操作
2. **ストラテジーパターン**: `ISearchAlgorithm`と`IVisualizeStrategy`
3. **値オブジェクト**: `BlockNumber`、`BlockPosition`、`PuzzleState`の不変構造体
4. **GPU Instancingパターン**: `InstancedMeshRenderer`による高性能描画

#### 型安全性と検証

- **ビットマスク検証**: パズル状態の整合性を構築時に保証
- **readonly構造体**: 不変データ型による意図しない変更の防止
- **範囲検証**: 位置とブロック番号の包括的な境界チェック

#### パフォーマンス最適化

- **GPU Instancing**: 数千のパズルインスタンスを効率的にレンダリング
- **Barnes-Hutアルゴリズム**: グラフレイアウトのO(n log n)力計算
- **辞書ベースの検索**: `GetHashCode()`を使用した高速状態比較
- **構造体ベース設計**: コアデータ型のヒープ割り当て最小化

#### 拡張性

- **新しい検索アルゴリズム**: `ISearchAlgorithm`インターフェース実装
- **カスタム可視化**: `IVisualizeStrategy`インターフェース実装
- **追加コマンド**: `ICommand`を拡張した新しい操作
- **異なるパズルサイズ**: `PuzzleState.GridSize`による設定可能

## アーキテクチャ

### アーキテクチャ概要

本プロジェクトは、**依存性注入（DI）**、**リアクティブプログラミング**、**MVP（Model-View-Presenter）パターン**を採用したモダンなUnityアーキテクチャを実装しています。

#### 採用技術スタック
- **VContainer**: 軽量で高性能な依存性注入コンテナ
- **R3**: 次世代Reactive Extensions for Unity
- **UniTask**: Unity向け非同期処理ライブラリ
- **MVP Pattern**: 責任分離とテスタビリティの向上

### コア構造

#### 1. 依存性注入システム (`Assets/Scripts/LifeTimeScope/`)
- **PuzzleLifetimeScope.cs**: VContainerによる依存性管理とコンポーネント登録
  - Puzzleインスタンスの生成と管理
  - 全コンポーネントの依存関係解決
  - シングルトンライフサイクル管理

#### 2. パズルコアシステム (`Assets/Scripts/Object/`)
- **Puzzle.cs**: 8-puzzleの状態を管理するメインドメインモデル
- **PuzzleState.cs**: 不変なパズル状態表現
- **BlockNumber.cs**: ブロック番号の型安全な管理
- **BlockPosition.cs**: ブロック位置の管理
- **BlockView.cs**: ブロックの表示制御

#### 3. パズル生成・可視化システム (`Assets/Scripts/Puzzle/`)
- **PuzzleCreator.cs**: パズルの生成と初期化
- **PuzzleVisualizer.cs**: 3D空間での可視化（GPU Instancing使用）
- **PuzzleNodeData.cs**: 探索アルゴリズムのノード情報

#### 4. 検索アルゴリズム (`Assets/Scripts/Search/`)
- **ISearchAlgorithm.cs**: 検索アルゴリズムの共通インターフェース
- **DepthFirstSearch.cs**: 深さ優先探索の実装
- **BreadthFirstSearch.cs**: 幅優先探索の実装
- **CompleteSpaceExplorer.cs**: 完全状態空間の探索

#### 5. グラフ可視化システム (`Assets/Scripts/Graph/`)
- **IVisualizeStrategy.cs**: 可視化戦略インターフェース
- **FruchtermanReingoldVisualizer.cs**: 力指向グラフレイアウト
- **JobSystemOptimizedFruchtermanReingoldVisualizer.cs**: Job System最適化版
- **SearchSpaceVisualizer.cs**: 検索空間の3D可視化

#### 6. ゲーム制御層（MVP Pattern実装）(`Assets/Scripts/Game/`)

##### Model層
- **Puzzle.cs**: ドメインロジックとビジネスルール
- **PuzzleState.cs**: 不変状態モデル

##### View層
- **PuzzleView.cs**: UI表示とユーザーインターフェース
- **PuzzleFinderView.cs**: 検索UI制御
- **VisualizeStateController.cs**: 状態可視化の制御

##### Presenter層
- **PuzzlePresenter.cs**: Model-View間の調整役
  - R3によるリアクティブな状態変更通知
  - PuzzleViewとPuzzleFollowCameraの同期
  - 依存性注入による疎結合設計

##### 専門化コンポーネント
- **PuzzleGame.cs**: ゲームフロー制御
- **PuzzleGameAgent.cs**: 自動解法エージェント（UniTask使用）
- **PuzzleDragger.cs**: ドラッグ&ドロップ操作
- **PuzzleGameBlockCreator.cs**: ゲームブロック生成
- **PuzzleBridge.cs**: コンポーネント間ブリッジ

#### 7. コマンドパターン (ルートディレクトリ)
- **ICommand.cs**: コマンドインターフェース
- **MoveCommand.cs**: 移動コマンドの実装
- **InvokeCommand.cs**: コマンドの実行・元に戻す機能

## 主要アーキテクチャパターン

### 1. 依存性注入パターン (Dependency Injection Pattern)

**実装**: `PuzzleLifetimeScope.cs` + VContainer

**目的**: コンポーネント間の疎結合化と依存関係の明確化

**構成要素**:
- **PuzzleLifetimeScope**: VContainerのLifetimeScopeを継承
- **[Inject]属性**: コンストラクタ/メソッドインジェクション
- **RegisterInstance/RegisterComponent**: 依存関係登録

**利点**:
- テスタビリティの向上
- 循環依存の回避
- 設定の集約化
- ランタイム依存関係解決

```csharp
// 依存性注入の使用例
[Inject]
public void Initialize(Puzzle puzzle) // Puzzleが自動注入
{
    _puzzle = puzzle;
}
```

### 2. MVPパターン (Model-View-Presenter Pattern)

**実装**: `PuzzlePresenter.cs` を中心とした責任分離

**構成要素**:
- **Model (Puzzle/PuzzleState)**: ビジネスロジックとデータ
- **View (PuzzleView)**: UI表示とユーザーインタラクション
- **Presenter (PuzzlePresenter)**: Model-View間の調整

**特徴**:
- R3によるリアクティブな状態伝播
- 依存性注入による疎結合
- 単方向データフロー

**利点**:
- テスト可能な分離設計
- UIロジックとビジネスロジックの分離
- 状態変更の自動伝播

```csharp
// MVPパターンの実装例
public class PuzzlePresenter : MonoBehaviour
{
    [Inject]
    public void Initialize(Puzzle puzzle)
    {
        puzzle.State
            .Pairwise()
            .Subscribe(x => puzzleView.AnimateMove(x.Current, x.Previous))
            .AddTo(this);
    }
}
```

### 3. リアクティブプログラミングパターン

**実装**: R3 (Reactive Extensions) による状態管理

**主要概念**:
- **Observable**: 状態変更の通知ストリーム
- **Subject**: 手動制御可能なObservable
- **Pairwise**: 前後の値ペア取得
- **Subscribe**: 状態変更の購読

**利点**:
- 宣言的な状態変更処理
- 自動的なイベント伝播
- メモリリーク防止（AddTo使用）
- 複雑な状態変更の簡潔な表現

```csharp
// リアクティブプログラミングの例
_subject
    .Where(x => x.HasValue)
    .Select(x => x.Value)
    .Where(_ => !PuzzleGameAgent.IsAutoSolving)
    .Subscribe(x => puzzle.TryMoveEmpty(x))
    .AddTo(this);
```

### 4. 非同期処理パターン

**実装**: UniTask による高性能非同期処理

**特徴**:
- **async/await**: 直感的な非同期コード
- **UniTaskVoid**: Fire-and-forget非同期メソッド
- **UniTask.Delay**: フレームレート非依存の待機
- **CancellationToken**: 適切な非同期処理キャンセル

**利点**:
- UIブロッキングなし
- メモリ効率的な非同期処理
- 例外安全な非同期操作
- 状態管理との統合

```csharp
// UniTaskによる非同期処理の例
public async UniTaskVoid ExecuteMoveSequenceAsync()
{
    IsAutoSolving = true;
    while (moveQueue.Count > 0 && IsAutoSolving)
    {
        var direction = moveQueue.Dequeue();
        _puzzle.TryMoveEmpty(direction);
        await UniTask.Delay(TimeSpan.FromSeconds(executionInterval));
    }
    IsAutoSolving = false;
}
```

### 5. コマンドパターン (Command Pattern)

**実装**: `ICommand.cs`, `MoveCommand.cs`, `InvokeCommand.cs`

このパターンは8-puzzleの移動操作とundo/redo機能を実現しています。

**構成要素**:
- **`ICommand`**: Execute()、Undo()、GetBoardState()を定義
- **`MoveCommand`**: 具体的なパズル移動コマンド
- **`InvokeCommand`**: コマンド履歴管理（undo/redoスタック）

**利点**:
- 完全なundo/redo機能
- 移動実行と移動ロジックの分離
- 訪問状態の追跡による可視化対応
- ゲームリプレイ機能のサポート

```csharp
// コマンドパターンの使用例
var moveCommand = new MoveCommand(puzzle, blockPosition);
invokeCommand.ExecuteCommand(moveCommand); // 実行
invokeCommand.UndoCommand(); // 元に戻す
```

### 2. ストラテジーパターン (Strategy Pattern)

#### 2.1 検索アルゴリズム戦略

**実装**: `ISearchAlgorithm.cs`とその実装クラス群

**構成要素**:
- **`ISearchAlgorithm`**: 全検索アルゴリズムの共通インターフェース
- **`DepthFirstSearch`**: 深さ優先探索実装
- **`BreadthFirstSearch`**: 幅優先探索実装
- **`CompleteSpaceExplorer`**: 完全状態空間探索

**利点**:
- 新しい検索アルゴリズムの簡単な追加
- ランタイムでのアルゴリズム切り替え
- 可視化用データ構造の統一
- プラガブルなアルゴリズムアーキテクチャ

#### 2.2 可視化戦略

**実装**: `IVisualizeStrategy.cs`

**構成要素**:
- **`IVisualizeStrategy`**: 可視化戦略のインターフェース
- **`FruchtermanReingoldVisualizer`**: Barnes-Hut最適化付き高度グラフレイアウト

**利点**:
- プラガブル可視化アルゴリズム
- 3D空間レイアウト最適化
- 異なるグラフレイアウトアプローチのサポート

### 3. 値オブジェクトパターン (Value Object Pattern)

**実装**: `BlockNumber.cs`, `BlockPosition.cs`

このパターンは型安全で不変なパズルコンポーネントを提供します。

**特徴**:
- **`readonly struct`**: 不変性の保証
- **`IEquatable<T>`**: 効率的な比較
- **入力検証**: 詳細なエラーメッセージ付き
- **暗黙的変換**: 適切な場所での利便性向上

**利点**:
- 無効な状態の作成防止
- 型安全性（位置と番号の混同防止）
- 不変性によるスレッドセーフ
- デバッグ用の明確なエラーメッセージ

```csharp
// 値オブジェクトの使用例
var blockNumber = new BlockNumber(5); // 検証付き
var position = new BlockPosition(1, 2); // 不変
// var invalid = new BlockNumber(10); // 例外が発生
```

### 4. 不変状態パターン (Immutable State Pattern)

**実装**: `PuzzleState.cs`

**特徴**:
- **不変`readonly struct`**: スレッドセーフな状態管理
- **ビットマスク検証**: 包括的な状態検証
- **効率的な状態比較**: アルゴリズム用最適化
- **有効状態のみ許可**: 構築時検証

**利点**:
- スレッドセーフな状態管理
- 検索アルゴリズム用効率的な辞書キー
- 無効なパズル構成の防止
- デバッグ用明確なエラーメッセージ

### 5. GPU Instancingパターン

**実装**: `InstancedMeshRenderer.cs`

**目的**: 数千のパズル状態の効率的レンダリング

**特徴**:
- **`GraphicsBuffer`**: マトリクスデータ用
- **`Graphics.RenderMeshPrimitives`**: バッチレンダリング
- **`IDisposable`**: 適切なリソース解放

**利点**:
- 大規模状態空間可視化の処理
- GPU加速レンダリング
- 大データセット用メモリ効率

### 6. アーキテクチャ原則

#### 6.1 SOLID原則の適用

**単一責任原則 (SRP)**:
- 各クラスが明確な単一目的を持つ
- ゲームロジック、可視化、アルゴリズムの関心事分離

**開放閉鎖原則 (OCP)**:
- `ISearchAlgorithm`による新アルゴリズム追加
- `IVisualizeStrategy`による新可視化戦略
- 拡張可能なコマンドシステム

**リスコフ置換原則 (LSP)**:
- 全検索アルゴリズムが相互交換可能
- 値オブジェクトの一貫した動作

**インターフェース分離原則 (ISP)**:
- 焦点を絞ったインターフェース設計
- 未使用メソッドへの強制依存なし

**依存性逆転原則 (DIP)**:
- 高レベルモジュールが抽象に依存
- 具象実装への直接依存回避

### 6. 現代的なアーキテクチャの特徴

#### 6.1 関心事の分離と層化

**依存性注入層**: コンポーネント間の依存関係管理 (PuzzleLifetimeScope)
**ドメイン層**: 純粋なパズルロジック (Puzzle, PuzzleState)
**アプリケーション層**: ビジネスロジックとユースケース (PuzzleGameAgent, PuzzleDragger)
**プレゼンテーション層**: MVPパターンによる責任分離 (PuzzlePresenter, PuzzleView)
**インフラ層**: Unity固有機能とGPU最適化 (GPU Instancing, レンダリング)

#### 6.2 リアクティブアーキテクチャ

**単方向データフロー**: Model → Presenter → View
**状態変更伝達**: R3によるリアクティブストリーム
**イベントドリブン**: 状態変更に基づく自動更新
**メモリ安全性**: AddTo()による自動リソース管理

#### 6.3 継承より合成と依存性注入

**合成パターン**:
- PuzzleがPuzzleStateとInvokeCommandを含有
- PuzzlePresenterが複数Viewを統合管理
- PuzzleGameAgentが検索アルゴリズムを合成

**依存性注入**:
- コンストラクタインジェクション
- メソッドインジェクション ([Inject]属性)
- ライフサイクル管理されたシングルトン

### 7. モダンなデータフローとアーキテクチャ

#### 7.1 依存性注入アーキテクチャ
```
PuzzleLifetimeScope (コンテナルート)
    ├── Puzzle (モデルシングルトン)
    ├── PuzzlePresenter (MVP Presenter)
    ├── PuzzleGame (ゲームコントローラー)
    ├── PuzzleGameAgent (自動解法エージェント)
    ├── PuzzleDragger (ドラッグハンドラー)
    ├── PuzzleBridge (コンポーネントブリッジ)
    └── PuzzleGameBlockCreator (ブロック生成管理)
```

#### 7.2 リアクティブデータフロー

**MVPパターンフロー**:
1. **ユーザー入力** → View (PuzzleView/PuzzleDragger)
2. **入力処理** → Model (Puzzle) → 状態変更
3. **状態通知** → Presenter (PuzzlePresenter) → R3ストリーム
4. **View更新** → PuzzleView.AnimateMove() + PuzzleFollowCamera

**依存性注入フロー**:
1. **コンテナ初期化** → PuzzleLifetimeScope.Configure()
2. **依存関係登録** → RegisterInstance/RegisterComponent
3. **ランタイム解決** → [Inject]メソッド呼び出し
4. **コンポーネント初期化** → Initialize()メソッド実行

**非同期処理フロー**:
1. **自動解法開始** → PuzzleGameAgent.StartAutoSolve()
2. **経路計算** → ISearchAlgorithm.Search()
3. **移動キュー** → Queue<MoveDirection>
4. **逐次実行** → UniTask.Delay() + TryMoveEmpty()

#### 7.3 リアクティブストリームフロー
```
Puzzle.State (Observable<PuzzleState>)
    ↓
.Pairwise() → (Previous, Current)
    ↓
.Subscribe() → PuzzlePresenter
    ↓
├── PuzzleView.AnimateMove()
└── PuzzleFollowCamera.FollowToPuzzleState()
```

### 8. 拡張性設計

#### 8.1 容易な拡張ポイント
1. **新検索アルゴリズム**: `ISearchAlgorithm`実装
2. **新可視化戦略**: `IVisualizeStrategy`実装
3. **新コマンド**: `ICommand`実装
4. **新パズルサイズ**: `PuzzleState.GridSize`変更

#### 8.2 モダンアーキテクチャの強み
- **保守性**: 明確な関心事分離と依存性注入による疎結合
- **テスタビリティ**: MVPパターンとDIによるモック化容易な設計
- **パフォーマンス**: GPU最適化とUniTaskによる非同期処理
- **拡張性**: インターフェースベースのプラグインアーキテクチャ
- **ユーザビリティ**: リアクティブUIと非同期処理によるスムーズな体験
- **コード品質**: 宣言的プログラミングと型安全性
- **メモリ安全性**: 自動リソース管理とメモリリーク防止

### モダンアーキテクチャの主要パターン

1. **依存性注入**: VContainerによる疎結合設計
2. **MVPパターン**: 責任分離とテスタビリティ向上
3. **リアクティブプログラミング**: R3による宣言的状態管理
4. **非同期パターン**: UniTaskによる高性能非同期処理
5. **コマンドパターン**: 移動操作のundo/redo機能
6. **ストラテジーパターン**: 複数の検索アルゴリズム
7. **値オブジェクト**: BlockNumber、BlockPositionによる型安全性
8. **GPU Instancing**: 大量のパズル状態を効率的に描画

### 入力システム

- **キーボード**: WASD/矢印キーでの移動
- **マウス**: ブロックのクリック・ドラッグ操作
- **トグル**: 逆方向操作の切り替え

### 可視化システム

- **InstancedMeshRenderer.cs**: GPU Instancingによる効率的な描画
- **InstancedMeshInfo.cs**: メッシュとマテリアル情報の管理
- **FruchtermanReingoldVisualizer.cs**: グラフレイアウトアルゴリズム

## 開発のベストプラクティス

### コーディング規約
- 日本語コメントを適切に使用
- 例外処理でのArgumentNullException、ArgumentOutOfRangeException使用
- readonly structの活用（BlockNumber、BlockPosition）
- IEquatable<T>、ICloneable実装による型安全性

### パフォーマンス
- GPU Instancingによる大量オブジェクト描画
- Dictionary<PuzzleState, T>によるパズル状態の高速検索
- ビットマスクによるブロック番号の効率的な検証

### メモリ管理
- IDisposableパターンの実装（InstancedMeshRenderer）
- OnDestroy()でのリソース解放

## 重要な実装詳細

### パズル状態の管理
```csharp
// PuzzleStateは不変な値オブジェクトとして設計
public readonly struct PuzzleState : IEquatable<PuzzleState>
{
    private readonly BlockNumber[,] _blockNumbers;
    // ビットマスクによるブロック番号の重複検証
    // 隣接するブロックのみの移動を許可
}

// PuzzleはPuzzleStateをラップして操作を提供
public class Puzzle
{
    private PuzzleState _state;
    public PuzzleState State => _state;
}
```

### 検索アルゴリズムの実装
```csharp
// 共通インターフェースにより複数のアルゴリズムを統一的に扱う
public interface ISearchAlgorithm
{
    bool Search(Puzzle initialPuzzle, PuzzleState goalPuzzle);
    Dictionary<PuzzleState, PuzzleNodeData> GetSearchDataMap();
}
```

### GPU Instancingによる可視化
```csharp
// 大量のパズル状態を効率的に描画
public class InstancedMeshRenderer : IDisposable
{
    private ComputeBuffer _matrixBuffer;
    // Graphics.DrawMeshInstanced使用
}
```

## エントリーポイントと初期化フロー

### メインエントリーポイント

#### 1. メインシーン
- **ファイル**: `Assets/Scenes/Main.unity`
- アプリケーション起動時に読み込まれる主要Unityシーン

#### 2. 主要コントローラースクリプト

**A. VisualizeStateController** - メインアプリケーションコントローラー
- **ファイル**: `Assets/Scripts/Game/VisualizeStateController.cs`
- **役割**: アプリケーション状態の切り替え管理
- **機能**:
  - 3D探索モードと2Dパズルゲームモード間の切り替え
  - カーソル状態とコンポーネント有効化の管理
  - 右クリックによるモード切り替え処理
  - **Update()**: モード切り替え用入力の連続監視

**B. PuzzleCreator** - メイン検索空間初期化
- **ファイル**: `Assets/Scripts/Puzzle/PuzzleCreator.cs`
- **役割**: コア初期化と検索空間生成
- **Start()メソッドによる主要ブートストラップ**:
  - 初期パズル状態の作成
  - CompleteSpaceExplorerによる完全検索空間生成
  - SearchSpaceVisualizerによる3D位置決め初期化
  - PuzzleVisualizerによるGPUインスタンス描画設定
  - 可視化用全マトリクスデータの適用

**C. PuzzleGame** - 2Dパズルゲームコントローラー
- **ファイル**: `Assets/Scripts/Game/PuzzleGame.cs`
- **役割**: インタラクティブパズルゲームロジック
- **Awake()**: 初期パズル状態とUIイベントハンドラーの設定
- **Update()**: WASD/矢印キー入力によるパズル移動処理

**D. PlayerController** - 3Dナビゲーションコントローラー
- **ファイル**: `Assets/Scripts/PlayerController.cs`
- **役割**: 一人称3D移動とカメラ制御
- **Start()**: カーソルロックとカメラ初期位置設定
- **Update()**: マウスルック、移動、状態遷移処理

### 初期化シーケンスとフロー

#### ブートストラップ順序:
1. **Unityシーン読み込み**: Main.unityが全GameObjectと共に読み込み
2. **Awake()フェーズ**: 
   - PuzzleGame.Awake() - 初期パズルとUIの設定
   - PuzzleVisualizer.Awake() - GPUインスタンシング初期化
   - BlockView.Awake() - 個別ブロックコンポーネント設定
3. **Start()フェーズ**:
   - **PuzzleCreator.Start()** - **メイン初期化**:
     - 初期パズル構成の作成
     - CompleteSpaceExplorerによる完全検索空間生成
     - SearchSpaceVisualizerによる全状態の3D配置
     - 可視化用GPUインスタンスデータの準備
   - PlayerController.Start() - 3Dナビゲーション設定
   - PuzzleView.Start() - 2D UIコンポーネント設定
4. **Update()フェーズ**: 全コントローラーの更新ループ実行

#### コンポーネント依存関係:
```
VisualizeStateController (ルート)
├── PlayerController (3Dナビゲーション)
├── PuzzleGame (2Dパズル)
│   └── PuzzleView (UI表示)
│       └── BlockView[] (個別ブロック)
└── PuzzleCreator (検索空間)
    ├── CompleteSpaceExplorer (アルゴリズム)
    ├── SearchSpaceVisualizer (レイアウト)
    └── PuzzleVisualizer (GPUレンダリング)
```

### 重要な初期化メソッド

#### 重要なStartメソッド:
- **PuzzleCreator.Start()** - 完全検索空間生成の主要ブートストラップ
- **PlayerController.Start()** - 3Dナビゲーション初期化
- **PuzzleView.Start()** - 2D UI初期化

#### 重要なAwakeメソッド:
- **PuzzleGame.Awake()** - 初期パズル状態とイベントハンドラー設定
- **PuzzleVisualizer.Awake()** - GPUインスタンシングインフラ初期化
- **BlockView.Awake()** - 個別ブロックコンポーネント設定

### アプリケーションフロー

1. **起動**: PuzzleCreatorが完全8-puzzle検索空間（全到達可能状態）を生成
2. **デフォルトモード**: PlayerController有効での3D探索
3. **モード切り替え**: 右クリックで3D探索と2Dパズルゲーム間を切り替え
4. **経路可視化**: パズルプレイに応じて3D可視化が訪問状態を表示

### メインエントリーポイント要約

**主要エントリーポイント**は**PuzzleCreator.Start()**で、以下を実行:
- CompleteSpaceExplorerによる完全検索空間生成
- SearchSpaceVisualizerによる全状態の3D空間配置
- PuzzleVisualizerによるGPUインスタンス描画初期化
- 3D可視化と2Dパズルインタラクション両方の基盤作成

**ルートコントローラー**は**VisualizeStateController**で、3D探索と2Dパズルゲームモード間の切り替えを含む全体的なアプリケーション動作を統制します。

## 注意点

- Unity 6000.0.26f1使用（Unity 6.x系列）
- Universal Render Pipeline (URP) 17.0.3使用
- Input System 1.11.2使用（旧Input Managerは非使用）
- TextMeshPro依存
- GPU Instancing対応のシェーダー必要
- 大量のオブジェクト描画のため、GPU性能に依存

## フォルダー構造とファイル組織

### ルートディレクトリ構造

```
8-PuzzleVisualize/
├── Assets/                 # コアプロジェクトアセット
├── Library/               # Unity生成キャッシュファイル
├── Packages/              # パッケージマネージャー設定
├── ProjectSettings/       # Unityプロジェクト設定
├── Temp/                  # Unity一時ファイル
├── UserSettings/          # ユーザー固有設定
├── Logs/                  # Unityログ
├── obj/                   # ビルド成果物
└── CLAUDE.md              # プロジェクトドキュメント
```

### Assetsディレクトリ - メインプロジェクトコンテンツ

#### スクリプト組織 (`Assets/Scripts/`)

#### ディレクトリ構造
- **`Assets/Scripts/Block/`** - パズル要素の基本データ型
- **`Assets/Scripts/Puzzle/`** - コアパズルロジックと可視化戦略
- **`Assets/Scripts/Search/`** - 検索アルゴリズムの実装
- **`Assets/Scripts/Game/`** - ゲームロジックとUIコントローラー
- **`Assets/Scripts/Tests/`** - 包括的テストスイート
- **ルートスクリプト** - コマンドパターンとレンダリングユーティリティ

**コアディレクトリ構造**:
```
Assets/Scripts/
├── Object/                 # オブジェクト
│   ├── Puzzle.cs           # メインパズル状態管理
│   ├── PuzzleState.cs      # 不変状態表現
│   ├── BlockNumber.cs      # ブロック番号型安全管理
│   ├── BlockPosition.cs    # ブロック位置管理
│   └── BlockView.cs        # ブロック表示制御
├── Graph/                  # グラフレイアウト
│   ├── FruchtermanReingoldVisualizer.cs # グラフレイアウト
│   ├── JobSystemOptimizedFruchtermanReingoldVisualizer.cs # グラフレイアウト
│   ├── SearchSpaceVisualizer.cs # 検索空間描画
│   └── IVisualizeStrategy.cs # 可視化戦略インターフェース
├── Puzzle/                 # コアパズルロジック
│   ├── PuzzleCreator.cs    # パズル生成
│   ├── PuzzleVisualizer.cs # 3D可視化
│   └── PuzzleNodeData.cs   # 検索アルゴリズムデータ
├── Search/                 # 検索アルゴリズム実装
│   ├── ISearchAlgorithm.cs # 共通インターフェース
│   ├── DepthFirstSearch.cs # DFS実装
│   ├── BreadthFirstSearch.cs # BFS実装
│   └── CompleteSpaceExplorer.cs # 完全空間探索
├── Game/                   # ゲーム制御とUI管理
│   ├── PuzzleGame.cs       # メインゲームコントローラー
│   ├── PuzzleView.cs       # UI表示制御
│   ├── PuzzleBridge.cs     # コンポーネントブリッジ
│   ├── PuzzleFinderView.cs # 検索可視化UI
│   └── VisualizeStateController.cs # 状態可視化制御
├── Tests/                  # 単体テスト
│   ├── Editor/ 
│   │   ├── EditModeTests.asmdef # テストアセンブリ定義
│   │   ├── BlockNumberTests.cs # 値オブジェクトテスト
│   │   ├── BlockPositionTests.cs # 位置ロジックテスト
│   │   ├── PuzzleTests.cs  # コアパズルテスト
│   │   ├── PuzzleStateTests.cs # 状態管理テスト
│   │   ├── DepthFirstSearchTests.cs # DFSアルゴリズムテスト
│   │   ├── BreadthFirstSearchTests.cs # BFSアルゴリズムテスト
│   │   └── CompleteSpaceExplorerTests.cs # 完全検索テスト
│   └── PlayModeTests.asmdef # テストアセンブリ定義
└── (ルートスクリプト)
    ├── ICommand.cs         # コマンドインターフェース
    ├── MoveCommand.cs      # 移動コマンド実装
    ├── InvokeCommand.cs    # コマンド実行・元に戻す
    ├── InstancedMeshRenderer.cs # GPU描画ユーティリティ
    ├── InstancedMeshInfo.cs # メッシュ情報管理
    ├── PlayerController.cs # プレイヤー入力処理
    └── Puzzle.asmdef       # コアアセンブリ定義
```

#### アセット組織
```
Assets/
├── Materials/              # シェーダーマテリアル
│   ├── Edge.mat            # グラフエッジマテリアル
│   ├── EdgeRoute.mat       # 経路可視化
│   ├── NumberBlock.mat     # ブロック番号表示
│   ├── PuzzleBlock.mat     # パズルピースマテリアル
│   └── Vertex.mat          # グラフ頂点マテリアル
├── Prefabs/                # 再利用可能ゲームオブジェクト
│   ├── 8-Puzzle.prefab     # メインパズルゲームオブジェクト
│   ├── GameNumberBlock.prefab # インタラクティブゲームピース
│   ├── NumberBlock.prefab   # ビジュアル番号ブロック
│   ├── Edge.prefab         # グラフエッジ描画
│   └── NumberText.prefab   # テキスト表示コンポーネント
├── Scenes/                 # Unityシーン
│   └── Main.unity          # メインシーン
├── Settings/               # URP描画設定
│   ├── PC_RPAsset.asset    # PC用レンダリングパイプライン
│   ├── Mobile_RPAsset.asset # モバイル用設定
│   └── DefaultVolumeProfile.asset # デフォルトボリューム
├── TextMesh Pro/           # フォントとテキスト描画アセット
│   ├── Fonts/              # フォントファイル
│   ├── Resources/          # TMP設定リソース
│   ├── Shaders/            # TMPシェーダー
│   └── Sprites/            # TMP用スプライト
├── RenderMeshLit.shader    # カスタムライティングシェーダー
├── RenderMeshUnlit.shader  # カスタムアンリットシェーダー
└── InputSystem_Actions.inputactions # 入力システム設定
```

### Unity固有ディレクトリ

#### Libraryディレクトリ
Unity内部キャッシュと処理済みアセット:
- `Artifacts/` - ビルド成果物キャッシュ
- `ScriptAssemblies/` - コンパイル済みC#アセンブリ
- `PackageCache/` - ダウンロード済みパッケージキャッシュ
- `Bee/` - UnityのBeeビルドシステムファイル

#### Packagesディレクトリ
外部依存関係管理:
- `manifest.json` - パッケージ依存関係
- `packages-lock.json` - ロック済みパッケージバージョン

#### ProjectSettingsディレクトリ
Unityプロジェクト設定ファイル:
- グラフィクス、物理、入力、オーディオ設定
- ビルドと品質設定
- URPパイプライン設定

### アセンブリ定義構造

**コアアセンブリ (`Assets/Scripts/Puzzle.asmdef`)**:
- メインゲームロジックを含有
- Unity Mathematicsパッケージ参照
- 他アセンブリから自動参照

### ファイル命名規則とパターン

#### 一貫した命名
- **クラス**: PascalCase (例: `PuzzleState`, `BlockNumber`)
- **メソッド**: PascalCase (例: `GetValidMoves()`)
- **フィールド**: camelCaseとアンダースコアプレフィックス (例: `_blockNumbers`)
- **プロパティ**: PascalCase (例: `State`, `Position`)

#### ファイル組織パターン
- クラス毎に1ファイル
- ファイル名とクラス名の完全一致
- アセンブリ定義は説明的な名前使用

### パフォーマンスと最適化構造

#### GPU Instancingサポート
- カスタムシェーダーがインスタンス描画対応
- `InstancedMeshRenderer.cs`が大規模描画管理
- GPU Instancing用マテリアル設定
- 効率的データ転送のためのコンピュートバッファ

#### メモリ管理
- 不変状態用値オブジェクト (`BlockNumber`, `BlockPosition`)
- GPUリソース用IDisposableパターン
- MonoBehaviourライフサイクルでの適切なリソースクリーンアップ

### 開発ワークフロー対応

#### バージョン管理
- Unity用適切な`.gitignore`設定
- LibraryとTempディレクトリ除外
- ユーザー設定をバージョン管理から除外

#### ビルド設定
- 複数ビルドプロファイル対応
- 異なるプラットフォーム用URP設定
- 高速コンパイル用アセンブリ定義

### 文書化と保守

#### プロジェクト文書化
- `CLAUDE.md`に包括的プロジェクト文書
- `.claude/documents/`以下にそのほかの文書
- ドメイン固有ロジック用日本語コメント
- 明確なアーキテクチャ説明

#### コード品質
- 一貫した例外処理パターン
- 型安全な値オブジェクト
- 拡張性のためのインターフェースベース設計
- 包括的テストカバレッジ

### フォルダー構造の特徴

このUnity 8-puzzleプロジェクトは優れた組織原則を実証:

1. **明確な関心事分離**: 機能別スクリプト組織 (Puzzle, Search, Game, Block)
2. **適切なアセンブリ管理**: コアロジックとテスト用分離アセンブリ
3. **アセット組織**: マテリアル、プレハブ、シェーダーの論理的グループ化
4. **テスト駆動開発**: 適切な分離を持つ包括的テストスイート
5. **パフォーマンス最適化**: GPU Instancingサポートと効率的描画
6. **Unityベストプラクティス**: URP、パッケージ管理、プロジェクト設定の適切使用
7. **保守可能なアーキテクチャ**: 明確な依存関係を持つインターフェースベース設計

## 主要データモデル

### 1. 値オブジェクト（コアドメイン型）

#### BlockNumber - 型安全な番号表現
**目的**: パズルブロック番号（0-8）を検証付きで表現
**型**: `readonly struct` - `IEquatable<BlockNumber>`実装

**主要特性**:
- `_value`: プライベートintフィールド（0-8範囲）
- 静的検証（`MinValue = 0`, `MaxValue = 8`）
- ゼロは空きスペースを表現

**不変条件**:
```csharp
// 範囲検証
if (value < MinValue || MaxValue < value)
{
    throw new ArgumentOutOfRangeException(
        $"ブロックの番号が有効範囲外です。有効範囲: {MinValue}～{MaxValue}");
}
```

**主要メソッド**:
- `IsZero()`: 空きスペースかチェック
- `Parse(string)`: 文字列解析（検証付き）
- `implicit operator int`: 暗黙的int変換

#### BlockPosition - 空間座標
**目的**: グリッド座標（行、列）と移動操作を表現
**型**: `readonly struct` - `IEquatable<BlockPosition>`実装

**主要プロパティ**:
- `Row`, `Column`: パブリックreadonly intプロパティ

**主要メソッド**:
- `CreateFromIndex(int)`: 線形インデックスから2D位置へ変換
- `Up()`, `Down()`, `Left()`, `Right()`: 移動操作
- `implicit operator int`: 線形インデックスへの変換

### 2. ドメインエンティティ - コア状態モデル

#### PuzzleState - 不変ゲーム状態
**目的**: 完全なパズル構成を不変値オブジェクトとして表現
**型**: `readonly struct` - `IEquatable<PuzzleState>`実装

**主要プロパティ**:
- `_blockNumbers`: プライベート2D BlockNumber配列（3x3）
- 静的定数: `GridSize = 3`, `TotalCells = 9`

**厳格な不変条件**:
```csharp
// ビットマスクによる重複検証
int bitmask = 0;
int expectedBitmask = (1 << TotalCells) - 1; // 0-8のすべてのビット

for (int row = 0; row < RowCount; row++)
{
    for (int col = 0; col < ColumnCount; col++)
    {
        int number = blockNumbers[row, col];
        int currentBit = 1 << number;
        
        if ((bitmask & currentBit) != 0)
        {
            throw new ArgumentException($"ブロック番号{number}が重複しています。");
        }
        bitmask |= currentBit;
    }
}
```

**主要メソッド**:
- `this[BlockPosition]`: 位置ベースアクセス用インデクサー
- `FindNumberBlockPosition(BlockNumber)`: 特定番号の位置検索
- `FindEmptyBlockPosition()`: 空きスペース（0）の位置検索
- `Swap(BlockPosition, BlockPosition)`: 有効移動後の新状態返却
- `Create(int[,])`: int配列からの作成ファクトリ

#### Puzzle - 可変ゲームコントローラー
**目的**: PuzzleStateをラップしてコマンドパターンでゲーム操作提供
**型**: `class`（可変エンティティ）

**主要プロパティ**:
- `_state`: 現在のPuzzleState
- `_invokeCommand`: undo/redo用コマンドパターン
- `State`: 現在状態のパブリックゲッター

**主要メソッド**:
- `TryMoveEmpty(MoveDirection)`: コマンド記録付き移動試行
- `TryMoveEmptyDirect(Vector2Int)`: コマンドなし直接移動
- `Clone()`: 検索アルゴリズム用コピー作成
- `ExecuteCommand()`, `UndoCommand()`, `RedoCommand()`: コマンドパターン
- `GetVisitedRoute()`: 訪問状態履歴返却

### 3. コマンドパターンモデル

#### ICommand - コマンドインターフェース
**目的**: 取り消し可能操作の抽象化

**メソッド**:
- `Execute()`: 操作実行
- `Undo()`: 操作取り消し
- `GetBoardState()`: 関連パズル状態返却

#### MoveCommand - 移動操作
**目的**: 単一移動操作のカプセル化

**プロパティ**:
- `_puzzle`: パズル参照
- `_puzzleState`: 移動前状態
- `_direction`: 移動方向

#### InvokeCommand - コマンドマネージャー
**目的**: コマンド実行と履歴管理

**プロパティ**:
- `_undoStack`: 実行済みコマンドスタック
- `_redoStack`: 取り消し済みコマンドスタック

**メソッド**:
- `ExecuteCommand()`: コマンド実行・追跡
- `GetVisitedRoute()`: 訪問状態セット構築

### 4. 検索アルゴリズムデータモデル

#### PuzzleNodeData - 検索ノード情報
**目的**: 各パズル状態の検索アルゴリズムメタデータ格納
**型**: `class`（可変データコンテナ）

**主要プロパティ**:
```csharp
public bool IsVisited { get; private set; }      // 処理済みフラグ
public List<PuzzleState> AdjacentStates { get; } // 隣接状態リスト
public PuzzleState? Parent { get; private set; } // 経路構築用親状態
public int Depth { get; set; }                   // 初期状態からの距離
```

**主要メソッド**:
- `MarkAsVisited()`: ノードを処理済みマーク
- `AddAdjacentState()`: 隣接関係追加
- `SetParent()`: 経路追跡用親設定

### 5. UI/ビューモデル

#### PuzzleView - UI状態管理
**目的**: 視覚表現とアニメーション管理

**主要プロパティ**:
- `positions`: ブロック用UI位置配列
- `children`: 各ブロックのUI要素
- `pastPuzzle`: アニメーション用前状態
- `_puzzle`: 現在パズルへの参照

**主要メソッド**:
- `SetPuzzle()`: パズル状態に合わせてUI更新
- `AnimateMove()`: 状態間遷移アニメーション
- `GetDragDirection()`: 入力からドラッグ方向決定

#### BlockView - 個別ブロックUI
**目的**: 個別パズルブロックのUIコンポーネント

**主要プロパティ**:
- `_blockNumber`: 関連BlockNumber
- `IsDragging`: 現在のドラッグ状態

**イベント**:
- `OnBlockClicked`: ブロッククリックイベント
- `OnBlockDragged`: ドラッグイベント

### 6. レンダリング/可視化モデル

#### InstancedMeshInfo - レンダリング設定
**目的**: GPU Instancing用メッシュとマテリアルペア
**型**: `struct`（シリアライズ可能データ）

**プロパティ**:
- `mesh`: メッシュ参照
- `mat`: マテリアル参照

#### InstancedMeshRenderer - GPU Instancingマネージャー
**目的**: 複数パズル状態のGPUインスタンス描画管理

**主要プロパティ**:
- `_matrices`: 変換マトリクスリスト
- `_matrixBuffer`: マトリクスデータ用GPUバッファ
- `_renderParams`: レンダリングパラメータ

**主要メソッド**:
- `AddMatrix()`: 変換マトリクス追加
- `ApplyMatrixData()`: GPUへのデータアップロード
- `Render()`: インスタンス描画実行

### 7. データフローと関係性

#### 状態管理フロー:
1. **PuzzleState**（不変） → **Puzzle**（可変ラッパー） → **PuzzleView**（UI）
2. ユーザー入力 → **PuzzleGame** → **Puzzle** → **MoveCommand** → **PuzzleState**
3. **InvokeCommand**がundo/redo用すべて状態変更追跡

#### 検索アルゴリズムフロー:
1. **CompleteSpaceExplorer**がすべて可能**PuzzleState**生成
2. **PuzzleNodeData**が各状態のメタデータ格納
3. **IVisualizeStrategy**が状態を3D位置に変換
4. **InstancedMeshRenderer**が数千状態を効率的描画

#### 検証と不変条件:
- **BlockNumber**: 範囲検証（0-8）
- **BlockPosition**: グリッド境界チェック
- **PuzzleState**: ビットマスク検証で各番号正確に1つ保証
- **Puzzle**: 移動検証で隣接する空きスペースとの交換のみ許可

#### 不変性パターン:
- **値オブジェクト**（`BlockNumber`, `BlockPosition`, `PuzzleState`）は不変
- **ドメインエンティティ**（`Puzzle`）は可変だが不変状態をラップ
- **コマンドパターン**が状態履歴保持
- **検索アルゴリズム**が既存修正より新インスタンス作成

### データモデルの特徴

このアーキテクチャは以下を実現:

1. **型安全性**: 値オブジェクトによる無効状態防止
2. **不変性**: コア状態オブジェクトの変更不可
3. **検証**: 構築時の包括的データ整合性チェック
4. **効率性**: GPU Instancingによる大規模可視化
5. **拡張性**: インターフェースベース設計
6. **保守性**: 明確な責任分離とパターン適用

複雑な状態空間の型安全、不変性、効率的可視化を持つ洗練されたドメインモデリングを実証しています。

## ファイル構成の特徴

- アセンブリ定義により明確な依存関係
- テストコードの分離
- プレハブとマテリアルの体系的な管理
- GPU Instancing用のカスタムシェーダー

## メモリ管理ガイドライン

### 推奨事項
- `IDisposable`パターンの厳密な実装
- 明示的なリソース解放
- メモリリークの防止
- 非同期処理での適切なキャンセル処理

#### 具体的なデータ値を特別扱いして良い例外

以下のような場合は、特定の値に基づく分岐が正当化されます。

- 意味的な特別扱い: 予約語、システムテーブル、特殊フラグなど、仕様上特別な意味を持つ値。
- 明示的な設定: 設定ファイルなどで明示された特別処理。
- ドメイン固有の規則: 業務ドメインで明確に定義された例外規則。