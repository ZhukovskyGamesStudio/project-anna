using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Отладка достижений из редактора. Steam здесь ни при чём: игра только кладёт
/// пустые файлы-заявки рядом с exe (в редакторе — в корень проекта), а открывает
/// достижения лаунчер антологии. Работает и вне Play Mode.
/// </summary>
public static class AchievementsMenu
{
    [MenuItem("Tools/Достижения/Показать состояние")]
    private static void ShowState()
    {
        string folder = LauncherAchievements.FolderPath;
        string report = "[Achievements] Папка заявок: " + folder + "\n";

        for (int i = 0; i < LauncherAchievements.All.Length; i++)
        {
            string id = LauncherAchievements.All[i];
            bool marked = LauncherAchievements.IsUnlocked(id);
            bool pending = !string.IsNullOrEmpty(folder) && File.Exists(Path.Combine(folder, id));
            report += "  " + id + ": выдано=" + marked + ", заявка лежит=" + pending + "\n";
        }

        report += "«заявка лежит» = лаунчер её ещё не забрал (не запущен, или имя не опубликовано в Steamworks).";
        Debug.Log(report);
    }

    [MenuItem("Tools/Достижения/Открыть папку заявок")]
    private static void OpenFolder()
    {
        string folder = LauncherAchievements.FolderPath;
        Directory.CreateDirectory(folder);
        EditorUtility.RevealInFinder(folder);
    }

    [MenuItem("Tools/Достижения/Сбросить локально")]
    private static void ResetLocal()
    {
        LauncherAchievements.ClearLocalRecord();

        string folder = LauncherAchievements.FolderPath;

        if (Directory.Exists(folder))
        {
            for (int i = 0; i < LauncherAchievements.All.Length; i++)
            {
                string path = Path.Combine(folder, LauncherAchievements.All[i]);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        Debug.Log("[Achievements] Локальная пометка и неотданные заявки очищены. " +
                  "В Steam уже открытое достижение это не отменяет — только на стороне игры.");
    }
}
