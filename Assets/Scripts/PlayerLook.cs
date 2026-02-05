using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera cam;
    
    [Header("Mouse Sensitivity")]
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    
    [Header("Camera Settings")]
    [Range(0f, 90f)]
    public float maxLookAngle = 80f;
    
    // Private variables
    private float xRotation = 0f;
    
    void Start()
    {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Initialize camera to center angle
        if (cam != null)
        {
            cam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            xRotation = 0f;
        }
    }

    public void ProcessLook(Vector2 input)
    {
        // Mouse input from Input System - multiply by sensitivity and deltaTime for smooth frame-independent movement
        float mouseX = input.x * xSensitivity * Time.deltaTime;
        float mouseY = input.y * ySensitivity * Time.deltaTime;
        
        // Rotate the player body horizontally (Y rotation around world up)
        transform.Rotate(Vector3.up * mouseX);
        
        // Calculate vertical camera rotation (X rotation - clamped to prevent over-rotation)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        
        // Apply vertical rotation to camera (local rotation)
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}

