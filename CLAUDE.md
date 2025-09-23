# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 開発方針・品質哲学

### 基本原則
- 要件の不明点は計画段階で必ず確認する
- 曖昧な要件は実装前にユーザーと共有理解を図る
- 推測実装を避け、機能を細分化して反復的に進める
- 公式APIドキュメントを必ず参照する
- 常に日本語で可愛らしく会話する

### コード品質基準
- **読み手に優しいコード** — 次のエンジニアがすぐ理解できる
- **品質最優先** — 妥協を許さず最高水準を追求
- **シンプルさこそ正義（KISS）** — 最も単純で意図が明快な実装
- **ボーイスカウト・ルール** — 触れたモジュールは「来たときよりきれい」に
- **最小依存** — 不要な外部依存を追加しない

### コーディング規約・ベストプラクティス
- 日本語コメントを適切に使用
- 例外処理でのArgumentNullException、ArgumentOutOfRangeException使用
- readonly structの活用（BlockNumber、BlockPosition）
- IEquatable<T>、ICloneable実装による型安全性
- 静的解析: Roslynアナライザーの使用
- EditorConfigによるコードフォーマット統一
- 一貫したC#命名規則の遵守

## プロジェクト概要

8-puzzleゲームの可視化アプリケーション。Unity 6000.0.26f1でC#で実装。
プレイヤーは8-puzzleをプレイしながら、検索アルゴリズムによる状態空間の探索を3D空間で可視化できる革新的な教育ツール。

### 主要機能
- **デュアルモード**: ゲームモード（右クリック有効）/ 探索モード（デフォルト）
- **インタラクティブパズル**: キーボード・マウス・ドラッグ対応
- **完全Undo/Redo**: コマンドパターンによる操作履歴
- **3D状態空間可視化**: GPU Instancingによる数千状態の同時表示
- **検索アルゴリズム**: BFS・DFS・完全空間探索
- **リアルタイム経路表示**: プレイヤー移動の3D可視化

## 技術スタック
- **VContainer**: 依存性注入コンテナ
- **R3**: 次世代Reactive Extensions for Unity
- **UniTask**: Unity向け非同期処理ライブラリ

## パフォーマンスガイドライン
- GPU Instancingによる大量オブジェクト描画
- Dictionary<PuzzleState, T>によるパズル状態の高速検索
- ビットマスクによるブロック番号の効率的な検証
- メモリ割り当て最小化: struct使用とオブジェクトプール
- UniTaskによるノンブロッキング実装

## メモリ管理
- IDisposableパターンの実装（InstancedMeshRenderer）
- OnDestroy()でのリソース解放

## 推奨事項とベストプラクティス

### 新機能追加時の考慮事項
- インターフェース設計: 既存のパターンに従う
- テスト駆動: 新機能にはテストを併せて作成
- アセンブリ分離: 適切なアセンブリに配置
- 依存性管理: 循環参照の回避

### プロジェクト管理
- タスク分割: 機能を小さな単位に分解
- 進捗管理: 定期的な進捗確認と調整
- リスク管理: 潜在的問題の事前特定と対策

### 保守性向上
- モジュール化: 機能の適切な分離
- インターフェース設計: 将来の変更に対応可能な設計
- 技術的負債管理: 定期的な負債の解消

## 詳細ドキュメント
- 📐 **アーキテクチャ**: @`.claude/documents/architecture.md`
- 🔧 **実装詳細**: @`.claude/documents/implementation-details.md`
- 📁 **プロジェクト構造**: @`.claude/documents/project-structure.md`
- 🧪 **テスト**: @`.claude/documents/testing.md`