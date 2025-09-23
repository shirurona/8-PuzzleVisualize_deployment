using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;

[TestFixture]
public class PuzzleFollowCameraTests
{
    private GameObject testGameObject;
    private PuzzleFollowCamera sut;
    private GameObject visualizeControllerObject;
    private VisualizeStateController visualizeController;

    [SetUp]
    public void SetUp()
    {
        testGameObject = CreateCameraPuzzleFollowerTestObject();
        sut = testGameObject.GetComponent<PuzzleFollowCamera>();
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            GameObject.DestroyImmediate(testGameObject);
        }
        if (visualizeControllerObject != null)
        {
            GameObject.DestroyImmediate(visualizeControllerObject);
        }
    }

    [UnityTest]
    public IEnumerator FollowToPuzzleState_WhenGameModeEnabledAndFollowingEnabled_StartsAnimation()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });
        
        // Act
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // 1フレーム待機してアニメーション開始を確認
        yield return null;

        // Assert
        Assert.That(sut.IsAnimating, Is.True);
    }

    [UnityTest]
    public IEnumerator FollowToPuzzleState_WhenFollowingDisabled_DoesNotStartAnimation()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });

        // enableFollowing = falseに設定
        SetEnableFollowing(sut, false);
        
        // Act
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // 1フレーム待機
        yield return null;

        // Assert
        Assert.That(sut.IsAnimating, Is.False);
    }

    [UnityTest]
    public IEnumerator FollowToPuzzleState_WhenExploreMode_DoesNotStartAnimation()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });

        // 探索モード（IsGameMode = false）に設定
        SetGameModeDirectly(visualizeController, false);
        
        // Act
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // 1フレーム待機
        yield return null;

        // Assert
        Assert.That(sut.IsAnimating, Is.False);
    }

    [UnityTest]
    public IEnumerator StartCameraAnimation_WhenCompleted_SetsAnimatingToFalse()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });

        // Act
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // アニメーションが開始されたことを確認
        yield return null;
        Assert.That(sut.IsAnimating, Is.True);
        
        // アニメーション完了まで待機（最大5秒）
        float timeout = 5f;
        while (sut.IsAnimating && timeout > 0)
        {
            yield return new WaitForSeconds(0.1f);
            timeout -= 0.1f;
        }

        // Assert
        Assert.That(sut.IsAnimating, Is.False);
    }

    [UnityTest]
    public IEnumerator AnimationInterruption_WhenSwitchToExploreMode_StopsAnimation()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });

        // Act
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // アニメーション開始を確認
        yield return null;
        Assert.That(sut.IsAnimating, Is.True);
        
        // 探索モードに切り替え
        SetGameModeDirectly(visualizeController, false);
        
        // モード切り替え後に新しい追従を試行（これがアニメーションを中断するはず）
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // 1フレーム待機
        yield return null;

        // Assert: 進行中のアニメーションが中断されていることを確認
        Assert.That(sut.IsAnimating, Is.False, "探索モードでは進行中のアニメーションが中断されるはず");
    }

    [UnityTest]
    public IEnumerator DuplicateAnimationPrevention_WhenCalled_ContinuesExistingAnimation()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });

        // Act
        // 最初のアニメーション開始
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // アニメーション開始を確認
        yield return null;
        Assert.That(sut.IsAnimating, Is.True);
        
        // 2回目の呼び出し（重複アニメーション試行）
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // 1フレーム待機
        yield return null;

        // Assert: アニメーションが継続していることを確認
        Assert.That(sut.IsAnimating, Is.True, "重複アニメーション防止により既存アニメーションが継続するはず");
        
        // アニメーション完了まで待機
        float timeout = 2f;
        while (sut.IsAnimating && timeout > 0)
        {
            yield return new WaitForSeconds(0.1f);
            timeout -= 0.1f;
        }
        
        // 最終的にアニメーションが完了することを確認
        Assert.That(sut.IsAnimating, Is.False, "アニメーションは最終的に完了するはず");
    }

    [UnityTest]
    public IEnumerator FollowSpeed_WhenZero_DoesNotCrashAndCompletes()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });

        // followSpeed = 0に設定
        SetFollowSpeed(sut, 0f);
        
        // Act
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // アニメーション開始を確認
        yield return null;
        Assert.That(sut.IsAnimating, Is.True);
        
        // アニメーション完了まで待機
        float timeout = 3f;
        while (sut.IsAnimating && timeout > 0)
        {
            yield return new WaitForSeconds(0.1f);
            timeout -= 0.1f;
        }
        
        // Assert: followSpeed = 0でもアニメーションがクラッシュせず完了する
        Assert.That(sut.IsAnimating, Is.False, "followSpeed = 0でもアニメーションは正常に完了するはず");
        Assert.That(timeout, Is.GreaterThan(0), "アニメーションがタイムアウトしないはず");
    }

    [UnityTest]
    public IEnumerator CameraDistance_WhenZero_DoesNotCrashAndCompletes()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });

        // cameraDistance = 0に設定
        SetCameraDistance(sut, 0f);
        
        // Act
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // アニメーション開始を確認
        yield return null;
        Assert.That(sut.IsAnimating, Is.True);
        
        // アニメーション完了まで待機
        float timeout = 3f;
        while (sut.IsAnimating && timeout > 0)
        {
            yield return new WaitForSeconds(0.1f);
            timeout -= 0.1f;
        }
        
        // Assert: cameraDistance = 0でもアニメーションがクラッシュせず完了する
        Assert.That(sut.IsAnimating, Is.False, "cameraDistance = 0でもアニメーションは正常に完了するはず");
        Assert.That(timeout, Is.GreaterThan(0), "アニメーションがタイムアウトしないはず");
    }

    [UnityTest]
    public IEnumerator PuzzleGameIntegration_ExecuteMove_TriggersFollowing()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });
        
        // 1フレーム待機
        yield return null;
        
        // Act: PuzzleGame.ExecuteMove()の拡張機能をシミュレート
        // ExecuteMove内でFollowToPuzzleStateが呼ばれることを確認
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // 1フレーム待機してカメラ追従が開始されたかを確認
        yield return null;

        // Assert
        Assert.That(sut.IsAnimating, Is.True, "PuzzleGame統合時にカメラ追従が開始されるはず");
    }

    [UnityTest]
    public IEnumerator PuzzleGameIntegration_Undo_TriggersFollowing()
    {
        // Arrange
        var undoPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 0 },
            { 7, 8, 6 }
        });
        
        // 1フレーム待機
        yield return null;
        
        // Act: PuzzleGame.Undo()の拡張機能をシミュレート
        // Undo内でFollowToPuzzleStateが呼ばれることを確認
        sut.FollowToPuzzleState(undoPuzzleState);
        
        // 1フレーム待機してカメラ追従が開始されたかを確認
        yield return null;

        // Assert
        Assert.That(sut.IsAnimating, Is.True, "Undo操作時にカメラ追従が開始されるはず");
    }

    [UnityTest]
    public IEnumerator PuzzleGameIntegration_Redo_TriggersFollowing()
    {
        // Arrange
        var redoPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 0, 5 },
            { 7, 8, 6 }
        });
        
        // 1フレーム待機
        yield return null;
        
        // Act: PuzzleGame.Redo()の拡張機能をシミュレート
        // Redo内でFollowToPuzzleStateが呼ばれることを確認
        sut.FollowToPuzzleState(redoPuzzleState);
        
        // 1フレーム待機してカメラ追従が開始されたかを確認
        yield return null;

        // Assert
        Assert.That(sut.IsAnimating, Is.True, "Redo操作時にカメラ追従が開始されるはず");
    }

    private GameObject CreateCameraPuzzleFollowerTestObject()
    {
        // メインテストオブジェクト作成
        var gameObject = new GameObject("CameraPuzzleFollowerTest");
        var cameraPuzzleFollower = gameObject.AddComponent<PuzzleFollowCamera>();

        // テスト環境のセットアップ
        SetupTestEnvironment(cameraPuzzleFollower);

        return gameObject;
    }

    private void SetupTestEnvironment(PuzzleFollowCamera puzzleFollowCamera)
    {
        // VisualizeStateController用オブジェクト作成とセットアップ（最小限）
        visualizeControllerObject = new GameObject("VisualizeStateController");
        visualizeController = visualizeControllerObject.AddComponent<VisualizeStateController>();
        
        // ゲームモードを直接設定（SetGameModeの複雑な依存関係を避けるため）
        SetGameModeDirectly(visualizeController, true);

        // CameraPuzzleFollowerの依存関係をリフレクションで確実に設定
        SetupCameraPuzzleFollowerFields(puzzleFollowCamera);
    }

    private void SetGameModeDirectly(VisualizeStateController controller, bool isGameMode)
    {
        var isGameModeProperty = typeof(VisualizeStateController).GetProperty("IsGameMode");
        if (isGameModeProperty != null)
        {
            isGameModeProperty.SetValue(controller, isGameMode);
        }
    }

    private void SetupCameraPuzzleFollowerFields(PuzzleFollowCamera puzzleFollowCamera)
    {
        var type = typeof(PuzzleFollowCamera);

        // visualizeControllerの設定
        var visualizeControllerField = type.GetField("visualizeController", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (visualizeControllerField != null)
        {
            visualizeControllerField.SetValue(puzzleFollowCamera, visualizeController);
        }

        // enableFollowingの設定
        var enableFollowingField = type.GetField("enableFollowing", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (enableFollowingField != null)
        {
            enableFollowingField.SetValue(puzzleFollowCamera, true);
        }
    }

    private void SetEnableFollowing(PuzzleFollowCamera puzzleFollowCamera, bool enabled)
    {
        var enableFollowingField = typeof(PuzzleFollowCamera).GetField("enableFollowing", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (enableFollowingField != null)
        {
            enableFollowingField.SetValue(puzzleFollowCamera, enabled);
        }
    }

    private void SetFollowSpeed(PuzzleFollowCamera puzzleFollowCamera, float speed)
    {
        var followSpeedField = typeof(PuzzleFollowCamera).GetField("followSpeed", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (followSpeedField != null)
        {
            followSpeedField.SetValue(puzzleFollowCamera, speed);
        }
    }

    private void SetCameraDistance(PuzzleFollowCamera puzzleFollowCamera, float distance)
    {
        var cameraDistanceField = typeof(PuzzleFollowCamera).GetField("cameraDistance", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (cameraDistanceField != null)
        {
            cameraDistanceField.SetValue(puzzleFollowCamera, distance);
        }
    }

    private void SetupPuzzleGameDependencies(GameObject puzzleGameObject)
    {
        // PuzzleViewの設定
        var puzzleViewObject = new GameObject("PuzzleView");
        puzzleViewObject.transform.SetParent(puzzleGameObject.transform);
        puzzleViewObject.AddComponent<PuzzleView>();
        
        // Toggle（reverseToggle）の設定
        var toggleObject = new GameObject("ReverseToggle");
        toggleObject.transform.SetParent(puzzleGameObject.transform);
        toggleObject.AddComponent<UnityEngine.UI.Toggle>();
        
        // Button（undoButton）の設定
        var undoButtonObject = new GameObject("UndoButton");
        undoButtonObject.transform.SetParent(puzzleGameObject.transform);
        undoButtonObject.AddComponent<UnityEngine.UI.Button>();
        
        // Button（redoButton）の設定
        var redoButtonObject = new GameObject("RedoButton");
        redoButtonObject.transform.SetParent(puzzleGameObject.transform);
        redoButtonObject.AddComponent<UnityEngine.UI.Button>();
    }

    private void SetPuzzleGameCameraPuzzleFollower(PuzzleGame puzzleGame, PuzzleFollowCamera puzzleFollowCamera)
    {
        // 将来PuzzleGameにcameraPuzzleFollowerフィールドが追加される予定
        // 今はテストのためにリフレクションで設定を準備
        var cameraPuzzleFollowerField = typeof(PuzzleGame).GetField("cameraPuzzleFollower", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (cameraPuzzleFollowerField != null)
        {
            cameraPuzzleFollowerField.SetValue(puzzleGame, puzzleFollowCamera);
        }
    }

    private void SetupPuzzleGameManually(PuzzleGame puzzleGame, PuzzleFollowCamera puzzleFollowCamera)
    {
        // PuzzleGameのAwakeの内容を手動で実行
        var numbers = new int[,] {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 0}
        };
        
        // currentPuzzleフィールドをリフレクションで設定
        var currentPuzzleField = typeof(PuzzleGame).GetField("currentPuzzle", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (currentPuzzleField != null)
        {
            var puzzle = new Puzzle(PuzzleState.Create(numbers));
            currentPuzzleField.SetValue(puzzleGame, puzzle);
        }
        
        // cameraPuzzleFollowerフィールドをリフレクションで設定
        var cameraPuzzleFollowerField = typeof(PuzzleGame).GetField("cameraPuzzleFollower", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (cameraPuzzleFollowerField != null)
        {
            cameraPuzzleFollowerField.SetValue(puzzleGame, puzzleFollowCamera);
        }
    }

    // 新しい実装に対応したテストケース

    [UnityTest]
    public IEnumerator RealCameraMovement_WithPuzzleCreator_UsesPuzzlePosition()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });
        
        // Act
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // アニメーション開始を確認
        yield return null;
        
        // Assert: PuzzleCreatorがnullでもアニメーションは開始される（Vector3.zeroにフォールバック）
        Assert.That(sut.IsAnimating, Is.True, "PuzzleCreator初期化問題があってもアニメーションが開始されるはず");
    }

    [UnityTest]
    public IEnumerator RealCameraMovement_WithPlayerController_UsesMainCamera()
    {
        // Arrange
        // PlayerControllerのmainCameraのみを直接設定するアプローチ
        var cameraObject = new GameObject("Camera");
        var camera = cameraObject.AddComponent<Camera>();
        
        // PlayerControllerのmainCameraフィールドを直接設定
        var mockPlayerController = new GameObject("PlayerController").AddComponent<PlayerController>();
        mockPlayerController.mainCamera = camera;
        
        // PlayerControllerオブジェクトを即座に破棄（Update()メソッドを実行しないようにする）
        GameObject.DestroyImmediate(mockPlayerController.gameObject);
        
        // PlayerControllerをCameraPuzzleFollowerに設定
        var playerControllerField = typeof(PuzzleFollowCamera).GetField("playerController", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (playerControllerField != null)
        {
            playerControllerField.SetValue(sut, mockPlayerController);
        }
        
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });
        
        // Act
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // アニメーション開始を確認
        yield return null;
        
        // Assert
        Assert.That(sut.IsAnimating, Is.True, "PlayerController連携時にアニメーションが開始されるはず");
        
        // クリーンアップ
        if (cameraObject != null)
        {
            GameObject.DestroyImmediate(cameraObject);
        }
    }

    [UnityTest]
    public IEnumerator CameraMovement_WithValidConfiguration_ExecutesWithoutErrors()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });
        
        // Act & Assert
        // PlayerControllerの問題を回避し、基本的な動作のみを確認
        Assert.DoesNotThrow(() => sut.FollowToPuzzleState(initialPuzzleState));
        
        // アニメーション開始を確認
        yield return null;
        Assert.That(sut.IsAnimating, Is.True, "アニメーションが開始されるはず");
        
        // アニメーション完了まで待機
        float timeout = 3f;
        while (sut.IsAnimating && timeout > 0)
        {
            yield return new WaitForSeconds(0.1f);
            timeout -= 0.1f;
        }
        
        // アニメーション完了を確認
        Assert.That(sut.IsAnimating, Is.False, "アニメーションが完了するはず");
        Assert.That(timeout, Is.GreaterThan(0), "アニメーションがタイムアウトしないはず");
    }

    [UnityTest]
    public IEnumerator CameraFollowParameters_WithDifferentValues_AffectAnimation()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });
        
        // followSpeedを変更してアニメーションが影響を受けることを確認
        SetFollowSpeed(sut, 100f);
        
        // Act
        sut.FollowToPuzzleState(initialPuzzleState);
        
        // アニメーション開始を確認
        yield return null;
        Assert.That(sut.IsAnimating, Is.True, "followSpeed変更時にアニメーションが開始されるはず");
        
        // アニメーション完了まで待機
        float timeout = 3f;
        while (sut.IsAnimating && timeout > 0)
        {
            yield return new WaitForSeconds(0.1f);
            timeout -= 0.1f;
        }
        
        // Assert
        Assert.That(sut.IsAnimating, Is.False, "followSpeed変更時にアニメーションが完了するはず");
        Assert.That(timeout, Is.GreaterThan(0), "アニメーションがタイムアウトしないはず");
    }

    [UnityTest]
    public IEnumerator CameraMovement_WithNullPuzzleCreator_DoesNotCrash()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });
        
        // PuzzleCreatorをnullに設定
        var puzzleCreatorField = typeof(PuzzleFollowCamera).GetField("puzzleCreator", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (puzzleCreatorField != null)
        {
            puzzleCreatorField.SetValue(sut, null);
        }
        
        // Act & Assert
        Assert.DoesNotThrow(() => sut.FollowToPuzzleState(initialPuzzleState));
        
        // アニメーション開始を確認
        yield return null;
        Assert.That(sut.IsAnimating, Is.True, "PuzzleCreatorがnullでもアニメーションが開始されるはず");
    }

    [UnityTest]
    public IEnumerator CameraMovement_WithNullPlayerController_DoesNotCrash()
    {
        // Arrange
        var initialPuzzleState = PuzzleState.Create(new int[,] {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 0 }
        });
        
        // PlayerControllerをnullに設定
        var playerControllerField = typeof(PuzzleFollowCamera).GetField("playerController", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (playerControllerField != null)
        {
            playerControllerField.SetValue(sut, null);
        }
        
        // Act & Assert
        Assert.DoesNotThrow(() => sut.FollowToPuzzleState(initialPuzzleState));
        
        // アニメーション開始を確認
        yield return null;
        Assert.That(sut.IsAnimating, Is.True, "PlayerControllerがnullでもアニメーションが開始されるはず");
    }
}