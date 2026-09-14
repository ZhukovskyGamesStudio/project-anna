using UnityEngine;

public class FinalBedInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Лечь спать";

    public void Interact()
    {
        if (FinalSequenceManager.Instance != null)
        {
            FinalSequenceManager.Instance.FinishDoorEnding();
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
