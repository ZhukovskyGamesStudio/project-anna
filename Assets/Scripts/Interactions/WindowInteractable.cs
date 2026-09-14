using UnityEngine;

public class WindowInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Посмотреть в окно";
    [SerializeField] private WindowPanel windowPanel;

    public void Interact()
    {
        if (windowPanel != null)
        {
            UIManager.Instance.OpenPanel(windowPanel);
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}