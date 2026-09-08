using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Transform target;
    public float stopInterval = 10f, stopDuration = 3f;
    public GameObject monster;
    public float Spawndelay = 30f;
    public float normalSpeed = 3.5f, highStressSpeed = 6f;
    public int stressThresholdBpm = 70;
    public LoseUIController loseUI;
    NavMeshAgent agent;
    Animator animator;
    Renderer[] visuals;
    Collider[] hitboxes;
    bool[] visualEnabled, colliderEnabled;
    bool hasLost, manuallyStopped, visible;
    float stopTimer, restRemaining;
    NeuroMaze.Pulse.EnemyEncounterClock clock;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        var body = monster != null ? monster : gameObject;
        visuals = body.GetComponentsInChildren<Renderer>(true);
        hitboxes = body.GetComponentsInChildren<Collider>(true);
        visualEnabled = new bool[visuals.Length]; colliderEnabled = new bool[hitboxes.Length];
        for (int i=0;i<visuals.Length;i++) visualEnabled[i]=visuals[i].enabled;
        for (int i=0;i<hitboxes.Length;i++) colliderEnabled[i]=hitboxes[i].enabled;
        clock = new NeuroMaze.Pulse.EnemyEncounterClock(Spawndelay);
        if (agent != null) agent.speed = normalSpeed;
        // Keep this controller active: deactivating its own GameObject used to break spawn timing.
        visible = true; ShowBody(false); StopAgent();
    }

    void Update()
    {
        if (hasLost) return;
        bool previouslySpawned=clock.Spawned;
        bool canAppear=clock.Advance(Time.deltaTime,SafeZonePulseTrigger.IsPlayerProtected,SafeZonePulseTrigger.LastExitRespawnDelay);
        if(!canAppear) {ShowBody(false);StopAgent();return;}
        if(!previouslySpawned) Debug.Log("PULSE_ENEMY_SPAWN");
        ShowBody(true);
        if (manuallyStopped) { StopAgent(); return; }
        if (restRemaining > 0) { restRemaining -= Time.deltaTime; StopAgent(); return; }
        stopTimer += Time.deltaTime;
        if (stopInterval > 0 && stopTimer >= stopInterval) { stopTimer=0; restRemaining=stopDuration; StopAgent(); return; }
        if (CanNavigate())
        {
            agent.isStopped = false;
            if (target != null) agent.SetDestination(target.position);
            if (animator != null) animator.SetBool("isMoving", agent.velocity.sqrMagnitude >= 0.01f);
        }
    }

    bool CanNavigate() { return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh; }
    void StopAgent() { if (CanNavigate()) agent.isStopped = true; if (animator != null) animator.SetBool("isMoving", false); }
    void ShowBody(bool show)
    {
        if (visuals == null || visible == show) return;
        visible=show;
        for(int i=0;i<visuals.Length;i++) if(visuals[i]!=null) visuals[i].enabled=show && visualEnabled[i];
        for(int i=0;i<hitboxes.Length;i++) if(hitboxes[i]!=null) hitboxes[i].enabled=show && colliderEnabled[i];
    }
    public void StopChasing() { manuallyStopped=true; StopAgent(); }
    public void ResumeChasing() { manuallyStopped=false; }
    public void OnNewBpm(int bpm) { if(agent!=null) agent.speed=bpm>=stressThresholdBpm?highStressSpeed:normalSpeed; }
    void OnTriggerEnter(Collider other)
    {
        if (hasLost || clock==null || !clock.Spawned || !visible || SafeZonePulseTrigger.IsPlayerProtected || manuallyStopped || clock.ReturnRemaining>0) return;
        if (!other.CompareTag("Player")) return;
        hasLost=true; StopAgent();
        if(loseUI!=null) loseUI.ShowLose();
    }
    void OnDisable() { StopAgent(); }
}
