using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public GameObject settingsPanel;
    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Debug.Log("Oyun kapatýlýyor...");
        Application.Quit();
    }
}
