# テスト指針

## 基本原則

- ユニットテストは小さく、高速で、独立していること
  - 各テストは1つの機能や動作のみをテストする
  - テスト間の依存関係を避ける
- テストはドキュメントとしての役割も果たす
  - テスト名は「何をテストするか」を明確に示す
  - テストコードは実装の使用例としても機能する
- 可能な限り本物のプロダクトコードを使ってテストする。やむを得ないときのみテストダブルを使用する

**テストの実行** `../commands/run-tests.md`を参照してください。
**テストの手順** `../commands/implement-code.md`を参照してください。

## テスト記述規則

- GameObjectやComponentなど、Unityのフレームワークを使用するテストはPlayModeテストを記述する
- 純粋なC#クラスなど、Unityのフレームワークを使用しないテストはEditModeテストを記述する
- SeralializeField属性のフィールドの参照がなくてテストができない場合のみリフレクションで参照を与える
- テストコードには XML Documentation Comments は不要
- 各コアクラスに対応するテストクラスを記述する
- テストにはNUnit3ベースのUnity Test Frameworkを使用する。See: https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/index.html
- テストクラスには `TestFixture` 属性をつけること
- テストメソッドの構造
  - Arrange, Act, Assert のパターンに従う
  - 各セクションの間を空行で区切ること。コメントは不要
- Assertは1つのテストメソッドにつき、1つのみとする
- Assertには制約モデル（`Assert.That`）を使用する。ドキュメントを参照して、最適な制約を使用すること。See: https://docs.nunit.org/api/NUnit.Framework.Constraints.html
- `Assert.That` の 引数 `message` は指定しない。テスト名と制約で十分に意図が伝わるようにすること
- テストコードはシンプルなシングルパスであること
  - Never use `if`, `switch`, `for`, `foreach`, and the ternary operator in test code.
- Parameterized tests を積極的に使用する
  - Arrangeが異なりActとAssertが同じテストは、`TestCase`, `TestCaseSource`, `Values`, `ValueSource` 属性を使用してパラメータ化できる
  - `ParametrizedIgnore` 属性で組み合わせの除外もできる
- テストで使用するオブジェクトの生成は、creation method pattern を積極的に使用すること。e.g., `private GameObject CreateSystemUnderTestObject()`
  - `TearDown` でリソースを開放するために private field に保持する場合でも、テストメソッドでは常に creation method の戻り値を使用する
- 各テストは独立して実行できること。他のテストの実行結果に依存しない
- テスト中に `GameObject` を生成する場合、テストメソッドに `CreateScene` 属性をつけること。もしすでに `LoadScene` 属性がついていれば不要
- 安易に`LogAssert`によるログメッセージの検証は避け、必要ならばSpyを作成して使用すること
- 非同期テストでは、むやみに `Delay`, `Wait` による指定時間待機を使用しないこと。1フレーム待つだけなら `yield return null` を使用できる
- 非同期メソッドで例外がスローされることを検証する場合、`Throws` 制約ではなく、try-catchブロックを使用して、例外が発生することを確認する（Unity Test Frameworkの制限事項）
```csharp
try
{
    await Foo.Bar(-1);
    Assert.Fail("例外が出ることを期待しているのでテスト失敗とする");
}
catch (ArgumentException expectedException)
{
    Assert.That(expectedException.Message, Is.EqualTo("Semper Paratus!"));
}
```

## テスト命名規則

- テストアセンブリ名は、テスト対象アセンブリ名 + ".Tests" とする
- テストコードの名前空間は、テスト対象と同一とする
- テストクラス名は テスト対象クラス名 + "Test" とする。e.g., `public class CharacterControllerTest`
- テストメソッド名は「テスト対象メソッド名」「条件」「期待される結果」をアンダースコアで連結した形式を使用する。e.g., `public void TakeDamage_WhenHealthIsZero_CharacterDies()`
- テスト対象オブジェクトには`sut`、実測値には`actual`、期待値には`expected`と命名し、役割を明示すること
- テストダブルを使用する場合、xUnit Test Patterns (xUTP) の定義に従って `stub`, `spy`, `dummy`, `fake`, `mock` のいずれかの接頭辞を使用すること

## アセンブリとフレームワーク

### アセンブリ構成
- `Assets/Scripts/Puzzle.asmdef` - コアパズルロジック
- `Assets/Scripts/Tests/Editor/EditModeTests.asmdef` - EditModeテスト
- `Assets/Scripts/Tests/PlayModeTests.asmdef` - PlayModeテスト

### テストフレームワーク
- **Unity Test Framework 1.4.5**
- **NUnit 2.0.5**: 単体テストフレームワーク
- **Performance Testing 3.0.3**: パフォーマンステスト

### テストファイル配置
- `Assets/Scripts/Tests/Editor/` - EditModeテスト
- `Assets/Scripts/Tests/` - PlayModeテスト

### テスト失敗時の対処手順

1. エラーメッセージを確認して原因を特定 
2. 期待値と実際の値の差異を分析 
3. 修正後、同じテストを再実行して確認 
4. 連続して失敗する場合は、テストコードと実装の両方を見直す 
5. 2連続で失敗する場合、状況を整理してユーザーに指示を仰ぐ

### テストコードは変更しない

テストコードは変更しないでください。
テストは仕様を表すものであり、実装の正しさを検証するためのものです。

テストが失敗している場合は、実装側を修正してください。
テストコードを実装に合わせないでください。

テストが誤っていると思ったらユーザーに質問してください。
独自判断でテストを書き換えないでください。

#### テストコードを変更しても良い例外

テストコードを変更して良い例外は以下のような場合です。

- テストを追加するタスクを依頼されている。
- テストを修正するタスクを依頼されている。
- テストコードに明らかな構文エラーがある。
- テスト仕様が矛盾している。
    - この場合は独自に判断するのではなく質問して確認してください。
- テストコードがテスト対象のAPIと互換性がなくなっている。
    - この場合は独自に判断するのではなく質問して確認してください。

### テストデータに依存した条件分岐は避ける

実装コードがテストで使用されている具体的なデータ値を特別扱いすることは、通常望ましくありません。
具体的なデータ値とは、例えば変数名やテーブル名などです。

これは以下のような問題を引き起こします。

- 脆弱なテスト: テストデータが変更されると実装が機能しなくなる。
- 隠れた仕様: 特定のデータ名に対する特別な処理が明示的な仕様ではなく暗黙的になる。
- 汎用性の欠如: 実際の運用環境では機能しない可能性がある。

#### 具体的なデータ値を特別扱いして良い例外

以下のような場合は、特定の値に基づく分岐が正当化されます。

- 意味的な特別扱い: 予約語、システムテーブル、特殊フラグなど、仕様上特別な意味を持つ値。
- 明示的な設定: 設定ファイルなどで明示された特別処理。
- ドメイン固有の規則: 業務ドメインで明確に定義された例外規則。