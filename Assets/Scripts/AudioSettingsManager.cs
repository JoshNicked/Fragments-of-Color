using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("Slider References")]
    public Slider slider_music;    // Controls Box Sound Volume
    public Slider slider_sfx;      // Controls Background Music Volume

    [Header("Audio Manager References")]
    public BackgroundMusicManager backgroundMusicManager;
    public BoxInteraction boxInteraction;

    [Header("Volume Settings")]
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    void Start()
    {
        // Load saved volumes
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);

        // Setup sliders
        if (slider_music != null)
        {
            slider_music.value = musicVolume;
            slider_music.onValueChanged.AddListener(SetBoxSoundVolume);
        }

        if (slider_sfx != null)
        {
            slider_sfx.value = sfxVolume;
            slider_sfx.onValueChanged.AddListener(SetBackgroundMusicVolume);
        }

        // Apply saved volumes
        SetBoxSoundVolume(musicVolume);
        SetBackgroundMusicVolume(sfxVolume);
    }

    public void SetBoxSoundVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();

        if (boxInteraction != null)
            boxInteraction.SetBoxSoundVolume(musicVolume);

        Debug.Log("Box Sound Volume: " + musicVolume);
    }

    public void SetBackgroundMusicVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFXVolumeKey, sfxVolume);
        PlayerPrefs.Save();

        if (backgroundMusicManager != null)
            backgroundMusicManager.SetVolume(sfxVolume);

        Debug.Log("Background Music Volume: " + sfxVolume);
    }
}
