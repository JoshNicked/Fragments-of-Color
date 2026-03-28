using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameTimer : MonoBehaviour
{
    private const string PurchasedTimeKey = "PurchasedTime";

    [Header("Timer Settings")]
    public float baseTime = 40f;
    public float timeRemaining = 60f;
    public bool timerIsRunning = true;
    public bool isGameOver = false;

    [Header("UI Elements")]
    public TMP_Text timerText;
    public GameObject gameOverPanel;
    public CanvasGroup gameOverCanvasGroup;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    [Header("Gameplay Objects")]
    public MonoBehaviour[] objectsToStop;

    private float saveInterval = 1f;
    private float saveTimer = 0f;

    void Start()
    {
        Time.timeScale = 1f;

        float purchasedTime = PlayerPrefs.GetFloat(PurchasedTimeKey, 0f);
        timeRemaining = baseTime + purchasedTime;

        timerIsRunning = true;
        isGameOver = false;

        foreach (var obj in objectsToStop)
            if (obj != null) obj.enabled = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.interactable = false;
            gameOverCanvasGroup.blocksRaycasts = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        DisplayTime(timeRemaining);
        SaveTime();
    }

    void Update()
    {
        if (!timerIsRunning)
            return;

        timeRemaining -= Time.unscaledDeltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerIsRunning = false;
            isGameOver = true;
            DisplayTime(timeRemaining);

            MenuManage menu = FindObjectOfType<MenuManage>();
            if (menu != null)
                menu.ForceCloseMenu();

            Time.timeScale = 1f;
            StartCoroutine(FadeInGameOver());
            return;
        }

        DisplayTime(timeRemaining);

        saveTimer += Time.unscaledDeltaTime;
        if (saveTimer >= saveInterval)
        {
            SaveTime();
            saveTimer = 0f;
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        float clampedTime = Mathf.Max(0f, timeToDisplay);
        float minutes = Mathf.FloorToInt(clampedTime / 60);
        float seconds = Mathf.FloorToInt(clampedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void AddTime(float seconds)
    {
        if (seconds <= 0f)
            return;

        timeRemaining += seconds;
        timerIsRunning = true;
        isGameOver = false;

        float purchasedTime = PlayerPrefs.GetFloat(PurchasedTimeKey, 0f) + seconds;
        PlayerPrefs.SetFloat(PurchasedTimeKey, purchasedTime);
        PlayerPrefs.Save();

        DisplayTime(timeRemaining);
    }

    public void ResetTimer()
    {
        float purchasedTime = PlayerPrefs.GetFloat(PurchasedTimeKey, 0f);
        timeRemaining = baseTime + purchasedTime;
        timerIsRunning = true;
        isGameOver = false;
        DisplayTime(timeRemaining);
        SaveTime();
    }

    /// <summary>
    /// Stops the countdown without triggering game over (e.g. level-complete / shrine cinematic).
    /// </summary>
    public void PauseTimer()
    {
        timerIsRunning = false;
        SaveTime();
    }

    public void SaveTime()
    {
        PlayerPrefs.SetFloat("SavedTimeRemaining", timeRemaining);
        PlayerPrefs.Save();
    }

    public float GetPurchasedTime()
    {
        return PlayerPrefs.GetFloat(PurchasedTimeKey, 0f);
    }

    IEnumerator FadeInGameOver()
    {
        foreach (var obj in objectsToStop)
            if (obj != null) obj.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            gameOverCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        gameOverCanvasGroup.interactable = true;
        gameOverCanvasGroup.blocksRaycasts = true;
    }

    // ✅ Retry current level
    public void RetryGame()
    {
        PlayerPrefs.SetFloat("SavedTimeRemaining", 0f);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ✅ ONE EXIT FUNCTION for menu & game over
    public void ExitToMenu(string sceneName)
    {
        Time.timeScale = 1f;

        // Reset timer & game over
        timerIsRunning = false;
        isGameOver = false;

        // Re-enable all gameplay objects in case menu persists
        foreach (var obj in objectsToStop)
            if (obj != null) obj.enabled = true;

        // Load menu scene
        SceneManager.LoadScene(sceneName);
    }
}


/* GAME TIMER PAUSED
﻿using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeRemaining = 60f;
    public bool timerIsRunning = true;
    public bool isGameOver = false;

    [Header("UI Elements")]
    public TMP_Text timerText;
    public GameObject gameOverPanel;
    public CanvasGroup gameOverCanvasGroup;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    [Header("Gameplay Objects")]
    public MonoBehaviour[] objectsToStop;

    void Start()
    {
        // ✅ ALWAYS RESET TIME
        Time.timeScale = 1f;

        timerIsRunning = true;
        isGameOver = false;

        gameOverPanel.SetActive(true);
        gameOverCanvasGroup.alpha = 0f;
        gameOverCanvasGroup.interactable = false;
        gameOverCanvasGroup.blocksRaycasts = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ✅ Stop timer when paused
        if (Time.timeScale == 0f) return;

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                isGameOver = true;

                StartCoroutine(FadeInGameOver());
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    IEnumerator FadeInGameOver()
    {
        // Stop gameplay scripts
        foreach (var obj in objectsToStop)
            obj.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            gameOverCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        gameOverCanvasGroup.interactable = true;
        gameOverCanvasGroup.blocksRaycasts = true;
    }

    public void RetryGame()
    {
        Time.timeScale = 1f; // 🔥 FIX FREEZE
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame(string sceneName)
    {
        Time.timeScale = 1f; // 🔥 FIX FREEZE
        SceneManager.LoadScene(sceneName);
    }
}
 */