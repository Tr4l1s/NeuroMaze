using UnityEngine;

public class FPSJoystickController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;

    [Header("Joystickler")]
    public VariableJoystick moveJoystick;   // Sol joystick (hareket)
    public VariableJoystick lookJoystick;   // Sað joystick (kamera)

    [Header("Bileþenler")]
    public Rigidbody rb;
    public Transform playerCamera; // Kamerayý buraya sürükle (Capsule içindeki Camera)

    private float pitch = 0f; // Kamera yukarý-aþaðý dönüþ (X ekseni)

    void FixedUpdate()
    {
        // --- Hareket ---
        Vector3 input = new Vector3(moveJoystick.Horizontal, 0f, moveJoystick.Vertical);
        Vector3 move = transform.TransformDirection(input.normalized) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    void LateUpdate()
    {
        // --- Kamera ve Oyuncu Rotasyonu ---
        float yaw = lookJoystick.Horizontal * lookSensitivity;   // Y ekseni (saða-sola)
        float xRot = lookJoystick.Vertical * lookSensitivity;    // X ekseni (yukarý-aþaðý)

        // Oyuncu gövdesini Y ekseninde döndür
        transform.Rotate(Vector3.up * yaw);

        // Kamerayý X ekseninde döndür
        pitch -= xRot;
        pitch = Mathf.Clamp(pitch, -60f, 60f); // Kameranýn çok yukarý/aþaðý bakmasýný engelle
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
