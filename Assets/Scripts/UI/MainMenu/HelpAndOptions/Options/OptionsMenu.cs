using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text musicVolumeText;
    [SerializeField] private TMP_Text sfxVolumeText;
    [SerializeField] private Button backButton;

    void Start()
    {
        // UI Initialization
        musicVolumeSlider.onValueChanged.AddListener(volume => ChangeMusicVolume(volume));
        sfxVolumeSlider.onValueChanged.AddListener(volume => ChangeSfxVolume(volume));
        backButton.onClick.AddListener(() => MenuManager.Instance.OnBackPressed());
        
        // Update options value based on PlayerPrefs stored in AudioManager
        musicVolumeSlider.value = AudioManager.Instance.GetCurrentVolume(AudioManager.AudioType.MusicAudio);
        sfxVolumeSlider.value = AudioManager.Instance.GetCurrentVolume(AudioManager.AudioType.SfxAudio);
    }

    private void ChangeMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
        
        int volumeInt = (int)(volume * 100);
        musicVolumeText.text = volumeInt.ToString() + "%";
    }
    
    private void ChangeSfxVolume(float volume)
    {
        AudioManager.Instance.SetSfxVolume(volume);

        int volumeInt = (int)(volume * 100);
        sfxVolumeText.text = volumeInt.ToString() + "%";
    }
}
