using UnityEngine;

public class WindowHallucinationManager : MonoBehaviour
{
    [SerializeField] private GameObject[] day1Hallucinations;
    [SerializeField] private GameObject[] day2Hallucinations;
    [SerializeField] private GameObject[] day3Hallucinations;

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

        ShowSet(day1Hallucinations, day == 1);
        ShowSet(day2Hallucinations, day == 2);
        ShowSet(day3Hallucinations, day == 3);
    }

    private void ShowSet(GameObject[] set, bool show)
    {
        if (set == null)
        {
            return;
        }

        for (int i = 0; i < set.Length; i++)
        {
            if (set[i] != null)
            {
                set[i].SetActive(show);
            }
        }
    }
}