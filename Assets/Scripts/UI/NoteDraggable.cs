using UnityEngine;
using UnityEngine.EventSystems;

public class NoteDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform boardArea;

    private RectTransform rect;
    private Canvas canvas;

    private Transform menuParent;
    private int menuSiblingIndex;
    private Vector2 menuAnchorMin;
    private Vector2 menuAnchorMax;
    private Vector2 menuAnchoredPos;

    private bool onBoard;

    public bool IsOnBoard()
    {
        return onBoard;
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        menuParent = transform.parent;
        menuSiblingIndex = transform.GetSiblingIndex();
        menuAnchorMin = rect.anchorMin;
        menuAnchorMax = rect.anchorMax;
        menuAnchoredPos = rect.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        CenterAnchors();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rect == null || canvas == null)
        {
            return;
        }

        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (boardArea == null || rect == null)
        {
            return;
        }

        bool insideBoard = RectTransformUtility.RectangleContainsScreenPoint(boardArea, eventData.position, eventData.pressEventCamera);

        if (!insideBoard)
        {
            ReturnToMenu();
            return;
        }

        transform.SetParent(boardArea, true);
        CenterAnchors();
        transform.SetAsLastSibling();
        ClampToBoard();
        onBoard = true;
    }

    private void ClampToBoard()
    {
        Vector2 boardHalf = boardArea.rect.size * 0.5f;
        Vector2 noteHalf = rect.rect.size * 0.5f;
        Vector2 pos = rect.anchoredPosition;

        float maxX = Mathf.Max(0f, boardHalf.x - noteHalf.x);
        float maxY = Mathf.Max(0f, boardHalf.y - noteHalf.y);

        pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
        pos.y = Mathf.Clamp(pos.y, -maxY, maxY);
        rect.anchoredPosition = pos;
    }

    private void CenterAnchors()
    {
        Vector3 worldPos = rect.position;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.position = worldPos;
    }

    private void ReturnToMenu()
    {
        transform.SetParent(menuParent, false);
        rect.anchorMin = menuAnchorMin;
        rect.anchorMax = menuAnchorMax;
        rect.anchoredPosition = menuAnchoredPos;
        transform.SetSiblingIndex(menuSiblingIndex);
        onBoard = false;
    }
}