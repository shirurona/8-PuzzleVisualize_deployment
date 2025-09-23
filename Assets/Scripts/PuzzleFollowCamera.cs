using System.Collections;
using UnityEngine;

public class PuzzleFollowCamera : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PuzzleCreator puzzleCreator;
    [SerializeField] private VisualizeStateController visualizeController;
    
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float tweenDuration = 0.2f;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private bool enableFollowing = true;
    
    // 内部状態
    private bool isAnimating = false;
    private Coroutine currentAnimationCoroutine;
    // テスト用プロパティ
    public bool IsAnimating => isAnimating;

    public void FollowToPuzzleState(PuzzleState puzzleState)
    {
        // 追従条件チェック
        if (!CanFollowPuzzle()) 
        {
            // 条件が満たされない場合は進行中のアニメーションを停止
            StopCurrentAnimation();
            return;
        }
        
        // 進行中のアニメーションがある場合は停止
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        
        // アニメーション開始
        StartAnimation(puzzleState);
    }

    private bool CanFollowPuzzle()
    {
        return enableFollowing && IsGameModeActive();
    }

    private bool IsGameModeActive()
    {
        return visualizeController != null && visualizeController.IsGameMode;
    }

    private void StartAnimation(PuzzleState puzzleState)
    {
        isAnimating = true;
        CameraFollowAnimation(puzzleState);
    }

    private void StopCurrentAnimation()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        isAnimating = false;
    }

    private void CameraFollowAnimation(PuzzleState puzzleState)
    {
        // パズル状態の3D位置を取得
        Vector3 puzzlePosition = GetPuzzlePosition(puzzleState);
        
        // カメラの目標位置と回転を計算
        Vector3 targetCameraPosition = CalculateCameraPosition(puzzlePosition);
        Quaternion targetCameraRotation = Quaternion.LookRotation(Vector3.forward);
        SetCameraRotation(targetCameraRotation);
        
        // アニメーション開始時の位置と回転を記録
        Vector3 startPosition = GetCurrentCameraPosition();

        CameraTweenAsync(startPosition, targetCameraPosition);
        
        // アニメーション完了
        isAnimating = false;
    }
    
    public async Awaitable CameraTweenAsync(Vector3 startPosition, Vector3 targetPositon)
    {
        float elapsedTime = 0f;
        while (elapsedTime < tweenDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / tweenDuration);
            float value = curve.Evaluate(progress);

            SetCameraPosition(CalculateCameraPosition(Vector3.Lerp(startPosition, targetPositon, value)));
            await Awaitable.NextFrameAsync();
        }
        SetCameraPosition(CalculateCameraPosition(targetPositon));
    }

    private Vector3 GetPuzzlePosition(PuzzleState puzzleState)
    {
        if (puzzleCreator != null)
        {
            return puzzleCreator.GetBoardStatePosition(puzzleState);
        }
        return Vector3.zero;
    }

    private Vector3 CalculateCameraPosition(Vector3 puzzlePosition)
    {
        return puzzlePosition + Vector3.back * cameraDistance;
    }

    private Vector3 GetCurrentCameraPosition()
    {
        if (playerController != null && playerController.mainCamera != null)
        {
            return playerController.mainCamera.transform.position;
        }
        return Vector3.zero;
    }

    private Quaternion GetCurrentCameraRotation()
    {
        if (playerController != null && playerController.mainCamera != null)
        {
            return playerController.mainCamera.transform.rotation;
        }
        return Quaternion.identity;
    }

    private void SetCameraPosition(Vector3 position)
    {
        if (playerController != null && playerController.mainCamera != null)
        {
            playerController.mainCamera.transform.position = position;
        }
    }

    private void SetCameraRotation(Quaternion rotation)
    {
        if (playerController != null && playerController.mainCamera != null)
        {
            playerController.mainCamera.transform.rotation = rotation;
        }
    }
}