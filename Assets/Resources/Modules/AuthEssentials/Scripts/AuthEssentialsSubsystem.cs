using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class AuthEssentialsSubsystem : MonoBehaviour
{
    // AccelByte's Multi Registry references
    private ApiClient apiClient;
    private User user;
    
    // required variables to login with other platform outside AccelByte
    private PlatformType _platformType;
    private string _platformToken;
    
    void Start()
    {
        apiClient = MultiRegistry.GetApiClient();
        user = apiClient.GetApi<User, UserApi>();
    }

    #region AB Service Functions

    public void Login(LoginType loginMethod, ResultCallback resultCallback)
    {
        switch (loginMethod)
        {
            case LoginType.DeviceId:
                _platformType = PlatformType.Device;
                _platformToken = SystemInfo.deviceUniqueIdentifier;
                break;
        }
        
        Debug.Log("[AuthEssentials] Trying to login with device id: "+ SystemInfo.deviceUniqueIdentifier);
        
        user.LoginWithOtherPlatform(_platformType, _platformToken, result => OnLoginCompleted(result, resultCallback));
    }

    #endregion
    
    #region Callback Functions

    /// <summary>
    /// Default Callback for LoginWithOtherPlatform() function
    /// </summary>
    /// <param name="result">result of the LoginWithOtherPlatform() function call</param>
    /// <param name="customCallback">additional callback function that can be customized from other script</param>
    private void OnLoginCompleted(Result result, ResultCallback customCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log("Login user successful.");
        }
        else
        {
            Debug.Log($"Login user failed. Message: {result.Error.Message}");
        }

        customCallback?.Invoke(result);
    }

    #endregion
}
