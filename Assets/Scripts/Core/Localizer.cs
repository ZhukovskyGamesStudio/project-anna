using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum GameLanguage
{
    Russian,
    English
}

public static class Localizer
{
    private const string LanguageKey = "Language";
    private const string ResourceName = "translations";

    public static event System.Action OnLanguageChanged;

    private static GameLanguage language;
    private static bool languageLoaded;
    private static Dictionary<string, string> map;

    public static GameLanguage Language
    {
        get
        {
            EnsureLanguageLoaded();
            return language;
        }
    }

    public static bool IsEnglish
    {
        get { return Language == GameLanguage.English; }
    }

    public static void SetLanguage(GameLanguage value)
    {
        EnsureLanguageLoaded();
        language = value;
        PlayerPrefs.SetInt(LanguageKey, (int)value);
        PlayerPrefs.Save();

        if (OnLanguageChanged != null)
        {
            OnLanguageChanged();
        }
    }

    public static void ToggleLanguage()
    {
        SetLanguage(Language == GameLanguage.Russian ? GameLanguage.English : GameLanguage.Russian);
    }

    public static string T(string russian)
    {
        if (string.IsNullOrEmpty(russian) || !IsEnglish)
        {
            return russian;
        }

        EnsureMapLoaded();

        string value;

        if (map.TryGetValue(Normalize(russian), out value) && !string.IsNullOrEmpty(value))
        {
            return value;
        }

        return russian;
    }

    private static void EnsureLanguageLoaded()
    {
        if (languageLoaded)
        {
            return;
        }

        language = (GameLanguage)PlayerPrefs.GetInt(LanguageKey, 0);
        languageLoaded = true;
    }

    private static void EnsureMapLoaded()
    {
        if (map != null)
        {
            return;
        }

        map = new Dictionary<string, string>();

        TextAsset asset = Resources.Load<TextAsset>(ResourceName);

        if (asset == null)
        {
            return;
        }

        List<string[]> rows = ParseCsv(asset.text);

        for (int i = 0; i < rows.Count; i++)
        {
            string[] row = rows[i];

            if (row.Length < 2)
            {
                continue;
            }

            if (i == 0 && row[0] == "ru" && row[1] == "en")
            {
                continue;
            }

            string ru = Normalize(row[0]);

            if (!string.IsNullOrEmpty(ru) && !map.ContainsKey(ru))
            {
                map[ru] = row[1];
            }
        }
    }

    public static string Normalize(string s)
    {
        if (s == null)
        {
            return "";
        }

        return s.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    }

    public static List<string[]> ParseCsv(string text)
    {
        List<string[]> rows = new List<string[]>();
        List<string> fields = new List<string>();
        StringBuilder sb = new StringBuilder();
        bool inQuotes = false;
        int i = 0;

        while (i < text.Length)
        {
            char c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        sb.Append('"');
                        i += 2;
                        continue;
                    }

                    inQuotes = false;
                    i++;
                    continue;
                }

                sb.Append(c);
                i++;
                continue;
            }

            if (c == '"')
            {
                inQuotes = true;
                i++;
                continue;
            }

            if (c == ',')
            {
                fields.Add(sb.ToString());
                sb.Length = 0;
                i++;
                continue;
            }

            if (c == '\r' || c == '\n')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                fields.Add(sb.ToString());
                sb.Length = 0;
                rows.Add(fields.ToArray());
                fields = new List<string>();
                i++;
                continue;
            }

            sb.Append(c);
            i++;
        }

        if (sb.Length > 0 || fields.Count > 0)
        {
            fields.Add(sb.ToString());
            rows.Add(fields.ToArray());
        }

        return rows;
    }
}
