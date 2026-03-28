using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelCompleteAll : MonoBehaviour
{
    public int currentLevel = 3;

    [Header("Slideshow Settings")]
    public GameObject slideshowPanel;   // panel (no fade)
    public RawImage slideshowImage;     // fades
    public Texture[] slides;            // set size = 2

    public float fadeDuration = 1f;
    public float displayDuration = 2f;

    public bool isPaused = false;
    // ===== BUTTON FUNCTION =====
    public void OnNextLevel()
    {
        int nextLevel = currentLevel + 1;

        PlayerPrefs.SetInt("Level" + nextLevel + "Unlocked", 1);
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;

        StartCoroutine(PlaySlideshowThenLoad());
    }

    // ===== SLIDESHOW =====
    IEnumerator PlaySlideshowThenLoad()
    {
        slideshowPanel.SetActive(true);
        Canvas.ForceUpdateCanvases(); // remove delay

        for (int i = 0; i < slides.Length; i++)
        {
            slideshowImage.texture = slides[i];

            // Fade in
            yield return StartCoroutine(FadeImage(0, 1));

            // Stay visible
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            yield return StartCoroutine(FadeImage(1, 0));
        }

        // After slideshow → go to Landing Scene (0)
        SceneManager.LoadScene(0);
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