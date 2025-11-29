using UnityEngine;
using System.Collections;

public class SafeZonePulseTrigger : MonoBehaviour
{
    public PulseBridge pulseBridge;
    public GameManager gameManager;
    public float cooldownSeconds = 40f;
    private bool isOnCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isOnCooldown) return;
        if (!other.CompareTag("Player")) return;

        if (pulseBridge != null)
        {
            Debug.Log("SafeZone: Oyuncu girdi, nabýz ölçümü baþlatýlýyor.");
            gameManager.OnMeasurementStarted();
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
}
