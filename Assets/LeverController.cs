using UnityEngine;

public class LeverController : MonoBehaviour
{
    public Transform leverHandle;   // Þalterin dönen kýsmý
    public Transform gate;          // Demir parmaklýklý kapý
    public Vector3 leverUpRotation = new Vector3(-45f, 0f, 0f); // Yukarý konumu
    public Vector3 leverDownRotation = new Vector3(0f, 0f, 0f); // Aþaðý konumu
    public Vector3 gateUpPosition = new Vector3(0f, 3f, 0f);    // Kapý yukarý konumu
    public float moveSpeed = 2f;

    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Oyuncu collider'a girerse
        {
            activated = true;
        }
    }

    private void Update()
    {
        if (activated)
        {
            // Þalteri yukarý çevir
            leverHandle.localRotation = Quaternion.Lerp(
                leverHandle.localRotation,
                Quaternion.Euler(leverUpRotation),
                Time.deltaTime * moveSpeed
            );

            // Kapýyý yukarý taþý
            gate.localPosition = Vector3.Lerp(
                gate.localPosition,
                gateUpPosition,
                Time.deltaTime * moveSpeed
            );
        }
    }
}
