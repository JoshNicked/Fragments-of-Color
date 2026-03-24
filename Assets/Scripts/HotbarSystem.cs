using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class HotbarItem
{
    public Sprite icon;
    public GameObject prefab;
}

public class HotbarSystem : MonoBehaviour
{
    [Header("UI")]
    public Image[] slotImages;

    [Header("Item Settings")]
    public HotbarItem[] items = new HotbarItem[5];

    [Header("Equip Settings")]
    public Transform handPoint;
    public Animator playerAnimator;

    private int currentIndex = -1;
    private GameObject equippedObject;
    private int IsEquippedHash;

    void Start()
    {
        IsEquippedHash = Animator.StringToHash("IsEquipped");

        // Clear UI
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
            {
                slotImages[i].sprite = null;
                slotImages[i].enabled = false;
                slotImages[i].color = Color.white;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
    }

    public bool AddItem(Sprite icon, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[Hotbar] Prefab is NULL!");
            return false;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null || items[i].prefab == null)
            {
                items[i] = new HotbarItem { icon = icon, prefab = prefab };

                // Update UI
                if (i < slotImages.Length && slotImages[i] != null)
                {
                    slotImages[i].sprite = icon;
                    slotImages[i].enabled = true;
                    slotImages[i].color = Color.white;
                }

                Debug.Log($"[Hotbar] Added '{prefab.name}' to slot {i}");
                return true;
            }
        }

        Debug.Log("[Hotbar] Hotbar full!");
        return false;
    }

    void SelectSlot(int index)
    {
        if (index < 0 || index >= items.Length)
            return;

        if (items[index] == null || items[index].prefab == null)
        {
            Debug.Log("[Hotbar] Slot empty.");
            return;
        }

        if (currentIndex == index)
        {
            StartCoroutine(UnequipRoutine());
            currentIndex = -1;
            HighlightSlot(-1);
        }
        else
        {
            currentIndex = index;
            HighlightSlot(index);
            StartCoroutine(EquipRoutine(items[index].prefab));
        }
    }

    IEnumerator EquipRoutine(GameObject prefab)
    {
        if (prefab == null) yield break;

        // Remove previous
        if (equippedObject != null)
            Destroy(equippedObject);

        if (playerAnimator != null)
            playerAnimator.SetBool(IsEquippedHash, true);

        yield return new WaitForSeconds(0.1f);

        // Instantiate
        equippedObject = Instantiate(prefab, handPoint.position, handPoint.rotation, handPoint);

        // Reset local transform
        equippedObject.transform.localPosition = Vector3.zero;
        equippedObject.transform.localRotation = Quaternion.identity;
        equippedObject.transform.localScale = Vector3.one * 0.3f;

        // Enable MeshRenderers
        foreach (Renderer r in equippedObject.GetComponentsInChildren<Renderer>())
            r.enabled = true;

        // Enable SpriteRenderers
        foreach (SpriteRenderer sr in equippedObject.GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = true;

        Debug.Log($"[Hotbar] Equipped '{prefab.name}'");
    }

    IEnumerator UnequipRoutine()
    {
        if (playerAnimator != null)
            playerAnimator.SetBool(IsEquippedHash, false);

        yield return new WaitForSeconds(0.1f);

        if (equippedObject != null)
        {
            Destroy(equippedObject);
            equippedObject = null;
        }
    }

    void HighlightSlot(int index)
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
                slotImages[i].color = (i == index) ? Color.yellow : Color.white;
        }
    }
}