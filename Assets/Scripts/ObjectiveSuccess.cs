using UnityEngine;

public class ObjectiveSuccess : MonoBehaviour
{
    public GameObject uiPanel;
    private bool isPaused = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            uiPanel.SetActive(true);
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;

            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            uiPanel.SetActive(false);
        }
    }
}