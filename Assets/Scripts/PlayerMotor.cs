using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float pushSpeed = 2f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;
    public bool isGrounded;

    [Header("Camera Reference")]
    public Transform cameraTransform;
    [HideInInspector]
    public bool isInteractingWithChest = false;

    /// <summary>When true, movement/input scripts should ignore player input (e.g. cinematics).</summary>
    [HideInInspector]
    public bool inputFrozen;
    public BoxInteraction boxInteraction;

    private Animator animator;

    private float velocityX = 0f;
    private float velocityZ = 0f;
    private float acceleration = 8f;
    private float deceleration = 8f;
    private float maximumWalkVelocity = 0.5f;
    private float maximumRunVelocity = 2f;

    private int VelocityZHash;
    private int VelocityXHash;
    private int IsPushingHash;
    private int IsPullingHash;

    private bool sprinting = false;
    private Vector2 lastInput = Vector2.zero;
    private bool jumpLocked = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        VelocityZHash = Animator.StringToHash("Velocity Z");
        VelocityXHash = Animator.StringToHash("Velocity X");
        IsPushingHash = Animator.StringToHash("IsPushing");
        IsPullingHash = Animator.StringToHash("IsPulling");

        playerVelocity.y = gravity * 0.01f;
    }

    void Update()
    {
        UpdateAnimator(lastInput);
    }

    public void ProcessMove(Vector2 input)
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && playerVelocity.y < 0f)
        {
            playerVelocity.y = -2f;
            jumpLocked = false;
        }

        Vector3 move = Vector3.zero;

        if (input.magnitude >= 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            move = camForward * input.y + camRight * input.x;

            if (boxInteraction == null || !boxInteraction.isRotationLocked)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }

        bool isPushing = boxInteraction != null && boxInteraction.isRotationLocked;

        float currentSpeed;

        if (isPushing)
            currentSpeed = pushSpeed;
        else
            currentSpeed = sprinting ? runSpeed : walkSpeed;

        Vector3 horizontalVelocity = move * currentSpeed;

        playerVelocity.y += gravity * Time.deltaTime;

        Vector3 finalVelocity = horizontalVelocity + Vector3.up * playerVelocity.y;
        controller.Move(finalVelocity * Time.deltaTime);

        lastInput = input;
    }

    public void Sprint()
    {
        sprinting = !sprinting;
    }

    public void Jump()
    {
        if (isGrounded && !jumpLocked)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpLocked = true;
        }
    }

    private void UpdateAnimator(Vector2 input)
    {
        if (animator == null) return;

        bool isPushing = boxInteraction != null && boxInteraction.isRotationLocked;

        animator.SetBool(IsPushingHash, isPushing);

        bool isPulling = false;

        if (isPushing)
        {
            if (input.y < -0.1f)
            {
                isPulling = true;
            }
        }

        animator.SetBool(IsPullingHash, isPulling);

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * input.y + camRight * input.x;
        Vector3 localMove = transform.InverseTransformDirection(moveDir);

        float maxVelocity = sprinting ? maximumRunVelocity : maximumWalkVelocity;

        velocityZ = Mathf.MoveTowards(
            velocityZ,
            localMove.z * maxVelocity,
            Time.deltaTime * acceleration
        );

        velocityX = Mathf.MoveTowards(
            velocityX,
            localMove.x * maxVelocity,
            Time.deltaTime * acceleration
        );

        animator.SetFloat(VelocityZHash, velocityZ);
        animator.SetFloat(VelocityXHash, velocityX);
    }
}