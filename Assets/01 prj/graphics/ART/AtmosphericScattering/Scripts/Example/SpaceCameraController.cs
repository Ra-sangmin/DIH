using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceCameraController : MonoBehaviour
{
    public float sensitivity = 1;
    public float rollSpeed = 10;
    public float moveSpeed = 10;

    private Quaternion axis = Quaternion.identity;
    private Vector2 rotation = Vector2.zero;

    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        rotation += input * sensitivity;
        rotation.x %= 360;
        rotation.y = Mathf.Clamp(rotation.y, -89, 89);

        float roll = 0f;
        if (Input.GetKey(KeyCode.E)) roll -= rollSpeed;
        if (Input.GetKey(KeyCode.Q)) roll += rollSpeed;
        
        Vector3 vaxis = axis * Quaternion.Euler(-rotation.y, rotation.x, 0f) * Vector3.forward;//Vector3.Cross(Vector3.Cross(before * Vector3.forward, axis * Vector3.up), axis * Vector3.up);

        axis *= Quaternion.AngleAxis(roll * Time.deltaTime, vaxis);

        transform.rotation = axis * Quaternion.Euler(-rotation.y, rotation.x, 0f);

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        float accend = 0f;
        if (Input.GetKey(KeyCode.Space)) accend += 1;
        if (Input.GetKey(KeyCode.LeftShift)) accend -= 1;
        transform.position += (transform.forward * input.y + transform.right * input.x + transform.up * accend) * moveSpeed * Time.deltaTime;
    }
}
