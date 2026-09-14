using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource panelMusicSource;
    [SerializeField] private AudioSource radioSource;
    [SerializeField] private AudioSource radioStaticSource;
    [SerializeField] private AudioSource phoneSource;
    [SerializeField] private AudioSource oneShotSource;

    [Header("Day Ambients")]
    [SerializeField] private AudioClip day1Ambient;
    [SerializeField] private AudioClip day2Ambient;
    [SerializeField] private AudioClip day3Ambient;

    [Header("Dream Ambients")]
    [SerializeField] private AudioClip dream1Ambient;
    [SerializeField] private AudioClip dream2Ambient;
    [SerializeField] private AudioClip dream3Ambient;

    [Header("Panel Music")]
    [SerializeField] private AudioClip defaultPanelMusicClip;

    [Header("Corridor Music")]
    [SerializeField] private AudioSource corridorMusicSource;
    [SerializeField] private AudioClip corridorMusicClip;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip interactTapClip;

    [Header("Radio Sounds")]
    [SerializeField] private AudioClip radioStaticClip;
    [SerializeField] private float radioStartDelay = 1.2f;
    [Range(0f, 1f)] [SerializeField] private float radioStaticVolume = 0.3f;
    [Range(0f, 1f)] [SerializeField] private float radioVoiceVolume = 1f;
    [SerializeField] private AudioClip day1RadioVoice;
    [SerializeField] private AudioClip day2RadioVoice;
    [SerializeField] private AudioClip day3RadioVoice;

    [Header("Phone Sounds")]
    [SerializeField] private AudioClip phoneBeepClip;
    [SerializeField] private AudioClip day1PhoneCallClip;
    [SerializeField] private AudioClip day2PhoneCallClip;
    [SerializeField] private AudioClip day3PhoneCallClip;
    [SerializeField] private AudioClip day1PsychiatristVoiceClip;
    [SerializeField] private AudioClip day2PsychiatristVoiceClip;
    [SerializeField] private AudioClip day3PsychiatristVoiceClip;


private void Start()
{
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

private void OnDayChanged(int newDay)
{
    Debug.Log($"Day changed to {newDay}");
}

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
    
    public void PlayDreamAmbientForDay(int day)
    {
        if (ambientSource == null) return;

        AudioClip clip = null;
        
        if (day == 1)
            clip = dream1Ambient;
        else if (day == 2)
            clip = dream2Ambient;
        else if (day == 3)
            clip = dream3Ambient;

        ambientSource.Stop();
        ambientSource.clip = clip;
        ambientSource.loop = true;
        
        if (clip != null)
            ambientSource.Play();
    }

    public void PlayAmbientForCurrentDay()
    {
        if (DayManager.Instance == null || ambientSource == null)
        {
            return;
        }

        AudioClip clip = null;
        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            clip = day1Ambient;
        }
        else if (day == 2)
        {
            clip = day2Ambient;
        }
        else
        {
            clip = day3Ambient;
        }

        ambientSource.Stop();
        ambientSource.clip = clip;
        ambientSource.loop = true;

        if (clip != null)
        {
            ambientSource.Play();
        }
    }

    public void PlayDreamAmbientForCurrentDay()
{
    if (DayManager.Instance != null)
        PlayDreamAmbientForDay(DayManager.Instance.CurrentDay);
}

    public void PlayDefaultPanelMusic()
    {
        if (panelMusicSource == null || defaultPanelMusicClip == null)
        {
            return;
        }

        panelMusicSource.Stop();
        panelMusicSource.clip = defaultPanelMusicClip;
        panelMusicSource.loop = true;
        panelMusicSource.Play();
    }

    public void StopPanelMusic()
    {
        if (panelMusicSource != null)
        {
            panelMusicSource.Stop();
        }
    }

    public void PlayCorridorMusic()
    {
        if (corridorMusicSource == null || corridorMusicClip == null)
        {
            return;
        }

        corridorMusicSource.Stop();
        corridorMusicSource.clip = corridorMusicClip;
        corridorMusicSource.loop = true;
        corridorMusicSource.Play();
    }

    public void StopCorridorMusic()
    {
        if (corridorMusicSource != null)
        {
            corridorMusicSource.Stop();
        }
    }

    public void StopAllNonPanelAudio()
    {
        if (ambientSource != null)
        {
            ambientSource.Stop();
        }

        StopRadioAudio();
        StopPhoneAudio();
    }

    public void PlayClick()
    {
        if (oneShotSource != null && clickClip != null)
        {
            oneShotSource.PlayOneShot(clickClip);
        }
    }

    public void PlayInteractTap()
    {
        if (oneShotSource != null && interactTapClip != null)
        {
            oneShotSource.PlayOneShot(interactTapClip);
        }
    }

    public float GetRadioStartDelay()
    {
        return radioStartDelay;
    }

    public void PlayRadioStatic()
    {
        if (radioStaticSource == null || radioStaticClip == null)
        {
            return;
        }

        radioStaticSource.Stop();
        radioStaticSource.clip = radioStaticClip;
        radioStaticSource.loop = true;
        radioStaticSource.volume = radioStaticVolume;
        radioStaticSource.Play();
    }

    public void StopRadioStatic()
    {
        if (radioStaticSource != null)
        {
            radioStaticSource.Stop();
        }
    }

    public float GetRadioVoiceLengthForCurrentDay()
    {
        if (DayManager.Instance == null)
        {
            return 0f;
        }

        AudioClip clip = null;
        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            clip = day1RadioVoice;
        }
        else if (day == 2)
        {
            clip = day2RadioVoice;
        }
        else
        {
            clip = day3RadioVoice;
        }

        if (clip == null)
        {
            return 0f;
        }

        return clip.length;
    }

    public void PlayRadioVoiceForCurrentDay()
    {
        if (radioSource == null || DayManager.Instance == null)
        {
            return;
        }

        AudioClip clip = null;
        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            clip = day1RadioVoice;
        }
        else if (day == 2)
        {
            clip = day2RadioVoice;
        }
        else
        {
            clip = day3RadioVoice;
        }

        radioSource.Stop();

        if (clip != null)
        {
            radioSource.clip = clip;
            radioSource.loop = true;
            radioSource.volume = radioVoiceVolume;
            radioSource.Play();
        }
    }

    public void StopRadioAudio()
    {
        StopRadioVoice();
        StopRadioStatic();
    }

    public void StopRadioVoice()
    {
        if (radioSource != null)
        {
            radioSource.Stop();
        }
    }

    public void PlayPhoneBeep()
    {
        if (oneShotSource != null && phoneBeepClip != null)
        {
            oneShotSource.PlayOneShot(phoneBeepClip);
        }
    }

    public float GetPhoneCallDelayForCurrentDay()
    {
        if (DayManager.Instance == null)
        {
            return 0f;
        }

        AudioClip callClip = null;
        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            callClip = day1PhoneCallClip;
        }
        else if (day == 2)
        {
            callClip = day2PhoneCallClip;
        }
        else
        {
            callClip = day3PhoneCallClip;
        }

        if (callClip == null)
        {
            return 0f;
        }

        return callClip.length;
    }

    public float GetPhoneVoiceLengthForCurrentDay()
    {
        if (DayManager.Instance == null)
        {
            return 0f;
        }

        AudioClip clip = null;
        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            clip = day1PsychiatristVoiceClip;
        }
        else if (day == 2)
        {
            clip = day2PsychiatristVoiceClip;
        }
        else
        {
            clip = day3PsychiatristVoiceClip;
        }

        if (clip == null)
        {
            return 0f;
        }

        return clip.length;
    }

    public void PlayPhoneCallForCurrentDay()
    {
        if (phoneSource == null || DayManager.Instance == null)
        {
            return;
        }

        AudioClip clip = null;
        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            clip = day1PhoneCallClip;
        }
        else if (day == 2)
        {
            clip = day2PhoneCallClip;
        }
        else
        {
            clip = day3PhoneCallClip;
        }

        phoneSource.Stop();

        if (clip != null)
        {
            phoneSource.clip = clip;
            phoneSource.loop = false;
            phoneSource.Play();
        }
    }

    public void PlayPhoneVoiceForCurrentDay()
    {
        if (phoneSource == null || DayManager.Instance == null)
        {
            return;
        }

        AudioClip clip = null;
        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            clip = day1PsychiatristVoiceClip;
        }
        else if (day == 2)
        {
            clip = day2PsychiatristVoiceClip;
        }
        else
        {
            clip = day3PsychiatristVoiceClip;
        }

        phoneSource.Stop();

        if (clip != null)
        {
            phoneSource.clip = clip;
            phoneSource.loop = true;
            phoneSource.Play();
        }
    }

    public void StopPhoneAudio()
    {
        if (phoneSource != null)
        {
            phoneSource.Stop();
        }
    }
}