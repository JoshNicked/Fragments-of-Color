using UnityEngine;

/// <summary>
/// Debug helper for Fragment Persistence System
/// Add this to a UI button or use in Inspector to test/manage persistence
/// </summary>
public class FragmentPersistenceDebug : MonoBehaviour
{
    private HotbarSystem hotbar;
    private FragmentRegistry registry;

    void Start()
    {
        hotbar = FindObjectOfType<HotbarSystem>();
        registry = FragmentRegistry.GetInstance();
    }

    /// <summary>
    /// Call from a button to log current hotbar state
    /// </summary>
    public void LogHotbarState()
    {
        if (hotbar == null)
        {
            Debug.Log("[FragmentDebug] HotbarSystem not found!");
            return;
        }

        Debug.Log("=== HOTBAR STATE ===");
        for (int i = 0; i < 5; i++)
        {
            var item = hotbar.GetItemAtSlot(i);
            if (item != null && item.prefab != null)
            {
                Debug.Log($"Slot {i+1}: {item.prefabName} (Icon: {(item.icon != null ? item.icon.name : "None")})");
            }
            else
            {
                Debug.Log($"Slot {i+1}: EMPTY");
            }
        }
    }

    /// <summary>
    /// Call from a button to log PlayerPrefs hotbar data
    /// </summary>
    public void LogPlayerPrefs()
    {
        Debug.Log("=== PLAYERPREFS DATA ===");
        bool hasAny = false;
        for (int i = 0; i < 5; i++)
        {
            string key = "Hotbar_Slot_" + i;
            if (PlayerPrefs.HasKey(key))
            {
                Debug.Log($"{key}: {PlayerPrefs.GetString(key)}");
                hasAny = true;
            }
        }
        if (!hasAny)
            Debug.Log("No fragments saved in PlayerPrefs");
    }

    /// <summary>
    /// Call from a button to manually save
    /// </summary>
    public void ManualSave()
    {
        if (hotbar == null)
        {
            Debug.Log("[FragmentDebug] HotbarSystem not found!");
            return;
        }
        hotbar.SaveHotbar();
        Debug.Log("[FragmentDebug] Manually saved hotbar");
        LogPlayerPrefs();
    }

    /// <summary>
    /// Call from a button to manually load
    /// </summary>
    public void ManualLoad()
    {
        if (hotbar == null)
        {
            Debug.Log("[FragmentDebug] HotbarSystem not found!");
            return;
        }
        hotbar.LoadHotbar();
        Debug.Log("[FragmentDebug] Manually loaded hotbar");
        LogHotbarState();
    }

    /// <summary>
    /// Call from a button to clear all saved fragments
    /// </summary>
    public void ClearAllFragments()
    {
        if (hotbar == null)
        {
            Debug.Log("[FragmentDebug] HotbarSystem not found!");
            return;
        }

        hotbar.ClearSavedFragments();
        Debug.Log("[FragmentDebug] Cleared all fragments");
        LogPlayerPrefs();
    }

    /// <summary>
    /// Call from a button to verify registry is set up
    /// </summary>
    public void VerifyRegistry()
    {
        if (registry == null)
        {
            Debug.LogError("[FragmentDebug] FragmentRegistry not found in scene!");
            return;
        }

        Debug.Log("=== REGISTRY STATUS ===");
        Debug.Log($"FragmentRegistry found: {registry.name}");
        Debug.Log($"Fragment entries: {registry.fragments.Length}");
        
        for (int i = 0; i < registry.fragments.Length; i++)
        {
            var entry = registry.fragments[i];
            if (entry != null)
            {
                bool hasIcon = entry.icon != null;
                bool hasPrefab = entry.prefab != null;
                Debug.Log($"  [{i}] {entry.fragmentName} - Icon: {(hasIcon ? "✓" : "✗")} Prefab: {(hasPrefab ? "✓" : "✗")}");
            }
        }
    }

    /// <summary>
    /// Run all diagnostics
    /// </summary>
    public void RunDiagnostics()
    {
        Debug.Log("╔════════════════════════════════════════════╗");
        Debug.Log("║     FRAGMENT PERSISTENCE DIAGNOSTICS      ║");
        Debug.Log("╚════════════════════════════════════════════╝");
        
        Debug.Log("\n1. Checking HotbarSystem...");
        hotbar = FindObjectOfType<HotbarSystem>();
        if (hotbar == null)
        {
            Debug.LogError("✗ HotbarSystem NOT FOUND");
        }
        else
        {
            Debug.Log("✓ HotbarSystem found");
        }

        Debug.Log("\n2. Checking FragmentRegistry...");
        registry = FragmentRegistry.GetInstance();
        if (registry == null)
        {
            Debug.LogError("✗ FragmentRegistry NOT FOUND in scene");
        }
        else
        {
            Debug.Log("✓ FragmentRegistry found");
            VerifyRegistry();
        }

        Debug.Log("\n3. Checking saved data...");
        LogPlayerPrefs();

        Debug.Log("\n4. Current hotbar state...");
        LogHotbarState();

        Debug.Log("\n╔════════════════════════════════════════════╗");
        Debug.Log("║           DIAGNOSTICS COMPLETE             ║");
        Debug.Log("╚════════════════════════════════════════════╝");
    }

    public HotbarItem GetItemAtSlot(int index)
    {
        if (hotbar == null) return null;
        return hotbar.GetItemAtSlot(index);
    }
}