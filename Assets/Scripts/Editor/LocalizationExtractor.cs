using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LocalizationExtractor
{
    private const string OutputPath = "Assets/Resources/translations.csv";
    private const string GameAssembly = "Assembly-CSharp";

    [MenuItem("ANNA/Локализация/Собрать текст в CSV")]
    public static void Extract()
    {
        HashSet<string> found = new HashSet<string>();

        CollectFromLoadedScenes(found);
        CollectFromPrefabs(found);

        Dictionary<string, string> existing = LoadExisting();

        List<string> keysInOrder = new List<string>();
        HashSet<string> seen = new HashSet<string>();

        foreach (string raw in found)
        {
            string key = Localizer.Normalize(raw);

            if (!string.IsNullOrEmpty(key) && seen.Add(key))
            {
                keysInOrder.Add(raw);
            }
        }

        keysInOrder.Sort(StringComparer.Ordinal);

        StringBuilder sb = new StringBuilder();
        sb.Append("ru,en\n");

        int translated = 0;

        foreach (string raw in keysInOrder)
        {
            string key = Localizer.Normalize(raw);
            string en = "";

            if (existing.TryGetValue(key, out string value))
            {
                en = value;

                if (!string.IsNullOrEmpty(en))
                {
                    translated++;
                }
            }

            sb.Append(Escape(raw));
            sb.Append(',');
            sb.Append(Escape(en));
            sb.Append('\n');
        }

        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
        File.WriteAllText(OutputPath, sb.ToString(), new UTF8Encoding(false));
        AssetDatabase.Refresh();

        Debug.Log($"Локализация: собрано {keysInOrder.Count} строк, из них уже переведено {translated}. Файл: {OutputPath}");
    }

    private static void CollectFromLoadedScenes(HashSet<string> found)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!scene.isLoaded)
            {
                continue;
            }

            GameObject[] roots = scene.GetRootGameObjects();

            for (int r = 0; r < roots.Length; r++)
            {
                CollectFromGameObject(roots[r], found);
            }
        }
    }

    private static void CollectFromPrefabs(HashSet<string> found)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);

            if (!path.StartsWith("Assets/"))
            {
                continue;
            }

            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (go != null)
            {
                CollectFromGameObject(go, found);
            }
        }
    }

    private static void CollectFromGameObject(GameObject go, HashSet<string> found)
    {
        MonoBehaviour[] components = go.GetComponentsInChildren<MonoBehaviour>(true);

        for (int i = 0; i < components.Length; i++)
        {
            MonoBehaviour mb = components[i];

            if (mb == null)
            {
                continue;
            }

            if (mb.GetType().Assembly.GetName().Name == GameAssembly)
            {
                Collect(mb, found);
            }
        }

        TMP_Text[] texts = go.GetComponentsInChildren<TMP_Text>(true);

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && HasCyrillic(texts[i].text))
            {
                found.Add(texts[i].text);
            }
        }
    }

    private static void Collect(object obj, HashSet<string> found)
    {
        if (obj == null)
        {
            return;
        }

        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo f = fields[i];

            if (!IsSerializedField(f))
            {
                continue;
            }

            Consume(f.GetValue(obj), found);
        }
    }

    private static void Consume(object value, HashSet<string> found)
    {
        if (value == null)
        {
            return;
        }

        if (value is string s)
        {
            if (HasCyrillic(s))
            {
                found.Add(s);
            }

            return;
        }

        if (value is UnityEngine.Object)
        {
            return;
        }

        if (value is IEnumerable enumerable)
        {
            foreach (object item in enumerable)
            {
                Consume(item, found);
            }

            return;
        }

        Type type = value.GetType();

        if (type.Assembly.GetName().Name == GameAssembly && Attribute.IsDefined(type, typeof(SerializableAttribute)))
        {
            Collect(value, found);
        }
    }

    private static bool IsSerializedField(FieldInfo f)
    {
        if (f.IsStatic)
        {
            return false;
        }

        if (f.IsPublic)
        {
            return !Attribute.IsDefined(f, typeof(NonSerializedAttribute));
        }

        return Attribute.IsDefined(f, typeof(SerializeField));
    }

    private static bool HasCyrillic(string s)
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

    private static Dictionary<string, string> LoadExisting()
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        if (!File.Exists(OutputPath))
        {
            return result;
        }

        List<string[]> rows = Localizer.ParseCsv(File.ReadAllText(OutputPath));

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

            string key = Localizer.Normalize(row[0]);

            if (!string.IsNullOrEmpty(key) && !result.ContainsKey(key))
            {
                result[key] = row[1];
            }
        }

        return result;
    }

    private static string Escape(string s)
    {
        if (s == null)
        {
            s = "";
        }

        bool needsQuote = s.IndexOf(',') >= 0 || s.IndexOf('"') >= 0 || s.IndexOf('\n') >= 0 || s.IndexOf('\r') >= 0;
        s = s.Replace("\"", "\"\"");

        if (needsQuote)
        {
            s = "\"" + s + "\"";
        }

        return s;
    }
}
