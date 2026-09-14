using UnityEngine;

public class ComputerInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Использовать компьютер";
    [SerializeField] private ComputerPanel computerPanel;

    public void Interact()
    {
        if (computerPanel != null)
        {
            UIManager.Instance.OpenPanel(computerPanel);
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}