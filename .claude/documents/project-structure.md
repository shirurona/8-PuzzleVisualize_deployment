# プロジェクト構造

## スクリプト構成 (`Assets/Scripts/`)
```
Assets/Scripts/
├── Object/           # パズル基本要素
│   ├── Puzzle.cs     # メイン状態管理
│   ├── PuzzleState.cs # 不変状態表現
│   ├── BlockNumber.cs # 型安全な番号
│   ├── BlockPosition.cs # 位置管理
│   └── BlockView.cs  # ブロック表示
├── Graph/            # 3D可視化
│   ├── FruchtermanReingoldVisualizer.cs
│   ├── JobSystemOptimizedFruchtermanReingoldVisualizer.cs
│   ├── SearchSpaceVisualizer.cs
│   └── IVisualizeStrategy.cs
├── Puzzle/           # コアロジック
│   ├── PuzzleCreator.cs
│   ├── PuzzleVisualizer.cs
│   └── PuzzleNodeData.cs
├── Search/           # 検索アルゴリズム
│   ├── ISearchAlgorithm.cs
│   ├── DepthFirstSearch.cs
│   ├── BreadthFirstSearch.cs
│   └── CompleteSpaceExplorer.cs
├── Game/             # ゲーム制御・UI
│   ├── PuzzleGame.cs
│   ├── PuzzleView.cs
│   ├── PuzzleBridge.cs
│   ├── PuzzlePresenter.cs
│   ├── PuzzleDragger.cs
│   ├── PuzzleGameAgent.cs
│   ├── PuzzleGameBlockCreator.cs
│   └── VisualizeStateController.cs
├── LifeTimeScope/    # DI設定
│   └── PuzzleLifetimeScope.cs
├── Tests/            # テスト
│   ├── Editor/       # EditModeテスト
│   └── PuzzleFollowCameraTests.cs
└── (ルート)          # 共通機能
    ├── ICommand.cs   # コマンドパターン
    ├── MoveCommand.cs
    ├── InvokeCommand.cs
    ├── InstancedMeshRenderer.cs # GPU描画
    ├── PlayerController.cs
    ├── PuzzleFollowCamera.cs
    └── Puzzle.asmdef
```

## アセット構成

```
Assets/
├── Materials/        # マテリアル
├── Prefabs/          # プレハブ
├── Scenes/           # シーン
├── Settings/         # URP設定
├── TextMesh Pro/     # フォント
└── *.shader          # カスタムシェーダー
```

## アセンブリ構成
- `Assets/Scripts/Puzzle.asmdef` - コアパズルロジック
- `Assets/Scripts/Tests/Editor/EditModeTests.asmdef` - EditModeテスト（NUnit）
- `Assets/Scripts/Tests/PlayModeTests.asmdef` - PlayModeテスト（NUnit）