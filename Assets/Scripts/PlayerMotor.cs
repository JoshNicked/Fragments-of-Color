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

    [Header("Interaction Settings")]
    public float interactDistance = 2f;
    public float interactSphereRadius = 1f; // radius of the detection sphere
    public LayerMask interactMask;
    private bool isHoldingInteract;
    private InteractionUI interactionUI;
    private Camera playerCamera;
    

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
        playerCamera = GetComponentInChildren<Camera>();
        VelocityZHash = Animator.StringToHash("Velocity Z");
        VelocityXHash = Animator.StringToHash("Velocity X");
        interactionUI = Object.FindFirstObjectByType<InteractionUI>(); if (interactionUI == null)
            Debug.LogError("No InteractionUI found in scene!");
        // Ensure player starts on ground
        if (controller != null)
        {
            // Apply gravity immediately on start
            playerVelocity.y = gravity * 0.01f;
        }
        if (playerCamera == null)
        {
            Debug.LogError("PlayerMotor: No Camera found in children!");
        }
    }

    void Update()
    {
        UpdateAnimator(lastInput, sprinting);
        if (isHoldingInteract)
        {
            TryPushBox();
        }
        HandleInteraction(); // Always check for UI prompt based on proximity
    }

    private bool isPromptVisible = false;   // track current state

    private void HandleInteraction()
    {
        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 direction = transform.forward;

        bool shouldShow = false;

        if (Physics.SphereCast(origin, interactSphereRadius, direction, out RaycastHit hit, interactDistance, interactMask))
        {
            if (hit.collider.GetComponent<MovableBox>() != null)
            {
                shouldShow = true;
            }
        }

        // Hysteresis: make it harder to turn off than to turn on
        if (shouldShow)
        {
            isPromptVisible = true;
            interactionUI?.Show();
        }
        else if (isPromptVisible)
        {
            // Only hide if clearly no longer in range
            // You can increase this distance a bit (e.g. interactDistance + 0.4f)
            if (!Physics.SphereCast(origin, interactSphereRadius, direction, out _, interactDistance + 0.3f, interactMask))
            {
                isPromptVisible = false;
                interactionUI?.Hide();
            }
            // else: stay visible (this is the anti-flicker part)
        }
    }

    public void Sprint()
    {
        sprinting = !sprinting;
        speed = sprinting ? 8f : 5f;
    }

    private void TryPushBox()
    {
        if (playerCamera == null) return;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactDistance, interactMask))
        {
            MovableBox box = hit.collider.GetComponent<MovableBox>();
            if (box != null)
            {
                box.Push(transform.forward);
            }
        }
    }

    public void ProcessMove(Vector2 input)
    {
        // Improved ground check using both CharacterController and raycast
        isGrounded = CheckGrounded();
        // Horizontal movement
        Vector3 moveDirection = new Vector3(input.x, 0, input.y).normalized;
        Vector3 horizontalVelocity = transform.TransformDirection(moveDirection) * speed;
        if (isGrounded && playerVelocity.y < 0f)
        {
            playerVelocity.y = -2f; // stick to ground
        }
        else
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }
        Vector3 finalVelocity = horizontalVelocity + Vector3.up * playerVelocity.y;
        controller.Move(finalVelocity * Time.deltaTime);
        // Double-check ground after movement to ensure accurate ground detection
        if (controller.isGrounded && !isGrounded)
        {
            isGrounded = true;
            playerVelocity.y = -2f;
        }
        lastInput = input;
    }

    public void InteractStart()
    {
        isHoldingInteract = true;
    }

    public void InteractEnd()
    {
        isHoldingInteract = false;
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