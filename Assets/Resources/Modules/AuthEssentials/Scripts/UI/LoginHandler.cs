using System;
using AccelByte.Api;
using AccelByte.Core;
using UnityEngine;
using UnityEngine.UI;

public class LoginHandler : MonoBehaviour
{
    [SerializeField] private GameObject loginStatePanel;
    [SerializeField] private GameObject loginLoadingPanel;
    [SerializeField] private GameObject loginFailedPanel;
    [SerializeField] private Button loginWithDeviceIdButton;
    [SerializeField] private Button retryLoginButton;
    [SerializeField] private Button quitGameButton;

    private AuthEssentialsSubsystem authSubsystem;
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
        authSubsystem = TutorialModuleManager.Instance.GetModuleClass<AuthEssentialsSubsystem>();
    }

    private void OnEnable()
    {
        // UI initialization
        loginWithDeviceIdButton.onClick.AddListener(() => Login(LoginType.DeviceId));
        retryLoginButton.onClick.AddListener(() => Login(_lastLoginMethod));
        quitGameButton.onClick.AddListener(Application.Quit);

        CurrentView = LoginView.LoginState;
    }

    private void Login(LoginType loginMethod)
    {
        CurrentView = LoginView.LoginLoading;
        _lastLoginMethod = loginMethod;
        authSubsystem.Login(loginMethod, OnLoginCompleted);
    }

    private void OnLoginCompleted(Result result)
    {
        if (!result.IsError)
        {
            MenuManager.Instance.ChangeToMenu(AssetEnum.MainMenuCanvas);
        }
        else
        {
            CurrentView = LoginView.LoginFailed;
        }
    }
}
