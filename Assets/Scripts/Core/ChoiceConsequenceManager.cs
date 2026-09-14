using UnityEngine;
using System.Collections;

public class ChoiceConsequenceManager : MonoBehaviour
{
    [Header("Реплики Анны на 3 день")]
    [TextArea(2, 4)]
    [SerializeField] private string watchedLine = "Они всё ещё здесь. Я чувствую их взгляд.";
    [TextArea(2, 4)]
    [SerializeField] private string uncertainLine = "Я больше не понимаю, что из этого настоящее.";
    [TextArea(2, 4)]
    [SerializeField] private string illLine = "Кажется, стало тише. Может, это правда помогает.";

    [Header("Деталь в комнате на 3 день")]
    [SerializeField] private GameObject[] watchedObjects;
    [SerializeField] private GameObject[] uncertainObjects;
    [SerializeField] private GameObject[] illObjects;

    [Header("Тайминг реплики")]
    [SerializeField] private float lineDelay = 2f;

    private bool appliedForDay3;

    private void Start()
    {
        HideAll();
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
        int day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;

        if (day != 3)
        {
            HideAll();
            return;
        }

        if (appliedForDay3)
        {
            return;
        }

        appliedForDay3 = true;

        DayManager.Leaning leaning = DayManager.Instance != null
            ? DayManager.Instance.GetLeaning()
            : DayManager.Leaning.Uncertain;

        ShowSet(watchedObjects, leaning == DayManager.Leaning.Watched);
        ShowSet(uncertainObjects, leaning == DayManager.Leaning.Uncertain);
        ShowSet(illObjects, leaning == DayManager.Leaning.Ill);

        StartCoroutine(ShowLineDelayed(GetLineForLeaning(leaning)));
    }

    private string GetLineForLeaning(DayManager.Leaning leaning)
    {
        if (leaning == DayManager.Leaning.Watched)
        {
            return watchedLine;
        }

        if (leaning == DayManager.Leaning.Ill)
        {
            return illLine;
        }

        return uncertainLine;
    }

    private IEnumerator ShowLineDelayed(string line)
    {
        yield return new WaitForSeconds(lineDelay);

        if (ObjectiveManager.Instance != null && !string.IsNullOrEmpty(line))
        {
            ObjectiveManager.Instance.ShowTemporaryAnnaLine(line);
        }
    }

    private void HideAll()
    {
        ShowSet(watchedObjects, false);
        ShowSet(uncertainObjects, false);
        ShowSet(illObjects, false);
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
