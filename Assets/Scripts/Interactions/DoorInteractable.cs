using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Дверь";
    [SerializeField] private string annaLine = "Я сейчас не хочу выходить из комнаты";

    public void Interact()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.ShowTemporaryAnnaLine(annaLine);
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}