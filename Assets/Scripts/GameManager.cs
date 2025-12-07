using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Baðlantýlar")]
    public PulseBridge pulseBridge;
    public EnemyController enemy;
    public TextMeshProUGUI currentBpmText;
    public TextMeshProUGUI weeklyBpmText;
    public TextMeshProUGUI measureInfoText;

    public EnvironmentDarkener envDarkener;

    public bool isMeasuring = false;

    private void Awake()
    {
        if (pulseBridge == null)
            pulseBridge = GetComponent<PulseBridge>();
    }

    public void OnPulseReceived(string pulseStr)
    {
        Debug.Log("Swift'ten BPM geldi: " + pulseStr);

        if (!int.TryParse(pulseStr, out int bpm))
        {
            Debug.LogWarning("BPM parse edilemedi: " + pulseStr);
            return;
        }

        if (currentBpmText != null)
            currentBpmText.text = $"Anlýk Nabýz: {bpm}";

        if (pulseBridge != null && weeklyBpmText != null)
        {
            int weekly = pulseBridge.GetWeeklyAverage();
            weeklyBpmText.text = $"Haftalýk Ortalama: {weekly}";
        }

        if (enemy != null)
        {
            enemy.OnNewBpm(bpm);
        }

        if (envDarkener != null)
            envDarkener.OnNewBpm(bpm);
    }

    public void OnMeasurementStarted()
    {
        isMeasuring = true;

        if (measureInfoText != null)
        {
            measureInfoText.gameObject.SetActive(true);
            measureInfoText.text = "Nabýz ölçülüyor...\nParmaðýnýzý kameraya tutun!";
        }

        if (enemy != null)
        {
            enemy.StopChasing();
        }
    }

    public void OnMeasurementFinished()
    {
        isMeasuring = false;

        if (measureInfoText != null)
            measureInfoText.gameObject.SetActive(false);

        if (enemy != null)
        {
            enemy.ResumeChasing();
        }
    }


    public void OnSafeZoneFinishedWithQuiz(int correctAnswers)
    {
        Debug.Log("SafeZone bitti, doðru sayýsý: " + correctAnswers);

    }
}
