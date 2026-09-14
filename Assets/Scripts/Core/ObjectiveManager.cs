using UnityEngine;
using TMPro;
using System.Collections;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [SerializeField] private TextMeshProUGUI objectiveTextUI;
    [SerializeField] private TextMeshProUGUI annaLineTextUI;
    [SerializeField] private float temporaryLineDuration = 2.5f;
    [SerializeField] private string windowHintLine = "Нужно подышать свежим воздухом";

    [Header("Hints")]
    [SerializeField] private string hintRadio = "Подсказка: послушать радио";
    [SerializeField] private string hintComputer = "Подсказка: проверить компьютер";
    [SerializeField] private string hintBoard = "Подсказка: осмотреть доску";
    [SerializeField] private string hintPhone = "Подсказка: поговорить с психиатром";
    [SerializeField] private string hintHallucinations = "Подсказка: прогнать галлюцинации";
    [SerializeField] private string hintPills = "Подсказка: решить, пить ли таблетки";

    [Header("Sleep Blocked Lines")]
    [SerializeField] private string blockedRadio = "Мне нужно ещё раз послушать радио";
    [SerializeField] private string blockedComputer = "Сначала нужно проверить компьютер";
    [SerializeField] private string blockedBoard = "Мне нужно ещё раз посмотреть на доску";
    [SerializeField] private string blockedPhone = "Сначала нужно поговорить с психиатром";
    [SerializeField] private string blockedHallucinations = "Я не могу лечь спать, пока эти вещи ещё здесь";
    [SerializeField] private string blockedPills = "Мне нужно решить, что делать с таблетками";

    [Header("Anna Lines")]
    [SerializeField] private string canSleepLine = "Теперь я могу пойти спать";

    private bool radioDone;
    private bool computerDone;
    private bool boardDone;
    private bool phoneDone;
    private bool hallucinationsDone;
    private bool pillsDone;

    private int lastDay = -1;
    private Coroutine temporaryLineCoroutine;

    private bool windowHintShown;

    private bool useOverrideTexts;
    private string overrideObjectiveText = "";
    private string overrideAnnaLineText = "";

    private enum ObjectiveType
    {
        Radio,
        Computer,
        Board,
        Phone,
        Hallucinations,
        Pills
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

    private void Start()
    {
        RefreshForCurrentDay();
    }

    public void RefreshForCurrentDay()
    {
        if (DayManager.Instance == null)
        {
            return;
        }

        int currentDay = DayManager.Instance.CurrentDay;

        if (currentDay == lastDay)
        {
            UpdateHallucinationsObjectiveState();
            pillsDone = DayManager.Instance.HasChoiceForCurrentDay();
            UpdateObjectiveText();
            return;
        }

        lastDay = currentDay;

        radioDone = false;
        computerDone = false;
        boardDone = false;
        phoneDone = false;
        hallucinationsDone = false;
        pillsDone = DayManager.Instance.HasChoiceForCurrentDay();

        windowHintShown = false;

        useOverrideTexts = false;
        overrideObjectiveText = "";
        overrideAnnaLineText = "";

        ClearAnnaLine();

        if (HallucinationManager.Instance != null)
        {
            HallucinationManager.Instance.RefreshHallucinations();
            UpdateHallucinationsObjectiveState();
        }
        else
        {
            hallucinationsDone = true;
        }

        UpdateObjectiveText();
    }

    public void MarkRadioComplete()
    {
        radioDone = true;
        UpdateObjectiveText();
    }

    public bool IsRadioComplete()
    {
        return radioDone;
    }

    public void MarkComputerComplete()
    {
        computerDone = true;
        CheckWindowHint(ObjectiveType.Computer);
        UpdateObjectiveText();
    }

    public void MarkBoardComplete()
    {
        boardDone = true;
        CheckWindowHint(ObjectiveType.Board);
        UpdateObjectiveText();
    }

    public void MarkPhoneComplete()
    {
        phoneDone = true;
        UpdateObjectiveText();
    }

    public bool IsPhoneComplete()
{
    return phoneDone;
}

    public void MarkPillsComplete()
    {
        pillsDone = true;
        UpdateObjectiveText();
    }

    public void UpdateHallucinationsObjectiveState()
    {
        if (HallucinationManager.Instance == null)
        {
            hallucinationsDone = true;
        }
        else
        {
            hallucinationsDone = HallucinationManager.Instance.GetActiveHallucinationCount() == 0;
        }

        UpdateObjectiveText();
    }

    public bool CanSleep()
    {
        return radioDone && computerDone && boardDone && phoneDone && hallucinationsDone && pillsDone;
    }

    public void ShowSleepBlockedLine()
    {
        ObjectiveType missingObjective = GetFirstIncompleteObjective();
        ShowTemporaryAnnaLine(GetSleepBlockedLine(missingObjective));
    }

    public void ShowTemporaryAnnaLine(string line)
    {
        if (temporaryLineCoroutine != null)
        {
            StopCoroutine(temporaryLineCoroutine);
        }

        temporaryLineCoroutine = StartCoroutine(ShowTemporaryAnnaLineCoroutine(line));
    }

    public void SetOverrideTexts(string objectiveText, string annaLine)
    {
        useOverrideTexts = true;
        overrideObjectiveText = objectiveText;
        overrideAnnaLineText = annaLine;
        ApplyTexts();
    }

    public void ClearOverrideTexts()
    {
        useOverrideTexts = false;
        overrideObjectiveText = "";
        overrideAnnaLineText = "";
        UpdateObjectiveText();
    }

    private void CheckWindowHint(ObjectiveType justCompleted)
    {
        if (windowHintShown || DayManager.Instance == null)
        {
            return;
        }

        int day = DayManager.Instance.CurrentDay;

        bool day1AfterBoard = day == 1 && justCompleted == ObjectiveType.Board;
        bool day2AfterComputer = day == 2 && justCompleted == ObjectiveType.Computer;

        if (day1AfterBoard || day2AfterComputer)
        {
            windowHintShown = true;
            ShowTemporaryAnnaLine(windowHintLine);
        }
    }

    private IEnumerator ShowTemporaryAnnaLineCoroutine(string line)
    {
        if (annaLineTextUI != null)
        {
            annaLineTextUI.text = Localizer.T(line);
        }

        yield return new WaitForSeconds(temporaryLineDuration);

        temporaryLineCoroutine = null;
        UpdateAnnaLine();
    }

    private void UpdateObjectiveText()
    {
        if (objectiveTextUI == null)
        {
            return;
        }

        if (!useOverrideTexts &&
            CanSleep() &&
            DayManager.Instance != null &&
            DayManager.Instance.CurrentDay == 3 &&
            FinalSequenceManager.Instance != null &&
            !FinalSequenceManager.Instance.HasStarted())
        {
            FinalSequenceManager.Instance.StartFinalSequence();
            return;
        }

        ApplyTexts();
    }

    private void ApplyTexts()
    {
        if (objectiveTextUI == null)
        {
            return;
        }

        if (useOverrideTexts)
        {
            objectiveTextUI.text = Localizer.T(overrideObjectiveText);
            UpdateAnnaLine();
            return;
        }

        if (CanSleep())
        {
            objectiveTextUI.text = "";
        }
        else
        {
            ObjectiveType nextObjective = GetFirstIncompleteObjective();
            objectiveTextUI.text = Localizer.T(GetHintText(nextObjective));
        }

        UpdateAnnaLine();
    }

    private void UpdateAnnaLine()
    {
        if (annaLineTextUI == null)
        {
            return;
        }

        if (temporaryLineCoroutine != null)
        {
            return;
        }

        if (useOverrideTexts)
        {
            annaLineTextUI.text = Localizer.T(overrideAnnaLineText);
            return;
        }

        if (CanSleep())
        {
            annaLineTextUI.text = Localizer.T(canSleepLine);
        }
        else
        {
            annaLineTextUI.text = "";
        }
    }

    private void ClearAnnaLine()
    {
        if (annaLineTextUI != null)
        {
            annaLineTextUI.text = "";
        }
    }

    private ObjectiveType GetFirstIncompleteObjective()
    {
        ObjectiveType[] orderedObjectives = GetOrderedObjectivesForCurrentDay();

        for (int i = 0; i < orderedObjectives.Length; i++)
        {
            if (!IsObjectiveComplete(orderedObjectives[i]))
            {
                return orderedObjectives[i];
            }
        }

        return ObjectiveType.Radio;
    }

    private ObjectiveType[] GetOrderedObjectivesForCurrentDay()
    {
        int day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;

        if (day == 1)
        {
            return new ObjectiveType[]
            {
                ObjectiveType.Radio,
                ObjectiveType.Computer,
                ObjectiveType.Board,
                ObjectiveType.Phone,
                ObjectiveType.Hallucinations,
                ObjectiveType.Pills
            };
        }

        if (day == 2)
        {
            return new ObjectiveType[]
            {
                ObjectiveType.Computer,
                ObjectiveType.Phone,
                ObjectiveType.Board,
                ObjectiveType.Radio,
                ObjectiveType.Hallucinations,
                ObjectiveType.Pills
            };
        }

        return new ObjectiveType[]
        {
            ObjectiveType.Radio,
            ObjectiveType.Phone,
            ObjectiveType.Computer,
            ObjectiveType.Board,
            ObjectiveType.Hallucinations,
            ObjectiveType.Pills
        };
    }

    private bool IsObjectiveComplete(ObjectiveType objectiveType)
    {
        if (objectiveType == ObjectiveType.Radio)
        {
            return radioDone;
        }

        if (objectiveType == ObjectiveType.Computer)
        {
            return computerDone;
        }

        if (objectiveType == ObjectiveType.Board)
        {
            return boardDone;
        }

        if (objectiveType == ObjectiveType.Phone)
        {
            return phoneDone;
        }

        if (objectiveType == ObjectiveType.Hallucinations)
        {
            return hallucinationsDone;
        }

        return pillsDone;
    }

    private string GetHintText(ObjectiveType objectiveType)
    {
        if (objectiveType == ObjectiveType.Radio)
        {
            return hintRadio;
        }

        if (objectiveType == ObjectiveType.Computer)
        {
            return hintComputer;
        }

        if (objectiveType == ObjectiveType.Board)
        {
            return hintBoard;
        }

        if (objectiveType == ObjectiveType.Phone)
        {
            return hintPhone;
        }

        if (objectiveType == ObjectiveType.Hallucinations)
        {
            return hintHallucinations;
        }

        return hintPills;
    }

    private string GetSleepBlockedLine(ObjectiveType objectiveType)
    {
        if (objectiveType == ObjectiveType.Radio)
        {
            return blockedRadio;
        }

        if (objectiveType == ObjectiveType.Computer)
        {
            return blockedComputer;
        }

        if (objectiveType == ObjectiveType.Board)
        {
            return blockedBoard;
        }

        if (objectiveType == ObjectiveType.Phone)
        {
            return blockedPhone;
        }

        if (objectiveType == ObjectiveType.Hallucinations)
        {
            return blockedHallucinations;
        }

        return blockedPills;
    }
}