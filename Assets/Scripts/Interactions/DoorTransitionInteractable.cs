using UnityEngine;

public class DoorTransitionInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Войти";
    [SerializeField] private Transform target;

    public void Interact()
    {
        if (target == null || UIManager.Instance == null || UIManager.Instance.playerMovement == null)
        {
            return;
        }

        GameObject player = UIManager.Instance.playerMovement.gameObject;
        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.position = target.position;
        player.transform.rotation = target.rotation;

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
