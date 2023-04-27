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

    private CloudSaveEssentialsWrapper _cloudSaveWrapper;
    private Dictionary<string, object> volumeSettings;

    // player record key and configurations
    private const string PLAYERSETTINGS_RECORDKEY = "GameOptions-Sound";
    private const string MUSICVOLUME_RECORDNAME = "musicvolume";
    private const string SFXVOLUME_RECORDNAME = "sfxvolume";

    void Start()
    {
        // get cloud save's wrapper
        _cloudSaveWrapper = TutorialModuleManager.Instance.GetModuleClass<CloudSaveEssentialsWrapper>();

        // Initialize options value based on PlayerPrefs stored in AudioManager
        musicVolumeSlider.value = AudioManager.Instance.GetCurrentVolume(AudioManager.AudioType.MusicAudio);
        sfxVolumeSlider.value = AudioManager.Instance.GetCurrentVolume(AudioManager.AudioType.SfxAudio);
        ChangeMusicVolume(musicVolumeSlider.value);
        ChangeSfxVolume(sfxVolumeSlider.value);

        // UI Initialization
        musicVolumeSlider.onValueChanged.AddListener(volume => ChangeMusicVolume(volume));
        sfxVolumeSlider.onValueChanged.AddListener(volume => ChangeSfxVolume(volume));
        backButton.onClick.AddListener(() => MenuManager.Instance.OnBackPressed());
        
        // Get player settings from Cloud Save
        TutorialModuleData cloudSaveEssentials = TutorialModuleManager.Instance.GetModule(AssetEnum.CloudSaveEssentialsAssetConfig.ToString());
        if (cloudSaveEssentials.isActive)
        {
            GetPlayerSettings();
        }
    }

    void OnEnable()
    {
        if (gameObject.activeSelf && _cloudSaveWrapper != null)
        {
            GetPlayerSettings();
        }
    }

    private void ChangeMusicVolume(float musicVolume)
    {
        AudioManager.Instance.SetMusicVolume(musicVolume);
        
        int musicVolumeInt = (int)(musicVolume * 100);
        musicVolumeText.text = musicVolumeInt.ToString() + "%";

        if (volumeSettings != null)
        {
            UpdatePlayerSettings(musicVolume, MUSICVOLUME_RECORDNAME);
        }
    }
    
    private void ChangeSfxVolume(float sfxVolume)
    {
        AudioManager.Instance.SetSfxVolume(sfxVolume);

        int sfxVolumeInt = (int)(sfxVolume * 100);
        sfxVolumeText.text = sfxVolumeInt.ToString() + "%";

        if (volumeSettings != null)
        {
            UpdatePlayerSettings(sfxVolume, SFXVOLUME_RECORDNAME);
        }
    }

    public void GetPlayerSettings()
    {
        if (volumeSettings == null)
        {
            volumeSettings = new Dictionary<string, object>()
            {
                {MUSICVOLUME_RECORDNAME, musicVolumeSlider.value},
                {SFXVOLUME_RECORDNAME, sfxVolumeSlider.value}
            };
        }
        _cloudSaveWrapper.GetUserRecord(PLAYERSETTINGS_RECORDKEY, OnGetPlayerSettingsCompleted);
    }

    private void SavePlayerSettings()
    {
        _cloudSaveWrapper.SaveUserRecord(PLAYERSETTINGS_RECORDKEY, volumeSettings, OnSavePlayerSettingsCompleted);
    }
    
    private void UpdatePlayerSettings(float newVolumeValue, string recordName)
    {
        float recordedVolume = Convert.ToSingle(volumeSettings[recordName]);
        
        if (!recordedVolume.Equals(newVolumeValue))
        {
            volumeSettings[recordName] = newVolumeValue;
            
            SavePlayerSettings();
        }
    }
    
    private void OnGetPlayerSettingsCompleted(Result<UserRecord> result)
    {
        if (!result.IsError)
        {
            Dictionary<string, object> recordData = result.Value.value;
            if (recordData != null)
            {
                volumeSettings[MUSICVOLUME_RECORDNAME] = recordData[MUSICVOLUME_RECORDNAME];
                musicVolumeSlider.value = Convert.ToSingle(recordData[MUSICVOLUME_RECORDNAME]);
                ChangeMusicVolume(musicVolumeSlider.value);

                volumeSettings[SFXVOLUME_RECORDNAME] = recordData[SFXVOLUME_RECORDNAME];
                sfxVolumeSlider.value = Convert.ToSingle(recordData[SFXVOLUME_RECORDNAME]);
                ChangeSfxVolume(sfxVolumeSlider.value);
            }
        }
        else
        { 
            SavePlayerSettings();
        }
    }

    private void OnSavePlayerSettingsCompleted(Result result)
    {
        if (!result.IsError)
        {
            Debug.Log("Player Settings updated to cloud save!");
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
