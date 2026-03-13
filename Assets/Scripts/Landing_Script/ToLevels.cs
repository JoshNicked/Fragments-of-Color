using UnityEngine;
using UnityEngine.SceneManagement; 

public class ToLevels : MonoBehaviour
{

    public void LoadSceneLanding(int sceneIndex)
    {
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
