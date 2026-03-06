using UnityEngine;
using System.Collections;

public class ChestInteraction : MonoBehaviour
{
    private Animator animator;
    private bool isOpen = false;
    private bool isAnimating = false;

    private HotbarSystem hotbar;
    private PlayerMotor playerMotor;

    public Transform player;
    public float pickupDistance = 2f;

    [Header("Fragment Settings")]
    public GameObject fragment;
    public Transform snapPoint;
    public float floatHeight = 0.5f;
    public float floatSpeed = 1f;

    void Start()
    {
        hotbar = FindObjectOfType<HotbarSystem>();
        animator = GetComponent<Animator>();
        playerMotor = player.GetComponent<PlayerMotor>();

        if (fragment)
            fragment.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isAnimating)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            if (distance <= pickupDistance)
            {
                playerMotor.isInteractingWithChest = true;

                if (isOpen)
                    StartCoroutine(CloseChest());
                else
                    StartCoroutine(OpenChest());
            }
        }

        // Floating fragment
        if (fragment && fragment.activeSelf)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * 0.2f;
            fragment.transform.localPosition =
                snapPoint.localPosition + Vector3.up * (floatHeight + yOffset);
        }

        // Pick up fragment with E
        if (fragment && fragment.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            if (Vector3.Distance(player.position, fragment.transform.position) <= pickupDistance)
            {
                FragmentData data = fragment.GetComponent<FragmentData>();

                if (data != null)
                {
                    bool added = hotbar.AddItem(data.icon, data.prefab);

                    if (added)
                        fragment.SetActive(false);
                }
            }
        }
    }

    private IEnumerator OpenChest()
    {
        isAnimating = true;
        animator.SetBool("isOpen", true);

        yield return new WaitForSeconds(GetAnimationLength("Open"));

        if (fragment)
            fragment.SetActive(true);

        isOpen = true;
        isAnimating = false;
        playerMotor.isInteractingWithChest = false;
    }

    private IEnumerator CloseChest()
    {
        isAnimating = true;
        animator.SetBool("isOpen", false);

        yield return new WaitForSeconds(GetAnimationLength("Close"));

        if (fragment)
            fragment.SetActive(false);

        isOpen = false;
        isAnimating = false;
        playerMotor.isInteractingWithChest = false;
    }

    private float GetAnimationLength(string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        return 0.5f;
    }
}