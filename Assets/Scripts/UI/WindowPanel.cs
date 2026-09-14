using UnityEngine;

public class WindowPanel : ObjectPanel
{
    [SerializeField] private Camera windowCamera;

    public override void Show()
    {
        base.Show();

        // Достижение за первый взгляд в окно. Unlock идемпотентен, так что
        // отдельный флаг «уже показывали» тут не нужен.
        LauncherAchievements.Unlock(LauncherAchievements.Window);

        if (windowCamera != null)
        {
            windowCamera.enabled = true;
        }
    }

    public override void Hide()
    {
        if (windowCamera != null)
        {
            windowCamera.enabled = false;
        }

        base.Hide();
    }
}