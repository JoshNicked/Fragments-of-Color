using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera cam;
    
    [Header("Mouse Sensitivity")]
    public float xSensitivity = 0.5f;
    public float ySensitivity = 0.5f;

    public float smoothSpeed = 10f;
    private float currentX;
    private float currentY;

    [Header("Camera Settings")]
    [Range(0f, 90f)]
    public float maxLookAngle = 50f;
    
    private float xRotation = 0f;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (cam != null)
        {
            cam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            xRotation = 0f;
        }
    }

    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x * xSensitivity * 10f * Time.deltaTime;
        float mouseY = input.y * ySensitivity * 10f * Time.deltaTime;

        // Rotate player body
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
