using UnityEngine;

public class FinalMirrorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Посмотреть в зеркало";

    public void Interact()
    {
        if (FinalSequenceManager.Instance != null)
        {
            FinalSequenceManager.Instance.OpenFinalMirror();
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
