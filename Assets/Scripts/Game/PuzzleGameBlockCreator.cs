using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class PuzzleGameBlockCreator : MonoBehaviour
{
    [SerializeField] private float spacing = 20.0f;
    [SerializeField] private float size = 100.0f;
    [SerializeField] private RectTransform gameNumberBlockPrefab;
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private Toggle visibleEmptyToggle;
    
    private BlockView[] children = new BlockView[PuzzleState.TotalCells];
    private Vector2?[] positions = new Vector2?[PuzzleState.TotalCells];
    
    [Inject]
    public void Initialize(Puzzle puzzle)
    {
        PuzzleState initialPuzzle = puzzle.State.CurrentValue;
        for (int row = 0; row < PuzzleState.RowCount; row++)
        {
            for (int column = 0; column < PuzzleState.ColumnCount; column++)
            {
                var blockPos = new BlockPosition(row, column);
                BlockNumber number = initialPuzzle[blockPos];
                GetBlockRect(number).anchoredPosition = GetLocalPosition(blockPos);
            }
        }
        visibleEmptyToggle.onValueChanged.AddListener(GetBlock(BlockNumber.Zero()).gameObject.SetActive);
    }

    public Vector2 GetLocalPosition(BlockPosition position)
    {
        if (!positions[position].HasValue)
        {
            float posX = (position.Column - 1) * (size + spacing);
            float posY = (1 - position.Row) * (size + spacing);
            positions[position] = new Vector2(posX, posY);
        }
        return positions[position].Value;
    }
    
    public BlockView GetBlock(BlockNumber number)
    {
        if (children[number] == null)
        {
            children[number] = Instantiate(gameNumberBlockPrefab, transform).GetComponent<BlockView>();
            children[number].GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
            children[number].Initialize(canvasRectTransform);
            children[number].SetBlockNumber(number);
        }
        return children[number];
    }

    public RectTransform GetBlockRect(BlockNumber number)
    {
        return GetBlock(number).GetComponent<RectTransform>();
    }
}
