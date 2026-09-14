using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MirrorPanel : ObjectPanel
{
    [SerializeField] private Image mirrorImage;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private Sprite day1Sprite;
    [SerializeField] private Sprite day2Sprite;
    [SerializeField] private Sprite day3Sprite;

    [TextArea(2, 5)]
    [SerializeField] private string day1Description = "";

    [TextArea(2, 5)]
    [SerializeField] private string day2Description = "";

    [TextArea(2, 5)]
    [SerializeField] private string day3Description = "";

    public override void Show()
    {
        base.Show();
        UpdateMirrorView();

        if (FinalSequenceManager.Instance != null)
        {
            FinalSequenceManager.Instance.OnMirrorOpened();

            if (FinalSequenceManager.Instance.ShouldShowMirrorFinalLine() && descriptionText != null)
            {
                descriptionText.text = Localizer.T(FinalSequenceManager.Instance.GetMirrorFinalLine());
            }
        }
    }

    private void UpdateMirrorView()
    {
        if (DayManager.Instance == null)
        {
            return;
        }

        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            if (mirrorImage != null) mirrorImage.sprite = day1Sprite;
            if (descriptionText != null) descriptionText.text = Localizer.T(day1Description);
        }
        else if (day == 2)
        {
            if (mirrorImage != null) mirrorImage.sprite = day2Sprite;
            if (descriptionText != null) descriptionText.text = Localizer.T(day2Description);
        }
        else
        {
            if (mirrorImage != null) mirrorImage.sprite = day3Sprite;
            if (descriptionText != null) descriptionText.text = Localizer.T(day3Description);
        }
    }
}