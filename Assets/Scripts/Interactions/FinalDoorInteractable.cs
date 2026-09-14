using UnityEngine;

public class FinalDoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Открыть дверь";

    public void Interact()
    {
        if (FinalSequenceManager.Instance != null && FinalSequenceManager.Instance.IsHideStage())
        {
            FinalSequenceManager.Instance.EnterCorridor();
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
