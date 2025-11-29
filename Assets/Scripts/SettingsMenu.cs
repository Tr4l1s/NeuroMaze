using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider volumeSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("volume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("volume");
            volumeSlider.value = savedVolume;
            SetVolume(savedVolume);
        }
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void SetVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MasterVolume", dB);

        PlayerPrefs.SetFloat("volume", value);
    }
}
