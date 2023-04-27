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
    private const string GAMEOPTIONS_RECORDKEY = "GameOptions-Sound";
    private const string MUSICVOLUME_ITEMNAME = "musicvolume";
    private const string SFXVOLUME_ITEMNAME = "sfxvolume";

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
            GetGameOptions();
        }
    }

    void OnEnable()
    {
        if (gameObject.activeSelf && _cloudSaveWrapper != null)
        {
            GetGameOptions();
        }
    }

    private void ChangeMusicVolume(float musicVolume)
    {
        AudioManager.Instance.SetMusicVolume(musicVolume);
        
        int musicVolumeInt = (int)(musicVolume * 100);
        musicVolumeText.text = musicVolumeInt.ToString() + "%";

        if (volumeSettings != null)
        {
            UpdateGameOptions(musicVolume, MUSICVOLUME_ITEMNAME);
        }
    }
    
    private void ChangeSfxVolume(float sfxVolume)
    {
        AudioManager.Instance.SetSfxVolume(sfxVolume);

        int sfxVolumeInt = (int)(sfxVolume * 100);
        sfxVolumeText.text = sfxVolumeInt.ToString() + "%";

        if (volumeSettings != null)
        {
            UpdateGameOptions(sfxVolume, SFXVOLUME_ITEMNAME);
        }
    }

    public void GetGameOptions()
    {
        if (volumeSettings == null)
        {
            volumeSettings = new Dictionary<string, object>()
            {
                {MUSICVOLUME_ITEMNAME, musicVolumeSlider.value},
                {SFXVOLUME_ITEMNAME, sfxVolumeSlider.value}
            };
        }
        _cloudSaveWrapper.GetUserRecord(GAMEOPTIONS_RECORDKEY, OnGetGameOptionsCompleted);
    }

    private void SaveGameOptions()
    {
        _cloudSaveWrapper.SaveUserRecord(GAMEOPTIONS_RECORDKEY, volumeSettings, OnSaveGameOptionsCompleted);
    }
    
    private void UpdateGameOptions(float newVolumeValue, string recordName)
    {
        float recordedVolume = Convert.ToSingle(volumeSettings[recordName]);
        
        if (!recordedVolume.Equals(newVolumeValue))
        {
            volumeSettings[recordName] = newVolumeValue;
            
            SaveGameOptions();
        }
    }
    
    private void OnGetGameOptionsCompleted(Result<UserRecord> result)
    {
        if (!result.IsError)
        {
            Dictionary<string, object> recordData = result.Value.value;
            if (recordData != null)
            {
                volumeSettings[MUSICVOLUME_ITEMNAME] = recordData[MUSICVOLUME_ITEMNAME];
                musicVolumeSlider.value = Convert.ToSingle(recordData[MUSICVOLUME_ITEMNAME]);
                ChangeMusicVolume(musicVolumeSlider.value);

                volumeSettings[SFXVOLUME_ITEMNAME] = recordData[SFXVOLUME_ITEMNAME];
                sfxVolumeSlider.value = Convert.ToSingle(recordData[SFXVOLUME_ITEMNAME]);
                ChangeSfxVolume(sfxVolumeSlider.value);
            }
        }
        else
        { 
            SaveGameOptions();
        }
    }

    private void OnSaveGameOptionsCompleted(Result result)
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
