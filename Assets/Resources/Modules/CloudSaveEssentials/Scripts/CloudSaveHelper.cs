using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class CloudSaveHelper : MonoBehaviour
{
    // player record key and configurations
    private const string GAMEOPTIONS_RECORDKEY = "GameOptions-Sound";
    private const string MUSICVOLUME_ITEMNAME = "musicvolume";
    private const string SFXVOLUME_ITEMNAME = "sfxvolume";

    private CloudSaveEssentialsWrapper _cloudSaveWrapper;
    private Dictionary<string, object> volumeSettings;

    // Start is called before the first frame update
    void Start()
    {
        // get cloud save's wrapper
        _cloudSaveWrapper = TutorialModuleManager.Instance.GetModuleClass<CloudSaveEssentialsWrapper>();

        OptionsMenu.onOptionsMenuActivated += (musicVolume, sfxVolume) => GetGameOptions(musicVolume, sfxVolume);
        OptionsMenu.onOptionsValueChanged += (settingType, volumeValue) => UpdateGameOptions(settingType, volumeValue);
    }

    #region Game Options Getter Setter Function

    public void GetGameOptions(float musicVolume, float sfxVolume)
    {
        if (volumeSettings == null)
        {
            volumeSettings = new Dictionary<string, object>()
            {
                {MUSICVOLUME_ITEMNAME, musicVolume},
                {SFXVOLUME_ITEMNAME, sfxVolume}
            };
        }
        _cloudSaveWrapper.GetUserRecord(GAMEOPTIONS_RECORDKEY, OnGetGameOptionsCompleted);
    }

    private void SaveGameOptions()
    {
        _cloudSaveWrapper.SaveUserRecord(GAMEOPTIONS_RECORDKEY, volumeSettings, OnSaveGameOptionsCompleted);
    }

    private void UpdateGameOptions(string settingName, float newVolumeValue)
    {
        float recordedVolume = Convert.ToSingle(volumeSettings[settingName]);

        if (!recordedVolume.Equals(newVolumeValue))
        {
            volumeSettings[settingName] = newVolumeValue;

            SaveGameOptions();
        }
    }

    #endregion

    #region Callback Function

    private void OnGetGameOptionsCompleted(Result<UserRecord> result)
    {
        if (!result.IsError)
        {
            Dictionary<string, object> recordData = result.Value.value;
            if (recordData != null)
            {
                volumeSettings[MUSICVOLUME_ITEMNAME] = recordData[MUSICVOLUME_ITEMNAME];
                AudioManager.Instance.SetMusicVolume(Convert.ToSingle(recordData[MUSICVOLUME_ITEMNAME]));

                volumeSettings[SFXVOLUME_ITEMNAME] = recordData[SFXVOLUME_ITEMNAME];
                AudioManager.Instance.SetSfxVolume(Convert.ToSingle(recordData[SFXVOLUME_ITEMNAME]));
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

    #endregion
}
