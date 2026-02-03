using UnityEngine;
using UnityEngine.SceneManagement; 

public class ToTestingLevel : MonoBehaviour
{

    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(2);
    }
}
