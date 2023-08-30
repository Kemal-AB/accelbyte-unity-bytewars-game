using System;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class MatchSessionWrapper : MonoBehaviour
{
    public static event Action<SessionV2GameSession> OnGameSessionUpdated;
    private static Lobby _lobby;
    private static Session _session;
    private static MatchSessionServerType _requestedSessionServerType = MatchSessionServerType.DedicatedServer;
    private static InGameMode _requestedGameMode = InGameMode.None;
    private const string ClassName = "[MatchSessionWrapper]";
    private static Action<string> _onCreatedMatchSession;
    private static SessionV2GameSession _v2GameSession;
    private static bool _isCreateMatchSessionCancelled;
    public void Start()
    {
#if UNITY_SERVER
        GameManager.Instance.OnRegisterServer += MatchSessionServerHelper.LoginAndRegisterServer;
        GameManager.Instance.OnDeregisterServer += MatchSessionServerHelper.UnRegisterServer;
        GameManager.Instance.OnRejectBackfill += MatchSessionServerHelper.OnBackFillRejected;
#else
        _lobby = MultiRegistry.GetApiClient().GetLobby();
        _session = MultiRegistry.GetApiClient().GetSession();
        _lobby.SessionV2DsStatusChanged += OnV2DSStatusChanged;
        _lobby.SessionV2GameSessionMemberChanged += OnV2GameSessionMemberChanged;
        GameManager.Instance.OnClientLeaveSession += LeaveGameSession;
        LoginHandler.onLoginCompleted += OnLoginSuccess;
#endif
    }

    #region CreateMatchSession
    public static void Create(InGameMode gameMode, 
        MatchSessionServerType sessionServerType, 
        Action<string> onCreatedMatchSession)
    {
        _isCreateMatchSessionCancelled = false;
        _requestedSessionServerType = sessionServerType;
        _requestedGameMode = gameMode;
        _v2GameSession = null;
        var config = MatchSessionConfig.MatchRequests;
        if (!config.TryGetValue(gameMode, out var matchTypeDict)) return;
        if (!matchTypeDict.TryGetValue(sessionServerType, out var request)) return;
        Debug.Log($"{ClassName} creating session {gameMode} {sessionServerType}");
        _onCreatedMatchSession = onCreatedMatchSession;
        var isUsingLocalDS = ConnectionHandler.GetArgument();
        if (isUsingLocalDS)
        {
            request.serverName = ConnectionHandler.LocalServerName;
        }
        _session.CreateGameSession(request, OnCreateGameSessionResult);
    }
    private static void OnCreateGameSessionResult(Result<SessionV2GameSession> result)
    {
        if (result.IsError)
        {
            Debug.Log($"{ClassName} error: {result.Error.Message}");
            _onCreatedMatchSession?.Invoke(result.Error.Message);
        }
        else
        {
            Debug.Log($"{ClassName} create session result: { JsonUtility.ToJson(result.Value) }");
            if (_isCreateMatchSessionCancelled) return;
            _v2GameSession = result.Value;
            SessionCache.SetJoinedSessionIdAndLeaderUserId(_v2GameSession.id, _v2GameSession.leaderId);
            if (_requestedSessionServerType == MatchSessionServerType.PeerToPeer)
            {
                // PeerToPeerHelper.StartAsP2PHost(_requestedGameMode);
            }
            //for MatchSessionServerType.DedicatedServer will wait OnV2DSStatusChanged to be called;
            _onCreatedMatchSession?.Invoke("");
            _onCreatedMatchSession = null;
        }
    }
    
    public static void CancelCreateMatchSession()
    {
        _isCreateMatchSessionCancelled = true;
        LeaveGameSession();
    }

    #endregion CreateMatchSession

    #region Events
    private static void OnV2DSStatusChanged(Result<SessionV2DsStatusUpdatedNotification> result)
    {
        if (result.IsError)
        {
            Debug.LogWarning($"{ClassName} receiving DS status change error: {result.Error.Message}");
        }
        else
        {
            if (_v2GameSession == null ||
                !_v2GameSession.id.Equals(result.Value.sessionId))
            {
                Debug.LogWarning($"{ClassName} unmatched DS session id is received");
                return;
            }
            if (_isCreateMatchSessionCancelled) return;
            _v2GameSession = result.Value.session;
            OnGameSessionUpdated?.Invoke(_v2GameSession);
            if (_v2GameSession.dsInformation.status != SessionV2DsStatus.AVAILABLE) return;
            MatchSessionHelper.ConnectToDs(_v2GameSession, _requestedGameMode);
        }
    }
    private static void OnV2GameSessionMemberChanged(Result<SessionV2GameMembersChangedNotification> result)
    {
        if (!result.IsError)
        {
            var gameSession = result.Value.session;
            SessionCache.SetSessionLeaderId(gameSession.id, gameSession.leaderId);
            OnGameSessionUpdated?.Invoke(gameSession);
        }
        LogJson(ClassName,"SessionV2GameSessionMemberChanged", result);
    }
    
    private static void OnLoginSuccess(TokenData tokenData)
    {
        MatchSessionHelper.GetCurrentUserPublicData(tokenData.user_id);
        if(!_lobby.IsConnected)
            _lobby.Connect();
    }
    private static void LeaveGameSession()
    {
        if (_v2GameSession == null) return;
        _session.LeaveGameSession(_v2GameSession.id, OnLeaveGameSession);
    }
    private static void OnLeaveGameSession(Result<SessionV2GameSession> result)
    {
        if (result.IsError)
        {
            Debug.LogWarning($"{ClassName} error leave session: {result.Error.Message}");
        }
        else
        {
            SessionCache.SetJoinedSessionId("");
            Debug.Log($"{ClassName} success leave session id: {_v2GameSession.id}");
        }
        if(_isCreateMatchSessionCancelled)
            _session.DeleteGameSession(_v2GameSession.id, OnDeleteGameSession);
    }
    private static void OnDeleteGameSession(Result result)
    {
        Debug.Log(result.IsError
            ? $"{ClassName} error delete game session: {result.Error.Message}"
            : $"{ClassName} delete session id:{_v2GameSession?.id} success");
    }
    #endregion
    /// <summary>
    /// leave game session if failed to connect to game server
    /// </summary>
    /// <param name="sessionId">session id to leave</param>
    public static void LeaveGameSession(string sessionId)
    {
        _session.LeaveGameSession(sessionId, null);
    }
    /// <summary>
    /// cached joined game session
    /// </summary>
    /// <param name="gameSession">joined game session</param>
    public static void SetJoinedGameSession(SessionV2GameSession gameSession)
    {
        _v2GameSession = gameSession;
    }
    public static void LogJson<T>(string className, string prefix, Result<T> result)
    {
        if (result.IsError)
        {
            Debug.LogWarning($"{className} fail {prefix} {result.Error.Message}");
        }
        else
        {
            Debug.Log($"{className} success {prefix} {result.Value.ToJsonString()}");
        }
    }

    #region Debug
    public static void GetDetail()
    {
        if (_v2GameSession == null) return;
        _session.GetGameSessionDetailsBySessionId(_v2GameSession.id, OnSessionDetailsRetrieved);
    }
    private static void OnSessionDetailsRetrieved(Result<SessionV2GameSession> result)
    {
        Debug.Log($"{ClassName} OnSessionDetailsRetrieved currentUserId:{MultiRegistry.GetApiClient().session.UserId}");
        LogJson(ClassName, "OnSessionDetailsRetrieved", result);
    }
    #endregion Debug
}
