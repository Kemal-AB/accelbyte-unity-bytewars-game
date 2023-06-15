using System;
using System.Linq;
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
    private LoginType _lastLoginMethod;
        
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

    private void OnLoginCompleted(Result<TokenData, OAuthError> result)
    {
        if (!result.IsError)
        {
            onLoginCompleted.Invoke(result.Value);
            
            MenuManager.Instance.ChangeToMenu(AssetEnum.MainMenuCanvas);
        }
        else
        {
            failedMessageText.text = "Login Failed: "  + result.Error.error;
            CurrentView = LoginView.LoginFailed;
        }
    }

    private void AutoLoginCmd()
    {
        string[] cmdArgs = Environment.GetCommandLineArgs();
        string username = "";
        string password = "";

        foreach (string cmdArg in cmdArgs)
        {
            if (cmdArg.Contains("-AUTH_LOGIN="))
            {
                username = cmdArg.Replace("-AUTH_LOGIN=", "");
            }

            if (cmdArg.Contains("-AUTH_PASSWORD="))
            {
                password = cmdArg.Replace("-AUTH_PASSWORD=", "");
            }
        }

        // try login with the username and password specified with command-line arguments
        if (username != "" && password != "")
        {
            _authWrapper.LoginWithUsername(username, password, OnLoginCompleted);
        }
    }
    
    private void OnLoginWithDeviceIdButtonClicked()
    {
        if (Environment.GetCommandLineArgs().Contains("-AUTH_TYPE=ACCELBYTE"))
        {
            AutoLoginCmd();
        }
        else
        {
            Login(LoginType.DeviceId);
        }
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
