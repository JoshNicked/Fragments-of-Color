using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float sensitivity = 120f;
    public float distance = 0f;
    public float minDistance = 2f;
    public float maxDistance = 2f;
    [Range(0f, 80f)]
    public float maxLookAngle = 35f;
    public float smoothSpeed = 15f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ProcessLook(Vector2 input)
    {
        yaw += input.x * sensitivity * Time.deltaTime;
        pitch -= input.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = rotation;

        Vector3 position = target.position - rotation * Vector3.forward * distance;
        transform.position = position;
    }

    /// <summary>Call after moving the camera externally so free-look matches the current transform.</summary>
    public void SyncAnglesFromTransform()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
        if (pitch > 180f)
            pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
    }
}