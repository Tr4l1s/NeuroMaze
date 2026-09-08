using UnityEngine;
using System.Collections.Generic;
using NeuroMaze.Pulse;

public class SafeZonePulseTrigger : MonoBehaviour
{
    public PulseBridge pulseBridge;
    public GameManager gameManager;
    public float cooldownSeconds = 40f;
    public QuizManager quizManager;
    public MonoBehaviour[] scriptsToDisable;
    public float panelDurationSeconds = 30f;
    public GameObject monster;
    public float monsterRespawnDelaySeconds = 15f;
    static readonly HashSet<SafeZonePulseTrigger> occupied = new HashSet<SafeZonePulseTrigger>();
    readonly HashSet<Collider> playerColliders = new HashSet<Collider>();
    readonly List<Behaviour> disabledControls = new List<Behaviour>();
    public static bool IsPlayerProtected { get { return occupied.Count > 0; } }
    public static float LastExitRespawnDelay { get; private set; }
    public bool PanelOpen { get { return panelOpen; } }
    bool panelOpen;
    float closesAt, cooldownUntil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState() { occupied.Clear(); LastExitRespawnDelay=0; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerColliders.Add(other);
        occupied.Add(this); // Protect even during cooldown, including before initial spawn.
        if (Time.time < cooldownUntil || panelOpen) return;
        OpenSafeZonePanel();
    }

    public void OpenSafeZonePanel()
    {
        if (panelOpen) return;
        // Overlapping safe-zone colliders must not start two concurrent quizzes.
        foreach (var zone in occupied) if (zone != this && zone.panelOpen) return;
        occupied.Add(this);
        panelOpen = true;
        panelDurationSeconds = 30f;
        cooldownUntil = Time.time + panelDurationSeconds + cooldownSeconds;
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (quizManager == null) quizManager = FindFirstObjectByType<QuizManager>();
        if (pulseBridge == null && gameManager != null) pulseBridge = gameManager.pulseBridge;
        if (scriptsToDisable != null) foreach (var control in scriptsToDisable) DisableControl(control);
        foreach (var control in FindObjectsByType<FPSJoystickController>(FindObjectsSortMode.None)) DisableControl(control);
        foreach (var control in FindObjectsByType<PauseMenuController>(FindObjectsSortMode.None)) DisableControl(control);
        foreach (var stick in FindObjectsByType<Joystick>(FindObjectsSortMode.None)) stick.OnPointerUp(null);
        if (quizManager != null) quizManager.StartQuiz();
        if (gameManager != null) gameManager.OnMeasurementStarted();
#if UNITY_ANDROID || UNITY_EDITOR
        var study = AndroidPulseStudy.Instance;
        if (study != null)
        {
            study.EnsureGameSession();
            var capture = study.GetComponent<GamePulseMeasurement>();
            if (capture == null) capture = study.gameObject.AddComponent<GamePulseMeasurement>();
            capture.Begin(gameManager, quizManager, gameObject.scene.name + "/" + name, 30f);
        }
#elif UNITY_IOS
        if (pulseBridge != null) pulseBridge.StartPulse();
#endif
        // Start the closing timer after quiz setup and capture Begin. Setup time must
        // never truncate the capture's final fixed window.
        closesAt = Time.realtimeSinceStartup + panelDurationSeconds;
        Debug.Log("PULSE_SAFE_ZONE_BEGIN " + name);
    }

    void DisableControl(Behaviour control)
    {
        if (control == null || control == this || control == gameManager || control == quizManager || control == pulseBridge || control is EnemyController) return;
        if (control.enabled && !disabledControls.Contains(control)) { disabledControls.Add(control); control.enabled = false; }
        var fps = control as FPSJoystickController;
        if (fps != null && fps.rb != null) { fps.rb.linearVelocity = Vector3.zero; fps.rb.angularVelocity = Vector3.zero; }
    }

    void Update()
    {
        if (panelOpen && Time.realtimeSinceStartup >= closesAt) CloseSafeZonePanel();
    }

    public void CloseSafeZonePanel()
    {
        if (!panelOpen) return;
        panelOpen = false;
        var study = AndroidPulseStudy.Instance;
        if (study != null)
        {
            var capture = study.GetComponent<GamePulseMeasurement>();
            if (capture != null) capture.Complete();
        }
        int correct = quizManager != null ? quizManager.correctAnswerCount : 0;
        if (quizManager != null) quizManager.EndQuiz();
        foreach (var control in disabledControls) if (control != null) control.enabled = true;
        disabledControls.Clear();
        if (gameManager != null)
        {
            gameManager.OnMeasurementFinished();
            gameManager.OnSafeZoneFinishedWithQuiz(correct);
        }
        if (playerColliders.Count == 0) ReleaseProtection();
        Debug.Log("PULSE_SAFE_ZONE_END " + name + ";correct=" + correct);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerColliders.Remove(other);
        // Keep protection until the timed quiz has closed, even after a physics displacement.
        if (playerColliders.Count == 0 && !panelOpen) ReleaseProtection();
    }
    void ReleaseProtection() { LastExitRespawnDelay=15f;occupied.Remove(this); }

    void OnApplicationPause(bool paused) { if (paused && panelOpen) CloseSafeZonePanel(); }
    void OnDisable() { CloseSafeZonePanel(); playerColliders.Clear(); occupied.Remove(this); }
}
