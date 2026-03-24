using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManage : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject shopPanel;
    public GameObject optionsPanel;

    public GameTimer gameTimer;

    private bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f;

        pauseMenu.SetActive(false);
        shopPanel.SetActive(false);
        optionsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ❌ Disable menu if game is over
        if (gameTimer != null && gameTimer.isGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (shopPanel.activeSelf || optionsPanel.activeSelf)
            {
                BackToMenu();
            }
            else
            {
                TogglePause();
            }
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        pauseMenu.SetActive(isPaused);
        shopPanel.SetActive(false);
        optionsPanel.SetActive(false);

        Time.timeScale = isPaused ? 0f : 1f;

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    public void ResumeGame()
    {
        isPaused = false;

        pauseMenu.SetActive(false);
        shopPanel.SetActive(false);
        optionsPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenShop()
    {
        pauseMenu.SetActive(false);
        shopPanel.SetActive(true);
    }

    public void OpenOptions()
    {
        pauseMenu.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseAllSubMenus()
    {
        shopPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void BackToMenu()
    {
        CloseAllSubMenus();
        pauseMenu.SetActive(true);
    }

    // ✅ IMPORTANT: CALLED BY GAME TIMER
    public void ForceCloseMenu()
    {
        isPaused = false;

        pauseMenu.SetActive(false);
        shopPanel.SetActive(false);
        optionsPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    public void QuitToMenu(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}