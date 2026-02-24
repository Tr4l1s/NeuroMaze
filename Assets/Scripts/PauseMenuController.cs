using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject settingsPanel;

    public string mainMenuSceneName = "MainMenu";

    public bool startHidden = true;
    public bool useEscToToggle = true;

    public GameObject[] hideOnPause;

    private bool isPaused;

    void Start()
    {
        if (startHidden)
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        SetGameplayUIVisible(true);

        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        if (!useEscToToggle) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        SetGameplayUIVisible(false);

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;

        SetGameplayUIVisible(true);
    }

    public void OpenSettings()
    {
        if (!isPaused) Pause();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void GoMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    void SetGameplayUIVisible(bool visible)
    {
        if (hideOnPause == null) return;

        for (int i = 0; i < hideOnPause.Length; i++)
        {
            if (hideOnPause[i] != null)
                hideOnPause[i].SetActive(visible);
        }
    }
}