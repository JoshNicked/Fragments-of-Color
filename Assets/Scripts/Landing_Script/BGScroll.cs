using UnityEngine;
using UnityEngine.UI;

public class BGScroll : MonoBehaviour
{
    public RawImage img; // Reference to the RawImage component
    public float x_speed; // X-axis scroll speed
    public float y_speed; // Y-axis scroll speed

    void Update()
    {
        // Update the UV rectangle's position for scrolling
        img.uvRect = new Rect(img.uvRect.position + new Vector2(x_speed, y_speed) * Time.deltaTime, img.uvRect.size);
    }
}
