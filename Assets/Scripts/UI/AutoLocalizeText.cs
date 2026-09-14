using UnityEngine;
using TMPro;

public class AutoLocalizeText : MonoBehaviour
{
    private TMP_Text[] texts;
    private string[] originals;
    private bool captured;

    private void Awake()
    {
        Capture();
    }

    private void OnEnable()
    {
        Apply();
        Localizer.OnLanguageChanged += Apply;
    }

    private void OnDisable()
    {
        Localizer.OnLanguageChanged -= Apply;
    }

    private void Capture()
    {
        texts = GetComponentsInChildren<TMP_Text>(true);
        originals = new string[texts.Length];

        for (int i = 0; i < texts.Length; i++)
        {
            originals[i] = texts[i].text;
        }

        captured = true;
    }

    private void Apply()
    {
        if (!captured)
        {
            Capture();
        }

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && HasCyrillic(originals[i]))
            {
                texts[i].text = Localizer.T(originals[i]);
            }
        }
    }

    private bool HasCyrillic(string s)
    {
        if (s == null)
        {
            return false;
        }

        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] >= 'Ѐ' && s[i] <= 'ӿ')
            {
                return true;
            }
        }

        return false;
    }
}
