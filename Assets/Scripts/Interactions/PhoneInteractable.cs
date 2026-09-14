using UnityEngine;

public class PhoneInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Подойти к телефону";
    [SerializeField] private PhonePanel phonePanel;

    public void Interact()
    {
        if (phonePanel != null)
        {
            UIManager.Instance.OpenPanel(phonePanel);
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}