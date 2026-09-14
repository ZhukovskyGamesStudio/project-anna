using UnityEngine;

public class DayCustomEffectsManager : MonoBehaviour
{
    [System.Serializable]
    public class DayEffectsPreset
    {
        public bool rgbShiftOn = true;
        public float rgbShiftAmount = 0.006f;
        public float rgbShiftSpeed = 0f;

        public bool hueSaturationOn = true;
        public int saturation = 0;
        public int angle = 154;
        public float hueSpeed = 0f;

        public bool posterizeOn = true;
        public int posterizeLevel = 180;
    }

    [SerializeField] private RGBShiftEffect rgbShiftEffect;
    [SerializeField] private HueSaturationEffect hueSaturationEffect;
    [SerializeField] private PosterizeEffect posterizeEffect;

    [SerializeField] private DayEffectsPreset day1Preset;
    [SerializeField] private DayEffectsPreset day2Preset;
    [SerializeField] private DayEffectsPreset day3Preset;

    private void Start()
    {
        ApplyCurrentDayPreset();

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
        ApplyCurrentDayPreset();
    }

    public void ApplyCurrentDayPreset()
    {
        if (DayManager.Instance == null)
        {
            ApplyPreset(day1Preset);
            return;
        }

        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            ApplyPreset(day1Preset);
        }
        else if (day == 2)
        {
            ApplyPreset(day2Preset);
        }
        else
        {
            ApplyPreset(day3Preset);
        }
    }

    private void ApplyPreset(DayEffectsPreset preset)
    {
        if (preset == null)
        {
            return;
        }

        if (rgbShiftEffect != null)
        {
            rgbShiftEffect.on = preset.rgbShiftOn;
            rgbShiftEffect.amount = preset.rgbShiftAmount;
            rgbShiftEffect.speed = preset.rgbShiftSpeed;
        }

        if (hueSaturationEffect != null)
        {
            hueSaturationEffect.on = preset.hueSaturationOn;
            hueSaturationEffect.saturation = preset.saturation;
            hueSaturationEffect.angle = preset.angle;
            hueSaturationEffect.speed = preset.hueSpeed;
        }

        if (posterizeEffect != null)
        {
            posterizeEffect.on = preset.posterizeOn;
            posterizeEffect.level = preset.posterizeLevel;
        }
    }
}