using System;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginHandler : MenuCanvas
{
    [SerializeField] private GameObject loginStatePanel;
    [SerializeField] private GameObject loginLoadingPanel;
    [SerializeField] private GameObject loginFailedPanel;
    [SerializeField] private Button loginWithDeviceIdButton;
    [SerializeField] private Button retryLoginButton;
    [SerializeField] private Button quitGameButton;
    [SerializeField] private TMP_Text failedMessageText;

    public delegate void LoginHandlerDelegate(TokenData tokenData);
    public static event LoginHandlerDelegate onLoginCompleted = delegate {};
    
    private AuthEssentialsWrapper _authWrapper;
    private CloudSaveEssentialsWrapper _cloudSaveWrapper;
    private LoginType _lastLoginMethod;

    // player record key and configurations
    private const string GAMEOPTIONS_RECORDKEY = "GameOptions-Sound";
    private const string MUSICVOLUME_ITEMNAME = "musicvolume";
    private const string SFXVOLUME_ITEMNAME = "sfxvolume";
    
    #region LoginView enum
    public enum LoginView
    {
        LoginState,
        LoginLoading,
        LoginFailed
    }
    
    private LoginView CurrentView
    {
        get => CurrentView;
        set
        {
            switch (value)
            {
                case LoginView.LoginState:
                    loginStatePanel.SetActive(true);
                    loginLoadingPanel.SetActive(false);
                    loginFailedPanel.SetActive(false);
                    break;

                case LoginView.LoginLoading:
                    loginStatePanel.SetActive(false);
                    loginLoadingPanel.SetActive(true);
                    loginFailedPanel.SetActive(false);
                    break;
                
                case LoginView.LoginFailed:
                    loginStatePanel.SetActive(false);
                    loginLoadingPanel.SetActive(false);
                    loginFailedPanel.SetActive(true);
                    break;
            }
        }
    }

    #endregion

    private void Start()
    {
        // get auth's subsystem
        _authWrapper = TutorialModuleManager.Instance.GetModuleClass<AuthEssentialsWrapper>();
        _cloudSaveWrapper = TutorialModuleManager.Instance.GetModuleClass<CloudSaveEssentialsWrapper>();
    }

    private void OnEnable()
    {
        // UI initialization
        loginWithDeviceIdButton.onClick.AddListener(OnLoginWithDeviceIdButtonClicked);
        retryLoginButton.onClick.AddListener(OnRetryLoginButtonClicked);
        quitGameButton.onClick.AddListener(OnQuitGameButtonClicked);

        CurrentView = LoginView.LoginState;
    }

    private void Login(LoginType loginMethod)
    {
        CurrentView = LoginView.LoginLoading;
        _lastLoginMethod = loginMethod;
        _authWrapper.Login(loginMethod, OnLoginCompleted);
    }

    private void GetGameOptions()
    {
        _cloudSaveWrapper.GetUserRecord(GAMEOPTIONS_RECORDKEY, OnGetGameOptionsCompleted);
    }

    private void OnLoginCompleted(Result<TokenData, OAuthError> result)
    {
        if (!result.IsError)
        {
            onLoginCompleted.Invoke(result.Value);
            
            MenuManager.Instance.ChangeToMenu(AssetEnum.MainMenuCanvas);
            Debug.Log(MultiRegistry.GetApiClient().session.UserId);
            
            // Get player settings from Cloud Save
            TutorialModuleData cloudSaveEssentials = TutorialModuleManager.Instance.GetModule(TutorialType.CloudSaveEssentials);
            if (cloudSaveEssentials.isActive)
            {
                GetGameOptions();
            }
        }
        else
        {
            failedMessageText.text = "Login Failed: "  + result.Error.error;
            CurrentView = LoginView.LoginFailed;
        }
    }

    private void OnGetGameOptionsCompleted(Result<UserRecord> result)
    {
        if (!result.IsError)
        {
            foreach (KeyValuePair<string, object> recordData in result.Value.value)
            {
                if (recordData.Key == MUSICVOLUME_ITEMNAME)
                {
                    AudioManager.Instance.SetMusicVolume(Convert.ToSingle(recordData.Value));
                }

                if (recordData.Key == SFXVOLUME_ITEMNAME)
                {
                    AudioManager.Instance.SetSfxVolume(Convert.ToSingle(recordData.Value));
                }
            }
        }
    }

    private void OnLoginWithDeviceIdButtonClicked()
    {
        Login(LoginType.DeviceId);
    }

    private void OnRetryLoginButtonClicked()
    {
        Login(_lastLoginMethod);
    }
    
    private void OnQuitGameButtonClicked()
    {
        Application.Quit();
    }

    public override GameObject GetFirstButton()
    {
        return loginWithDeviceIdButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.LoginMenuCanvas;
    }
}
