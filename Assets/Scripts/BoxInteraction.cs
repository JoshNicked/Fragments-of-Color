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
    }

    void Update()
    {
        CheckForBox();
        HandleToggleInput();
    }

    void FixedUpdate()
    {
        // Use the locked reference while interacting, not the live raycast one
        bool canInteract = lockedBoxRigidbody != null
                           && isInteracting
                           && !playerMotor.isInteractingWithChest;

        if (canInteract)
        {
            isRotationLocked = true;
            lockedBoxRigidbody.isKinematic = false;
            MoveBoxWithPlayer();
            LockPlayerRotationTowardBox();
        }
        else
        {
            isRotationLocked = false;

            if (lockedBoxRigidbody != null)
                lockedBoxRigidbody.isKinematic = true;
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
            if (boxRigidbody != null && !playerMotor.isInteractingWithChest)
            {
                isInteracting = true;
                lockedBoxRigidbody = boxRigidbody; // lock onto this box
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

        Vector3 targetPos =
            lockedBoxRigidbody.position +
            moveDir * pushPullSpeed * Time.fixedDeltaTime;

        lockedBoxRigidbody.MovePosition(targetPos);
    }

    void CheckForBox()
    {
        // While already locked on, skip raycasting so the reference isn't wiped
        if (isInteracting) return;

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
        if (lockedBoxRigidbody == null) return;

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