using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    public bool isGrounded;
    public float speed = 5f;
    public float gravity = -9.8f;
    public float jumpHeight = 1.5f;

    [Header("Ground Check Settings")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask = -1; // Check all layers by default

    private Animator animator;
    private float velocityX = 0.0f;
    private float velocityZ = 0.0f;
    private float acceleration = 2.0f;
    private float deceleration = 2.0f;
    private float maximumWalkVelocity = 0.5f;
    private float maximumRunVelocity = 2.0f;

    private int VelocityZHash;
    private int VelocityXHash;

    private bool sprinting = false;
    
    private Vector2 lastInput = Vector2.zero;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        VelocityZHash = Animator.StringToHash("Velocity Z");
        VelocityXHash = Animator.StringToHash("Velocity X");
        
        // Ensure player starts on ground
        if (controller != null)
        {
            // Apply gravity immediately on start
            playerVelocity.y = gravity * 0.01f;
        }
    }

    void Update()
    {
        UpdateAnimator(lastInput, sprinting);
    }

    public void Sprint()
    {
        sprinting = !sprinting;
        speed = sprinting ? 8f : 5f;
    }

    public void ProcessMove(Vector2 input)
    {
        // Improved ground check using both CharacterController and raycast
        isGrounded = CheckGrounded();

        // Horizontal movement
        Vector3 moveDirection = new Vector3(input.x, 0, input.y).normalized;
        Vector3 horizontalVelocity = transform.TransformDirection(moveDirection) * speed;

        // Vertical movement (gravity)
        if (isGrounded)
        {
            // When grounded, always apply downward force to maintain ground contact
            // This prevents floating issues
            playerVelocity.y = -2f; // Stronger downward force to stick to ground
        }
        else
        {
            // Apply gravity when not grounded
            playerVelocity.y += gravity * Time.fixedDeltaTime;
            // Clamp fall velocity to prevent falling too fast
            playerVelocity.y = Mathf.Max(playerVelocity.y, gravity * 3f);
        }

        // Combine horizontal and vertical
        Vector3 finalVelocity = horizontalVelocity + new Vector3(0, playerVelocity.y, 0);

        // Move the character using fixedDeltaTime since we're in FixedUpdate
        controller.Move(finalVelocity * Time.fixedDeltaTime);

        // Double-check ground after movement to ensure accurate ground detection
        if (controller.isGrounded && !isGrounded)
        {
            isGrounded = true;
            playerVelocity.y = -2f;
        }

        lastInput = input;
    }

    private bool CheckGrounded()
    {
        // Check CharacterController first
        if (controller.isGrounded)
        {
            return true;
        }

        // Backup: Use raycast from controller center to detect ground
        Vector3 rayStart = transform.position + controller.center;
        float rayLength = controller.height / 2f + controller.skinWidth + groundCheckDistance;
        
        if (Physics.Raycast(rayStart, Vector3.down, rayLength, groundMask))
        {
            return true;
        }

        // Additional check: SphereCast from bottom of controller
        Vector3 sphereStart = transform.position + controller.center + Vector3.down * (controller.height / 2f - controller.radius);
        if (Physics.CheckSphere(sphereStart, controller.radius + controller.skinWidth, groundMask))
        {
            return true;
        }

        return false;
    }


    private void UpdateAnimator(Vector2 input, bool isRunning)
    {
        if (animator == null) return;

        float targetZ = input.y;
        float targetX = input.x;

        float maxVelocity = isRunning ? maximumRunVelocity : maximumWalkVelocity;

        velocityZ = Mathf.MoveTowards(
            velocityZ,
            targetZ * maxVelocity,
            Time.deltaTime * (Mathf.Abs(targetZ) > 0 ? acceleration : deceleration)
        );

        velocityX = Mathf.MoveTowards(
            velocityX,
            targetX * maxVelocity,
            Time.deltaTime * (Mathf.Abs(targetX) > 0 ? acceleration : deceleration)
        );

        animator.SetFloat(VelocityZHash, velocityZ);
        animator.SetFloat(VelocityXHash, velocityX);
    }

    public void Jump()
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -1.5f * gravity);
        }
    }
}
