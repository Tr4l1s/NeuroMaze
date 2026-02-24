using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseUIController : MonoBehaviour
{
    public GameObject losePanel;
    public bool resumeTimeOnClose = true;

    public GameObject[] hideWhenLoseActive;

    public void ShowLose()
    {
        if (losePanel != null)
            losePanel.SetActive(true);
        Debug.Log("SHOW LOSE ÇALIÞTI");
        SetGameplayUIVisible(false);

        Time.timeScale = 0f;
    }

    public void RestartScene()
    {
        if (resumeTimeOnClose) Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CloseLose()
    {
        if (losePanel != null)
            losePanel.SetActive(false);

        SetGameplayUIVisible(true);

        if (resumeTimeOnClose) Time.timeScale = 1f;
    }

    public void LoadSceneByName(string sceneName)
    {
        if (resumeTimeOnClose) Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    void SetGameplayUIVisible(bool visible)
    {
        if (hideWhenLoseActive == null) return;

        for (int i = 0; i < hideWhenLoseActive.Length; i++)
        {
            if (hideWhenLoseActive[i] != null)
                hideWhenLoseActive[i].SetActive(visible);
        }
    }
}