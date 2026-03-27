using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class CurrencyUI : MonoBehaviour
{
    public static CurrencyUI Instance;

    [Header("Fragment Displays")]
    public TextMeshProUGUI fragmentText; // Legacy single text
    public List<TextMeshProUGUI> fragmentTexts = new List<TextMeshProUGUI>(); // Multiple texts

    void Awake()
    {
        Instance = this;
    }

    public void UpdateFragments(int amount)
    {
        string displayText = "Color Fragments: " + amount;

        // Update legacy single text
        if (fragmentText != null)
        {
            fragmentText.text = displayText;
        }

        // Update all texts in the list
        foreach (TextMeshProUGUI text in fragmentTexts)
        {
            if (text != null)
            {
                text.text = displayText;
            }
        }
    }
}

