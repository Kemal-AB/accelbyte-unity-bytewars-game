using System;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class SinglePlatformAuthWrapper_Starter : MonoBehaviour
{
    private const string ClassName = "SinglePlatformAuthWrapper_Starter";
    private User user;
    private LoginHandler loginHandler = null;
    private SteamHelper steamHelper;
    private const PlatformType PlatformType = AccelByte.Models.PlatformType.Steam;
    private ResultCallback<TokenData, OAuthError> platformLoginCallback;
    private TokenData tokenData;
    private void Start()
    {
        Debug.Log($"[{ClassName}] is started");
        steamHelper = new SteamHelper();
        var apiClient = MultiRegistry.GetApiClient();
        user = apiClient.GetApi<User, UserApi>();
        SetLoginWithSteamButtonClickCallback();
    }
    private void OnLoginWithSteamButtonClicked()
    {
        if (loginHandler == null) return;
        loginHandler.onRetryLoginClicked = OnLoginWithSteamButtonClicked;
        loginHandler.SetView(LoginHandler.LoginView.LoginLoading);
        //get steam token to be used as platform token later
        steamHelper.GetAuthSessionTicket(OnGetAuthSessionTicketFinished);
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
                        TutorialModuleManager.Instance
                            .IsModuleActive(TutorialType.SinglePlatformAuth);
                    bool isLoginWithSteam = SteamManager.Initialized && isSingleAuthModuleActive;
                    if (isLoginWithSteam)
                    {
                        if (GConfig.GetSteamAutoLogin())
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

    private void OnGetAuthSessionTicketFinished(string steamAuthSessionTicket)
    {
        if (loginHandler == null) return;
        if (String.IsNullOrEmpty(steamAuthSessionTicket))
        {
            loginHandler.OnLoginCompleted(
                CreateLoginErrorResult(ErrorCode.CachedTokenNotFound,
                    "Failed to get steam token"));
        }
        else
        {
            //login using steamAuthSessionTicket
            user.LoginWithOtherPlatform(PlatformType,
                steamAuthSessionTicket, OnLoginWithOtherPlatformCompleted);
        }
    }
    private void OnLoginWithOtherPlatformCompleted(Result<TokenData, OAuthError> result)
    {
        if (result.IsError)
        {
            loginHandler.OnLoginCompleted(result);
        }
        else
        {
            tokenData = result.Value;
            GetUserPublicData(tokenData.user_id);
        }
    }
    private void GetUserPublicData(string receivedUserId)
    {
        GameData.CachedPlayerState.playerId = receivedUserId;
        user.GetUserByUserId(receivedUserId, OnGetUserPublicDataFinished);
    }
    private void OnGetUserPublicDataFinished(Result<PublicUserData> result)
    {
        if (result.IsError)
        {
            Debug.Log($"[{ClassName}] error OnGetUserPublicDataFinished:{result.Error.Message}");
            loginHandler.onRetryLoginClicked = () => GetUserPublicData(tokenData.user_id);
            loginHandler.OnLoginCompleted(CreateLoginErrorResult(result.Error.Code, result.Error.Message));
        }
        else
        {
            var publicUserData = result.Value;
            GameData.CachedPlayerState.avatarUrl = publicUserData.avatarUrl;
            GameData.CachedPlayerState.playerName = publicUserData.displayName;
            loginHandler.OnLoginCompleted(Result<TokenData, OAuthError>.CreateOk(tokenData));
        }
    }

    private Result<TokenData, OAuthError> CreateLoginErrorResult(ErrorCode errorCode, string errorDescription)
    {
        return Result<TokenData, OAuthError>.CreateError(new OAuthError()
        {
            error = errorCode.ToString(),
            error_description = errorDescription
        });
    }
}
