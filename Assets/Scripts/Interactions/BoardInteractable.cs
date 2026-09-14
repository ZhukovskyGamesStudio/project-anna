using UnityEngine;

public class BoardInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Осмотреть доску";
    [SerializeField] private BoardPanel boardPanel;

    public void Interact()
    {
        if (boardPanel != null)
        {
            UIManager.Instance.OpenPanel(boardPanel);
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}