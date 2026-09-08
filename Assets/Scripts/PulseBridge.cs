using System.Runtime.InteropServices;
using UnityEngine;

public class PulseBridge : MonoBehaviour
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void StartPulseMeasurement();
    [DllImport("__Internal")] private static extern int GetWeeklyAverageBPM();
    [DllImport("__Internal")] private static extern void AppendBpmManual(int value);
#endif

    public void StartPulse()
    {
#if UNITY_IOS && !UNITY_EDITOR
        StartPulseMeasurement();
#elif UNITY_ANDROID || UNITY_EDITOR
        var study = NeuroMaze.Pulse.AndroidPulseStudy.Instance;
        if (study != null)
        {
            study.EnsureGameSession();
            var capture = study.GetComponent<NeuroMaze.Pulse.GamePulseMeasurement>();
            if (capture == null) capture = study.gameObject.AddComponent<NeuroMaze.Pulse.GamePulseMeasurement>();
            capture.Begin(FindFirstObjectByType<GameManager>(), FindFirstObjectByType<QuizManager>(), "safe_zone", 30f);
        }
#else
        Debug.Log("Bu platformda kamera nabız ölçümü desteklenmiyor.");
#endif
    }

    public int GetWeeklyAverage()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return GetWeeklyAverageBPM();
#else
        // Android research records use explicit session/phase summaries in the study
        // panel. Never present mixed participants as a weekly average.
        return 0;
#endif
    }

    public void AppendManualBpm(int bpm)
    {
#if UNITY_IOS && !UNITY_EDITOR
        AppendBpmManual(bpm);
#else
        Debug.LogWarning("Android çalışma kayıtlarına kaynaksız manuel BPM eklenmez.");
#endif
    }
}
