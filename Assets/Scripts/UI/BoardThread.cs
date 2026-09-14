using UnityEngine;
using UnityEngine.UI;

public class BoardThread : MonoBehaviour
{
    [SerializeField] private float thickness = 4f;

    private RectTransform rect;
    private Image image;
    private RectTransform noteA;
    private RectTransform noteB;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void Setup(RectTransform a, RectTransform b)
    {
        noteA = a;
        noteB = b;
    }

    private void Update()
    {
        if (noteA == null || noteB == null || rect == null)
        {
            return;
        }

        bool bothOnBoard = noteA.gameObject.activeInHierarchy && noteB.gameObject.activeInHierarchy
            && IsOnBoard(noteA) && IsOnBoard(noteB);

        if (image != null)
        {
            image.enabled = bothOnBoard;
        }

        if (!bothOnBoard)
        {
            return;
        }

        Vector2 localA = WorldToContainer(noteA.position);
        Vector2 localB = WorldToContainer(noteB.position);
        Vector2 dir = localB - localA;
        float distance = dir.magnitude;

        rect.anchoredPosition = (localA + localB) * 0.5f;
        rect.sizeDelta = new Vector2(distance, thickness);
        rect.rotation = Quaternion.FromToRotation(Vector3.right, dir);
    }

    private bool IsOnBoard(RectTransform note)
    {
        return note.parent != null && note.parent.name == "BoardNotes";
    }

    private Vector2 WorldToContainer(Vector3 worldPos)
    {
        RectTransform parent = rect.parent as RectTransform;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, RectTransformUtility.WorldToScreenPoint(null, worldPos), null, out local);
        return local;
    }
}