using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections; // Required for Coroutines

public class yes : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI theText;
    public Color hoverColor = Color.white;
    private Color normalColor;
    // Public variable for the exit delay (set to 0.1s by default)
    public float exitDelay = 1f;

    void Start()
    {
        if (theText == null)
        {
            theText = GetComponentInChildren<TextMeshProUGUI>();
        }
        if (theText != null)
        {
            normalColor = theText.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Stop any running exit coroutines and change color instantly
        StopAllCoroutines();
        if (theText != null)
        {
            theText.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Start the coroutine to change color back to normal after a delay
        StopAllCoroutines(); // Stop any pending instant color changes just in case
        StartCoroutine(RevertColorAfterDelay());
    }

    // Coroutine to handle the exit delay
    IEnumerator RevertColorAfterDelay()
    {
        // Wait for the specified amount of time (0.1s)
        yield return new WaitForSeconds(exitDelay);

        // Change the color after the delay has passed
        if (theText != null)
        {
            theText.color = normalColor;
        }
    }
}
