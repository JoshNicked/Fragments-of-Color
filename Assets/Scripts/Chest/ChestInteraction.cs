using UnityEngine;
using System.Collections;

public class ChestInteraction : MonoBehaviour
{
    private Animator animator;
    private bool isOpen = false;
    private bool isAnimating = false;

    private HotbarSystem hotbar;
    private PlayerMotor playerMotor;
    private ObjectiveSuccess objectiveSuccess;

    public Transform player;
    public float pickupDistance = 4f;

    [Header("Fragment Settings")]
    public FragmentData fragmentData;   // Assign ScriptableObject
    public Transform snapPoint;
    public float floatHeight = 0.5f;
    public float floatSpeed = 1f;

    private GameObject fragmentInstance;
    private bool hasGivenItem = false;

    void Start()
    {
        hotbar = FindObjectOfType<HotbarSystem>();
        animator = GetComponent<Animator>();
        playerMotor = player.GetComponent<PlayerMotor>();
        objectiveSuccess = FindObjectOfType<ObjectiveSuccess>();

        // Spawn fragment but keep hidden
        if (fragmentData != null && fragmentData.prefab != null)
        {
            fragmentInstance = Instantiate(fragmentData.prefab, snapPoint.position, Quaternion.identity);
            fragmentInstance.transform.parent = snapPoint;
            fragmentInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isAnimating)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if (distance <= pickupDistance)
            {
                playerMotor.isInteractingWithChest = true;
                StartCoroutine(isOpen ? CloseChest() : OpenChest());
            }
        }

        // Floating effect
        if (fragmentInstance != null && fragmentInstance.activeSelf)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * 0.2f;
            fragmentInstance.transform.localPosition = Vector3.up * (floatHeight + yOffset);
        }

        // Pickup fragment
        if (fragmentInstance != null && fragmentInstance.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            float distance = Vector3.Distance(player.position, fragmentInstance.transform.position);
            if (distance <= pickupDistance && !hasGivenItem)
            {
                bool added = hotbar.AddItem(fragmentData.icon, fragmentData.prefab);
                if (added)
                {
                    fragmentInstance.SetActive(false);
                    hasGivenItem = true;
                    Debug.Log($"[Chest] Picked up '{fragmentData.prefab.name}'");

                    // Trigger success when fragment is picked up
                    if (objectiveSuccess != null)
                    {
                        objectiveSuccess.TriggerSuccess();
                    }
                }
            }
        }
    }

    IEnumerator OpenChest()
    {
        isAnimating = true;
        animator.SetBool("isOpen", true);
        yield return new WaitForSeconds(GetAnimationLength("Open"));

        if (!hasGivenItem && fragmentInstance != null)
            fragmentInstance.SetActive(true);

        isOpen = true;
        isAnimating = false;
        playerMotor.isInteractingWithChest = false;
    }

    IEnumerator CloseChest()
    {
        isAnimating = true;
        animator.SetBool("isOpen", false);
        yield return new WaitForSeconds(GetAnimationLength("Close"));

        if (fragmentInstance != null)
            fragmentInstance.SetActive(false);

        isOpen = false;
        isAnimating = false;
        playerMotor.isInteractingWithChest = false;
    }

    float GetAnimationLength(string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 0.5f;
    }
}