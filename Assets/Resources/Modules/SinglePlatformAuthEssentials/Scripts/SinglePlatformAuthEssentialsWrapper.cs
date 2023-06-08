using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class SinglePlatformAuthEssentialsWrapper : MonoBehaviour
{
    private const string ClassName = "SinglePlatformAuthEssentialsWrapper";
    private User user;
    private LoginHandler loginHandler = null;
    private SteamHelper steamHelper;
    private const PlatformType PlatformType = AccelByte.Models.PlatformType.Steam;
    private ResultCallback<TokenData, OAuthError> platformLoginCallback;

    private void OnEnable()
    {
        LoginHandler.onLoginCompleted += OnLoginCompleted;
    }
    private void OnDisable()
    {
        LoginHandler.onLoginCompleted -= OnLoginCompleted;
    }
    private void Start()
    {
        Debug.Log($"{ClassName} is started");
        var apiClient = MultiRegistry.GetApiClient();
        user = apiClient.GetApi<User, UserApi>();
        SetLoginWithSteamButtonClickCallback();
    }

    private void SetLoginWithSteamButtonClickCallback()
    {
        if (loginHandler == null)
        {
            if (MenuManager.Instance != null)
            {
                var menuCanvas = MenuManager.Instance.GetMenu(AssetEnum.LoginMenuCanvas);
                if (menuCanvas != null && menuCanvas is LoginHandler loginHandlerC)
                {
                    loginHandler = loginHandlerC;
                    var loginWithSteamButton = loginHandler.GetLoginButton(LoginType.Steam);
                    bool isSingleAuthModuleActive =
                        TutorialModuleManager.Instance.IsModuleActive(TutorialType.SinglePlatformAuthEssentials);
                    bool isLoginWithSteam = isSingleAuthModuleActive && SteamManager.Initialized;
                    if (isLoginWithSteam)
                    {
                        if (GConfig.GetBool("Steamworks", "bAutoLogin", true))
                        {
                            OnLoginWithSteamButtonClicked();
                        }
                        else
                        {
                            loginWithSteamButton.onClick.AddListener(OnLoginWithSteamButtonClicked);
                            loginWithSteamButton.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    private void OnLoginWithSteamButtonClicked()
    {
        if (loginHandler != null)
        {
            loginHandler.onRetryLoginClicked = OnLoginWithSteamButtonClicked;
            loginHandler.SetView(LoginHandler.LoginView.LoginLoading);
            LoginWithSteam();
        }
    }
    
    private void OnLoginCompleted(TokenData tokenData)
    {
        var userId = tokenData.user_id;
        GameData.CachedPlayerState.playerId = userId;
        user.GetUserByUserId(userId, OnGetUserCompleted);
    }
    
    private void OnGetUserCompleted(Result<PublicUserData> result)
    {
        if (result.IsError)
        {
            Debug.Log($"[{ClassName}] error OnGetUserCompleted:{result.Error.Message}");
        }
        else
        {
            var publicUserData = result.Value;
            GameData.CachedPlayerState.avatarUrl = publicUserData.avatarUrl;
            GameData.CachedPlayerState.playerName = publicUserData.displayName;
        }
    }
    
    private void LoginWithSteam()
    {
        if (steamHelper == null)
            steamHelper = new SteamHelper();
        //get steam token to be used as platform token later
        steamHelper.GetToken(OnGetSteamTokenFinished);
    }

    private void OnGetSteamTokenFinished(string steamSessionTicket)
    {
        if (loginHandler != null)
        {
            //login with platform token
            user.LoginWithOtherPlatform(PlatformType, steamSessionTicket, loginHandler.OnLoginCompleted);
        }
    }
}
