using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum AudioType
    {
        MusicAudio,
        SfxAudio
    }
    
    // private instance
    private static AudioManager _instance;
    // instance getter
    public static AudioManager Instance => _instance;

    [Header("Music Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip[] musicAudioClips;

    [Header("Sfx Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip[] sfxAudioClips;

    private float currentMusicVolume = 1;
    private float currentSfxVolume = 1;
    
    private void Awake()
    {
        // check if another instance for TutorialModuleManager exists
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        currentMusicVolume = PlayerPrefs.GetFloat(AudioType.MusicAudio.ToString(), 1f);
        currentSfxVolume = PlayerPrefs.GetFloat(AudioType.SfxAudio.ToString(), 1f);
        SetMusicVolume(currentMusicVolume);
        SetSfxVolume(currentSfxVolume);
    }

    #region Audio Play functions

    public void PlayMusic(string clipName)
    {
        foreach (AudioClip clip in musicAudioClips)
        {
            if (clip.name == clipName)
            {
                musicSource.clip = clip;
                musicSource.Play();
                break;
            }
        }
    }

    public void PlaySfx(string clipName)
    {
        foreach (AudioClip clip in sfxAudioClips)
        {
            if (clip.name == clipName)
            {
                sfxSource.PlayOneShot(clip);
                break;
            }
        }
    }

    #endregion
    
    #region Audio Settings functions

    public float GetCurrentVolume(AudioType audioType)
    {
        switch (audioType)
        {
            case AudioType.MusicAudio:
                return currentMusicVolume;
            case AudioType.SfxAudio:
                return currentSfxVolume;
        }

        // return default value for volume (100%)
        return 1;
    }
    
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        
        // store current volume
        SetPlayerPrefsVolume(AudioType.MusicAudio.ToString(), volume);
        currentMusicVolume = volume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxSource.volume = volume;
        
        // store current volume
        SetPlayerPrefsVolume(AudioType.SfxAudio.ToString(), volume);
        currentSfxVolume = volume;
    }
    
    private void SetPlayerPrefsVolume(string prefKeyName, float volume)
    {
        PlayerPrefs.SetFloat(prefKeyName, volume);
    }

    #endregion
    
}
