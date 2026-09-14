using UnityEngine;

public class PillsInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Посмотреть таблетки";
    [SerializeField] private PillsPanel pillsPanel;

    public void Interact()
    {
        if (pillsPanel != null)
        {
            UIManager.Instance.OpenPanel(pillsPanel);
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}