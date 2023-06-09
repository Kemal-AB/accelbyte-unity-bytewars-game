using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class SinglePlatformAuthEssentialsWrapper_Starter : MonoBehaviour
{
    private const string ClassName = "SinglePlatformAuthEssentialsWrapper_Starter";
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
        // TODO: call this function if login with steam button is clicked,
        // TODO: this function will call login with steam
        Debug.Log("OnLoginWithSteamButtonClicked");
    }

    private void SetLoginWithSteamButtonClickCallback()
    {
        // TODO: this function will get login with steam button reference and assign onClick callback to it
    }
}
