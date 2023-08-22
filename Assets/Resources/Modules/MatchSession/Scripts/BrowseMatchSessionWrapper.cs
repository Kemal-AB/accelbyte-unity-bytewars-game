using System;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class BrowseMatchSessionWrapper
{
    private static Session _session;
    private const string ClassName = "[BrowseMatchSessionWrapper]";
    private static Action<PublicUserData> _onGetPlayerPublicData;
    private static string _nextPage;
    private static Action<BrowseMatchResult> _onQueryMatchSessionFinished;
    private static Action<BrowseMatchResult> _onQueryNextPageMatchSessionFinished;
    private static bool _isBrowseMatchSessionsCanceled;
    private static bool _isQueryingNextMatchSessions;
    private static bool _isJoinMatchSessionCancelled;
    private static Action<string> _onJoinedMatchSession;
    private static InGameMode _requestedGameMode = InGameMode.None;
    private static void Init()
    {
        if (_session != null) return;
        var lobby = MultiRegistry.GetApiClient().GetLobby();
        if(!lobby.IsConnected)
            lobby.Connect();
        _session = MultiRegistry.GetApiClient().GetSession();
    }

    #region BrowseMatchSession
    public static void BrowseMatch(Action<BrowseMatchResult> onSessionRetrieved)
    {
        Init();
        _nextPage = "";
        _isBrowseMatchSessionsCanceled = false;
        _onQueryMatchSessionFinished = onSessionRetrieved;
        _session.QueryGameSession(MatchSessionConfig.CreatedMatchSessionAttribute, OnBrowseMatchSessionFinished);
    }
    private static void OnBrowseMatchSessionFinished(Result<PaginatedResponse<SessionV2GameSession>> result)
    {
        if (result.IsError)
        {
            Debug.LogWarning($"{ClassName} error:{result.Error.ToJsonString()}");
            if(!_isBrowseMatchSessionsCanceled)
                _onQueryMatchSessionFinished?.Invoke(new BrowseMatchResult(null, result.Error.Message));
        }
        else
        {
            Debug.Log($"{ClassName} success getting match sessions");
            if(!_isBrowseMatchSessionsCanceled)
                _onQueryMatchSessionFinished?.Invoke(new BrowseMatchResult(result.Value.data));
            _nextPage = result.Value.paging.next;
        }
    }
    public static void CancelBrowseMatchSessions()
    {
        _isBrowseMatchSessionsCanceled = true;
    }
    #endregion BrowseMatchSession

    public static void GetUserDisplayName(string userId, ResultCallback<PublicUserData> onPublicUserDataRetrieved)
    {
        MultiRegistry.GetApiClient().GetUser()
            .GetUserByUserId(userId, onPublicUserDataRetrieved);
    }

    #region JoinMatchSession
    public static void JoinMatchSession(string sessionId, 
        InGameMode gameMode,
        Action<string> onJoinedGameSession)
    {
        Init();
        _isJoinMatchSessionCancelled = false;
        _onJoinedMatchSession = onJoinedGameSession;
        _requestedGameMode = gameMode;
        _session.JoinGameSession(sessionId, OnJoinedGameSession);
    }
    private static void OnJoinedGameSession(Result<SessionV2GameSession> result)
    {
        if (result.IsError)
        {
            Debug.LogWarning($"{ClassName} error:{result.Error.ToJsonString()}");
            if(!_isJoinMatchSessionCancelled)
                _onJoinedMatchSession?.Invoke(result.Error.Message);
        }
        else
        {
            var gameSession = result.Value;
            if (gameSession.configuration.type == SessionConfigurationTemplateType.DS)
            {
                if (gameSession.dsInformation.status == SessionV2DsStatus.AVAILABLE)
                {
                    Debug.Log($"{ClassName} success joined match _session");
                    if (_isJoinMatchSessionCancelled) return;
                    MatchSessionWrapper.SetJoinedGameSession(gameSession);
                    SessionCache.SetJoinedSessionIdAndLeaderUserId(gameSession.id, gameSession.leaderId);
                    MatchSessionHelper.ConnectToDs(gameSession, _requestedGameMode);
                }
                else
                {
                    Debug.LogWarning($"{ClassName} Failed to join _session, no response from the server");
                    MatchSessionWrapper.LeaveGameSession(gameSession.id);
                    if(!_isJoinMatchSessionCancelled)
                        _onJoinedMatchSession?.Invoke("Failed to join _session, no response from the server");
                }
            }
            else if (gameSession.configuration.type == SessionConfigurationTemplateType.P2P)
            {
                PeerToPeerHelper.StartAsP2PClient(gameSession.leaderId, _requestedGameMode);
            }
        }
    }
    public static void CancelJoinMatchSession()
    {
        _isJoinMatchSessionCancelled = true;
    }
    #endregion JoinMatchSession

    #region QueryNextPageMatchSessions
    public static void QueryNextMatchSessions(Action<BrowseMatchResult> onQueryNextMatchSessionsFinished)
    {
        if (String.IsNullOrEmpty(_nextPage))
        {
            onQueryNextMatchSessionsFinished?
                .Invoke(new BrowseMatchResult(Array.Empty<SessionV2GameSession>()));
            _isQueryingNextMatchSessions = false;
        }
        else
        {
            if (!_isQueryingNextMatchSessions)
            {
                var req = GenerateRequestFromNextPage(_nextPage);
                if(_session==null)
                    Init();
                _onQueryNextPageMatchSessionFinished = onQueryNextMatchSessionsFinished;
                _session?.QueryGameSession(req, OnQueryNextPageFinished);
            }
        }
    }

    private static void OnQueryNextPageFinished(Result<PaginatedResponse<SessionV2GameSession>> result)
    {
        if (result.IsError)
        {
            _onQueryNextPageMatchSessionFinished?.Invoke(new BrowseMatchResult(null, result.Error.Message));
        }
        else
        {
            _nextPage = result.Value.paging.next;
            if(!String.IsNullOrEmpty(_nextPage))
                Debug.Log($"{ClassName} next page: {_nextPage}");
            _onQueryNextPageMatchSessionFinished?.Invoke(new BrowseMatchResult(result.Value.data));
        }
        _isQueryingNextMatchSessions = false;
    }

    private static Dictionary<string, object> GenerateRequestFromNextPage(string nextPageUrl)
    {
        _isQueryingNextMatchSessions = true;
        var result = MatchSessionConfig.CreatedMatchSessionAttribute;
        var fullUrl = nextPageUrl.Split('?');
        if (fullUrl.Length < 2)
            return result;
        var joinedParameters = fullUrl[1];
        var parameters = joinedParameters.Split('&');
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var keyValue = parameter.Split('=');
            if (keyValue.Length < 2)
                return result;
            result.Add(keyValue[0], keyValue[1]);
        }
        return result;
    }
    #endregion QueryNextPageMatchSessions
}

public struct BrowseMatchResult
{
    public BrowseMatchResult(SessionV2GameSession[] result, string errorMessage="")
    {
        Result = result;
        ErrorMessage = errorMessage;
    }
    public readonly SessionV2GameSession[] Result;
    public readonly string ErrorMessage;
}
