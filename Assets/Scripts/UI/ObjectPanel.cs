using System.Collections;
using UnityEngine;

public class ObjectPanel : MonoBehaviour
{
    [SerializeField] private bool useDefaultPanelMusic = true;

    [Header("Fade")]
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 0.25f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    public virtual bool CanOpen()
    {
        return true;
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);

        if (!useFade)
        {
            return;
        }

        EnsureCanvasGroup();

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeIn());
    }

    public virtual void Hide()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        gameObject.SetActive(false);
    }

    public bool UseDefaultPanelMusic()
    {
        return useDefaultPanelMusic;
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        fadeCoroutine = null;
    }
}
