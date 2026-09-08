using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public PulseBridge pulseBridge;
    public EnemyController enemy;
    public TextMeshProUGUI currentBpmText, weeklyBpmText, measureInfoText;
    public EnvironmentDarkener envDarkener;
    public bool isMeasuring;
    bool hasGameResult;
    void Awake() { if(pulseBridge==null) pulseBridge=GetComponent<PulseBridge>(); }
    public void OnPulseReceived(string pulseStr)
    {
        int bpm;
        if(!int.TryParse(pulseStr,out bpm)) return;
        if(currentBpmText!=null) currentBpmText.text="Anlık Nabız: "+bpm;
        if(pulseBridge!=null && weeklyBpmText!=null) weeklyBpmText.text="Haftalık Ortalama: "+pulseBridge.GetWeeklyAverage();
        if(enemy!=null) enemy.OnNewBpm(bpm);
        if(envDarkener!=null) envDarkener.OnNewBpm(bpm);
    }
    public void OnMeasurementStarted()
    {
        isMeasuring=true; hasGameResult=false;
        SetGamePulseText("Nabız ölçülüyor…\nParmağınızı kameraya tutun ve soruları cevaplayın.");
        if(enemy!=null) enemy.StopChasing();
    }
    public void SetGamePulseText(string value)
    {
        if(measureInfoText!=null) {measureInfoText.gameObject.SetActive(true);measureInfoText.text=value;}
    }
    public void SetGamePulseResult(string value)
    {
        hasGameResult=true;
        SetGamePulseText(value);
        if(currentBpmText!=null) currentBpmText.text="Güvenli alan ölçümü tamamlandı";
        if(weeklyBpmText!=null) weeklyBpmText.text="";
    }
    public void OnMeasurementFinished()
    {
        isMeasuring=false;
        if(measureInfoText!=null && !hasGameResult) measureInfoText.gameObject.SetActive(false);
        if(enemy!=null) enemy.ResumeChasing();
    }
    public void OnSafeZoneFinishedWithQuiz(int correctAnswers) { Debug.Log("SafeZone doğru cevap: "+correctAnswers); }
}
