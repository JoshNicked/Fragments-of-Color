using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject landingPanel;
    public GameObject levelsPanel;
    public GameObject settingsPanel;
    public GameObject exitPanel;
    public GameObject shopPanel;
    public GameObject exitGame;

    public void ShowLevels()
    {
        levelsPanel.SetActive(true);
    }

    public void ShowLanding()
    {
        settingsPanel.SetActive(false);
        exitPanel.SetActive(false);
        levelsPanel.SetActive(false);
        shopPanel.SetActive(false);
    }

    public void ShowSettings()
    { 
        settingsPanel.SetActive(true);
    }

    public void ShowShop()
    {
        shopPanel.SetActive(true);
    }

    public void ShowExit()
    { 
        exitPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Game is exiting...");
        Application.Quit();
    }

}
