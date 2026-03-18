using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public GameObject prompt;

    void Awake()
    {
        if (prompt != null)
            prompt.SetActive(false);
    }

    public void Show()
    {
        if (prompt != null && !prompt.activeSelf)
            prompt.SetActive(true);
    }

    public void Hide()
    {
        if (prompt != null && prompt.activeSelf)
            prompt.SetActive(false);
    }
}
