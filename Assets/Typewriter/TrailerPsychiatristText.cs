using UnityEngine;
using TMPro;
using System.Collections;

public class TrailerPsychiatristText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textUI;
    [SerializeField] private string[] lines;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float pauseBetweenLines = 1f;
    [SerializeField] private float startDelay = 0.3f;

    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip voiceClip;

    private void Start()
    {
        StartCoroutine(PlayLines());
    }

    private IEnumerator PlayLines()
    {
        if (textUI != null)
        {
            textUI.text = "";
        }

        yield return new WaitForSeconds(startDelay);

        if (lines == null)
        {
            yield break;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            yield return StartCoroutine(TypeLine(lines[i]));
            yield return new WaitForSeconds(pauseBetweenLines);

            if (textUI != null)
            {
                textUI.text = "";
            }
        }
    }

    private IEnumerator TypeLine(string line)
    {
        if (textUI == null || string.IsNullOrEmpty(line))
        {
            yield break;
        }

        textUI.text = "";

        if (voiceSource != null && voiceClip != null)
        {
            voiceSource.clip = voiceClip;
            voiceSource.loop = true;
            voiceSource.Play();
        }

        for (int i = 0; i < line.Length; i++)
        {
            textUI.text += line[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        if (voiceSource != null)
        {
            voiceSource.Stop();
        }
    }
}