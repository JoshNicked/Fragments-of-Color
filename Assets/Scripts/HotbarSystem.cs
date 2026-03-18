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
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null || items[i].prefab == null)
            {
                items[i] = new HotbarItem();
                items[i].icon = icon;
                items[i].prefab = prefab;

                if (i < slotImages.Length && slotImages[i] != null)
                {
                    slotImages[i].sprite = icon;
                    slotImages[i].enabled = true;
                    slotImages[i].color = Color.white;
                }

                return true;
            }
        }

        Debug.Log("Hotbar Full!");
        return false;
    }

    void SelectSlot(int index)
    {
        if (items[index] == null || items[index].prefab == null)
        {
            Debug.Log("Slot Empty");
            return;
        }

        // Toggle same slot
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
        // Remove old object
        if (equippedObject != null)
        {
            Destroy(equippedObject);
            equippedObject = null;
        }

        // Play equip animation
        if (playerAnimator != null)
            playerAnimator.SetBool(IsEquippedHash, true);

        // Small delay to sync with animation
        yield return new WaitForSeconds(0.2f);

        equippedObject = Instantiate(prefab, handPoint);
        equippedObject.transform.localPosition = Vector3.zero;
        equippedObject.transform.localRotation = Quaternion.identity;
    }

    IEnumerator UnequipRoutine()
    {
        // Play unequip animation
        if (playerAnimator != null)
            playerAnimator.SetBool(IsEquippedHash, false);

        yield return new WaitForSeconds(0.15f);

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