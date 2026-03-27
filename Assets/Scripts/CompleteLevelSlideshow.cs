using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CompleteLevelSlideshow : MonoBehaviour
{
    [Header("Slideshow Settings")]
    public GameObject slideshowPanel;   // panel (no fade)
    public RawImage slideshowImage;     // fades
    public Texture slide;               // single image

    public float fadeDuration = 1f;
    public float displayDuration = 2f;

    public int landingSceneIndex = 0;   // landing scene

    // ===== BUTTON FUNCTION =====
    public void OnCompleteLevel()
    {
        StartCoroutine(ShowImageThenLoad());
    }

    // ===== SHOW IMAGE =====
    IEnumerator ShowImageThenLoad()
    {
        slideshowPanel.SetActive(true);

        slideshowImage.texture = slide;

        // Fade in
        yield return StartCoroutine(FadeImage(0, 1));

        // Stay visible
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        yield return StartCoroutine(FadeImage(1, 0));

        // Go to Landing Scene
        SceneManager.LoadScene(landingSceneIndex);
    }

    IEnumerator FadeImage(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = slideshowImage.color;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            slideshowImage.color = new Color(color.r, color.g, color.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        slideshowImage.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}