using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MenuCanvas
{
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text musicVolumeText;
    [SerializeField] private TMP_Text sfxVolumeText;
    [SerializeField] private Button backButton;

    private CloudSaveEssentialsWrapper _cloudSaveWrapper = new CloudSaveEssentialsWrapper();
    private Dictionary<string, object> volumeSettings;

    // player record key and configurations
    private const string PLAYERSETTINGS_RECORDKEY = "PLAYER_SETTINGS";
    private const string MUSICVOLUME_RECORDNAME = "music-volume";
    private const string SFXVOLUME_RECORDNAME = "sfx-volume"; 

    void Start()
    {
        // get cloud save's wrapper
        _cloudSaveWrapper = TutorialModuleManager.Instance.GetModuleClass<CloudSaveEssentialsWrapper>();
        // Volume Settings dictionary initialization
        volumeSettings = new Dictionary<string, object>()
        {
            {MUSICVOLUME_RECORDNAME, 0},
            {SFXVOLUME_RECORDNAME, 0}
        };
        
        // Initialize options value based on PlayerPrefs stored in AudioManager
        musicVolumeSlider.value = AudioManager.Instance.GetCurrentVolume(AudioManager.AudioType.MusicAudio);
        sfxVolumeSlider.value = AudioManager.Instance.GetCurrentVolume(AudioManager.AudioType.SfxAudio);
        ChangeMusicVolume(musicVolumeSlider.value);
        ChangeSfxVolume(sfxVolumeSlider.value);
        
        // UI Initialization
        musicVolumeSlider.onValueChanged.AddListener(volume => ChangeMusicVolume(volume));
        sfxVolumeSlider.onValueChanged.AddListener(volume => ChangeSfxVolume(volume));
        backButton.onClick.AddListener(() => MenuManager.Instance.OnBackPressed());
    }

    void OnEnable()
    {
        _cloudSaveWrapper.GetUserRecord(PLAYERSETTINGS_RECORDKEY, OnGetUserRecordCompleted);
    }

    private void ChangeMusicVolume(float musicVolume)
    {
        AudioManager.Instance.SetMusicVolume(musicVolume);
        
        int musicVolumeInt = (int)(musicVolume * 100);
        musicVolumeText.text = musicVolumeInt.ToString() + "%";
        
        SavePlayerSettings(musicVolume, MUSICVOLUME_RECORDNAME);
    }
    
    private void ChangeSfxVolume(float sfxVolume)
    {
        AudioManager.Instance.SetSfxVolume(sfxVolume);

        int sfxVolumeInt = (int)(sfxVolume * 100);
        sfxVolumeText.text = sfxVolumeInt.ToString() + "%";
        
        SavePlayerSettings(sfxVolume, SFXVOLUME_RECORDNAME);
    }

    private void SavePlayerSettings(float newVolumeValue, string recordName)
    {
        Debug.Log(volumeSettings[recordName]);
        
        float recordedVolume = (float) volumeSettings[recordName];
        if (recordedVolume != newVolumeValue)
        {
            volumeSettings[recordName] = newVolumeValue;
            
            _cloudSaveWrapper.SaveUserRecord(PLAYERSETTINGS_RECORDKEY, volumeSettings, null);
        }
    }
    
    private void OnGetUserRecordCompleted(Result<UserRecord> result)
    {
        if (!result.IsError)
        {
            foreach (KeyValuePair<string, object> record in result.Value.value)
            {
                switch (record.Key)
                {
                    case MUSICVOLUME_RECORDNAME:
                        volumeSettings[MUSICVOLUME_RECORDNAME] = record.Value;
                        musicVolumeSlider.value = (float) record.Value;
                        break;
                    case SFXVOLUME_RECORDNAME:
                        volumeSettings[SFXVOLUME_RECORDNAME] = record.Value;
                        sfxVolumeSlider.value = (float) record.Value;
                        break;
                }
            }
        }
    }

    public override GameObject GetFirstButton()
    {
        return musicVolumeSlider.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.OptionsMenuCanvas;
    }
}
