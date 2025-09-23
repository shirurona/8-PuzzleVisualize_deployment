using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class BlockView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private int blockNumberValue;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    private RectTransform _rectTransform;
    private RectTransform _canvasRectTransform;
    
    private BlockNumber _blockNumber;
    public static event Action<BlockNumber> OnBlockClicked;
    public static event Action<Vector2> OnBlockDragging;
    public static event Action OnBlockEndDrag;
    public bool IsDragging { get; private set; }

    private void Awake()
    {
        SetBlockNumber(new BlockNumber(blockNumberValue));
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(RectTransform canvasRect)
    {
        _canvasRectTransform = canvasRect;
    }

    public void SetBlockNumber(BlockNumber number)
    {
        _blockNumber = number;
        textMeshPro.text = number.ToString();
    } 

    public void OnPointerClick(PointerEventData eventData)
    {
        OnBlockClicked?.Invoke(_blockNumber);
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_blockNumber.IsZero() || PuzzleGameAgent.IsAutoSolving) return;

        IsDragging = true;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!_blockNumber.IsZero() || PuzzleGameAgent.IsAutoSolving) return;
        
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _canvasRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 mouseWorldPosition
        );
        Vector3 centerToPivotOffset = _rectTransform.position - _rectTransform.TransformPoint(_rectTransform.rect.center);
        
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            eventData.pressEventCamera,
            mouseWorldPosition + centerToPivotOffset
        );
        _rectTransform.position = screenPoint;
        OnBlockDragging?.Invoke(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_blockNumber.IsZero() || PuzzleGameAgent.IsAutoSolving) return;
        
        OnBlockEndDrag?.Invoke();
        IsDragging = false;
    }
}
