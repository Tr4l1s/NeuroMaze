using System.Collections;
using System.Collections.Generic;
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

    private float timer;
    private bool isStopped = false;

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
        timer += Time.deltaTime;

        if (!isStopped && timer >= stopInterval)
        {
            StartCoroutine(StopAndWait());
        }

        if (!isStopped && agent != null && target != null)
        {
            agent.SetDestination(target.position);

            if (agent.velocity.magnitude < 0.1f)
            {
                if (animator != null)
                    animator.SetBool("isMoving", false);
            }
            else
            {
                if (animator != null)
                    animator.SetBool("isMoving", true);
            }
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

        if (agent != null)
            agent.isStopped = false;

        isStopped = false;
        timer = 0f;
    }


    /// Swift'ten BPM geldiðinde GameManager burayý çaðýracak.
    /// Nabza göre canavarýn hýzýný ayarlýyoruz.
  
    public void OnNewBpm(int bpm)
    {
        Debug.Log($"EnemyController: Yeni BPM = {bpm}");

        if (agent == null) return;

        if (bpm >= stressThresholdBpm)
        {
            agent.speed = highStressSpeed;
        }
        else
        {
            agent.speed = normalSpeed;
        }
    }

    public void StopChasing()
    {
        isStopped = true;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        if (animator != null)
            animator.SetBool("isMoving", false);
    }

    public void ResumeChasing()
    {
        isStopped = false;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }
}
