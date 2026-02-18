using UnityEngine;

public class LeverController : MonoBehaviour
{
    public Transform leverHandle;
    public Transform gate;
    public Vector3 leverUpRotation = new Vector3(-45f, 0f, 0f);
    public Vector3 leverDownRotation = new Vector3(0f, 0f, 0f);
    public Vector3 gateUpPosition = new Vector3(0f, 3f, 0f);
    public float moveSpeed = 2f;

    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activated = true;
        }
    }

    private void Update()
    {
        if (activated)
        {

            leverHandle.localRotation = Quaternion.Lerp(
                leverHandle.localRotation,
                Quaternion.Euler(leverUpRotation),
                Time.deltaTime * moveSpeed
            );


            gate.localPosition = Vector3.Lerp(
                gate.localPosition,
                gateUpPosition,
                Time.deltaTime * moveSpeed
            );
        }
    }
}
