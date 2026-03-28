using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HotbarItem
{
    public Sprite icon;
    public GameObject prefab;
    public string prefabName;  // Add this for saving/loading
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

    [Header("Persistence")]
    [Tooltip("Auto-load saved fragments on level start")]
    public bool autoLoadFragments = true;

    private int currentIndex = -1;
    private GameObject equippedObject;
    private int IsEquippedHash;
    private const string HOTBAR_PREFIX = "Hotbar_Slot_";

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

        // Load saved fragments
        if (autoLoadFragments)
        {
            LoadHotbar();
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
                items[i] = new HotbarItem { icon = icon, prefab = prefab, prefabName = prefab.name };

                // Update UI
                if (i < slotImages.Length && slotImages[i] != null)
                {
                    slotImages[i].sprite = icon;
                    slotImages[i].enabled = true;
                    slotImages[i].color = Color.white;
                }

                Debug.Log($"[Hotbar] Added '{prefab.name}' to slot {i}");
                
                // Save to PlayerPrefs
                SaveHotbar();
                
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

    /// <summary>
    /// Saves current hotbar state to PlayerPrefs for persistence across scenes
    /// </summary>
    public void SaveHotbar()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].prefab != null)
            {
                PlayerPrefs.SetString(HOTBAR_PREFIX + i, items[i].prefab.name);
                Debug.Log($"[Hotbar] Saved slot {i}: {items[i].prefab.name}");
            }
            else
            {
                PlayerPrefs.DeleteKey(HOTBAR_PREFIX + i);
            }
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads saved hotbar state from PlayerPrefs and reconstructs inventory
    /// </summary>
    public void LoadHotbar()
    {
        FragmentRegistry registry = FragmentRegistry.GetInstance();
        
        if (registry == null)
        {
            Debug.LogWarning("[Hotbar] FragmentRegistry not found in scene!");
            return;
        }

        // Clear current hotbar
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = null;
        }

        // Load saved fragments
        for (int i = 0; i < items.Length; i++)
        {
            string key = HOTBAR_PREFIX + i;
            if (PlayerPrefs.HasKey(key))
            {
                string fragmentName = PlayerPrefs.GetString(key);
                var fragmentEntry = registry.GetFragment(fragmentName);

                if (fragmentEntry != null)
                {
                    items[i] = new HotbarItem
                    {
                        icon = fragmentEntry.icon,
                        prefab = fragmentEntry.prefab,
                        prefabName = fragmentName
                    };

                    // Update UI
                    if (i < slotImages.Length && slotImages[i] != null && fragmentEntry.icon != null)
                    {
                        slotImages[i].sprite = fragmentEntry.icon;
                        slotImages[i].enabled = true;
                        slotImages[i].color = Color.white;
                    }

                    Debug.Log($"[Hotbar] Loaded slot {i}: {fragmentName}");
                }
                else
                {
                    Debug.LogWarning($"[Hotbar] Could not find fragment '{fragmentName}' in registry");
                }
            }
        }
    }

    /// <summary>
    /// Clears all saved fragments from PlayerPrefs (useful for testing or new game)
    /// </summary>
    public void ClearSavedFragments()
    {
        for (int i = 0; i < items.Length; i++)
        {
            PlayerPrefs.DeleteKey(HOTBAR_PREFIX + i);
        }
        PlayerPrefs.Save();
        Debug.Log("[Hotbar] Cleared all saved fragments");
    }

    /// <summary>
    /// Removes an item from a specific slot and saves
    /// </summary>
    public void RemoveItem(int index)
    {
        if (index < 0 || index >= items.Length)
            return;

        if (items[index] != null)
        {
            Debug.Log($"[Hotbar] Removed item from slot {index}");
            items[index] = null;

            // Update UI
            if (index < slotImages.Length && slotImages[index] != null)
            {
                slotImages[index].sprite = null;
                slotImages[index].enabled = false;
            }

            SaveHotbar();
        }
    }

    /// <summary>
    /// Checks if a fragment is already in the hotbar
    /// </summary>
    public bool HasFragment(string fragmentName)
    {
        foreach (var item in items)
        {
            if (item != null && item.prefabName.ToLower() == fragmentName.ToLower())
                return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the first empty slot index, or -1 if hotbar is full
    /// </summary>
    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null || items[i].prefab == null)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Gets item at specific slot (for debugging)
    /// </summary>
    public HotbarItem GetItemAtSlot(int index)
    {
        if (index >= 0 && index < items.Length)
            return items[index];
        return null;
    }

    /// <summary>Number of hotbar slots that currently hold an item.</summary>
    public int GetFilledSlotCount()
    {
        int n = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].prefab != null)
                n++;
        }
        return n;
    }

    /// <summary>Currently selected slot when equipping, or -1 if none.</summary>
    public int GetEquippedSlotIndex() => currentIndex;

    /// <summary>
    /// Removes the equipped item from the hotbar, destroys the hand instance, and saves.
    /// Use when placing a fragment in the world.
    /// </summary>
    public bool ConsumeEquippedItem(out HotbarItem consumed)
    {
        consumed = null;
        if (currentIndex < 0 || currentIndex >= items.Length)
            return false;
        if (items[currentIndex] == null || items[currentIndex].prefab == null)
            return false;

        consumed = items[currentIndex];
        items[currentIndex] = null;

        if (equippedObject != null)
        {
            Destroy(equippedObject);
            equippedObject = null;
        }

        if (playerAnimator != null)
            playerAnimator.SetBool(IsEquippedHash, false);

        int cleared = currentIndex;
        currentIndex = -1;
        HighlightSlot(-1);

        if (cleared < slotImages.Length && slotImages[cleared] != null)
        {
            slotImages[cleared].sprite = null;
            slotImages[cleared].enabled = false;
        }

        SaveHotbar();
        return true;
    }
}