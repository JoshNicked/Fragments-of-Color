using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    public static CurrencyUI Instance;

    public TextMeshProUGUI fragmentText;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateFragments(int amount)
    {
        fragmentText.text = "Color Fragments: " + amount;
    }
}