using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject confirmExitPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    public bool IsPaused()
    {
        return isPaused;
    }

    public bool IsConfirmOpen()
    {
        return confirmExitPanel != null && confirmExitPanel.activeSelf;
    }

    private void Start()
    {
        pausePanel.SetActive(false);
        confirmExitPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenPauseMenu()
    {
        pausePanel.SetActive(true);
        confirmExitPanel.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        confirmExitPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenExitConfirmation()
    {
        confirmExitPanel.SetActive(true);
    }

    public void CloseExitConfirmation()
    {
        confirmExitPanel.SetActive(false);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}