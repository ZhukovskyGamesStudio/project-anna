using UnityEngine;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 3f;
    public TextMeshProUGUI interactTextUI;

    private IInteractable currentInteractable;

    private void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsPanelOpen())
        {
            currentInteractable = null;
            interactTextUI.gameObject.SetActive(false);
            return;
        }

        CheckInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayInteractTap();
            }

            currentInteractable.Interact();
        }
    }

    private void CheckInteractable()
    {
        currentInteractable = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                string text = interactable.GetInteractText();

                if (!string.IsNullOrEmpty(text))
                {
                    currentInteractable = interactable;
                    interactTextUI.text = "E - " + Localizer.T(text);
                    interactTextUI.gameObject.SetActive(true);
                    return;
                }
            }
        }

        interactTextUI.gameObject.SetActive(false);
    }
}