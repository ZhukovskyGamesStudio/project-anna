// Файловые операции есть только на standalone и в редакторе.
#if UNITY_STANDALONE || UNITY_EDITOR
#define FILE_ACHIEVEMENTS
#endif

using System.Collections.Generic;
using UnityEngine;
#if FILE_ACHIEVEMENTS
using System.IO;
#endif

/// <summary>
/// Достижения Steam через лаунчер антологии. Сама игра Steam не поднимает:
/// App ID принадлежит антологии, и SetAchievement вызывает лаунчер.
///
/// Протокол: игра создаёт пустой файл "папка exe"/achievements/"API_NAME",
/// лаунчер раз в секунду (и ещё раз при выходе из игры) читает эту папку,
/// открывает достижение в Steam и УДАЛЯЕТ файл.
///
/// Отсюда две вещи, из-за которых класс не сводится к одной строке:
/// файл стирают, поэтому по нему нельзя понять, выдано достижение или нет
/// (игра ведёт свою пометку в PlayerPrefs), и существующий файл трогать
/// нельзя — он ещё в очереди у лаунчера.
///
/// Папка именно рядом с exe, а не Application.persistentDataPath: лаунчер
/// смотрит в папку игры внутри своей раздачи. У standalone-билда exe и
/// папка *_Data лежат рядом, Application.dataPath — это *_Data. В редакторе
/// dataPath — это Assets, то есть заявки падают в корень проекта; проверять
/// схему целиком надо на Player-билде, запущенном из лаунчера.
///
/// Пока API Name не заведено и не опубликовано в Steamworks, файл просто
/// останется лежать, а лаунчер напишет "Unknown Steam achievement API name".
/// Это ожидаемо и на стороне игры никак не чинится.
/// </summary>
public static class LauncherAchievements
{
    public const string EndingWatched = "PA_ENDING_WATCHED";
    public const string EndingUncertain = "PA_ENDING_UNCERTAIN";
    public const string EndingIll = "PA_ENDING_ILL";
    public const string Window = "PA_WINDOW";

    public static readonly string[] All =
    {
        EndingWatched,
        EndingUncertain,
        EndingIll,
        Window
    };

    private const string FolderName = "achievements";
    private const string UnlockedKey = "AchievementsUnlocked";
    private const char Separator = ';';

    // Заявки, уже поданные в этом запуске: лаунчер удаляет файл почти сразу,
    // и без этого списка повторный вызов писал бы его снова и снова.
    private static readonly HashSet<string> requested = new HashSet<string>();

    private static List<string> unlocked;
    private static string cachedRaw;

    /// <summary>Папка с заявками. Пустая строка — платформа без файлов.</summary>
    public static string FolderPath
    {
        get
        {
#if FILE_ACHIEVEMENTS
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, FolderName);
#else
            return string.Empty;
#endif
        }
    }

    /// <summary>
    /// Открыть достижение: подать заявку лаунчеру и пометить у себя.
    /// Повторный вызов на уже открытом безвреден и ничего не делает.
    /// </summary>
    public static void Unlock(string apiName)
    {
        if (string.IsNullOrEmpty(apiName))
        {
            return;
        }

        MarkUnlocked(apiName);

        if (!requested.Add(apiName))
        {
            return;
        }

        WriteFlag(apiName);
    }

    /// <summary>
    /// Игра считает достижение выданным. Это СВОЯ пометка, а не состояние
    /// Steam: по папке заявок его не узнать, а Steam игра не опрашивает.
    /// </summary>
    public static bool IsUnlocked(string apiName)
    {
        Load();
        return unlocked.Contains(apiName);
    }

    /// <summary>
    /// Переподать заявки на всё, что игра уже считает выданным. Зовётся
    /// из главного меню.
    ///
    /// Нужно потому, что пометка живёт в PlayerPrefs, а они общие на всю
    /// машину (реестр по company/product) и не зависят от папки, из которой
    /// игра запущена. Играл в отдельную сборку, потом поставил антологию —
    /// пометка есть, а заявки в папке антологии нет и не будет. Лишняя
    /// заявка стоит одного SetAchievement на уже открытом достижении,
    /// Steam это игнорирует.
    /// </summary>
    public static void ResubmitUnlocked()
    {
        Load();

        for (int i = 0; i < unlocked.Count; i++)
        {
            if (requested.Add(unlocked[i]))
            {
                WriteFlag(unlocked[i]);
            }
        }
    }

    /// <summary>Забыть, что достижения выдавались, — только для отладки.</summary>
    public static void ClearLocalRecord()
    {
        requested.Clear();
        unlocked = new List<string>();
        Save();
    }

    private static void WriteFlag(string apiName)
    {
#if FILE_ACHIEVEMENTS
        try
        {
            string dir = FolderPath;
            Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, apiName);

            if (File.Exists(path))
            {
                // Заявка ещё в очереди: лаунчер удалит файл сам, править его нельзя.
                return;
            }

            File.WriteAllBytes(path, new byte[0]);
            Debug.Log("[Achievements] Заявка лаунчеру: " + apiName + " -> " + path);
        }
        catch (System.Exception e)
        {
            // Папка игры может оказаться недоступной на запись (установка
            // в Program Files и т.п.). Достижение — не повод ронять игру.
            Debug.LogWarning("[Achievements] Не удалось записать заявку " + apiName + ": " + e.Message);
        }
#endif
    }

    private static void MarkUnlocked(string apiName)
    {
        Load();

        if (unlocked.Contains(apiName))
        {
            return;
        }

        unlocked.Add(apiName);
        Save();
    }

    private static void Load()
    {
        string raw = PlayerPrefs.GetString(UnlockedKey, string.Empty);

        if (unlocked != null && raw == cachedRaw)
        {
            return;
        }

        unlocked = new List<string>();
        cachedRaw = raw;

        if (string.IsNullOrEmpty(raw))
        {
            return;
        }

        string[] parts = raw.Split(Separator);

        for (int i = 0; i < parts.Length; i++)
        {
            if (!string.IsNullOrEmpty(parts[i]) && !unlocked.Contains(parts[i]))
            {
                unlocked.Add(parts[i]);
            }
        }
    }

    private static void Save()
    {
        cachedRaw = string.Join(Separator.ToString(), unlocked.ToArray());
        PlayerPrefs.SetString(UnlockedKey, cachedRaw);
        PlayerPrefs.Save();
    }
}
