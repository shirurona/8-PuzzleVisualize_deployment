using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PuzzleLifetimeScope : LifetimeScope
{
    [SerializeField] private PuzzleCreator creator;
    [SerializeField] private PuzzleGame game;
    [SerializeField] private PuzzleGameAgent agent;
    [SerializeField] private PuzzleBridge bridge;
    [SerializeField] private PuzzleDragger dragger;
    [SerializeField] private PuzzleGameBlockCreator blockCreator;
    [SerializeField] private PuzzlePresenter presenter;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(CreateInstance());
        builder.RegisterComponent(creator);
        builder.RegisterComponent(game);
        builder.RegisterComponent(agent);
        builder.RegisterComponent(bridge);
        builder.RegisterComponent(dragger);
        builder.RegisterComponent(presenter);
        builder.RegisterComponent(blockCreator);
    }

    private Puzzle CreateInstance()
    {
        var numbers = new [,]
        {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 0}
        };
        return new Puzzle(PuzzleState.Create(numbers));// 3x3のブロック配列を作成
    }
}