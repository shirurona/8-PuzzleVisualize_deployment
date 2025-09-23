# アーキテクチャ詳細

## アーキテクチャ概要

本プロジェクトは、**依存性注入（DI）**、**リアクティブプログラミング**、**MVP（Model-View-Presenter）パターン**を採用したモダンなUnityアーキテクチャを実装しています。

### 採用アーキテクチャパターン
- **MVP Pattern**: Model-View-Presenter による責任分離
- **依存性注入**: VContainer による疎結合設計
- **リアクティブプログラミング**: R3 による宣言的状態管理
- **コマンドパターン**: Undo/Redo機能
- **ストラテジーパターン**: プラガブル検索アルゴリズム

## データフローと関係性

### 状態管理フロー
1. **PuzzleState**（不変） → **Puzzle**（可変ラッパー） → **PuzzleView**（UI）
2. ユーザー入力 → **PuzzleGame** → **Puzzle** → **MoveCommand** → **PuzzleState**
3. **InvokeCommand**がundo/redo用すべて状態変更追跡

### 検索アルゴリズムフロー
1. **CompleteSpaceExplorer**がすべて可能**PuzzleState**生成
2. **PuzzleNodeData**が各状態のメタデータ格納
3. **IVisualizeStrategy**が状態を3D位置に変換
4. **InstancedMeshRenderer**が数千状態を効率的描画

## 主要アーキテクチャパターン

### 1. 依存性注入パターン (Dependency Injection Pattern)
**実装**: `PuzzleLifetimeScope.cs` + VContainer による疎結合設計
- コンポーネント間の循環依存を回避し、テスタビリティを向上

### 2. MVPパターン (Model-View-Presenter Pattern)
**実装**: `PuzzlePresenter.cs` を中心とした責任分離
- Model (Puzzle/PuzzleState): ビジネスロジックとデータ
- View (PuzzleView): UI表示とユーザーインタラクション  
- Presenter (PuzzlePresenter): Model-View間の調整とR3による状態伝播

### 3. リアクティブプログラミングパターン
**実装**: R3 (Reactive Extensions) による宣言的状態管理
- Observable/Subject による状態変更ストリーム
- 自動的なイベント伝播とメモリリーク防止

### 4. 非同期処理パターン
**実装**: UniTask による高性能非同期処理
- async/await による直感的な非同期コード
- UIブロッキングなしの状態管理統合

### 5. コマンドパターン (Command Pattern)
**実装**: `ICommand.cs`, `MoveCommand.cs`, `InvokeCommand.cs`
- 完全なundo/redo機能とゲームリプレイのサポート

## 現代的なアーキテクチャの特徴

### 関心事の分離と層化
- **依存性注入層**: コンポーネント間依存関係管理 (PuzzleLifetimeScope)
- **ドメイン層**: 純粋なパズルロジック (Puzzle, PuzzleState)
- **アプリケーション層**: ビジネスロジックとユースケース (PuzzleGameAgent, PuzzleDragger)
- **プレゼンテーション層**: MVPパターンによる責任分離 (PuzzlePresenter, PuzzleView)
- **インフラ層**: Unity固有機能とGPU最適化 (GPU Instancing, レンダリング)

### リアクティブアーキテクチャ
- **単方向データフロー**: Model → Presenter → View
- **状態変更伝達**: R3によるリアクティブストリーム
- **イベントドリブン**: 状態変更に基づく自動更新
- **メモリ安全性**: AddTo()による自動リソース管理

### 継承より合成と依存性注入
**合成パターン**:
- PuzzleがPuzzleStateとInvokeCommandを含有
- PuzzlePresenterが複数Viewを統合管理
- PuzzleGameAgentが検索アルゴリズムを合成

**依存性注入**:
- コンストラクタ/メソッドインジェクション ([Inject]属性)
- ライフサイクル管理されたシングルトン

## SOLID原則の適用

- **単一責任原則**: 各クラスが明確な単一目的を持つ
- **開放閉鎖原則**: インターフェースによる拡張可能設計
- **リスコフ置換原則**: 実装クラス間の相互交換性
- **インターフェース分離原則**: 焦点を絞った小さなインターフェース
- **依存性逆転原則**: 抽象への依存、具象実装からの独立

## 拡張性設計

### 容易な拡張ポイント
1. **新検索アルゴリズム**: `ISearchAlgorithm`実装で追加可能
2. **新可視化戦略**: `IVisualizeStrategy`実装で追加可能
3. **新コマンド**: `ICommand`実装でUndo/Redo機能拡張
4. **新パズルサイズ**: `PuzzleState.GridSize`変更で15-puzzle等に対応