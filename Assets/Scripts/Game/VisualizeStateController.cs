using UnityEngine;

public class VisualizeStateController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PuzzleGame puzzleGame;
    public bool IsGameMode { get; private set; } = false;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (!IsGameMode)
            {
                SetGameMode();
            }
            else
            {
                UnSetGameMode();
            }
        }
    }

    public void SetGameMode()
    {
        IsGameMode = true;
        
        playerController.myRigidbody.isKinematic = true;
        playerController.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        puzzleGame.enabled = true;
    }

    public void UnSetGameMode()
    {
        IsGameMode = false;
        
        playerController.enabled = true;
        playerController.myRigidbody.isKinematic = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        puzzleGame.enabled = false;
    }
    
    
}
