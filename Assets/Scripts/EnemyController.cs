using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Hedef ve Bileþenler")]
    public Transform target;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Durma Ayarlarý")]
    public float stopInterval = 10f;
    public float stopDuration = 3f;

    [Header("Doðma Ayarlarý")]
    public GameObject monster;
    public float Spawndelay = 60f;

    [Header("Nabza Göre Hýz Ayarlarý")]
    public float normalSpeed = 3.5f;
    public float highStressSpeed = 6f;
    public int stressThresholdBpm = 70;

    [Header("Kaybetme")]
    public LoseUIController loseUI;

    private float timer;
    private bool isStopped = false;
    private bool hasLost = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent != null)
            agent.speed = normalSpeed;

        if (monster != null)
        {
            monster.SetActive(false);
            Invoke(nameof(ActivateMonster), Spawndelay);
        }
    }

    void Update()
    {
        if (hasLost) return;

        timer += Time.deltaTime;

        if (!isStopped && timer >= stopInterval)
        {
            StartCoroutine(StopAndWait());
        }

        if (!isStopped && agent != null && target != null)
        {
            agent.SetDestination(target.position);

            if (animator != null)
                animator.SetBool("isMoving", agent.velocity.magnitude >= 0.1f);
        }
    }

    void ActivateMonster()
    {
        if (monster != null)
            monster.SetActive(true);
    }

    IEnumerator StopAndWait()
    {
        isStopped = true;

        if (agent != null)
            agent.isStopped = true;

        if (animator != null)
            animator.SetBool("isMoving", false);

        yield return new WaitForSeconds(stopDuration);

        if (!hasLost && agent != null)
            agent.isStopped = false;

        isStopped = false;
        timer = 0f;
    }

    public void OnNewBpm(int bpm)
    {
        if (agent == null) return;

        agent.speed = (bpm >= stressThresholdBpm) ? highStressSpeed : normalSpeed;
    }

    public void StopChasing()
    {
        isStopped = true;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = true;

        if (animator != null)
            animator.SetBool("isMoving", false);
    }

    public void ResumeChasing()
    {
        isStopped = false;

        if (!hasLost && agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasLost) return;

        if (other.CompareTag("Player"))
        {
            hasLost = true;
            StopChasing();

            if (loseUI != null)
                loseUI.ShowLose();
            else
                Debug.LogError("EnemyController: loseUI atanmadý! Inspector'dan LoseUIController objesini sürükle.");
        }
    }
}