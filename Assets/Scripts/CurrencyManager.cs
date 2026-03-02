using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    int fragments = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        fragments = PlayerPrefs.GetInt("Fragments", 0);
        CurrencyUI.Instance.UpdateFragments(fragments);
    }

    public void AddFragments(int amount){
        fragments += amount;

        CurrencyUI.Instance.UpdateFragments(fragments);

        Debug.Log("Color Fragments: " + fragments);
        PlayerPrefs.SetInt("Fragments", fragments);
    }

    public bool SpendFragments(int amount){
        if(fragments >= amount)
        {
            fragments -= amount;
            CurrencyUI.Instance.UpdateFragments(fragments);
            return true;
        }

        return false;
    }
}