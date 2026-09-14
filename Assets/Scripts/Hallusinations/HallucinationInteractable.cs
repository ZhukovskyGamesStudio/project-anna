using UnityEngine;

public class HallucinationInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Нажимайте, чтобы прогнать";
    [SerializeField] private HallucinationPanel hallucinationPanel;
    [SerializeField] private AudioSource proximityAudioSource;
    [SerializeField] private int clicksToDispel = 8;
    [SerializeField] private bool showOnDay1 = true;
    [SerializeField] private bool showOnDay2 = true;
    [SerializeField] private bool showOnDay3 = true;
    [SerializeField] private float hearDistance = 3f;
    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private float maxVolume = 1f;

    private int currentClicks;
    private bool isDispelled;
    private Transform playerTransform;
    private float currentVolume;
    private float volumeMultiplier = 1f;

    public void SetVolumeMultiplier(float value)
    {
        volumeMultiplier = value;
    }

    private void OnEnable()
    {
        currentClicks = 0;
        currentVolume = 0f;

        if (Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }

        if (proximityAudioSource != null)
        {
            proximityAudioSource.loop = true;
            proximityAudioSource.playOnAwake = false;
            proximityAudioSource.volume = 0f;
            proximityAudioSource.Stop();
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            if (Camera.main != null)
            {
                playerTransform = Camera.main.transform;
            }
            else
            {
                return;
            }
        }

        if (proximityAudioSource == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float targetVolume = 0f;

        if (distance <= hearDistance)
        {
            float t = 1f - (distance / hearDistance);
            targetVolume = t * maxVolume * volumeMultiplier;
        }

        currentVolume = Mathf.Lerp(currentVolume, targetVolume, fadeSpeed * Time.deltaTime);
        proximityAudioSource.volume = currentVolume;

        if (currentVolume > 0.01f)
        {
            if (!proximityAudioSource.isPlaying)
            {
                proximityAudioSource.Play();
            }
        }
        else
        {
            if (proximityAudioSource.isPlaying)
            {
                proximityAudioSource.Stop();
            }
        }
    }

    private void OnDisable()
    {
        if (proximityAudioSource != null)
        {
            proximityAudioSource.Stop();
            proximityAudioSource.volume = 0f;
        }
    }

    public void Interact()
    {
        if (hallucinationPanel != null)
        {
            hallucinationPanel.SetCurrentHallucination(this);
            UIManager.Instance.OpenPanel(hallucinationPanel);
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }

    public void RegisterClick()
    {
        if (isDispelled)
        {
            return;
        }

        currentClicks++;

        if (currentClicks >= clicksToDispel)
        {
            isDispelled = true;
            UIManager.Instance.CloseCurrentPanel();
            gameObject.SetActive(false);

            if (HallucinationManager.Instance != null)
            {
                HallucinationManager.Instance.NotifyHallucinationChanged();
            }
        }
    }

    public void ResetForNewDay()
    {
        currentClicks = 0;
        isDispelled = false;
    }

    public bool CanAppearOnDay(int day)
    {
        if (day == 1)
        {
            return showOnDay1;
        }

        if (day == 2)
        {
            return showOnDay2;
        }

        if (day == 3)
        {
            return showOnDay3;
        }

        return false;
    }
}