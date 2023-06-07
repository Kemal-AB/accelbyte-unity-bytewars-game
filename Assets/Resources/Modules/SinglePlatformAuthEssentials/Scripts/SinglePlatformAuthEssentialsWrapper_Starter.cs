using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class SinglePlatformAuthEssentialsWrapper_Starter : MonoBehaviour
{
    private const string ClassName = "SinglePlatformAuthEssentialsWrapper";
    private User user;
    private LoginHandler loginHandler = null;
    private SteamHelper steamHelper;
    private const PlatformType PlatformType = AccelByte.Models.PlatformType.Steam;
    private ResultCallback<TokenData, OAuthError> platformLoginCallback;
    
    private void OnLoginWithSteamButtonClicked()
    {
        // TODO: call this function if login with steam button is clicked,
        // TODO: this function will call login with steam
    }
}
