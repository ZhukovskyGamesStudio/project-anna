using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public PlayerMovement playerMovement;
    public PlayerLook playerLook;
    public SleepPanel introSleepPanel;
    public PauseMenuUI pauseMenu;

    private ObjectPanel currentPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        yield return null;

        if (DayManager.Instance != null && DayManager.Instance.DebugSkipIntro)
        {
            yield break;
        }

        if (DayManager.Instance != null &&
            DayManager.Instance.CurrentDay == 1 &&
            introSleepPanel != null)
        {
            introSleepPanel.OpenIntroDream();
        }
        else
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAmbientForCurrentDay();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscape();
        }

        UpdateCursorState();
    }

    private void HandleEscape()
    {
        if (pauseMenu != null && pauseMenu.IsConfirmOpen())
        {
            pauseMenu.CloseExitConfirmation();
            return;
        }

        if (pauseMenu != null && pauseMenu.IsPaused())
        {
            pauseMenu.ResumeGame();
            return;
        }

        if (currentPanel != null)
        {
            if (FinalSequenceManager.Instance != null && FinalSequenceManager.Instance.BlockEscapeClose())
            {
                return;
            }

            CloseCurrentPanel();
            return;
        }

        if (playerMovement != null && !playerMovement.CanMove())
        {
            return;
        }

        if (pauseMenu != null)
        {
            pauseMenu.OpenPauseMenu();
        }
    }

    private void UpdateCursorState()
    {
        if (playerMovement == null)
        {
            return;
        }

        if (pauseMenu != null && pauseMenu.IsPaused())
        {
            return;
        }

        if (playerMovement.CanMove())
        {
            if (Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            if (!Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    public void OpenPanel(ObjectPanel panel)
    {
        if (panel == null)
        {
            return;
        }

        if (!panel.CanOpen())
        {
            return;
        }

        if (currentPanel != null)
        {
            currentPanel.Hide();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllNonPanelAudio();
            AudioManager.Instance.StopPanelMusic();
        }

        currentPanel = panel;
        currentPanel.Show();

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }

        if (playerLook != null)
        {
            playerLook.SetCanLook(false);
        }

        if (AudioManager.Instance != null && currentPanel.UseDefaultPanelMusic())
        {
            AudioManager.Instance.PlayDefaultPanelMusic();
        }
    }

    public void CloseCurrentPanel()
    {
        if (currentPanel == null)
        {
            return;
        }

        currentPanel.Hide();
        currentPanel = null;

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }

        if (playerLook != null)
        {
            playerLook.SetCanLook(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopRadioAudio();
            AudioManager.Instance.StopPhoneAudio();
            AudioManager.Instance.StopPanelMusic();

            if (FinalSequenceManager.Instance == null || !FinalSequenceManager.Instance.ShouldKeepAmbientMuted())
            {
                AudioManager.Instance.PlayAmbientForCurrentDay();
            }
        }
    }

    public bool IsPanelOpen()
    {
        return currentPanel != null;
    }
}