using UnityEngine;

public class MirrorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Посмотреть в зеркало";
    [SerializeField] private MirrorPanel mirrorPanel;

    public void Interact()
    {
        if (mirrorPanel != null)
        {
            UIManager.Instance.OpenPanel(mirrorPanel);
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}