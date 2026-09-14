using UnityEngine;
using TMPro;

public class PillsPanel : ObjectPanel
{
    [SerializeField] private TextMeshProUGUI descriptionText;

    [TextArea(2, 5)]
    [SerializeField] private string defaultText = "Таблетки лежат на тумбочке. Их нужно принять перед сном.";

    [TextArea(2, 5)]
    [SerializeField] private string alreadyChosenText = "На сегодня выбор уже сделан.";

    public override void Show()
    {
        base.Show();
        UpdateText();
    }

    public void TakePills()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.SetPillsChoiceForCurrentDay(true);
        }

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.MarkPillsComplete();
        }

        UIManager.Instance.CloseCurrentPanel();
    }

    public void SkipPills()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.SetPillsChoiceForCurrentDay(false);
        }

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.MarkPillsComplete();
        }

        UIManager.Instance.CloseCurrentPanel();
    }

    private void UpdateText()
    {
        if (descriptionText == null)
        {
            return;
        }

        if (DayManager.Instance != null && DayManager.Instance.HasChoiceForCurrentDay())
        {
            descriptionText.text = Localizer.T(alreadyChosenText);
        }
        else
        {
            descriptionText.text = Localizer.T(defaultText);
        }
    }
}