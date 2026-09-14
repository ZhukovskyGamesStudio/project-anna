using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ComputerBrowserTab
{
    public string title;

    [TextArea(3, 12)]
    public string content;
}

[System.Serializable]
public class ComputerNoteData
{
    public string title;

    [TextArea(3, 12)]
    public string content;
}

[System.Serializable]
public class ComputerFileData
{
    public string title;

    [TextArea(3, 12)]
    public string content;
}

public class ComputerPanel : ObjectPanel
{
    [Header("Pages")]
    [SerializeField] private GameObject browserPage;
    [SerializeField] private GameObject notesPage;
    [SerializeField] private GameObject filesPage;

    [Header("Desktop Buttons")]
    [SerializeField] private GameObject browserButton;
    [SerializeField] private GameObject notesButton;
    [SerializeField] private GameObject filesButton1;
    [SerializeField] private GameObject filesButton2;
    [SerializeField] private GameObject filesButton3;

    [Header("Desktop Button Labels")]
    [SerializeField] private TextMeshProUGUI filesButton1Label;
    [SerializeField] private TextMeshProUGUI filesButton2Label;
    [SerializeField] private TextMeshProUGUI filesButton3Label;

    [Header("Browser")]
    [SerializeField] private TextMeshProUGUI browserText;
    [SerializeField] private GameObject tabButton1;
    [SerializeField] private GameObject tabButton2;
    [SerializeField] private GameObject tabButton3;
    [SerializeField] private TextMeshProUGUI tabButton1Label;
    [SerializeField] private TextMeshProUGUI tabButton2Label;
    [SerializeField] private TextMeshProUGUI tabButton3Label;

    [Header("Notes")]
    [SerializeField] private TextMeshProUGUI noteTitleText;
    [SerializeField] private TextMeshProUGUI notesText;
    [SerializeField] private Button prevNoteButton;
    [SerializeField] private Button nextNoteButton;

    [Header("Files")]
    [SerializeField] private TextMeshProUGUI fileTitleText;
    [SerializeField] private TextMeshProUGUI filesText;

    [Header("Day 1")]
    [SerializeField] private ComputerBrowserTab[] day1BrowserTabs;
    [SerializeField] private ComputerNoteData[] day1Notes;
    [SerializeField] private ComputerFileData[] day1Files;

    [Header("Day 2")]
    [SerializeField] private ComputerBrowserTab[] day2BrowserTabs;
    [SerializeField] private ComputerNoteData[] day2Notes;
    [SerializeField] private ComputerFileData[] day2Files;

    [Header("Day 3")]
    [SerializeField] private ComputerBrowserTab[] day3BrowserTabs;
    [SerializeField] private ComputerNoteData[] day3Notes;
    [SerializeField] private ComputerFileData[] day3Files;

    private ComputerBrowserTab[] currentBrowserTabs;
    private ComputerNoteData[] currentNotes;
    private ComputerFileData[] currentFiles;

    private int currentBrowserTabIndex;
    private int currentNoteIndex;

    public override void Show()
    {
        base.Show();

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.MarkComputerComplete();
        }

        LoadCurrentDayData();
        CloseAllWindows();
        UpdateDesktopButtons();
        UpdateBrowserTabButtons();
        ClearTexts();
    }

    public void OpenBrowser()
    {
        CloseAllWindows();
        currentBrowserTabIndex = 0;

        if (browserPage != null)
        {
            browserPage.SetActive(true);
        }

        UpdateBrowserTabButtons();
        UpdateBrowserView();
    }

    public void OpenNotes()
    {
        CloseAllWindows();
        currentNoteIndex = 0;

        if (notesPage != null)
        {
            notesPage.SetActive(true);
        }

        UpdateNotesView();
    }

    public void OpenFile1()
    {
        OpenFileByIndex(0);
    }

    public void OpenFile2()
    {
        OpenFileByIndex(1);
    }

    public void OpenFile3()
    {
        OpenFileByIndex(2);
    }

    public void CloseBrowserPage()
    {
        if (browserPage != null)
        {
            browserPage.SetActive(false);
        }
    }

    public void CloseNotesPage()
    {
        if (notesPage != null)
        {
            notesPage.SetActive(false);
        }
    }

    public void CloseFilesPage()
    {
        if (filesPage != null)
        {
            filesPage.SetActive(false);
        }
    }

    public void ShowBrowserTab1()
    {
        ShowBrowserTabByIndex(0);
    }

    public void ShowBrowserTab2()
    {
        ShowBrowserTabByIndex(1);
    }

    public void ShowBrowserTab3()
    {
        ShowBrowserTabByIndex(2);
    }

    public void PrevNote()
    {
        if (currentNotes == null || currentNotes.Length == 0)
        {
            return;
        }

        if (currentNoteIndex > 0)
        {
            currentNoteIndex--;
            UpdateNotesView();
        }
    }

    public void NextNote()
    {
        if (currentNotes == null || currentNotes.Length == 0)
        {
            return;
        }

        if (currentNoteIndex < currentNotes.Length - 1)
        {
            currentNoteIndex++;
            UpdateNotesView();
        }
    }

    private void LoadCurrentDayData()
    {
        if (DayManager.Instance == null)
        {
            currentBrowserTabs = null;
            currentNotes = null;
            currentFiles = null;
            return;
        }

        int day = DayManager.Instance.CurrentDay;

        if (day == 1)
        {
            currentBrowserTabs = day1BrowserTabs;
            currentNotes = day1Notes;
            currentFiles = day1Files;
        }
        else if (day == 2)
        {
            currentBrowserTabs = day2BrowserTabs;
            currentNotes = day2Notes;
            currentFiles = day2Files;
        }
        else
        {
            currentBrowserTabs = day3BrowserTabs;
            currentNotes = day3Notes;
            currentFiles = day3Files;
        }
    }

    private void UpdateDesktopButtons()
    {
        if (browserButton != null)
        {
            browserButton.SetActive(true);
        }

        if (notesButton != null)
        {
            notesButton.SetActive(true);
        }

        if (filesButton1 != null)
        {
            filesButton1.SetActive(currentFiles != null && currentFiles.Length > 0);
        }

        if (filesButton2 != null)
        {
            filesButton2.SetActive(currentFiles != null && currentFiles.Length > 1);
        }

        if (filesButton3 != null)
        {
            filesButton3.SetActive(currentFiles != null && currentFiles.Length > 2);
        }

        if (filesButton1Label != null)
        {
            filesButton1Label.text = currentFiles != null && currentFiles.Length > 0 ? Localizer.T(currentFiles[0].title) : "";
        }

        if (filesButton2Label != null)
        {
            filesButton2Label.text = currentFiles != null && currentFiles.Length > 1 ? Localizer.T(currentFiles[1].title) : "";
        }

        if (filesButton3Label != null)
        {
            filesButton3Label.text = currentFiles != null && currentFiles.Length > 2 ? Localizer.T(currentFiles[2].title) : "";
        }
    }

    private void UpdateBrowserTabButtons()
    {
        if (tabButton1 != null)
        {
            tabButton1.SetActive(currentBrowserTabs != null && currentBrowserTabs.Length > 0);
        }

        if (tabButton2 != null)
        {
            tabButton2.SetActive(currentBrowserTabs != null && currentBrowserTabs.Length > 1);
        }

        if (tabButton3 != null)
        {
            tabButton3.SetActive(currentBrowserTabs != null && currentBrowserTabs.Length > 2);
        }

        if (tabButton1Label != null)
        {
            tabButton1Label.text = currentBrowserTabs != null && currentBrowserTabs.Length > 0 ? Localizer.T(currentBrowserTabs[0].title) : "";
        }

        if (tabButton2Label != null)
        {
            tabButton2Label.text = currentBrowserTabs != null && currentBrowserTabs.Length > 1 ? Localizer.T(currentBrowserTabs[1].title) : "";
        }

        if (tabButton3Label != null)
        {
            tabButton3Label.text = currentBrowserTabs != null && currentBrowserTabs.Length > 2 ? Localizer.T(currentBrowserTabs[2].title) : "";
        }
    }

    private void UpdateBrowserView()
    {
        if (browserText == null)
        {
            return;
        }

        if (currentBrowserTabs == null || currentBrowserTabs.Length == 0)
        {
            browserText.text = "";
            return;
        }

        if (currentBrowserTabIndex < 0 || currentBrowserTabIndex >= currentBrowserTabs.Length)
        {
            currentBrowserTabIndex = 0;
        }

        browserText.text = Localizer.T(currentBrowserTabs[currentBrowserTabIndex].content);
    }

    private void ShowBrowserTabByIndex(int index)
    {
        if (currentBrowserTabs == null || currentBrowserTabs.Length == 0)
        {
            return;
        }

        if (index < 0 || index >= currentBrowserTabs.Length)
        {
            return;
        }

        currentBrowserTabIndex = index;
        UpdateBrowserView();
    }

    private void UpdateNotesView()
    {
        if (currentNotes == null || currentNotes.Length == 0)
        {
            if (noteTitleText != null)
            {
                noteTitleText.text = "";
            }

            if (notesText != null)
            {
                notesText.text = "";
            }

            if (prevNoteButton != null)
            {
                prevNoteButton.interactable = false;
            }

            if (nextNoteButton != null)
            {
                nextNoteButton.interactable = false;
            }

            return;
        }

        if (currentNoteIndex < 0)
        {
            currentNoteIndex = 0;
        }

        if (currentNoteIndex >= currentNotes.Length)
        {
            currentNoteIndex = currentNotes.Length - 1;
        }

        if (noteTitleText != null)
        {
            noteTitleText.text = Localizer.T(currentNotes[currentNoteIndex].title);
        }

        if (notesText != null)
        {
            notesText.text = Localizer.T(currentNotes[currentNoteIndex].content);
        }

        if (prevNoteButton != null)
        {
            prevNoteButton.interactable = currentNoteIndex > 0;
        }

        if (nextNoteButton != null)
        {
            nextNoteButton.interactable = currentNoteIndex < currentNotes.Length - 1;
        }
    }

    private void OpenFileByIndex(int index)
    {
        if (currentFiles == null || index < 0 || index >= currentFiles.Length)
        {
            return;
        }

        CloseAllWindows();

        if (filesPage != null)
        {
            filesPage.SetActive(true);
        }

        if (fileTitleText != null)
        {
            fileTitleText.text = Localizer.T(currentFiles[index].title);
        }

        if (filesText != null)
        {
            filesText.text = Localizer.T(currentFiles[index].content);
        }
    }

    private void CloseAllWindows()
    {
        if (browserPage != null)
        {
            browserPage.SetActive(false);
        }

        if (notesPage != null)
        {
            notesPage.SetActive(false);
        }

        if (filesPage != null)
        {
            filesPage.SetActive(false);
        }
    }

    private void ClearTexts()
    {
        if (browserText != null)
        {
            browserText.text = "";
        }

        if (noteTitleText != null)
        {
            noteTitleText.text = "";
        }

        if (notesText != null)
        {
            notesText.text = "";
        }

        if (fileTitleText != null)
        {
            fileTitleText.text = "";
        }

        if (filesText != null)
        {
            filesText.text = "";
        }
    }
}