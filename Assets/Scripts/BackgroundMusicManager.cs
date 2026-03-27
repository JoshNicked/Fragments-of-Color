using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip backgroundMusic;
    public float volume = 1f;

    private AudioSource audioSource;

    void Start()
    {
        // Get or create AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Configure AudioSource
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.volume = volume;

        // Play background music if assigned
        if (backgroundMusic != null)
        {
            audioSource.Play();
            Debug.Log("Background music started");
        }
        else
        {
            Debug.LogWarning("BackgroundMusicManager: No audio clip assigned!");
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
            Debug.Log("Background Music Volume Set To: " + volume);
        }
    }

    public void Stop()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    public void Pause()
    {
        if (audioSource != null)
            audioSource.Pause();
    }

    public void Resume()
    {
        if (audioSource != null)
            audioSource.Play();
    }
}