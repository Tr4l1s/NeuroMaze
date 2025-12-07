using UnityEngine;
using System.Collections;

public class SafeZonePulseTrigger : MonoBehaviour
{
    public PulseBridge pulseBridge;
    public GameManager gameManager;
    public float cooldownSeconds = 40f;
    private bool isOnCooldown = false;

    public QuizManager quizManager;

    public MonoBehaviour[] scriptsToDisable;

    public float panelDurationSeconds = 30f;

    public GameObject monster;
    public float monsterRespawnDelaySeconds = 30f;


    private bool panelOpen = false;

    private Coroutine panelTimerCoroutine;
    private Coroutine monsterRespawnCoroutine;


    private void OnTriggerEnter(Collider other)
    {
        if (isOnCooldown) return;
        if (!other.CompareTag("Player")) return;

        OpenSafeZonePanel();
        StartPanelTimer();

        if (monster != null)
        {
            monster.SetActive(false);
        }

        if (monsterRespawnCoroutine != null)
        {
            StopCoroutine(monsterRespawnCoroutine);
            monsterRespawnCoroutine = null;
        }

        if (pulseBridge != null)

        {




            if (gameManager != null)
            {
                gameManager.OnMeasurementStarted();
            }

            
            pulseBridge.StartPulse();
            StartCoroutine(Cooldown());
        }
    }

    private IEnumerator Cooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownSeconds);
        isOnCooldown = false;
    }

    public void OpenSafeZonePanel()
    {
        if (panelOpen) return;
        panelOpen = true;

        if (quizManager != null)
        {
            quizManager.StartQuiz();
        }

        if (scriptsToDisable != null)
        {
            foreach (var script in scriptsToDisable)
            {
                if (script != null)
                    script.enabled = false;
            }
        }
    }

    public void CloseSafeZonePanel()
    {
        if (!panelOpen) return;
        panelOpen = false;

        int correctCount = 0;

        if (quizManager != null)
        {
            correctCount = quizManager.correctAnswerCount;
            quizManager.EndQuiz();
        }

        if (gameManager != null)
        {
            gameManager.OnMeasurementFinished();
            gameManager.OnSafeZoneFinishedWithQuiz(correctCount);
        }

        if (scriptsToDisable != null)
        {
            foreach (var script in scriptsToDisable)
            {
                if (script != null)
                    script.enabled = true;
            }
        }

        if (panelTimerCoroutine != null)
        {
            StopCoroutine(panelTimerCoroutine);
            panelTimerCoroutine = null;
        }
    }

    private void StartPanelTimer()
    {
        if (panelTimerCoroutine != null)
        {
            StopCoroutine(panelTimerCoroutine);
        }

        panelTimerCoroutine = StartCoroutine(PanelTimerRoutine());
    }

    private IEnumerator PanelTimerRoutine()
    {
        float elapsed = 0f;

        while (elapsed < panelDurationSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        CloseSafeZonePanel();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (monster != null)
        {
            if (monsterRespawnCoroutine != null)
            {
                StopCoroutine(monsterRespawnCoroutine);
            }

            monsterRespawnCoroutine = StartCoroutine(ReactivateMonsterAfterDelay());
        }
    }

    private IEnumerator ReactivateMonsterAfterDelay()
    {
        yield return new WaitForSeconds(monsterRespawnDelaySeconds);

        if (monster != null)
        {
            monster.SetActive(true);
        }

        monsterRespawnCoroutine = null;
    }

    // (Oyuncu güvenli alandan beklemeden çýkarsa paneli kapatma kodu)
    /*
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        CloseSafeZonePanel();
    }
    */
}
