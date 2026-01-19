using UnityEngine;

public class Movements : MonoBehaviour
{
    Animator animator;

    float velocityX = 0.0f;
    float velocityZ = 0.0f;

    public float acceleration = 2.0f;
    public float deceleration = 2.0f;
    public float maximumWalkVelocity = 0.5f;
    public float maximumRunVelocity = 2.0f;

    int VelocityZHash = Animator.StringToHash("Velocity Z");
    int VelocityXHash = Animator.StringToHash("Velocity X");

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void UpdateAnimator(Vector2 input, bool isRunning)
    {
        float targetZ = input.y;
        float targetX = input.x;

        float maxVelocity = isRunning ? maximumRunVelocity : maximumWalkVelocity;

        // Forward / Backward
        velocityZ = Mathf.MoveTowards(
            velocityZ,
            targetZ * maxVelocity,
            Time.deltaTime * (Mathf.Abs(targetZ) > 0 ? acceleration : deceleration)
        );

        // Left / Right
        velocityX = Mathf.MoveTowards(
            velocityX,
            targetX * maxVelocity,
            Time.deltaTime * (Mathf.Abs(targetX) > 0 ? acceleration : deceleration)
        );

        animator.SetFloat(VelocityZHash, velocityZ);
        animator.SetFloat(VelocityXHash, velocityX);
    }
}
