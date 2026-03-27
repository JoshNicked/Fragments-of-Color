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

    [Header("Audio Settings")]
    public AudioClip boxSound;

    [HideInInspector]
    public bool isRotationLocked = false;

    private Rigidbody boxRigidbody;
    private BoxCollider boxCollider;
    private AudioSource audioSource;

    void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        boxRigidbody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (boxRigidbody != null)
            boxRigidbody.isKinematic = true;

        if (boxCollider != null)
            boxCollider.isTrigger = true;
    }

    void Update()
    {
        CheckForBox();
    }

    void FixedUpdate()
    {
        if (boxRigidbody != null &&
            Input.GetKey(interactKey) &&
            !playerMotor.isInteractingWithChest)
        {
            isRotationLocked = true;
            boxRigidbody.isKinematic = false;

            MoveBoxWithPlayer();
            LockPlayerRotationTowardBox();

            // Play box sound if it's not already playing
            if (boxSound != null && audioSource != null && !audioSource.isPlaying)
            {
                audioSource.clip = boxSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            isRotationLocked = false;

            if (boxRigidbody != null)
                boxRigidbody.isKinematic = true;

            // Stop box sound when not moving
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

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
            boxRigidbody.position +
            moveDir * pushPullSpeed * Time.fixedDeltaTime;

        boxRigidbody.MovePosition(targetPos);
    }

    void CheckForBox()
    {
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

            boxRigidbody = null; // 🔥 THIS IS THE FIX
        }
    }

    void LockPlayerRotationTowardBox()
    {
        if (boxRigidbody == null) return;

        Vector3 lookDir =
            (boxRigidbody.position - playerTransform.position).normalized;

        lookDir.y = 0;

        playerTransform.rotation =
            Quaternion.Slerp(
                playerTransform.rotation,
                Quaternion.LookRotation(lookDir),
                15f * Time.fixedDeltaTime);
    }

    public void SetBoxSoundVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }
}