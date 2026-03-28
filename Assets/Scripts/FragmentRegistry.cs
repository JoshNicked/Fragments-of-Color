using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Registry that maps fragment names to their Sprite and Prefab.
/// Drag fragments into this list to register them for save/load persistence.
/// </summary>
public class FragmentRegistry : MonoBehaviour
{
    [System.Serializable]
    public class FragmentEntry
    {
        public string fragmentName;  // e.g., "blue_fragment"
        public Sprite icon;
        public GameObject prefab;
    }

    public FragmentEntry[] fragments = new FragmentEntry[0];

    private static FragmentRegistry instance;
    private Dictionary<string, FragmentEntry> fragmentMap;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeMap();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void InitializeMap()
    {
        fragmentMap = new Dictionary<string, FragmentEntry>();
        foreach (var fragment in fragments)
        {
            if (!string.IsNullOrEmpty(fragment.fragmentName))
            {
                fragmentMap[fragment.fragmentName.ToLower()] = fragment;
                Debug.Log($"[FragmentRegistry] Registered fragment: {fragment.fragmentName}");
            }
        }
    }

    public static FragmentRegistry GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<FragmentRegistry>();
            if (instance != null)
                instance.InitializeMap();
        }
        return instance;
    }

    public FragmentEntry GetFragment(string fragmentName)
    {
        if (fragmentMap == null)
            InitializeMap();

        string key = fragmentName.ToLower();
        if (fragmentMap.TryGetValue(key, out var entry))
            return entry;

        Debug.LogWarning($"[FragmentRegistry] Fragment not found: {fragmentName}");
        return null;
    }

    public Sprite GetIcon(string fragmentName)
    {
        var entry = GetFragment(fragmentName);
        return entry?.icon;
    }

    public GameObject GetPrefab(string fragmentName)
    {
        var entry = GetFragment(fragmentName);
        return entry?.prefab;
    }
}
