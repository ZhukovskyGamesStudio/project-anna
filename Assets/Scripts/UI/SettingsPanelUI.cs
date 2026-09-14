using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        if (GameSettings.Instance == null)
        {
            return;
        }

        musicSlider.SetValueWithoutNotify(GameSettings.Instance.GetMusicVolume());
        sfxSlider.SetValueWithoutNotify(GameSettings.Instance.GetSfxVolume());
    }

    public void OnMusicSliderChanged(float value)
    {
        if (GameSettings.Instance == null)
        {
            return;
        }

        GameSettings.Instance.SetMusicVolume(value);
    }

    public void OnSfxSliderChanged(float value)
    {
        if (GameSettings.Instance == null)
        {
            return;
        }

        GameSettings.Instance.SetSfxVolume(value);
    }
}