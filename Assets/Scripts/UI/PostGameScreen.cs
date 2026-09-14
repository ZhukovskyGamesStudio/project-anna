using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PostGameScreen : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private string counterFormat = "Открыто {0} из {1} концовок";
    [SerializeField] private string mainMenuSceneName = "Menu";

    public void Show()
    {
        if (counterText != null)
        {
            counterText.text = string.Format(Localizer.T(counterFormat), EndingProgress.UnlockedCount(), EndingProgress.Total);
        }

        if (root != null)
        {
            root.SetActive(true);
            root.transform.SetAsLastSibling();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
