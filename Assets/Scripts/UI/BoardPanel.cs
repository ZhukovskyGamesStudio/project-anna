using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardPanel : ObjectPanel
{
    [SerializeField] private Image boardImage;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private Sprite day1Sprite;
    [SerializeField] private Sprite day2Sprite;
    [SerializeField] private Sprite day3Sprite;

    [TextArea(2, 5)]
    [SerializeField] private string day1Description = "На доске несколько заметок, фотографии и первые связи между ними.";

    [TextArea(2, 5)]
    [SerializeField] private string day2Description = "Нитей стало больше. Некоторые записи повторяются, а связи выглядят тревожнее.";

    [TextArea(2, 5)]
    [SerializeField] private string day3Description = "Доска почти полностью заполнена. Повсюду записи, круги, стрелки и беспорядочные пометки.";

    public override void Show()
    {
        base.Show();

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.MarkBoardComplete();
        }

        UpdateBoardView();
    }

    private void UpdateBoardView()
    {
        if (DayManager.Instance == null)
        {
            return;
        }

        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            ApplyView(day1Sprite, day1Description);
        }
        else if (day == 2)
        {
            ApplyView(day2Sprite, day2Description);
        }
        else
        {
            ApplyView(day3Sprite, day3Description);
        }
    }

    private void ApplyView(Sprite sprite, string description)
    {
        if (boardImage != null && sprite != null)
        {
            boardImage.sprite = sprite;
        }

        if (descriptionText != null)
        {
            descriptionText.text = Localizer.T(description);
        }
    }
}