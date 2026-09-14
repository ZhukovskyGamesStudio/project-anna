using UnityEngine;

public class HallucinationManager : MonoBehaviour
{
    public static HallucinationManager Instance;

    [SerializeField] private HallucinationInteractable[] hallucinations;
    [Range(0f, 1f)] [SerializeField] private float hallucinationVolume = 0.5f;

    private int lastDay = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyVolumeToAll();
        RefreshHallucinations();
    }

    private void ApplyVolumeToAll()
    {
        if (hallucinations == null)
        {
            return;
        }

        for (int i = 0; i < hallucinations.Length; i++)
        {
            if (hallucinations[i] != null)
            {
                hallucinations[i].SetVolumeMultiplier(hallucinationVolume);
            }
        }
    }

    public void RefreshHallucinations()
    {
        if (DayManager.Instance == null || hallucinations == null)
        {
            return;
        }

        int currentDay = DayManager.Instance.CurrentDay;

        if (currentDay != lastDay)
        {
            for (int i = 0; i < hallucinations.Length; i++)
            {
                if (hallucinations[i] != null)
                {
                    hallucinations[i].ResetForNewDay();
                }
            }

            lastDay = currentDay;
        }

        bool shouldShow = DayManager.Instance.ShouldShowHallucinationsForCurrentDay();

        for (int i = 0; i < hallucinations.Length; i++)
        {
            if (hallucinations[i] == null)
            {
                continue;
            }

            bool canAppearToday = hallucinations[i].CanAppearOnDay(currentDay);
            hallucinations[i].gameObject.SetActive(shouldShow && canAppearToday);
        }
    }

    public int GetActiveHallucinationCount()
    {
        if (hallucinations == null)
        {
            return 0;
        }

        int count = 0;

        for (int i = 0; i < hallucinations.Length; i++)
        {
            if (hallucinations[i] != null && hallucinations[i].gameObject.activeInHierarchy)
            {
                count++;
            }
        }

        return count;
    }

    public void NotifyHallucinationChanged()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.UpdateHallucinationsObjectiveState();
        }
    }
}