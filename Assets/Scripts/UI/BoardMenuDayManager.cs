using UnityEngine;

public class BoardMenuDayManager : MonoBehaviour
{
    [SerializeField] private NoteDraggable[] day1Notes;
    [SerializeField] private NoteDraggable[] day2Notes;
    [SerializeField] private NoteDraggable[] day3Notes;

    private void Start()
    {
        ApplyCurrentDay();

        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayChanged += OnDayChanged;
        }
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayChanged -= OnDayChanged;
        }
    }

    private void OnDayChanged(int day)
    {
        ApplyCurrentDay();
    }

    private void ApplyCurrentDay()
    {
        int day = 1;

        if (DayManager.Instance != null)
        {
            day = DayManager.Instance.CurrentDay;
        }

        ShowSet(day1Notes, day == 1);
        ShowSet(day2Notes, day == 2);
        ShowSet(day3Notes, day == 3);
    }

    private void ShowSet(NoteDraggable[] set, bool show)
    {
        if (set == null)
        {
            return;
        }

        for (int i = 0; i < set.Length; i++)
        {
            if (set[i] == null)
            {
                continue;
            }

            if (set[i].IsOnBoard())
            {
                continue;
            }

            set[i].gameObject.SetActive(show);
        }
    }
}