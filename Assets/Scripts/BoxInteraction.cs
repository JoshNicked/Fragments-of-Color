using UnityEngine;

public class BoxInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactDistance = 2f;
    public KeyCode interactKey = KeyCode.F;
    public float pushPullSpeed = 3f;
    public LayerMask boxLayer;

    [Header("UI")]
    public GameObject interactPrompt;

    [Header("Player Reference")]
    public Transform playerTransform;
    public PlayerMotor playerMotor;

    [HideInInspector]
    public bool isRotationLocked = false;

    private Rigidbody boxRigidbody;
    private BoxCollider boxCollider;

    // --- Toggle state ---
    private bool isInteracting = false;
    private Rigidbody lockedBoxRigidbody; // keeps reference while toggled on

    void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        boxRigidbody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        if (boxRigidbody != null)
            boxRigidbody.isKinematic = true;

        // Try to auto-assign player references if not set in inspector
        if (playerMotor == null)
            playerMotor = Object.FindFirstObjectByType<PlayerMotor>();

        if (playerTransform == null && playerMotor != null)
            playerTransform = playerMotor.transform;

        if (playerMotor == null || playerTransform == null)
            Debug.LogWarning("BoxInteraction: playerMotor or playerTransform not assigned. Assign in inspector or ensure a PlayerMotor exists in scene.");
    }

    void Update()
    {
        CheckForBox();
        HandleToggleInput();
    }

    void FixedUpdate()
    {
        // Use the locked reference while interacting, not the live raycast one
        bool canInteract;
        if (playerMotor != null)
            canInteract = lockedBoxRigidbody != null && isInteracting && !playerMotor.isInteractingWithChest;
        else
            canInteract = lockedBoxRigidbody != null && isInteracting;

        if (canInteract)
        {
            isRotationLocked = true;

            // enable physics movement and better collision detection while interacting
            lockedBoxRigidbody.isKinematic = false;
            lockedBoxRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            lockedBoxRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            MoveBoxWithPlayer();
            LockPlayerRotationTowardBox();
        }
        else
        {
            isRotationLocked = false;

            if (lockedBoxRigidbody != null)
            {
                // revert to kinematic when not interacting so other physics don't move it
                lockedBoxRigidbody.isKinematic = true;
                lockedBoxRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                lockedBoxRigidbody.interpolation = RigidbodyInterpolation.None;
            }
        }
    }

    // -------------------------------------------------------
    //  Toggle logic
    // -------------------------------------------------------
    void HandleToggleInput()
    {
        if (!Input.GetKeyDown(interactKey)) return;

        if (!isInteracting)
        {
            // Only start if the player is actually looking at a box right now
            if (boxRigidbody != null && (playerMotor == null || !playerMotor.isInteractingWithChest))
            {
                isInteracting = true;
                lockedBoxRigidbody = boxRigidbody; // lock onto this box

                // cache collider if missing
                if (boxCollider == null && lockedBoxRigidbody != null)
                    boxCollider = lockedBoxRigidbody.GetComponent<BoxCollider>();
            }
        }
        else
        {
            StopInteracting();
        }
    }

    void StopInteracting()
    {
        isInteracting = false;

        if (lockedBoxRigidbody != null)
            lockedBoxRigidbody.isKinematic = true;

        lockedBoxRigidbody = null;
    }

    // -------------------------------------------------------
    //  Existing helpers (unchanged except boxRigidbody → lockedBoxRigidbody)
    // -------------------------------------------------------
    void MoveBoxWithPlayer()
    {
        if (playerTransform == null || lockedBoxRigidbody == null) return;

        Vector3 moveInput = new Vector3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical")
        );

        if (moveInput.magnitude < 0.1f) return;

        Vector3 moveDir =
            playerTransform.forward * moveInput.z +
            playerTransform.right * moveInput.x;

        moveDir.Normalize();

        // Calculate desired delta movement this physics step
        Vector3 delta = moveDir * pushPullSpeed * Time.fixedDeltaTime;
        float distance = delta.magnitude;
        if (distance <= 0f) return;

        // Perform a sweep test to check if moving the box in that direction would collide
        RaycastHit hit;
        bool hitSomething = false;

        // Ensure we have a collider reference
        if (boxCollider == null)
            boxCollider = lockedBoxRigidbody.GetComponent<BoxCollider>();

        // Use Rigidbody.SweepTest which approximates moving the collider and reports first hit
        if (lockedBoxRigidbody.SweepTest(moveDir, out hit, distance))
        {
            hitSomething = true;
        }

        if (hitSomething)
        {
            // Move as far as possible without penetrating the hit surface
            float safeDistance = Mathf.Max(0f, hit.distance - 0.01f);
            Vector3 safePos = lockedBoxRigidbody.position + moveDir * safeDistance;
            lockedBoxRigidbody.MovePosition(safePos);
        }
        else
        {
            // No obstacle, perform normal MovePosition
            Vector3 targetPos = lockedBoxRigidbody.position + delta;
            lockedBoxRigidbody.MovePosition(targetPos);
        }
    }

    void CheckForBox()
    {
        // While already locked on, skip raycasting so the reference isn't wiped
        if (isInteracting) return;

        if (playerTransform == null) return;

        Ray ray = new Ray(playerTransform.position + Vector3.up * 0.5f, playerTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, boxLayer))
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(true);

            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
                boxRigidbody = rb;
        }
        else
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(false);

            boxRigidbody = null;
        }
    }

    void LockPlayerRotationTowardBox()
    {
        if (lockedBoxRigidbody == null || playerTransform == null) return;

        Vector3 lookDir =
            (lockedBoxRigidbody.position - playerTransform.position).normalized;
        lookDir.y = 0;

        playerTransform.rotation =
            Quaternion.Slerp(
                playerTransform.rotation,
                Quaternion.LookRotation(lookDir),
                15f * Time.fixedDeltaTime);
    }
}