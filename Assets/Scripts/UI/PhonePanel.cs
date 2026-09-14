using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class PhoneChoiceBlock
{
    [TextArea(3, 10)]
    public string choiceTextA;

    [TextArea(3, 10)]
    public string choiceTextB;

    [TextArea(3, 10)]
    public string phoneReplyA;

    [TextArea(3, 10)]
    public string phoneReplyB;

    [TextArea(3, 10)]
    public string[] extraLinesAfterReplyA;

    [TextArea(3, 10)]
    public string[] extraLinesAfterReplyB;

    public int scoreA;
    public int scoreB;
}

[System.Serializable]
public class PhoneDayData
{
    [TextArea(3, 10)]
    public string[] normalLinesBeforeChoice1;

    public PhoneChoiceBlock choiceBlock1;

    [TextArea(3, 10)]
    public string[] linesBetweenChoice1AndChoice2;

    public PhoneChoiceBlock choiceBlock2;

    [TextArea(3, 10)]
    public string annaFinalLine = "Хорошо";
}

public class PhonePanel : ObjectPanel
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float fallbackTypingSpeed = 0.03f;
    [SerializeField] private float typingSpeedMultiplier = 1f;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button choiceButtonA;
    [SerializeField] private Button choiceButtonB;
    [SerializeField] private TextMeshProUGUI choiceButtonAText;
    [SerializeField] private TextMeshProUGUI choiceButtonBText;

    [Header("Dialogue By Day")]
    [SerializeField] private PhoneDayData day1Data;
    [SerializeField] private PhoneDayData day2Data;
    [SerializeField] private PhoneDayData day3Data;

    [Header("Repeated Listen")]
    [SerializeField] private string repeatedListenLine = "Я уже говорила с психиатром сегодня";

    [Header("Choice Safety")]
    [SerializeField] private float choiceInputDelay = 0.3f;

    private float choiceInputBlockedUntil;

    private PhoneDayData currentData;
    private int currentNormalIndex;
    private int currentBetweenIndex;
    private int selectedChoice = -1;
    private int currentExtraReplyIndex;
    private string[] currentExtraReplyLines;
    private int currentChoiceNumber = 1;

    private Coroutine typingCoroutine;
    private Coroutine startDelayCoroutine;
    private Coroutine beginLineCoroutine;

    private bool isTyping;
    private int lastNextFrame = -1;
    private int pendingScore;
    private bool isWaitingForCall;

    private PhoneState currentState;
    private string currentDisplayedLine = "";

    private enum PhoneState
    {
        NormalBeforeChoice1,
        Choosing1,
        Reply1,
        ExtraReplyLines1,
        BetweenChoices,
        Choosing2,
        Reply2,
        ExtraReplyLines2,
        AnnaConfirm,
        Finished
    }

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextLine);
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
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.IsPhoneComplete())
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

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        currentNormalIndex = 0;
        currentBetweenIndex = 0;
        selectedChoice = -1;
        pendingScore = 0;
        currentExtraReplyIndex = 0;
        currentExtraReplyLines = null;
        currentChoiceNumber = 1;
        currentState = PhoneState.NormalBeforeChoice1;
        currentDisplayedLine = "";
        isWaitingForCall = true;

        HideAllSpecialButtons();
        ShowNextButtonOnly();
        ApplyChoiceTextsForCurrentChoice();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPhoneCallForCurrentDay();

            if (startDelayCoroutine != null)
            {
                StopCoroutine(startDelayCoroutine);
            }

            startDelayCoroutine = StartCoroutine(StartDialogueAfterDelay(AudioManager.Instance.GetPhoneCallDelayForCurrentDay()));
        }
        else
        {
            isWaitingForCall = false;
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
        isWaitingForCall = false;
        currentState = PhoneState.Finished;
        currentDisplayedLine = "";
        currentExtraReplyLines = null;
        currentExtraReplyIndex = 0;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopPhoneAudio();
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

        if (isWaitingForCall)
        {
            return;
        }

        if (isTyping || beginLineCoroutine != null)
        {
            FinishCurrentLineInstantly();
            return;
        }

        if (currentState == PhoneState.NormalBeforeChoice1)
        {
            currentNormalIndex++;

            if (currentData == null || currentData.normalLinesBeforeChoice1 == null || currentNormalIndex >= currentData.normalLinesBeforeChoice1.Length)
            {
                currentChoiceNumber = 1;
                currentState = PhoneState.Choosing1;
                ShowChoiceButtons();
                return;
            }

            ShowCurrentNormalLine();
            return;
        }

        if (currentState == PhoneState.Reply1)
        {
            if (HasExtraReplyLines())
            {
                StartExtraReplyLines1();
            }
            else
            {
                StartBetweenChoices();
            }

            return;
        }

        if (currentState == PhoneState.ExtraReplyLines1)
        {
            currentExtraReplyIndex++;

            if (currentExtraReplyLines == null || currentExtraReplyIndex >= currentExtraReplyLines.Length)
            {
                StartBetweenChoices();
                return;
            }

            ShowCurrentExtraReplyLine();
            return;
        }

        if (currentState == PhoneState.BetweenChoices)
        {
            currentBetweenIndex++;

            if (currentData == null || currentData.linesBetweenChoice1AndChoice2 == null || currentBetweenIndex >= currentData.linesBetweenChoice1AndChoice2.Length)
            {
                currentChoiceNumber = 2;
                currentState = PhoneState.Choosing2;
                ShowChoiceButtons();
                return;
            }

            ShowCurrentBetweenLine();
            return;
        }

        if (currentState == PhoneState.Reply2)
        {
            if (HasExtraReplyLines())
            {
                StartExtraReplyLines2();
            }
            else
            {
                currentState = PhoneState.AnnaConfirm;
                ShowAnnaConfirmButton();
            }

            return;
        }

        if (currentState == PhoneState.ExtraReplyLines2)
        {
            currentExtraReplyIndex++;

            if (currentExtraReplyLines == null || currentExtraReplyIndex >= currentExtraReplyLines.Length)
            {
                currentState = PhoneState.AnnaConfirm;
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

    private IEnumerator StartDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isWaitingForCall = false;
        startDelayCoroutine = null;

        if (currentData == null || currentData.normalLinesBeforeChoice1 == null || currentData.normalLinesBeforeChoice1.Length == 0)
        {
            currentChoiceNumber = 1;
            currentState = PhoneState.Choosing1;
            ShowChoiceButtons();
            yield break;
        }

        ShowCurrentNormalLine();
    }

    private void ShowCurrentNormalLine()
    {
        if (currentData == null || currentData.normalLinesBeforeChoice1 == null || currentData.normalLinesBeforeChoice1.Length == 0)
        {
            currentChoiceNumber = 1;
            currentState = PhoneState.Choosing1;
            ShowChoiceButtons();
            return;
        }

        if (currentNormalIndex < 0 || currentNormalIndex >= currentData.normalLinesBeforeChoice1.Length)
        {
            currentChoiceNumber = 1;
            currentState = PhoneState.Choosing1;
            ShowChoiceButtons();
            return;
        }

        ShowNextButtonOnly();
        ShowPhoneLine(currentData.normalLinesBeforeChoice1[currentNormalIndex]);
    }

    private void StartBetweenChoices()
    {
        currentBetweenIndex = 0;

        if (currentData == null || currentData.linesBetweenChoice1AndChoice2 == null || currentData.linesBetweenChoice1AndChoice2.Length == 0)
        {
            currentChoiceNumber = 2;
            currentState = PhoneState.Choosing2;
            ShowChoiceButtons();
            return;
        }

        currentState = PhoneState.BetweenChoices;
        ShowCurrentBetweenLine();
    }

    private void ShowCurrentBetweenLine()
    {
        if (currentData == null || currentData.linesBetweenChoice1AndChoice2 == null || currentData.linesBetweenChoice1AndChoice2.Length == 0)
        {
            currentChoiceNumber = 2;
            currentState = PhoneState.Choosing2;
            ShowChoiceButtons();
            return;
        }

        if (currentBetweenIndex < 0 || currentBetweenIndex >= currentData.linesBetweenChoice1AndChoice2.Length)
        {
            currentChoiceNumber = 2;
            currentState = PhoneState.Choosing2;
            ShowChoiceButtons();
            return;
        }

        ShowNextButtonOnly();
        ShowPhoneLine(currentData.linesBetweenChoice1AndChoice2[currentBetweenIndex]);
    }

    private void OnChoiceAPressed()
    {
        if (Time.unscaledTime < choiceInputBlockedUntil)
        {
            return;
        }

        if (currentState != PhoneState.Choosing1 && currentState != PhoneState.Choosing2)
        {
            return;
        }

        selectedChoice = 0;

        PhoneChoiceBlock blockA = GetCurrentChoiceBlock();
        if (blockA != null)
        {
            pendingScore += blockA.scoreA;
        }

        ShowSelectedPhoneReply();
    }

    private void OnChoiceBPressed()
    {
        if (Time.unscaledTime < choiceInputBlockedUntil)
        {
            return;
        }

        if (currentState == PhoneState.AnnaConfirm)
        {
            FinishDialogue();
            return;
        }

        if (currentState != PhoneState.Choosing1 && currentState != PhoneState.Choosing2)
        {
            return;
        }

        selectedChoice = 1;

        PhoneChoiceBlock blockB = GetCurrentChoiceBlock();
        if (blockB != null)
        {
            pendingScore += blockB.scoreB;
        }

        ShowSelectedPhoneReply();
    }

    private void ShowSelectedPhoneReply()
    {
        currentExtraReplyIndex = 0;
        currentExtraReplyLines = GetExtraReplyLinesForSelectedChoice();

        HideAllSpecialButtons();
        ShowNextButtonOnly();

        if (currentChoiceNumber == 1)
        {
            currentState = PhoneState.Reply1;
        }
        else
        {
            currentState = PhoneState.Reply2;
        }

        if (selectedChoice == 0)
        {
            ShowPhoneLine(GetCurrentChoiceBlock() != null ? GetCurrentChoiceBlock().phoneReplyA : "");
        }
        else
        {
            ShowPhoneLine(GetCurrentChoiceBlock() != null ? GetCurrentChoiceBlock().phoneReplyB : "");
        }
    }

    private bool HasExtraReplyLines()
    {
        return currentExtraReplyLines != null && currentExtraReplyLines.Length > 0;
    }

    private void StartExtraReplyLines1()
    {
        currentState = PhoneState.ExtraReplyLines1;
        currentExtraReplyIndex = 0;
        ShowNextButtonOnly();
        ShowCurrentExtraReplyLine();
    }

    private void StartExtraReplyLines2()
    {
        currentState = PhoneState.ExtraReplyLines2;
        currentExtraReplyIndex = 0;
        ShowNextButtonOnly();
        ShowCurrentExtraReplyLine();
    }

    private void ShowCurrentExtraReplyLine()
    {
        if (currentExtraReplyLines == null || currentExtraReplyLines.Length == 0)
        {
            if (currentChoiceNumber == 1)
            {
                StartBetweenChoices();
            }
            else
            {
                currentState = PhoneState.AnnaConfirm;
                ShowAnnaConfirmButton();
            }

            return;
        }

        if (currentExtraReplyIndex < 0 || currentExtraReplyIndex >= currentExtraReplyLines.Length)
        {
            if (currentChoiceNumber == 1)
            {
                StartBetweenChoices();
            }
            else
            {
                currentState = PhoneState.AnnaConfirm;
                ShowAnnaConfirmButton();
            }

            return;
        }

        ShowNextButtonOnly();
        ShowPhoneLine(currentExtraReplyLines[currentExtraReplyIndex]);
    }

    private PhoneChoiceBlock GetCurrentChoiceBlock()
    {
        if (currentData == null)
        {
            return null;
        }

        if (currentChoiceNumber == 1)
        {
            return currentData.choiceBlock1;
        }

        return currentData.choiceBlock2;
    }

    private string[] GetExtraReplyLinesForSelectedChoice()
    {
        PhoneChoiceBlock block = GetCurrentChoiceBlock();

        if (block == null)
        {
            return null;
        }

        if (selectedChoice == 0)
        {
            return block.extraLinesAfterReplyA;
        }

        return block.extraLinesAfterReplyB;
    }

    private void ShowAnnaConfirmButton()
    {
        currentDisplayedLine = "";
        HideAllSpecialButtons();

        choiceInputBlockedUntil = Time.unscaledTime + choiceInputDelay;

        if (choiceButtonB != null)
        {
            choiceButtonB.gameObject.SetActive(true);
        }

        if (choiceButtonBText != null)
        {
            choiceButtonBText.text = Localizer.T(currentData != null ? currentData.annaFinalLine : "Хорошо");
        }
    }

    private void ShowPhoneLine(string line)
    {
        StartTypedLine(Localizer.T(line));
    }

    private void StartTypedLine(string line)
    {
        if (dialogueText == null)
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
        beginLineCoroutine = StartCoroutine(BeginTypedLine(line));
    }

    private IEnumerator BeginTypedLine(string line)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopPhoneAudio();
        }

        yield return null;

        float duration = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPhoneVoiceForCurrentDay();
            duration = AudioManager.Instance.GetPhoneVoiceLengthForCurrentDay();
        }

        typingCoroutine = StartCoroutine(TypeLine(line, duration));
        beginLineCoroutine = null;
    }

    private IEnumerator TypeLine(string line, float duration)
    {
        isTyping = true;
        dialogueText.text = "";

        if (string.IsNullOrEmpty(line))
        {
            isTyping = false;
            typingCoroutine = null;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopPhoneAudio();
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
            dialogueText.text += line[i];
            yield return new WaitForSeconds(delayPerChar);
        }

        isTyping = false;
        typingCoroutine = null;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopPhoneAudio();
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

        if (dialogueText != null)
        {
            dialogueText.text = currentDisplayedLine;
        }

        isTyping = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopPhoneAudio();
        }
    }

    private void FinishDialogue()
    {
        currentState = PhoneState.Finished;

        if (DayManager.Instance != null)
        {
            DayManager.Instance.AddChoiceScore(pendingScore);
        }

        pendingScore = 0;

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.MarkPhoneComplete();
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

    private void ApplyChoiceTextsForCurrentChoice()
    {
        PhoneChoiceBlock block = GetCurrentChoiceBlock();

        if (choiceButtonAText != null)
        {
            choiceButtonAText.text = block != null ? Localizer.T(block.choiceTextA) : "";
        }

        if (choiceButtonBText != null)
        {
            choiceButtonBText.text = block != null ? Localizer.T(block.choiceTextB) : "";
        }
    }

    private void HideAllSpecialButtons()
    {
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
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

    private void ShowChoiceButtons()
    {
        HideAllSpecialButtons();

        choiceInputBlockedUntil = Time.unscaledTime + choiceInputDelay;

        if (choiceButtonA != null)
        {
            choiceButtonA.gameObject.SetActive(true);
        }

        if (choiceButtonB != null)
        {
            choiceButtonB.gameObject.SetActive(true);
        }

        ApplyChoiceTextsForCurrentChoice();
    }
}