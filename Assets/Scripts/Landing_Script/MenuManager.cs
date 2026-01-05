using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject landingPanel;
    public GameObject levelsPanel;
    public GameObject settingsPanel;
    public GameObject exitPanel;

    public void ShowLevels()
    {
        landingPanel.SetActive(false);
        levelsPanel.SetActive(true);
    }

    public void ShowLanding()
    {
        settingsPanel.SetActive(false);
        exitPanel.SetActive(false);
        levelsPanel.SetActive(false);
        landingPanel.SetActive(true);
    }

    public void ShowSettings()
    { 
        settingsPanel.SetActive(true);
    }
    
    public void ShowExit()
    { 
        exitPanel.SetActive(true);
    }
    
}
