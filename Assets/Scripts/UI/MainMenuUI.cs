using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private string gameSceneName = "Main";
    [SerializeField] private AudioSource menuMusicSource;
    [SerializeField] private TextMeshProUGUI languageLabel;

    private void Start()
    {
        Time.timeScale = 1f;

        // Заявки лаунчеру подаются заново в каждом запуске: папка заявок
        // у разных сборок разная, а пометка в PlayerPrefs — общая.
        LauncherAchievements.ResubmitUnlocked();
        EndingProgress.SyncAchievements();

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (GameSettings.Instance != null)
        {
            if (musicSlider != null)
            {
                musicSlider.value = GameSettings.Instance.GetMusicVolume();
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = GameSettings.Instance.GetSfxVolume();
            }

            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.value = GameSettings.Instance.GetMouseSensitivity();
            }

            if (vSyncToggle != null)
            {
                vSyncToggle.isOn = GameSettings.Instance.GetVSync();
            }
        }

        if (menuMusicSource != null && !menuMusicSource.isPlaying)
        {
            menuMusicSource.loop = true;
            menuMusicSource.Play();
        }

        UpdateLanguageLabel();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnLanguageButtonPressed()
    {
        Localizer.ToggleLanguage();
        UpdateLanguageLabel();
    }

    private void UpdateLanguageLabel()
    {
        if (languageLabel != null)
        {
            languageLabel.text = Localizer.IsEnglish ? "EN" : "RU";
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        if (menuMusicSource != null)
        {
            menuMusicSource.Stop();
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.SetMusicVolume(value);
        }
    }

    public void OnSfxVolumeChanged(float value)
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.SetSfxVolume(value);
        }
    }

    public void OnMouseSensitivityChanged(float value)
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.SetMouseSensitivity(value);
        }
    }

    public void OnVSyncChanged(bool value)
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.SetVSync(value);
        }
    }
}