using System;
using UnityEngine;

public class PuzzleView : MonoBehaviour
{
    [SerializeField] private PuzzleGameBlockCreator blockCreator;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float tweenDuration = 0.2f;

    public void AnimateMove(PuzzleState puzzleState, PuzzleState pastPuzzleState)
    {
        BlockPosition from = puzzleState.EmptyBlockPosition;
        BlockPosition to = pastPuzzleState.EmptyBlockPosition;
        
        if (from.Equals(to))
        {
            return;
        }
        
        int stepRow = Math.Sign(to.Row - from.Row);
        int stepCol = Math.Sign(to.Column - from.Column);

        int distance = Math.Abs(to.Row - from.Row) + Math.Abs(to.Column - from.Column);
        for (int i = 0; i < distance; i++)
        {
            var panelCurrentPos = new BlockPosition(from.Row + i * stepRow, from.Column + i * stepCol);
            var panelTargetPos = new BlockPosition(from.Row + (i + 1) * stepRow, from.Column + (i + 1) * stepCol);

            var pieceNumber = pastPuzzleState[panelCurrentPos];

            if (pieceNumber.IsZero()) continue;

            try
            {
                TweenAsync(pieceNumber, panelCurrentPos, panelTargetPos);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
    
    public async Awaitable TweenAsync(BlockNumber num, BlockPosition from, BlockPosition to)
    {
        var rect = blockCreator.GetBlockRect(num);
        var zeroBlockView = blockCreator.GetBlock(BlockNumber.Zero());
        var zeroRect = blockCreator.GetBlockRect(BlockNumber.Zero());
        Vector2 fromPos = blockCreator.GetLocalPosition(from);
        Vector2 toPos = blockCreator.GetLocalPosition(to);
        float elapsedTime = 0f;
        while (elapsedTime < tweenDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / tweenDuration);
            float value = curve.Evaluate(progress);

            rect.anchoredPosition = Vector2.Lerp(fromPos, toPos, value);
            if (!zeroBlockView.IsDragging)
            {
                zeroRect.anchoredPosition = Vector2.Lerp(toPos, fromPos, value);
            }
            await Awaitable.NextFrameAsync();
        }
        rect.anchoredPosition = toPos;
        if (!zeroBlockView.IsDragging)
        {
            zeroRect.anchoredPosition = fromPos;
        }
    }
}
