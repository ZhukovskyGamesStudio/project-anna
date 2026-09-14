using UnityEngine;

public static class EndingProgress
{
    private const string WatchedKey = "Ending_Watched";
    private const string UncertainKey = "Ending_Uncertain";
    private const string IllKey = "Ending_Ill";

    public const int Total = 3;

    public static void MarkWatched()
    {
        PlayerPrefs.SetInt(WatchedKey, 1);
        PlayerPrefs.Save();
        LauncherAchievements.Unlock(LauncherAchievements.EndingWatched);
    }

    public static void MarkUncertain()
    {
        PlayerPrefs.SetInt(UncertainKey, 1);
        PlayerPrefs.Save();
        LauncherAchievements.Unlock(LauncherAchievements.EndingUncertain);
    }

    public static void MarkIll()
    {
        PlayerPrefs.SetInt(IllKey, 1);
        PlayerPrefs.Save();
        LauncherAchievements.Unlock(LauncherAchievements.EndingIll);
    }

    /// <summary>
    /// Выдать достижения за концовки, которые уже открыты. Нужно тем, кто
    /// прошёл игру до того, как достижения появились: концовка отмечена
    /// в PlayerPrefs, а заявки лаунчеру по ней никто не подавал.
    /// </summary>
    public static void SyncAchievements()
    {
        if (PlayerPrefs.GetInt(WatchedKey, 0) == 1)
        {
            LauncherAchievements.Unlock(LauncherAchievements.EndingWatched);
        }

        if (PlayerPrefs.GetInt(UncertainKey, 0) == 1)
        {
            LauncherAchievements.Unlock(LauncherAchievements.EndingUncertain);
        }

        if (PlayerPrefs.GetInt(IllKey, 0) == 1)
        {
            LauncherAchievements.Unlock(LauncherAchievements.EndingIll);
        }
    }

    public static int UnlockedCount()
    {
        int count = 0;

        if (PlayerPrefs.GetInt(WatchedKey, 0) == 1)
        {
            count++;
        }

        if (PlayerPrefs.GetInt(UncertainKey, 0) == 1)
        {
            count++;
        }

        if (PlayerPrefs.GetInt(IllKey, 0) == 1)
        {
            count++;
        }

        return count;
    }
}
