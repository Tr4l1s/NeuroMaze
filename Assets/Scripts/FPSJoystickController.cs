using UnityEngine;

public class FPSJoystickController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;

    [Header("Joystickler")]
    public VariableJoystick moveJoystick;
    public VariableJoystick lookJoystick;

    [Header("Bileþenler")]
    public Rigidbody rb;
    public Transform playerCamera;

    private float pitch = 0f;

    void FixedUpdate()
    {

        Vector3 input = new Vector3(moveJoystick.Horizontal, 0f, moveJoystick.Vertical);
        Vector3 move = transform.TransformDirection(input.normalized) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    void LateUpdate()
    {

        float yaw = lookJoystick.Horizontal * lookSensitivity;
        float xRot = lookJoystick.Vertical * lookSensitivity;


        transform.Rotate(Vector3.up * yaw);


        pitch -= xRot;
        pitch = Mathf.Clamp(pitch, -60f, 60f);
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
