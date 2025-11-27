using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickPlayerExample : MonoBehaviour
{
    public float speed;
    public float turnSpeed;
    public VariableJoystick variableJoystick;
    public Rigidbody rb;

    void FixedUpdate()
    {
        Vector3 input = new Vector3(variableJoystick.Horizontal, 0f, variableJoystick.Vertical);
        Vector3 dir = input.normalized;

        // Konumu yumuşakça taşı
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);

        // Yalnızca Y ekseninde bakış döndür
        if (dir.sqrMagnitude > 0.0001f)
        {
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, yaw, 0f);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        }
    }
}