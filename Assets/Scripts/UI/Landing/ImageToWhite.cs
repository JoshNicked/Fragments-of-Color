using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ImageToWhite : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RawImage targetImage;

    public Color hoverColor = Color.white;
    private Color normalColor;

    [Header("Delay")]
    public float exitDelay = 1f;

    void Start()
    {
        // Auto-assign if not set
        if (targetImage == null)
        {
            targetImage = GetComponent<RawImage>();
        }

        if (targetImage != null)
        {
            normalColor = targetImage.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();

        if (targetImage != null)
        {
            targetImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(RevertColorAfterDelay());
    }

    IEnumerator RevertColorAfterDelay()
    {
        yield return new WaitForSeconds(exitDelay);

        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }
    }
}