using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Durma Ayarlarý")]
    public float stopInterval = 10f; // Kaç saniyede bir dursun
    public float stopDuration = 3f;  // Kaç saniye dursun

    public GameObject monster;      // Sahnedeki canavar objesi
    public float Spawndelay = 60f;

    private float timer;
    private bool isStopped = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        monster.SetActive(false);
        Invoke(nameof(ActivateMonster), Spawndelay);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Eðer durma zamaný geldiyse Coroutine baþlat
        if (!isStopped && timer >= stopInterval)
        {
            StartCoroutine(StopAndWait());
        }

        // Eðer durmuyorsa hedefe doðru hareket et
        if (!isStopped)
        {
            agent.SetDestination(target.position);

            if (agent.velocity.magnitude < 0.1f)
            {
                animator.SetBool("isMoving", false);
            }
            else
            {
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
        agent.isStopped = true;              // hareketi durdur
        animator.SetBool("isMoving", false); // idle animasyonu devreye girsin

        yield return new WaitForSeconds(stopDuration);

        agent.isStopped = false; // tekrar kovalasýn
        isStopped = false;
        timer = 0f;              // süreyi sýfýrla
    }
}
