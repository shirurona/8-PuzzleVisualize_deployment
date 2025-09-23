using UnityEngine;
using VContainer;

public class PuzzleBridge : MonoBehaviour
{
    [SerializeField] PuzzleFinderView finderView;
    private Puzzle _puzzle;

    [Inject]
    public void Initialize(Puzzle puzzle)
    {
        _puzzle = puzzle;
    }

    public void FinderToGame()
    {
        PuzzleState puzzleState = finderView.GetCurrentState();
        _puzzle.SetPuzzle(puzzleState);
    }

    public void GameToFinder()
    {
        finderView.SetPuzzle(_puzzle.State.CurrentValue);
    }
}
