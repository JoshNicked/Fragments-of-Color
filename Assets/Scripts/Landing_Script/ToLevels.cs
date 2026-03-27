using UnityEngine;
using UnityEngine.SceneManagement; 

public class ToLevels : MonoBehaviour
{

    public void LoadSceneLanding(int sceneIndex)
    {
        Time.timeScale = 1f; //Researched for an hour only to find out I just need 1 line of code to load animations, PAIN
        SceneManager.LoadScene(0);
    }
    
    public void LoadSceneTutorial(int sceneIndex)
    {
        SceneManager.LoadScene(1);
    }

    public void LoadSceneLevel1(int sceneIndex)
    {
        SceneManager.LoadScene(2);
    }


}
