using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class mousemove : MonoBehaviour
{
    Vector3 move;
    public float gain = 15;
    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;
    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    void Start()
    {
        //stream.Open();
        move = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (Input.GetKey(KeyCode.S))
            rb.AddRelativeForce(Vector3.back * gain);
        if (Input.GetKey(KeyCode.W))
            rb.AddRelativeForce(Vector3.forward * gain);
        if (Input.GetKey(KeyCode.A))
            rb.AddRelativeForce(Vector3.left * gain);
        if (Input.GetKey(KeyCode.D))
            rb.AddRelativeForce(Vector3.right * gain);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");
        rotY += mouseX * mouseSensitivity * Time.deltaTime;
        rotX += mouseY * mouseSensitivity * Time.deltaTime;
        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);
        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;
    }
}

