using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToLevels : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
