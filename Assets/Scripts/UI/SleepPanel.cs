using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;

[System.Serializable]
public class SleepSlide
{
    public Sprite image;

    [TextArea(2, 8)]
    public string[] textBlocks;
}

public class SleepPanel : ObjectPanel
{
    [SerializeField] private Image sleepImage;
    [SerializeField] private TextMeshProUGUI sleepText;
    [SerializeField] private float typingDelay = 0.08f;

    [SerializeField] private SleepSlide[] day1Slides;
    [SerializeField] private SleepSlide[] day2Slides;
    [SerializeField] private SleepSlide[] day3Slides;

    [SerializeField] private VideoOverlayPanel wakeVideoPanel;
    [SerializeField] private VideoClip wakeVideoClip;

    private SleepSlide[] currentSlides;
    private int currentSlideIndex;
    private int currentTextIndex;

    private Coroutine typingCoroutine;
    private bool isTyping;
    private bool introDreamMode;

    public void OpenIntroDream()
    {
        introDreamMode = true;
        UIManager.Instance.OpenPanel(this);
    }

    public override void Show()
    {
        base.Show();
        SetupCurrentSlides();
        currentSlideIndex = 0;
        currentTextIndex = 0;

        if (AudioManager.Instance != null)
        {
            int dreamNumber = GetCurrentDreamNumber();
            if (dreamNumber > 0)
            {
                Debug.Log($"Playing dream ambient for dream #{dreamNumber}");
                AudioManager.Instance.PlayDreamAmbientForDay(dreamNumber);
            }
        }

        ShowCurrentSlideAndText();
    }

    public override void Hide()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        base.Hide();
    }

    public void NextStep()
    {
        if (currentSlides == null || currentSlides.Length == 0)
        {
            FinishSleep();
            return;
        }

        SleepSlide currentSlide = currentSlides[currentSlideIndex];

        if (currentSlide == null || currentSlide.textBlocks == null || currentSlide.textBlocks.Length == 0)
        {
            MoveToNextSlideOrFinish();
            return;
        }

        if (isTyping)
        {
            FinishCurrentTextInstantly();
            return;
        }

        if (currentTextIndex < currentSlide.textBlocks.Length - 1)
        {
            currentTextIndex++;
            StartTypingCurrentText();
            return;
        }

        MoveToNextSlideOrFinish();
    }

    private int GetCurrentDreamNumber()
    {
        if (introDreamMode)
            return 1;

        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
            return 2;
        else if (day == 2)
            return 3;
        else
            return 0;
    }

    private void SetupCurrentSlides()
    {
        if (DayManager.Instance == null)
        {
            currentSlides = null;
            return;
        }

        if (introDreamMode)
        {
            currentSlides = day1Slides;
            return;
        }

        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            currentSlides = day2Slides;
        }
        else if (day == 2)
        {
            currentSlides = day3Slides;
        }
        else
        {
            currentSlides = null;
        }
    }

    private void ShowCurrentSlideAndText()
    {
        if (currentSlides == null || currentSlides.Length == 0)
        {
            if (sleepImage != null)
            {
                sleepImage.sprite = null;
            }

            if (sleepText != null)
            {
                sleepText.text = "";
            }

            return;
        }

        SleepSlide currentSlide = currentSlides[currentSlideIndex];

        if (sleepImage != null)
        {
            if (currentSlide != null)
            {
                sleepImage.sprite = currentSlide.image;
            }
            else
            {
                sleepImage.sprite = null;
            }
        }

        StartTypingCurrentText();
    }

    private void StartTypingCurrentText()
    {
        if (sleepText == null)
        {
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        string textToType = GetCurrentTextBlock();
        typingCoroutine = StartCoroutine(TypeText(textToType));
    }

    private IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        sleepText.text = "";

        if (string.IsNullOrEmpty(textToType))
        {
            isTyping = false;
            typingCoroutine = null;
            yield break;
        }

        float delay = Mathf.Max(typingDelay, 0.01f);

        for (int i = 0; i < textToType.Length; i++)
        {
            sleepText.text += textToType[i];
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private string GetCurrentTextBlock()
    {
        if (currentSlides == null || currentSlides.Length == 0)
        {
            return "";
        }

        SleepSlide currentSlide = currentSlides[currentSlideIndex];

        if (currentSlide == null || currentSlide.textBlocks == null || currentSlide.textBlocks.Length == 0)
        {
            return "";
        }

        if (currentTextIndex < 0 || currentTextIndex >= currentSlide.textBlocks.Length)
        {
            return "";
        }

        return Localizer.T(currentSlide.textBlocks[currentTextIndex]);
    }

    private void FinishCurrentTextInstantly()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        sleepText.text = GetCurrentTextBlock();
        isTyping = false;
    }

    private void MoveToNextSlideOrFinish()
    {
        if (currentSlides == null || currentSlides.Length == 0)
        {
            FinishSleep();
            return;
        }

        if (currentSlideIndex < currentSlides.Length - 1)
        {
            currentSlideIndex++;
            currentTextIndex = 0;
            ShowCurrentSlideAndText();
        }
        else
        {
            FinishSleep();
        }
    }

    private void FinishSleep()
    {
        if (introDreamMode)
        {
            introDreamMode = false;

            if (wakeVideoPanel != null && wakeVideoClip != null)
            {
                wakeVideoPanel.PlayClip(wakeVideoClip, FinishIntroAfterVideo, true);
            }
            else
            {
                UIManager.Instance.CloseCurrentPanel();
                FinishIntroAfterVideo();
            }

            return;
        }

        if (DayManager.Instance != null)
        {
            DayManager.Instance.NextDay();
        }

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.RefreshForCurrentDay();
        }

        if (HallucinationManager.Instance != null)
        {
            HallucinationManager.Instance.RefreshHallucinations();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAmbientForCurrentDay();
        }

        UIManager.Instance.CloseCurrentPanel();
    }

    private void FinishIntroAfterVideo()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.RefreshForCurrentDay();
        }

        if (HallucinationManager.Instance != null)
        {
            HallucinationManager.Instance.RefreshHallucinations();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAmbientForCurrentDay();
        }
    }
}