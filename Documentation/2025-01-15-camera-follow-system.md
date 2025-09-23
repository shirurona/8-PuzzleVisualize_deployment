# カメラ追従機能 仕様書

**作成日**: 2025-01-15  
**対象プロジェクト**: 8-PuzzleVisualize  
**機能名**: パズルゲームカメラ追従システム

## 要件

### 背景・動機
- 8-puzzleゲームにおいて、プレイヤーの操作と3D状態空間の可視化を直感的に連携させる必要がある
- 現在のシステムでは、2Dパズルゲームと3D探索空間が独立しており、教育的価値を最大化できていない
- パズルの状態変更時に対応する3D空間位置へカメラが自動移動することで、状態空間理論の理解を促進したい

### 目的
- **教育的価値の向上**: パズル操作と状態空間可視化の直感的な連携
- **ユーザー体験の向上**: 滑らかで自然なカメラワークによる没入感の向上
- **理解促進**: ゲームモードでの状態空間理解の促進

## 仕様

### 基本動作仕様

#### 追従タイミング
- **パズル移動と同時**: パズルブロックの移動開始と同じタイミングでカメラアニメーション開始
- **同期開始**: パズルアニメーションとカメラ移動は開始タイミングのみ同期
- **独立動作**: 開始後は独立してアニメーション実行（終了タイミングは異なってもよい）

#### 追従条件
- **ゲームモード時のみ**: 右クリックで切り替わるゲームモード時にのみ追従機能が有効
- **探索モード無効**: 3D探索モード中は追従機能を無効化
- **Undo/Redo対応**: Undo/Redo操作時も追従機能が動作
- **トグル制御**: Inspector上でON/OFF切り替え可能

#### カメラ位置・向き仕様
- **参考実装**: PuzzleFinderのFind機能と同じ動作
- **位置計算**: `puzzlePosition + Vector3.back * cameraDistance`
  - `cameraDistance`: デフォルト10ユニット（調整可能）
- **向き設定**: `Quaternion.LookRotation(Vector3.forward)`
- **カメラ高さ**: `transform.position + Vector3.up * cameraHeight`
  - `cameraHeight`: デフォルト0.5ユニット（調整可能）

#### アニメーション仕様
- **補間方式**: `Vector3.Lerp()`による線形補完
- **アニメーション速度**: SerializeFieldで調整可能
  - デフォルト値: `followSpeed = 20f`（PuzzleFinderと同値）
- **フレーム独立**: `Time.deltaTime`を使用したフレームレート独立動作

#### 操作対象
- **通常移動**: キーボード・マウスクリック・ドラッグによる移動
- **Undo操作**: Undoボタンクリック・キーボードショートカット
- **Redo操作**: Redoボタンクリック・キーボードショートカット

### パラメータ仕様

以下のパラメータをInspector上で調整可能とする：

```csharp
[SerializeField] private float followSpeed = 20f;      // アニメーション速度
[SerializeField] private float cameraDistance = 10f;   // カメラ距離
[SerializeField] private float cameraHeight = 0.5f;    // カメラ高さ
[SerializeField] private bool enableFollowing = true;  // 追従機能ON/OFF
```

### 制約・例外処理
- **無効状態**: パズル状態が3D空間に存在しない場合は追従しない
- **モード切り替え**: 探索モードへの切り替え時は進行中のアニメーションを停止
- **マウスルック維持**: アニメーション完了後はマウスルック機能を維持

## 設計

### アーキテクチャ概要

#### 新規コンポーネント
**CameraPuzzleFollower**: カメラ追従機能の中核コンポーネント

```csharp
public class CameraPuzzleFollower : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PuzzleCreator puzzleCreator;
    [SerializeField] private VisualizeStateController visualizeController;
    
    // 調整可能パラメータ
    [SerializeField] private float followSpeed = 20f;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraHeight = 0.5f;
    [SerializeField] private bool enableFollowing = true;
    
    // 内部状態
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 currentCameraPosition;
    private bool isAnimating = false;
}
```

#### 既存コンポーネントの拡張

**PuzzleGame**:
- `ExecuteMove()`, `Undo()`, `Redo()` メソッドに追従トリガー追加
- カメラ追従システムへの状態変更通知

**VisualizeStateController**:
- カメラ追従機能のモード別制御
- ゲームモード突入時の制御

### 実装方式

#### 追従ロジック
```csharp
public void FollowToPuzzleState(PuzzleState puzzleState)
{
    // 追従条件チェック
    if (!enableFollowing || !visualizeController.IsGameMode) return;
    
    // 3D位置取得
    Vector3 puzzlePosition = puzzleCreator.GetBoardStatePosition(puzzleState);
    
    // PuzzleFinderと同じ位置計算
    Vector3 targetPos = puzzlePosition + Vector3.back * cameraDistance;
    Quaternion targetRot = Quaternion.LookRotation(Vector3.forward);
    
    // アニメーション開始
    StartCameraAnimation(targetPos, targetRot);
}
```

#### アニメーション実装
```csharp
private async Awaitable StartCameraAnimation(Vector3 targetPos, Quaternion targetRot)
{
    isAnimating = true;
    float elapsedTime = 0f;
    Vector3 startPos = playerController.transform.position;
    Quaternion startRot = playerController.mainCamera.transform.rotation;
    
    while (elapsedTime < 1f && isAnimating)
    {
        elapsedTime += Time.deltaTime;
        
        // 位置補間
        Vector3 lerpedPos = Vector3.Lerp(startPos, targetPos, elapsedTime * followSpeed);
        playerController.transform.position = lerpedPos;
        
        // 向き補間
        Quaternion lerpedRot = Quaternion.Lerp(startRot, targetRot, elapsedTime * followSpeed);
        playerController.mainCamera.transform.rotation = lerpedRot;
        
        // カメラ位置調整（PuzzleFinderと同様）
        Vector3 camTargetPosition = playerController.transform.position + Vector3.up * cameraHeight;
        currentCameraPosition = Vector3.Lerp(currentCameraPosition, camTargetPosition, Time.deltaTime * followSpeed);
        playerController.mainCamera.transform.position = currentCameraPosition;
        
        await Awaitable.NextFrameAsync();
    }
    
    isAnimating = false;
}
```

### 統合方式

#### PuzzleGameとの統合
```csharp
// PuzzleGame.ExecuteMove()の拡張
private void ExecuteMove(Puzzle.MoveDirection direction)
{
    currentPuzzle.TryMoveEmpty(direction);
    puzzleView.AnimateMove();
    
    // カメラ追従トリガー
    cameraPuzzleFollower?.FollowToPuzzleState(currentPuzzle.State);
}
```

#### VisualizeStateControllerとの統合
```csharp
// VisualizeStateController.SetGameMode()の拡張
public void SetGameMode()
{
    IsGameMode = true;
    // 既存処理...
    
    // 即座に現在位置へ追従
    if (cameraPuzzleFollower.enableFollowing)
    {
        cameraPuzzleFollower.FollowToPuzzleState(puzzleGame.GetCurrentPuzzle().State);
    }
}
```

### パフォーマンス考慮

#### 最適化ポイント
- **条件分岐**: モード判定による不要な処理の回避
- **アニメーション制御**: 進行中判定による重複アニメーション防止
- **Update()最適化**: アニメーション中のみアクティブな更新処理

#### メモリ管理
- **非同期処理**: `Awaitable`による適切なメモリ管理
- **参照管理**: SerializeFieldによる明示的な依存関係

### 拡張性

#### 将来的な拡張ポイント
- **カメラアングル**: 複数の視点パターン
- **アニメーション**: イージング関数の追加
- **トリガー**: 他のゲームイベントへの対応
- **設定**: ユーザー設定による動的調整

#### 互換性
- **既存機能**: PlayerControllerの既存機能との完全互換
- **マウスルック**: アニメーション完了後の手動制御復帰
- **モード切り替え**: 既存のモード切り替えシステムとの統合

## テストケース

### テスト技法の選択
- **状態遷移テスト**: ゲームモード/探索モードの遷移パターン
- **同値分割法**: パラメータの有効/無効範囲
- **境界値分析**: 数値パラメータの境界値
- **シナリオテスト**: 実際の使用パターンに基づく統合テスト

### CameraPuzzleFollowerクラス

#### FollowToPuzzleState メソッド

**TC-001: 正常追従 - ゲームモード有効時**
- **前提条件**: enableFollowing = true, ゲームモード有効
- **操作**: 有効なPuzzleStateを指定してFollowToPuzzleStateを呼び出し
- **期待結果**: カメラアニメーションが開始され、isAnimating = trueになる

**TC-002: 追従無効 - enableFollowing = false**
- **前提条件**: enableFollowing = false, ゲームモード有効
- **操作**: 有効なPuzzleStateを指定してFollowToPuzzleStateを呼び出し
- **期待結果**: カメラアニメーションが開始されず、isAnimating = falseのまま

**TC-003: 追従無効 - 探索モード時**
- **前提条件**: enableFollowing = true, 探索モード有効
- **操作**: 有効なPuzzleStateを指定してFollowToPuzzleStateを呼び出し
- **期待結果**: カメラアニメーションが開始されず、isAnimating = falseのまま

**TC-007: パラメータ境界値 - followSpeed = 0**
- **前提条件**: followSpeed = 0, ゲームモード有効, enableFollowing = true
- **操作**: FollowToPuzzleStateを呼び出し
- **期待結果**: アニメーションが非常にゆっくり実行される（停止しない）

**TC-008: パラメータ境界値 - cameraDistance = 0**
- **前提条件**: cameraDistance = 0, ゲームモード有効, enableFollowing = true
- **操作**: FollowToPuzzleStateを呼び出し
- **期待結果**: カメラがパズル位置と同じ位置に移動する

#### StartCameraAnimation メソッド

**TC-004: アニメーション実行 - 正常完了**
- **前提条件**: 有効なtargetPos, targetRotが設定済み
- **操作**: StartCameraAnimationを呼び出し、アニメーション完了まで待機
- **期待結果**: カメラ位置・回転が目標値に収束し、isAnimating = falseになる

**TC-005: アニメーション中断 - モード切り替え**
- **前提条件**: アニメーション実行中（isAnimating = true）
- **操作**: 探索モードに切り替え
- **期待結果**: アニメーションが中断され、isAnimating = falseになる

**TC-006: 重複アニメーション防止**
- **前提条件**: アニメーション実行中（isAnimating = true）
- **操作**: 再度StartCameraAnimationを呼び出し
- **期待結果**: 新しいアニメーションが開始されず、既存アニメーションが継続

### PuzzleGame統合テスト

**TC-009: ExecuteMove連携 - 通常移動**
- **前提条件**: ゲームモード有効, カメラ追従有効
- **操作**: キーボードでパズルブロックを移動
- **期待結果**: パズルアニメーションとカメラ追従が同時開始される

**TC-010: Undo操作連携**
- **前提条件**: パズル移動履歴あり, ゲームモード有効, カメラ追従有効
- **操作**: Undoボタンをクリック
- **期待結果**: パズル状態復元とカメラ追従が実行される

**TC-011: Redo操作連携**
- **前提条件**: Undo実行済み, ゲームモード有効, カメラ追従有効
- **操作**: Redoボタンをクリック
- **期待結果**: パズル状態再実行とカメラ追従が実行される

### VisualizeStateController統合テスト

**TC-012: ゲームモード切り替え時の即座追従**
- **前提条件**: 探索モード状態, カメラ追従有効
- **操作**: ゲームモードに切り替え
- **期待結果**: 現在のパズル状態位置へカメラが即座に追従開始

**TC-013: 探索モード切り替え時のアニメーション停止**
- **前提条件**: ゲームモード状態, カメラアニメーション実行中
- **操作**: 探索モードに切り替え
- **期待結果**: 進行中のカメラアニメーションが停止される

### パラメータ検証テスト

**TC-014: followSpeed上限値**
- **前提条件**: followSpeed = 100f（高速設定）
- **操作**: カメラ追従実行
- **期待結果**: 高速だが安定したアニメーション実行

**TC-015: cameraHeight負値**
- **前提条件**: cameraHeight = -1f
- **操作**: カメラ追従実行
- **期待結果**: パズル位置より下方にカメラが配置される

**TC-016: cameraDistance負値**
- **前提条件**: cameraDistance = -5f
- **操作**: カメラ追従実行
- **期待結果**: パズル位置の前方（Z軸正方向）にカメラが配置される

### シナリオテスト

**TC-017: 連続移動時の追従**
- **前提条件**: ゲームモード, カメラ追従有効
- **操作**: パズルブロックを連続で複数回移動
- **期待結果**: 各移動に対してカメラが適切に追従し、アニメーションが重複しない

**TC-018: モード切り替え後の機能復帰**
- **前提条件**: ゲームモード, カメラ追従有効
- **操作**: 探索モード→ゲームモード→パズル移動
- **期待結果**: モード復帰後にカメラ追従機能が正常動作する

### エラーケーステスト

**TC-019: 無効PuzzleState指定**
- **前提条件**: ゲームモード, カメラ追従有効
- **操作**: 3D空間に存在しないPuzzleStateを指定
- **期待結果**: 例外が発生せず、追従処理がスキップされる

**TC-020: コンポーネント参照null**
- **前提条件**: playerController参照がnull
- **操作**: FollowToPuzzleStateを呼び出し
- **期待結果**: NullReferenceExceptionが発生せず、エラーハンドリングされる