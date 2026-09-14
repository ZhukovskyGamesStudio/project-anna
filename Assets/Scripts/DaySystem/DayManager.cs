using UnityEngine;
using System;
public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    public event Action<int> OnDayChanged;

    [SerializeField] private int currentDay = 1;

    [SerializeField] private int pillsTakenScore = 1;
    [SerializeField] private int pillsSkippedScore = -1;

    [SerializeField] private int watchedThreshold = -4;
    [SerializeField] private int illThreshold = 4;

    [Header("Отладка")]
    [SerializeField] private bool debugStartInCorridor;
    [SerializeField] private bool debugStartAtKnock;
    [SerializeField] private bool debugOverrideScore;
    [SerializeField] private int debugScore;

    private bool day1ChoiceMade;
    private bool day2ChoiceMade;
    private bool day3ChoiceMade;

    private bool day1TookPills;
    private bool day2TookPills;
    private bool day3TookPills;

    private int choiceScore;

    public int CurrentDay
    {
        get { return currentDay; }
    }

    public int ChoiceScore
    {
        get { return choiceScore; }
    }

    public bool DebugStartInCorridor
    {
        get { return debugStartInCorridor; }
    }

    public bool DebugStartAtKnock
    {
        get { return debugStartAtKnock; }
    }

    public bool DebugSkipIntro
    {
        get { return debugStartInCorridor || debugStartAtKnock; }
    }

    public enum Leaning
    {
        Watched,
        Uncertain,
        Ill
    }

    public Leaning GetLeaning()
    {
        if (choiceScore <= watchedThreshold)
        {
            return Leaning.Watched;
        }

        if (choiceScore >= illThreshold)
        {
            return Leaning.Ill;
        }

        return Leaning.Uncertain;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (debugOverrideScore)
            {
                choiceScore = debugScore;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

       public void SetDay(int day)
    {
        int newDay = Mathf.Clamp(day, 1, 3);
        if (currentDay != newDay)
        {
            currentDay = newDay;
            OnDayChanged?.Invoke(currentDay);
        }
    }

    public void NextDay()
    {
        SetDay(currentDay + 1);
    }


    public void AddChoiceScore(int amount)
    {
        choiceScore += amount;
    }

    public void SetPillsChoiceForCurrentDay(bool tookPills)
    {
        if (currentDay == 1)
        {
            if (day1ChoiceMade)
            {
                return;
            }
            day1ChoiceMade = true;
            day1TookPills = tookPills;
        }
        else if (currentDay == 2)
        {
            if (day2ChoiceMade)
            {
                return;
            }
            day2ChoiceMade = true;
            day2TookPills = tookPills;
        }
        else if (currentDay == 3)
        {
            if (day3ChoiceMade)
            {
                return;
            }
            day3ChoiceMade = true;
            day3TookPills = tookPills;
        }
        else
        {
            return;
        }

        AddChoiceScore(tookPills ? pillsTakenScore : pillsSkippedScore);
    }

    public bool HasChoiceForCurrentDay()
    {
        if (currentDay == 1)
        {
            return day1ChoiceMade;
        }

        if (currentDay == 2)
        {
            return day2ChoiceMade;
        }

        return day3ChoiceMade;
    }

    public bool TookPillsOnDay(int day)
    {
        if (day == 1)
        {
            return day1ChoiceMade && day1TookPills;
        }

        if (day == 2)
        {
            return day2ChoiceMade && day2TookPills;
        }

        if (day == 3)
        {
            return day3ChoiceMade && day3TookPills;
        }

        return false;
    }

    public bool ShouldShowHallucinationsForCurrentDay()
    {
        if (currentDay == 1)
        {
            return true;
        }

        if (currentDay == 2)
        {
            return !TookPillsOnDay(1);
        }

        if (currentDay == 3)
        {
            return !TookPillsOnDay(2);
        }

        return false;
    }
}