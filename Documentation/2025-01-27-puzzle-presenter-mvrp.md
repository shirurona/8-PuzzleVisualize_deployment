# PuzzlePresenter作成とMVRPパターン実装仕様書

## 要件・問題・動機

### 現在の問題点

現在のアーキテクチャでは、`PuzzleGame`クラスが以下の複数の責任を持っており、単一責任原則に違反している：

1. **ユーザー入力処理** - キーボード入力、マウス入力、ドラッグ処理
2. **Model操作** - `Puzzle`クラスの直接操作
3. **View更新** - `PuzzleView`の直接更新指示
4. **外部コンポーネント制御** - `CameraPuzzleFollower`の直接制御
5. **状態管理** - 自動実行フラグなどの状態管理

### 解決すべき課題

- **責任の分散**: 複数の責任が混在し、コードの保守性が低下
- **結合度の高さ**: ModelとViewが密結合
- **テスト性の低さ**: 単体テストが困難
- **拡張性の問題**: 新機能追加時の影響範囲が広い

### 移行の動機

- **MVRP（Model-View-Reactive-Presenter）パターン**による責任分離
- **R3のReactiveProperty**を使用したObserverパターンでのイベント駆動設計
- **段階的なリファクタリング**による既存機能への影響最小化

## 仕様

### 実装対象

#### 1. PuzzlePresenter.cs（新規作成）
- **責任**: PuzzleStateの変更監視と通知に特化
- **配置**: `Assets/Scripts/Game/PuzzlePresenter.cs`

#### 2. 既存クラスの最小限修正
- **PuzzleGame.cs** - Presenter連携機能追加
- **PuzzleView.cs** - ReactiveProperty購読機能追加
- **CameraPuzzleFollower.cs** - ReactiveProperty購読機能追加

### 機能仕様

#### PuzzlePresenter機能
- `ReactiveProperty<PuzzleState> CurrentPuzzleState` - パズル状態の監視
- 状態変更時の自動通知機能
- 購読者への変更通知配信

#### 購読機能
- PuzzleViewの自動UI更新
- CameraPuzzleFollowerの自動カメラ追従
- 将来的な機能拡張への対応

### 技術仕様

#### 使用ライブラリ
- **R3（ReactiveProperty）** - `com.cysharp.r3`（既存導入済み）
- **UniTask** - 非同期処理対応（既存導入済み）

#### 対象Unity版本
- **Unity 6000.0.26f1**

## 設計

### アーキテクチャ設計

#### MVRP パターン適用
```
Model (Puzzle) ←→ Presenter (PuzzlePresenter) ←→ View (PuzzleView)
                          ↓ (ReactiveProperty)
                   Other Components (CameraPuzzleFollower)
```

#### データフロー
```
ユーザー入力 → PuzzleGame → Puzzle（Model更新）
                    ↓
             PuzzlePresenter → ReactiveProperty発火
                    ↓
            購読者（View, Camera）→ 自動更新
```

### クラス設計

#### PuzzlePresenter.cs
```csharp
public class PuzzlePresenter : MonoBehaviour
{
    // ReactiveProperty
    private ReactiveProperty<PuzzleState> _currentPuzzleState;
    public ReadOnlyReactiveProperty<PuzzleState> CurrentPuzzleState { get; }
    
    // 初期化
    public void Initialize(Puzzle puzzle)
    
    // 状態更新
    public void UpdatePuzzleState()
    
    // 購読メソッド
    public IDisposable SubscribePuzzleStateChanged(Action<PuzzleState> onChanged)
}
```

#### PuzzleGame.cs修正点
```csharp
public class PuzzleGame : MonoBehaviour
{
    [SerializeField] private PuzzlePresenter puzzlePresenter; // 追加
    
    private void ExecuteMove(Puzzle.MoveDirection direction)
    {
        currentPuzzle.TryMoveEmpty(direction);
        puzzlePresenter.UpdatePuzzleState(); // 追加
        // puzzleView.AnimateMove(); // Presenterからの自動更新に移行
    }
}
```

#### PuzzleView.cs追加機能
```csharp
public class PuzzleView : MonoBehaviour
{
    private CompositeDisposable _disposables = new();
    
    public void SubscribeToPresenter(PuzzlePresenter presenter)
    {
        presenter.CurrentPuzzleState
            .Subscribe(OnPuzzleStateChanged)
            .AddTo(_disposables);
    }
    
    private void OnPuzzleStateChanged(PuzzleState newState)
    {
        // UI自動更新処理
        SetPuzzle();
        AnimateMove();
    }
}
```

#### CameraPuzzleFollower.cs追加機能
```csharp
public class CameraPuzzleFollower : MonoBehaviour
{
    private CompositeDisposable _disposables = new();
    
    public void SubscribeToPresenter(PuzzlePresenter presenter)
    {
        presenter.CurrentPuzzleState
            .Subscribe(OnPuzzleStateChanged)
            .AddTo(_disposables);
    }
    
    private void OnPuzzleStateChanged(PuzzleState newState)
    {
        FollowToPuzzleState(newState);
    }
}
```

### 実装手順

#### Phase 1: Presenter基盤作成
1. `PuzzlePresenter.cs`新規作成
2. ReactivePropertyによる状態管理実装
3. 購読機能の実装

#### Phase 2: 既存クラス連携
1. `PuzzleGame`にPresenter連携機能追加
2. `PuzzleView`に購読機能追加
3. `CameraPuzzleFollower`に購読機能追加

#### Phase 3: 動作確認とテスト
1. 既存機能の動作確認
2. ReactivePropertyの動作確認
3. 必要に応じて調整

### 設計原則

#### 1. 最小影響原則
- 既存コードへの変更を最小限に抑制
- 段階的なリファクタリングを可能にする設計

#### 2. 単一責任原則
- PuzzlePresenterはPuzzleState監視のみに特化
- 各クラスの責任を明確に分離

#### 3. 開放閉鎖原則
- 新しい購読者の追加が容易
- 既存機能を変更せずに拡張可能

#### 4. 依存性逆転原則
- ViewがPresenterの抽象に依存
- 具体実装への直接依存を回避

### 将来の拡張性

#### 追加可能な機能
- 他の状態値の監視（IsAutoSolving, CanUndo等）
- 複数のView同時更新
- ログ機能やアナリティクス連携
- リプレイ機能の実装

#### テスト戦略
- Presenterの単体テスト
- ReactivePropertyのMock/Stub
- ViewとModelの独立テスト

---

**作成日**: 2025-01-27  
**対象プロジェクト**: 8-PuzzleVisualize  
**Unity版本**: 6000.0.26f1  
**使用パターン**: MVRP (Model-View-Reactive-Presenter)