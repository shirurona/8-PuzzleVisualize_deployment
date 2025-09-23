# 実装詳細・コンポーネントリファレンス

## 主要データモデル詳細

### 値オブジェクト（コアドメイン型）

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

#### BlockPosition - 空間座標
**目的**: グリッド座標（行、列）と移動操作を表現
**型**: `readonly struct` - `IEquatable<BlockPosition>`実装

#### PuzzleState - 不変ゲーム状態
**目的**: 完全なパズル構成を不変値オブジェクトとして表現
**型**: `readonly struct` - `IEquatable<PuzzleState>`実装

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

### コマンドパターンモデル

#### ICommand - コマンドインターフェース
**目的**: 取り消し可能操作の抽象化

#### MoveCommand - 移動操作
**目的**: 単一移動操作のカプセル化

#### InvokeCommand - コマンドマネージャー
**目的**: コマンド実行と履歴管理

### UI/ビューモデル

#### PuzzleView - UI状態管理
**目的**: 視覚表現とアニメーション管理

#### BlockView - 個別ブロックUI
**目的**: 個別パズルブロックのUIコンポーネント

### レンダリング/可視化モデル

#### InstancedMeshRenderer - GPU Instancingマネージャー
**目的**: 複数パズル状態のGPUインスタンス描画管理

## パフォーマンスと最適化詳細

### GPU Instancingサポート
- カスタムシェーダーがインスタンス描画対応
- `InstancedMeshRenderer.cs`が大規模描画管理
- GPU Instancing用マテリアル設定
- 効率的データ転送のためのコンピュートバッファ

### メモリ管理
- 不変状態用値オブジェクト (`BlockNumber`, `BlockPosition`)
- GPUリソース用IDisposableパターン
- MonoBehaviourライフサイクルでの適切なリソースクリーンアップ

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

## 拡張ポイント

### 検索アルゴリズム
- **新検索アルゴリズム**: `ISearchAlgorithm`実装
- **新可視化戦略**: `IVisualizeStrategy`実装
- **新コマンド**: `ICommand`実装でUndo/Redo拡張
- **新パズルサイズ**: `PuzzleState.GridSize`変更で15-puzzle対応

### 技術改善計画
- Job System活用 / GPU Compute Shader / WebGL対応 / VR/AR対応
