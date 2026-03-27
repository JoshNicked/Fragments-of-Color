using UnityEngine;

public class LevelUnlock : MonoBehaviour
{
    public int levelIndex = 2;

    void Start()
    {
        if (PlayerPrefs.GetInt("Level" + levelIndex + "Unlocked", 0) == 1)
        {
            gameObject.SetActive(false);
        }
    }
}
