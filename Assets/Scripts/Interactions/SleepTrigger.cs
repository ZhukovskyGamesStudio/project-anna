using UnityEngine;

public class SleepTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Лечь спать";
    [SerializeField] private string underBedText = "Залезть под кровать";
    [SerializeField] private SleepPanel sleepPanel;

    private bool IsBedBlockedForDay3()
    {
        if (FinalSequenceManager.Instance != null && FinalSequenceManager.Instance.HasStarted())
        {
            return false;
        }

        return DayManager.Instance != null && DayManager.Instance.CurrentDay >= 3;
    }

    public void Interact()
    {
        if (IsBedBlockedForDay3())
        {
            return;
        }

        if (FinalSequenceManager.Instance != null)
        {
            if (FinalSequenceManager.Instance.IsHideStage())
            {
                FinalSequenceManager.Instance.EnterUnderBed();
                return;
            }

            if (FinalSequenceManager.Instance.IsBlockingBed())
            {
                FinalSequenceManager.Instance.ShowBlockedBedLine();
                return;
            }
        }

        if (sleepPanel == null)
        {
            return;
        }

        if (ObjectiveManager.Instance != null && !ObjectiveManager.Instance.CanSleep())
        {
            ObjectiveManager.Instance.ShowSleepBlockedLine();
            return;
        }

        UIManager.Instance.OpenPanel(sleepPanel);
    }

    public string GetInteractText()
    {
        if (FinalSequenceManager.Instance != null && FinalSequenceManager.Instance.IsHideStage())
        {
            return underBedText;
        }

        if (IsBedBlockedForDay3())
        {
            return "";
        }

        return interactText;
    }
}
