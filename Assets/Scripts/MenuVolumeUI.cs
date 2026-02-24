using UnityEngine;
using UnityEngine.UI;

public class MenuVolumeUI : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        if (AudioManager.Instance != null && volumeSlider != null)
        {
            float saved = AudioManager.Instance.GetSavedLinear();
            volumeSlider.SetValueWithoutNotify(saved);
        }
    }

    public void OnSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolumeLinear(value);
    }
}