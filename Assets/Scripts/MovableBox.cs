using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovableBox : MonoBehaviour
{
    private Rigidbody rb;
    public float pushForce = 6f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Push(Vector3 direction)
    {
        direction.y = 0f;
        direction.Normalize();
        rb.AddForce(direction * pushForce, ForceMode.Force);
    }
}