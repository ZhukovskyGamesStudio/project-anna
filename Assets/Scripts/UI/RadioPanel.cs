using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class RadioDayData
{
    [TextArea(3, 10)]
    public string[] normalLines;

    [TextArea(3, 10)]
    public string tuningLine1;

    [TextArea(3, 10)]
    public string tuningLine2;

    [TextArea(3, 10)]
    public string signalRestoredLine;

    [TextArea(3, 10)]
    public string choiceTextA;

    [TextArea(3, 10)]
    public string choiceTextB;

    [TextArea(3, 10)]
    public string radioReplyA;

    [TextArea(3, 10)]
    public string radioReplyB;

    [TextArea(3, 10)]
    public string[] extraLinesAfterReplyA;

    [TextArea(3, 10)]
    public string[] extraLinesAfterReplyB;

    [TextArea(3, 10)]
    public string annaFinalLine = "Хорошо";

    public int scoreA;
    public int scoreB;
}

public class RadioPanel : ObjectPanel
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float fallbackTypingSpeed = 0.03f;
    [SerializeField] private float typingSpeedMultiplier = 1f;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button tuneButton;
    [SerializeField] private Button choiceButtonA;
    [SerializeField] private Button choiceButtonB;
    [SerializeField] private TextMeshProUGUI choiceButtonAText;
    [SerializeField] private TextMeshProUGUI choiceButtonBText;

    [Header("Dialogue By Day")]
    [SerializeField] private RadioDayData day1Data;
    [SerializeField] private RadioDayData day2Data;
    [SerializeField] private RadioDayData day3Data;

    [Header("Extra Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip lostSignalClip;
    [SerializeField] private AudioClip tuningClip;
    [SerializeField] private AudioClip signalRestoredClip;

    [Header("Repeated Listen")]
    [SerializeField] private string repeatedListenLine = "Я уже слушала радио сегодня";

    private RadioDayData currentData;
    private int currentNormalIndex;
    private int tuningStep;
    private int selectedChoice = -1;
    private int currentExtraReplyIndex;
    private string[] currentExtraReplyLines;

    private Coroutine typingCoroutine;
    private Coroutine startDelayCoroutine;
    private Coroutine beginLineCoroutine;

    private bool isTyping;
    private int lastNextFrame = -1;
    private int pendingScore;
    private bool isWaitingForSignal;

    private RadioState currentState;
    private DisplayMode currentDisplayMode;
    private string currentDisplayedLine = "";

    private enum RadioState
    {
        Normal,
        LostSignal,
        Choosing,
        RadioReply,
        ExtraReplyLines,
        AnnaConfirm,
        Finished
    }

    private enum DisplayMode
    {
        None,
        Radio,
        Anna
    }

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextLine);
        }

        if (tuneButton != null)
        {
            tuneButton.onClick.AddListener(OnTunePressed);
        }

        if (choiceButtonA != null)
        {
            choiceButtonA.onClick.AddListener(OnChoiceAPressed);
        }

        if (choiceButtonB != null)
        {
            choiceButtonB.onClick.AddListener(OnChoiceBPressed);
        }
    }

    private void OnDestroy()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextLine);
        }

        if (tuneButton != null)
        {
            tuneButton.onClick.RemoveListener(OnTunePressed);
        }

        if (choiceButtonA != null)
        {
            choiceButtonA.onClick.RemoveListener(OnChoiceAPressed);
        }

        if (choiceButtonB != null)
        {
            choiceButtonB.onClick.RemoveListener(OnChoiceBPressed);
        }
    }

    public override bool CanOpen()
    {
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.IsRadioComplete())
        {
            ObjectiveManager.Instance.ShowTemporaryAnnaLine(repeatedListenLine);
            return false;
        }

        return true;
    }

    public override void Show()
    {
        base.Show();

        SetupDialogue();

        if (messageText != null)
        {
            messageText.text = "";
        }

        currentNormalIndex = 0;
        tuningStep = 0;
        selectedChoice = -1;
        pendingScore = 0;
        currentExtraReplyIndex = 0;
        currentExtraReplyLines = null;
        currentState = RadioState.Normal;
        currentDisplayMode = DisplayMode.None;
        currentDisplayedLine = "";
        isWaitingForSignal = true;

        HideAllSpecialButtons();
        ShowNextButtonOnly();

        ApplyChoiceTexts();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRadioStatic();

            if (startDelayCoroutine != null)
            {
                StopCoroutine(startDelayCoroutine);
            }

            startDelayCoroutine = StartCoroutine(StartRadioAfterDelay(AudioManager.Instance.GetRadioStartDelay()));
        }
        else
        {
            isWaitingForSignal = false;
            ShowCurrentNormalLine();
        }
    }

    public override void Hide()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (startDelayCoroutine != null)
        {
            StopCoroutine(startDelayCoroutine);
            startDelayCoroutine = null;
        }

        if (beginLineCoroutine != null)
        {
            StopCoroutine(beginLineCoroutine);
            beginLineCoroutine = null;
        }

        isTyping = false;
        isWaitingForSignal = false;
        currentState = RadioState.Finished;
        currentDisplayMode = DisplayMode.None;
        currentDisplayedLine = "";
        currentExtraReplyLines = null;
        currentExtraReplyIndex = 0;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopRadioAudio();
            AudioManager.Instance.StopRadioVoice();
        }

        base.Hide();
    }

    public void NextLine()
    {
        if (lastNextFrame == Time.frameCount)
        {
            return;
        }

        lastNextFrame = Time.frameCount;

        if (isWaitingForSignal)
        {
            return;
        }

        if (isTyping || beginLineCoroutine != null)
        {
            FinishCurrentLineInstantly();
            return;
        }

        if (currentState == RadioState.Normal)
        {
            currentNormalIndex++;

            if (currentData == null || currentData.normalLines == null || currentNormalIndex >= currentData.normalLines.Length)
            {
                EnterLostSignal();
                return;
            }

            ShowCurrentNormalLine();
            return;
        }

        if (currentState == RadioState.RadioReply)
        {
            if (HasExtraReplyLines())
            {
                StartExtraReplyLines();
            }
            else
            {
                currentState = RadioState.AnnaConfirm;
                ShowAnnaConfirmButton();
            }

            return;
        }

        if (currentState == RadioState.ExtraReplyLines)
        {
            currentExtraReplyIndex++;

            if (currentExtraReplyLines == null || currentExtraReplyIndex >= currentExtraReplyLines.Length)
            {
                currentState = RadioState.AnnaConfirm;
                ShowAnnaConfirmButton();
                return;
            }

            ShowCurrentExtraReplyLine();
        }
    }

    private void SetupDialogue()
    {
        if (DayManager.Instance == null)
        {
            currentData = null;
            return;
        }

        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            currentData = day1Data;
        }
        else if (day == 2)
        {
            currentData = day2Data;
        }
        else
        {
            currentData = day3Data;
        }
    }

    private IEnumerator StartRadioAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isWaitingForSignal = false;
        startDelayCoroutine = null;

        if (currentData == null || currentData.normalLines == null || currentData.normalLines.Length == 0)
        {
            EnterLostSignal();
            yield break;
        }

        ShowCurrentNormalLine();
    }

    private void ShowCurrentNormalLine()
    {
        if (currentData == null || currentData.normalLines == null || currentData.normalLines.Length == 0)
        {
            EnterLostSignal();
            return;
        }

        if (currentNormalIndex < 0 || currentNormalIndex >= currentData.normalLines.Length)
        {
            EnterLostSignal();
            return;
        }

        ShowNextButtonOnly();
        ShowRadioLine(currentData.normalLines[currentNormalIndex]);
    }

    private void EnterLostSignal()
    {
        currentState = RadioState.LostSignal;
        PlaySfx(lostSignalClip);

        if (messageText != null)
        {
            messageText.text = "";
        }

        currentDisplayedLine = "";
        HideAllSpecialButtons();
        ShowTuneButtonOnly();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopRadioVoice();
        }
    }

    private void OnTunePressed()
    {
        if (currentState == RadioState.AnnaConfirm)
        {
            FinishDialogue();
            return;
        }

        if (currentState != RadioState.LostSignal)
        {
            return;
        }

        tuningStep++;
        PlaySfx(tuningClip);

        if (tuningStep == 1)
        {
            ShowRadioLine(currentData != null ? currentData.tuningLine1 : "");
            return;
        }

        if (tuningStep == 2)
        {
            ShowRadioLine(currentData != null ? currentData.tuningLine2 : "");
            return;
        }

        if (tuningStep >= 3)
        {
            PlaySfx(signalRestoredClip);
            currentState = RadioState.Choosing;
            ShowRadioLine(currentData != null ? currentData.signalRestoredLine : "");
            StartCoroutine(ShowChoicesAfterTyping());
        }
    }

    private IEnumerator ShowChoicesAfterTyping()
    {
        while (isTyping)
        {
            yield return null;
        }

        if (currentState != RadioState.Choosing)
        {
            yield break;
        }

        ShowChoiceButtons();
    }

    private void OnChoiceAPressed()
    {
        if (currentState != RadioState.Choosing)
        {
            return;
        }

        selectedChoice = 0;

        if (currentData != null)
        {
            pendingScore += currentData.scoreA;
        }

        ShowSelectedRadioReply();
    }

    private void OnChoiceBPressed()
    {
        if (currentState == RadioState.AnnaConfirm)
        {
            FinishDialogue();
            return;
        }

        if (currentState != RadioState.Choosing)
        {
            return;
        }

        selectedChoice = 1;

        if (currentData != null)
        {
            pendingScore += currentData.scoreB;
        }

        ShowSelectedRadioReply();
    }

    private void ShowSelectedRadioReply()
    {
        currentState = RadioState.RadioReply;
        currentExtraReplyIndex = 0;
        currentExtraReplyLines = GetExtraReplyLinesForSelectedChoice();
        HideAllSpecialButtons();
        ShowNextButtonOnly();

        if (selectedChoice == 0)
        {
            ShowRadioLine(currentData != null ? currentData.radioReplyA : "");
        }
        else
        {
            ShowRadioLine(currentData != null ? currentData.radioReplyB : "");
        }
    }

    private bool HasExtraReplyLines()
    {
        return currentExtraReplyLines != null && currentExtraReplyLines.Length > 0;
    }

    private void StartExtraReplyLines()
    {
        currentState = RadioState.ExtraReplyLines;
        currentExtraReplyIndex = 0;
        ShowNextButtonOnly();
        ShowCurrentExtraReplyLine();
    }

    private void ShowCurrentExtraReplyLine()
    {
        if (currentExtraReplyLines == null || currentExtraReplyLines.Length == 0)
        {
            currentState = RadioState.AnnaConfirm;
            ShowAnnaConfirmButton();
            return;
        }

        if (currentExtraReplyIndex < 0 || currentExtraReplyIndex >= currentExtraReplyLines.Length)
        {
            currentState = RadioState.AnnaConfirm;
            ShowAnnaConfirmButton();
            return;
        }

        ShowNextButtonOnly();
        ShowRadioLine(currentExtraReplyLines[currentExtraReplyIndex]);
    }

    private string[] GetExtraReplyLinesForSelectedChoice()
    {
        if (currentData == null)
        {
            return null;
        }

        if (selectedChoice == 0)
        {
            return currentData.extraLinesAfterReplyA;
        }

        return currentData.extraLinesAfterReplyB;
    }

    private void ShowAnnaConfirmButton()
    {
        currentDisplayedLine = "";
        HideAllSpecialButtons();

        if (choiceButtonB != null)
        {
            choiceButtonB.gameObject.SetActive(true);
        }

        if (choiceButtonBText != null)
        {
            choiceButtonBText.text = Localizer.T(currentData != null ? currentData.annaFinalLine : "Хорошо");
        }
    }

    private void ShowRadioLine(string line)
    {
        currentDisplayMode = DisplayMode.Radio;
        StartTypedLine(Localizer.T(line), true);
    }

    private void StartTypedLine(string line, bool useRadioVoiceTiming)
    {
        if (messageText == null)
        {
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (beginLineCoroutine != null)
        {
            StopCoroutine(beginLineCoroutine);
            beginLineCoroutine = null;
        }

        currentDisplayedLine = line;
        beginLineCoroutine = StartCoroutine(BeginTypedLine(line, useRadioVoiceTiming));
    }

    private IEnumerator BeginTypedLine(string line, bool useRadioVoiceTiming)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopRadioVoice();
        }

        yield return null;

        float duration = 0f;

        if (useRadioVoiceTiming && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRadioVoiceForCurrentDay();
            duration = AudioManager.Instance.GetRadioVoiceLengthForCurrentDay();
        }

        typingCoroutine = StartCoroutine(TypeLine(line, duration, useRadioVoiceTiming));
        beginLineCoroutine = null;
    }

    private IEnumerator TypeLine(string line, float duration, bool stopVoiceAtEnd)
    {
        isTyping = true;
        messageText.text = "";

        if (string.IsNullOrEmpty(line))
        {
            isTyping = false;
            typingCoroutine = null;

            if (stopVoiceAtEnd && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopRadioVoice();
            }

            yield break;
        }

        float delayPerChar = fallbackTypingSpeed;

        if (duration > 0f)
        {
            delayPerChar = duration / line.Length;
        }

        if (typingSpeedMultiplier > 0f)
        {
            delayPerChar /= typingSpeedMultiplier;
        }

        for (int i = 0; i < line.Length; i++)
        {
            messageText.text += line[i];
            yield return new WaitForSeconds(delayPerChar);
        }

        isTyping = false;
        typingCoroutine = null;

        if (stopVoiceAtEnd && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopRadioVoice();
        }
    }

    private void FinishCurrentLineInstantly()
    {
        if (beginLineCoroutine != null)
        {
            StopCoroutine(beginLineCoroutine);
            beginLineCoroutine = null;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (messageText != null)
        {
            messageText.text = currentDisplayedLine;
        }

        isTyping = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopRadioVoice();
        }
    }

    private void FinishDialogue()
    {
        currentState = RadioState.Finished;

        if (DayManager.Instance != null)
        {
            DayManager.Instance.AddChoiceScore(pendingScore);
        }

        pendingScore = 0;

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.MarkRadioComplete();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseCurrentPanel();
        }
        else
        {
            Hide();
        }
    }

    private void ApplyChoiceTexts()
    {
        if (choiceButtonAText != null)
        {
            choiceButtonAText.text = currentData != null ? Localizer.T(currentData.choiceTextA) : "";
        }

        if (choiceButtonBText != null)
        {
            choiceButtonBText.text = currentData != null ? Localizer.T(currentData.choiceTextB) : "";
        }
    }

    private void HideAllSpecialButtons()
    {
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }

        if (tuneButton != null)
        {
            tuneButton.gameObject.SetActive(false);
        }

        if (choiceButtonA != null)
        {
            choiceButtonA.gameObject.SetActive(false);
        }

        if (choiceButtonB != null)
        {
            choiceButtonB.gameObject.SetActive(false);
        }
    }

    private void ShowNextButtonOnly()
    {
        HideAllSpecialButtons();

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    private void ShowTuneButtonOnly()
    {
        HideAllSpecialButtons();

        if (tuneButton != null)
        {
            tuneButton.gameObject.SetActive(true);
        }
    }

    private void ShowChoiceButtons()
    {
        HideAllSpecialButtons();

        if (choiceButtonA != null)
        {
            choiceButtonA.gameObject.SetActive(true);
        }

        if (choiceButtonB != null)
        {
            choiceButtonB.gameObject.SetActive(true);
        }

        ApplyChoiceTexts();
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}