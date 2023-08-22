using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using Unity.Netcode;
using UnityEngine;

public static class MatchSessionHelper
{
    private static ApiClient _apiClient;
    private static User _user;
    private const string ClassName = "[MatchSessionHelper]";
    private static void Init()
    {
        if (_user==null)
        {
            _apiClient = MultiRegistry.GetApiClient();
            _user = _apiClient.GetApi<User, UserApi>();
        }
    }
    
    public static void ConnectToDs(SessionV2GameSession sessionV2Game, InGameMode gameMode)
    {
        if (NetworkManager.Singleton.IsListening) return;
        int port = ConnectionHandler.LocalPort;
        if (sessionV2Game.dsInformation.server.ports.Count > 0)
        {
            sessionV2Game.dsInformation.server.ports.TryGetValue("unityds", out port);
        }
        else
        {
            port = sessionV2Game.dsInformation.server.port;
        }
        var ip = sessionV2Game.dsInformation.server.ip;
        var portUshort = (ushort)port;
        var initialData = new InitialConnectionData()
        {
            sessionId = "",
            inGameMode = gameMode,
            serverSessionId = sessionV2Game.id
        };
        GameManager.Instance
            .StartAsClient(ip, portUshort, initialData);
    }

    #region GetCurrentUserPublicData
    public static void GetCurrentUserPublicData(string receivedUserId)
    {
        Init();
        GameData.CachedPlayerState.playerId = receivedUserId;
        _user.GetUserByUserId(receivedUserId, OnGetUserPublicDataFinished);
    }

    private static void OnGetUserPublicDataFinished(Result<PublicUserData> result)
    {
        if (result.IsError)
        {
            Debug.Log($"{ClassName} error OnGetUserPublicDataFinished:{result.Error.Message}");
        }
        else
        {
            var publicUserData = result.Value;
            GameData.CachedPlayerState.playerId = publicUserData.userId;
            GameData.CachedPlayerState.avatarUrl = publicUserData.avatarUrl;
            GameData.CachedPlayerState.playerName = publicUserData.displayName;
        }
    }
    #endregion GetCurrentUserPublicData
}
