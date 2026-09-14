using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DayPostProcessManager : MonoBehaviour
{
    [SerializeField] private PostProcessVolume postProcessVolume;
    [SerializeField] private PostProcessProfile day1Profile;
    [SerializeField] private PostProcessProfile day2Profile;
    [SerializeField] private PostProcessProfile day3Profile;

    private void Start()
    {
        ApplyCurrentDayProfile();

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
        ApplyCurrentDayProfile();
    }

    public void ApplyCurrentDayProfile()
    {
        if (postProcessVolume == null)
        {
            return;
        }

        if (DayManager.Instance == null)
        {
            postProcessVolume.profile = day1Profile;
            return;
        }

        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            postProcessVolume.profile = day1Profile;
        }
        else if (day == 2)
        {
            postProcessVolume.profile = day2Profile;
        }
        else
        {
            postProcessVolume.profile = day3Profile;
        }
    }
}