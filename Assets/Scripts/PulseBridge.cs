using System.Runtime.InteropServices;
using UnityEngine;

public class PulseBridge : MonoBehaviour
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void StartPulseMeasurement();

    [DllImport("__Internal")]
    private static extern int GetWeeklyAverageBPM();

    [DllImport("__Internal")]
    private static extern void AppendBpmManual(int value);
#else
    // Editor veya iOS dýþý platformlar için stub:
    private static void StartPulseMeasurement()
    {
        Debug.Log("StartPulseMeasurement (Editor Stub): Gerçek ölçüm sadece iOS cihazda çalýþýr.");
    }

    private static int GetWeeklyAverageBPM()
    {
        Debug.Log("GetWeeklyAverageBPM (Editor Stub): Cihazda gerçek deðer döner.");
        return 0;
    }

    private static void AppendBpmManual(int value)
    {
        Debug.Log("AppendBpmManual (Editor Stub): Cihazda UserDefaults'a kaydedilir.");
    }
#endif


    public void StartPulse()
    {
        StartPulseMeasurement();
    }


    public int GetWeeklyAverage()
    {
        return GetWeeklyAverageBPM();
    }


    public void AppendManualBpm(int bpm)
    {
        AppendBpmManual(bpm);
    }
}
