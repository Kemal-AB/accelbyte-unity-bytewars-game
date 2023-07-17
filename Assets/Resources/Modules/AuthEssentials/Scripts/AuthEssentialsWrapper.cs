using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class AuthEssentialsWrapper : MonoBehaviour
{
    // AccelByte's Multi Registry references
    private ApiClient apiClient;
    private User user;
    
    // required variables to login with other platform outside AccelByte
    private PlatformType _platformType;
    private string _platformToken;

    public TokenData userData;

    void Start()
    {
        apiClient = MultiRegistry.GetApiClient();
        user = apiClient.GetApi<User, UserApi>();
    }

    #region AB Service Functions

    public void Login(LoginType loginMethod, ResultCallback<TokenData, OAuthError> resultCallback)
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
    
    public void LoginWithUsername(string username, string password, ResultCallback<TokenData, OAuthError> resultCallback)
    {
        user.LoginWithUsernameV3(username, password, result => OnLoginCompleted(result, resultCallback), false);
    }

    /// <summary>
    /// Get user info of some users in bulk
    /// </summary>
    /// <param name="userIds">an array of user id from the desired users</param>
    /// <param name="resultCallback">callback function to get result from other script</param>
    public void BulkGetUserInfo(string[] userIds, ResultCallback<ListBulkUserInfoResponse> resultCallback)
    {
        user.BulkGetUserInfo(userIds, result => OnBulkGetUserInfo(result, resultCallback));
    }
    
    #endregion
    
    #region Callback Functions

    /// <summary>
    /// Default Callback for LoginWithOtherPlatform() function
    /// </summary>
    /// <param name="result">result of the LoginWithOtherPlatform() function call</param>
    /// <param name="customCallback">additional callback function that can be customized from other script</param>
    private void OnLoginCompleted(Result<TokenData, OAuthError> result, ResultCallback<TokenData, OAuthError> customCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log("Login user successful.");

            userData = result.Value;
        }
        else
        {
            Debug.Log($"Login user failed. Message: {result.Error.error}");
        }

        customCallback?.Invoke(result);
    }

    /// <summary>
    /// Default Callback for BulkGetUserInfo() function
    /// </summary>
    /// <param name="result">result of the BulkGetUserInfo() function call</param>
    /// <param name="customCallback">additional callback function that can be customized from other script</param>
    private void OnBulkGetUserInfo(Result<ListBulkUserInfoResponse> result, ResultCallback<ListBulkUserInfoResponse> customCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log("Bulk get user info success!");
        }
        else
        {
            Debug.Log($"Bulk get user info failed. Message: {result.Error.Message}");
        }
        
        customCallback?.Invoke(result);
    }

    #endregion
}
