using UnityEngine;
using TMPro;

public class HallucinationPanel : ObjectPanel
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private string defaultText = "Нажимайте, чтобы прогнать";
    [SerializeField] private string inhaleText = "вдох";
    [SerializeField] private string exhaleText = "выдох";

    private HallucinationInteractable currentHallucination;
    private bool showInhale = true;

    public override void Show()
    {
        base.Show();

        if (descriptionText != null)
        {
            descriptionText.text = Localizer.T(defaultText);
        }

        showInhale = true;
        UpdateButtonText();
    }

    public void SetCurrentHallucination(HallucinationInteractable hallucination)
    {
        currentHallucination = hallucination;
    }

    public void ClickToDispel()
    {
        if (currentHallucination != null)
        {
            currentHallucination.RegisterClick();
        }

        showInhale = !showInhale;
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (buttonText == null)
        {
            return;
        }

        if (showInhale)
        {
            buttonText.text = Localizer.T(inhaleText);
        }
        else
        {
            buttonText.text = Localizer.T(exhaleText);
        }
    }
}