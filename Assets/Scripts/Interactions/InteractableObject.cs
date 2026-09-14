using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Взаимодействовать";
    [SerializeField] private ObjectPanel panelToOpen;

    public virtual void Interact()
    {
        if (panelToOpen != null)
        {
            UIManager.Instance.OpenPanel(panelToOpen);
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}