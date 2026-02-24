using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject questionPanel;

    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (questionPanel != null) questionPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OpenQuestion()
    {
        if (questionPanel != null) questionPanel.SetActive(true);
    }

    public void CloseQuestion()
    {
        if (questionPanel != null) questionPanel.SetActive(false);
    }
}