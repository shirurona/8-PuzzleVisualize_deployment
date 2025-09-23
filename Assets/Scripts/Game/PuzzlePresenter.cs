using UnityEngine;
using R3;
using VContainer;

/// <summary>
/// Puzzleの変更をPuzzleViewとPuzzleGameとPuzzleFollowCameraに適用する
/// </summary>
public class PuzzlePresenter : MonoBehaviour
{
    [SerializeField] private PuzzleView puzzleView;
    [SerializeField] private PuzzleFollowCamera puzzleFollowCamera;
    
    [Inject]
    public void Initialize(Puzzle puzzle)
    {
        puzzle.State
            .Pairwise()
            .Subscribe(x =>
            {
                puzzleView.AnimateMove(x.Current, x.Previous);
                puzzleFollowCamera.FollowToPuzzleState(x.Current);
            })
            .AddTo(this);
    }
}