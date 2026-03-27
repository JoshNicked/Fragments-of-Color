using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    private const string FragmentsKey = "Fragments";
    private int fragments = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        fragments = PlayerPrefs.GetInt(FragmentsKey, 0);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (CurrencyUI.Instance != null)
        {
            CurrencyUI.Instance.UpdateFragments(fragments);
        }
    }

    public int GetFragments()
    {
        return fragments;
    }

    public void AddFragments(int amount)
    {
        fragments += amount;
        SaveFragments();
        UpdateUI();

        Debug.Log("Color Fragments: " + fragments);
    }

    public bool SpendFragments(int amount)
    {
        if (fragments >= amount)
        {
            fragments -= amount;
            SaveFragments();
            UpdateUI();
            Debug.Log("Spent " + amount + " fragments. Remaining: " + fragments);
            return true;
        }

        Debug.LogWarning("Not enough fragments to spend: " + amount + " needed, " + fragments + " available.");
        return false;
    }

    private void SaveFragments()
    {
        PlayerPrefs.SetInt(FragmentsKey, fragments);
        PlayerPrefs.Save();
    }
}