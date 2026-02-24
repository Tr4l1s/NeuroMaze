using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioMixer mixer;
    public string exposedParamName = "MasterVolume";
    public float defaultLinear = 0.8f;

    private const string PREF_KEY = "MasterVolumeValue";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApplySaved();
    }

    public float GetSavedLinear()
    {
        return PlayerPrefs.GetFloat(PREF_KEY, defaultLinear);
    }

    public void SetVolumeLinear(float value)
    {
        float v = Mathf.Clamp(value, 0.0001f, 1f);
        float db = Mathf.Log10(v) * 20f;

        if (mixer != null)
            mixer.SetFloat(exposedParamName, db);

        PlayerPrefs.SetFloat(PREF_KEY, v);
        PlayerPrefs.Save();
    }

    public void ApplySaved()
    {
        SetVolumeLinear(GetSavedLinear());
    }
}