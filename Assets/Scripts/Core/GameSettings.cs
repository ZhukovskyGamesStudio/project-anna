using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float defaultMusicVolume = 0.7f;
    [SerializeField] private float defaultSfxVolume = 0.85f;
    [SerializeField] private float defaultMouseSensitivity = 4f;
    [SerializeField] private bool defaultVSync = true;

    private float musicVolume;
    private float sfxVolume;
    private float mouseSensitivity;
    private bool vSyncEnabled;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string MouseSensitivityKey = "MouseSensitivity";
    private const string VSyncKey = "VSync";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
            ApplySettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ApplySettingsNextFrame());
    }

    private IEnumerator ApplySettingsNextFrame()
    {
        yield return null;
        ApplySettings();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp(value, 0.0001f, 1f);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        ApplyMusicVolume();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp(value, 0.0001f, 1f);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        ApplySfxVolume();
    }

    public void SetMouseSensitivity(float value)
    {
        mouseSensitivity = Mathf.Clamp(value, 0.1f, 20f);
        PlayerPrefs.SetFloat(MouseSensitivityKey, mouseSensitivity);
        ApplyMouseSensitivity();
    }

    public void SetVSync(bool value)
    {
        vSyncEnabled = value;
        PlayerPrefs.SetInt(VSyncKey, vSyncEnabled ? 1 : 0);
        ApplyVSync();
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSfxVolume()
    {
        return sfxVolume;
    }

    public float GetMouseSensitivity()
    {
        return mouseSensitivity;
    }

    public bool GetVSync()
    {
        return vSyncEnabled;
    }

    public void ResetToDefaults()
    {
        musicVolume = defaultMusicVolume;
        sfxVolume = defaultSfxVolume;
        mouseSensitivity = defaultMouseSensitivity;
        vSyncEnabled = defaultVSync;

        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.SetFloat(MouseSensitivityKey, mouseSensitivity);
        PlayerPrefs.SetInt(VSyncKey, vSyncEnabled ? 1 : 0);

        ApplySettings();
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey(MusicVolumeKey))
        {
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey);
        }
        else
        {
            musicVolume = defaultMusicVolume;
        }

        if (PlayerPrefs.HasKey(SfxVolumeKey))
        {
            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey);
        }
        else
        {
            sfxVolume = defaultSfxVolume;
        }

        if (PlayerPrefs.HasKey(MouseSensitivityKey))
        {
            mouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityKey);
        }
        else
        {
            mouseSensitivity = defaultMouseSensitivity;
        }

        if (PlayerPrefs.HasKey(VSyncKey))
        {
            vSyncEnabled = PlayerPrefs.GetInt(VSyncKey) == 1;
        }
        else
        {
            vSyncEnabled = defaultVSync;
        }
    }

    private void ApplySettings()
    {
        ApplyMusicVolume();
        ApplySfxVolume();
        ApplyMouseSensitivity();
        ApplyVSync();
    }

    private void ApplyMusicVolume()
    {
        if (audioMixer == null)
        {
            return;
        }

        float db = Mathf.Log10(musicVolume) * 20f;
        audioMixer.SetFloat("MusicVolume", db);
    }

    private void ApplySfxVolume()
    {
        if (audioMixer == null)
        {
            return;
        }

        float db = Mathf.Log10(sfxVolume) * 20f;
        audioMixer.SetFloat("SfxVolume", db);
    }

    private void ApplyMouseSensitivity()
    {
        PlayerLook playerLook = FindObjectOfType<PlayerLook>();

        if (playerLook != null)
        {
            playerLook.SetMouseSensitivity(mouseSensitivity);
        }
    }

    private void ApplyVSync()
    {
        QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;

        if (vSyncEnabled)
        {
            Application.targetFrameRate = -1;
        }
    }
}