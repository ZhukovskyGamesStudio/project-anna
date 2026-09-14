using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;

public class FinalSequenceManager : MonoBehaviour
{
    public static FinalSequenceManager Instance;

    [Header("UI")]
    [SerializeField] private ObjectPanel underBedPanel;
    [SerializeField] private Image flashOverlay;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private PostGameScreen postGameScreen;
    [SerializeField] private float postGameDelay = 8f;

    [Header("Final Lines")]
    [SerializeField] private string firstLine = "Посмотри на себя";
    [SerializeField] private string mirrorLine = "В твоей голове тьма. Они пришли чтобы достать её оттуда.";
    [SerializeField] private string hideHint = "Подсказка: спрячьтесь";
    [SerializeField] private string openDoorHint = "Подсказка: откройте дверь";
    [SerializeField] private string uncertainBedLine = "Прятаться бессмысленно. Мне нужно открыть дверь.";
    [SerializeField] private string mirrorPromptHint = "Подсказка: посмотреть в зеркало";
    [SerializeField] private string mirrorFirstCheckLine = "Сначала нужно посмотреть в зеркало";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource knockSource;
    [SerializeField] private AudioSource sirenSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip firstVoiceClip;
    [SerializeField] private AudioClip secondVoiceClip;
    [SerializeField] private AudioClip knockingClip;
    [SerializeField] private AudioClip sirenClip;

    [Header("Final Video")]
    [SerializeField] private VideoOverlayPanel knockVideoPanel;
    [SerializeField] private VideoClip knockVideoClip;

    [Header("Door Ending (коридор)")]
    [SerializeField] private GameObject finalDoorObject;
    [SerializeField] private Transform corridorTeleportTarget;
    [SerializeField] private TextMeshProUGUI doorEndingText;
    [SerializeField] private float doorEndingTextDuration = 6f;
    [SerializeField] private ObjectPanel finalMirrorPanel;
    [SerializeField] private float mirrorViewDuration = 5f;

    [Header("Timings")]
    [SerializeField] private float firstVoiceFallbackDuration = 2f;
    [SerializeField] private float secondVoiceFallbackDuration = 4f;
    [SerializeField] private float pauseBeforeSiren = 1.5f;
    [SerializeField] private float flashDuration = 8f;
    [SerializeField] private float fadeDuration = 3.5f;
    [SerializeField] private float flashInterval = 0.2f;
    [SerializeField] private float flashAlpha = 0.35f;

    private Coroutine mirrorSequenceCoroutine;
    private Coroutine flashCoroutine;

    private enum FinalStage
    {
        None,
        MirrorPrompt,
        MirrorOpened,
        HidePrompt,
        Corridor,
        UnderBed,
        Finished
    }

    private FinalStage currentStage = FinalStage.None;

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

    private IEnumerator Start()
    {
        yield return null;

        if (DayManager.Instance == null)
        {
            yield break;
        }

        if (DayManager.Instance.DebugStartInCorridor)
        {
            StartCorridorForDebug();
        }
        else if (DayManager.Instance.DebugStartAtKnock)
        {
            StartAtKnockForDebug();
        }
    }

    private void StartAtKnockForDebug()
    {
        currentStage = FinalStage.HidePrompt;

        if (ObjectiveManager.Instance != null)
        {
            string hint = ShouldHintDoor() ? openDoorHint : hideHint;
            ObjectiveManager.Instance.SetOverrideTexts(hint, "");
        }

        if (finalDoorObject != null)
        {
            finalDoorObject.SetActive(true);
        }
    }

    private void StartCorridorForDebug()
    {
        currentStage = FinalStage.Corridor;

        if (finalDoorObject != null)
        {
            finalDoorObject.SetActive(false);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllNonPanelAudio();
            AudioManager.Instance.PlayCorridorMusic();
        }

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetOverrideTexts("", "");
        }

        TeleportPlayer(corridorTeleportTarget);
    }

    public bool HasStarted()
    {
        return currentStage != FinalStage.None;
    }

    public bool IsHideStage()
    {
        return currentStage == FinalStage.HidePrompt;
    }

    public bool IsBlockingBed()
    {
        return currentStage == FinalStage.MirrorPrompt || currentStage == FinalStage.MirrorOpened;
    }

    public bool ShouldKeepAmbientMuted()
    {
        return currentStage != FinalStage.None;
    }

    public bool BlockEscapeClose()
    {
        return currentStage == FinalStage.MirrorOpened || currentStage == FinalStage.Corridor || currentStage == FinalStage.UnderBed || currentStage == FinalStage.Finished;
    }

    public bool ShouldShowMirrorFinalLine()
    {
        return currentStage == FinalStage.MirrorOpened;
    }

    public string GetMirrorFinalLine()
    {
        return mirrorLine;
    }

    public void StartFinalSequence()
    {
        if (currentStage != FinalStage.None)
        {
            return;
        }

        currentStage = FinalStage.MirrorPrompt;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllNonPanelAudio();
        }

        PlayVoice(firstVoiceClip);

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetOverrideTexts(mirrorPromptHint, firstLine);
        }
    }

    public void OnMirrorOpened()
    {
        if (currentStage != FinalStage.MirrorPrompt)
        {
            return;
        }

        currentStage = FinalStage.MirrorOpened;

        if (mirrorSequenceCoroutine != null)
        {
            StopCoroutine(mirrorSequenceCoroutine);
        }

        mirrorSequenceCoroutine = StartCoroutine(MirrorSequence());
    }

    public void ShowBlockedBedLine()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.ShowTemporaryAnnaLine(mirrorFirstCheckLine);
        }
    }

    public void EnterUnderBed()
    {
        if (currentStage != FinalStage.HidePrompt)
        {
            return;
        }

        if (ShouldHintDoor())
        {
            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.ShowTemporaryAnnaLine(uncertainBedLine);
            }

            return;
        }

        currentStage = FinalStage.UnderBed;

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetOverrideTexts("", "");
        }

        StopAllFinalAudio();

        if (underBedPanel != null)
        {
            UIManager.Instance.OpenPanel(underBedPanel);
        }

        StartCoroutine(UnderBedSequence());
    }

    private IEnumerator MirrorSequence()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetOverrideTexts("", "");
        }

        PlayVoice(secondVoiceClip);

        float waitTime = secondVoiceClip != null ? secondVoiceClip.length : secondVoiceFallbackDuration;
        yield return new WaitForSeconds(waitTime);

        if (UIManager.Instance != null && UIManager.Instance.IsPanelOpen())
        {
            UIManager.Instance.CloseCurrentPanel();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllNonPanelAudio();
        }

        StartKnocking();

        mirrorSequenceCoroutine = null;

        if (IsWatchedEnding())
        {
            if (knockVideoPanel != null && knockVideoClip != null)
            {
                knockVideoPanel.PlayClip(knockVideoClip, OnWatchedVideoFinished, false);
            }
            else
            {
                OnWatchedVideoFinished();
            }
        }
        else
        {
            OnKnockVideoFinished();
        }
    }

    private void OnWatchedVideoFinished()
    {
        StartCoroutine(WatchedEndingSequence());
    }

    private IEnumerator WatchedEndingSequence()
    {
        currentStage = FinalStage.Finished;

        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.playerMovement != null)
            {
                UIManager.Instance.playerMovement.SetCanMove(false);
            }

            if (UIManager.Instance.playerLook != null)
            {
                UIManager.Instance.playerLook.SetCanLook(false);
            }
        }

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetOverrideTexts("", "");
        }

        HideCredits();
        ClearFlash();

        yield return StartCoroutine(FadeToBlack());

        StopAllFinalAudio();

        ShowCredits();
    }

    public bool IsWatchedEnding()
    {
        return DayManager.Instance != null
            && DayManager.Instance.GetLeaning() == DayManager.Leaning.Watched;
    }

    private void OnKnockVideoFinished()
    {
        currentStage = FinalStage.HidePrompt;

        if (ObjectiveManager.Instance != null)
        {
            string hint = ShouldHintDoor() ? openDoorHint : hideHint;
            ObjectiveManager.Instance.SetOverrideTexts(hint, "");
        }

        if (finalDoorObject != null)
        {
            finalDoorObject.SetActive(true);
        }
    }

    public void EnterCorridor()
    {
        if (currentStage != FinalStage.HidePrompt)
        {
            return;
        }

        currentStage = FinalStage.Corridor;

        if (finalDoorObject != null)
        {
            finalDoorObject.SetActive(false);
        }

        StopAllFinalAudio();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCorridorMusic();
        }

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetOverrideTexts("", "");
        }

        TeleportPlayer(corridorTeleportTarget);
    }

    public void FinishDoorEnding()
    {
        if (currentStage != FinalStage.Corridor)
        {
            return;
        }

        StartCoroutine(DoorEndingSequence());
    }

    public void OpenFinalMirror()
    {
        if (currentStage != FinalStage.Corridor)
        {
            return;
        }

        currentStage = FinalStage.Finished;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopCorridorMusic();
        }

        if (finalMirrorPanel != null && UIManager.Instance != null)
        {
            UIManager.Instance.OpenPanel(finalMirrorPanel);
        }

        StartCoroutine(FinalMirrorSequence());
    }

    private IEnumerator FinalMirrorSequence()
    {
        yield return new WaitForSeconds(mirrorViewDuration);

        HideCredits();
        ClearFlash();

        yield return StartCoroutine(FadeToBlack());

        ShowCredits();
    }

    private void TeleportPlayer(Transform target)
    {
        if (target == null || UIManager.Instance == null || UIManager.Instance.playerMovement == null)
        {
            return;
        }

        GameObject player = UIManager.Instance.playerMovement.gameObject;
        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.position = target.position;
        player.transform.rotation = target.rotation;

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private IEnumerator DoorEndingSequence()
    {
        currentStage = FinalStage.Finished;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopCorridorMusic();
        }

        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.playerMovement != null)
            {
                UIManager.Instance.playerMovement.SetCanMove(false);
            }

            if (UIManager.Instance.playerLook != null)
            {
                UIManager.Instance.playerLook.SetCanLook(false);
            }
        }

        HideCredits();
        ClearFlash();

        yield return StartCoroutine(FadeToBlack());

        if (doorEndingText != null)
        {
            doorEndingText.gameObject.SetActive(true);
            doorEndingText.transform.SetAsLastSibling();

            yield return new WaitForSeconds(doorEndingTextDuration);

            doorEndingText.gameObject.SetActive(false);
        }

        ShowCredits();
    }

    public bool ShouldHintDoor()
    {
        return DayManager.Instance != null
            && DayManager.Instance.GetLeaning() == DayManager.Leaning.Uncertain;
    }

    private IEnumerator UnderBedSequence()
    {
        HideCredits();
        ClearFlash();

        yield return new WaitForSeconds(pauseBeforeSiren);

        StartSiren();
        StartFlashing();

        yield return new WaitForSeconds(flashDuration);

        StopFlashing();

        yield return StartCoroutine(FadeToBlack());

        if (sirenSource != null)
        {
            sirenSource.Stop();
        }

        ShowCredits();
        currentStage = FinalStage.Finished;
    }

    private IEnumerator FadeToBlack()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float a = timer / fadeDuration;

            if (flashOverlay != null)
            {
                flashOverlay.color = new Color(0f, 0f, 0f, a);
                flashOverlay.transform.SetAsLastSibling();
            }

            yield return null;
        }
    }

    private void StopFlashing()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
    }

    private void PlayVoice(AudioClip clip)
    {
        if (voiceSource == null)
        {
            return;
        }

        voiceSource.Stop();

        if (clip != null)
        {
            voiceSource.clip = clip;
            voiceSource.loop = false;
            voiceSource.Play();
        }
    }

    private void StartKnocking()
    {
        if (knockSource == null || knockingClip == null)
        {
            return;
        }

        knockSource.Stop();
        knockSource.clip = knockingClip;
        knockSource.loop = true;
        knockSource.Play();
    }

    private void StartSiren()
    {
        if (sirenSource == null || sirenClip == null)
        {
            return;
        }

        sirenSource.Stop();
        sirenSource.clip = sirenClip;
        sirenSource.loop = true;
        sirenSource.Play();
    }

    private void StopAllFinalAudio()
    {
        if (voiceSource != null)
        {
            voiceSource.Stop();
        }

        if (knockSource != null)
        {
            knockSource.Stop();
        }

        if (sirenSource != null)
        {
            sirenSource.Stop();
        }
    }

    private void StartFlashing()
    {
        if (flashOverlay == null)
        {
            return;
        }

        flashOverlay.gameObject.SetActive(true);
        flashOverlay.transform.SetAsLastSibling();

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        bool showRed = true;

        while (true)
        {
            if (flashOverlay != null)
            {
                flashOverlay.color = showRed
                    ? new Color(1f, 0f, 0f, flashAlpha)
                    : new Color(0f, 0.35f, 1f, flashAlpha);

                flashOverlay.transform.SetAsLastSibling();
            }

            showRed = !showRed;
            yield return new WaitForSeconds(flashInterval);
        }
    }

    private void ClearFlash()
    {
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            flashOverlay.color = new Color(0f, 0f, 0f, 0f);
        }
    }

    private void ShowCredits()
    {
        if (creditsText != null)
        {
            creditsText.gameObject.SetActive(true);
            creditsText.transform.SetAsLastSibling();
        }

        RecordEnding();

        StartCoroutine(ShowPostGameAfterCredits());
    }

    private IEnumerator ShowPostGameAfterCredits()
    {
        yield return new WaitForSeconds(postGameDelay);

        if (postGameScreen != null)
        {
            postGameScreen.Show();
        }
    }

    private void RecordEnding()
    {
        if (DayManager.Instance == null)
        {
            return;
        }

        DayManager.Leaning leaning = DayManager.Instance.GetLeaning();

        if (leaning == DayManager.Leaning.Watched)
        {
            EndingProgress.MarkWatched();
        }
        else if (leaning == DayManager.Leaning.Ill)
        {
            EndingProgress.MarkIll();
        }
        else
        {
            EndingProgress.MarkUncertain();
        }
    }

    private void HideCredits()
    {
        if (creditsText != null)
        {
            creditsText.gameObject.SetActive(false);
        }
    }
}