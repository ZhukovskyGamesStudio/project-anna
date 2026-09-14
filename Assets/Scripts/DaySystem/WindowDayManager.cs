using UnityEngine;

public class WindowDayManager : MonoBehaviour
{
    [SerializeField] private GameObject day1Set;
    [SerializeField] private GameObject day2Set;
    [SerializeField] private GameObject day3Set;

    private void Start()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayChanged += ApplyDay;
            ApplyDay(DayManager.Instance.CurrentDay);
        }
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayChanged -= ApplyDay;
        }
    }

    private void ApplyDay(int day)
    {
        if (day1Set != null)
        {
            day1Set.SetActive(day == 1);
        }

        if (day2Set != null)
        {
            day2Set.SetActive(day == 2);
        }

        if (day3Set != null)
        {
            day3Set.SetActive(day == 3);
        }
    }
}