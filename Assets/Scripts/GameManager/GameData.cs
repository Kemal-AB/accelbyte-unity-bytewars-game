using System;
using System.Collections.Generic;

public static class GameData
{
    public static GameModeSO GameModeSo;
    public static PlayerState CachedPlayerState = new PlayerState();
    public static ServerType ServerType = ServerType.Offline;
    /// <summary>
    /// match/game/party session id that dedicated server claimed by
    /// always set by server
    /// </summary>
    public static string ServerSessionID = "";
}

public enum ServerType
{
    Offline,
    OnlineDedicatedServer,
    OnlinePeer2Peer
}
/// <summary>
/// connecting wrappers (the class that access Accelbyte API) and game user interface
/// </summary>
public static class SessionCache
{
    private static readonly Dictionary<string, CachedSession> k_CachedSessions = 
        new Dictionary<string, CachedSession>();
    private static string _currentPlayerJoinedSessionId;
    public static void SetJoinedSessionIdAndLeaderUserId(string sessionId, string leaderId)
    {
        _currentPlayerJoinedSessionId = sessionId;
        SetSessionLeaderId(sessionId, leaderId);
    }
    public static void SetJoinedSessionId(string sessionId)
    {
        _currentPlayerJoinedSessionId = sessionId;
    }
    public static void SetSessionLeaderId(string sessionId, string sessionLeaderId)
    {
        if (k_CachedSessions.TryGetValue(sessionId, out var cachedSession))
        {
            cachedSession.SessionLeaderUserId = sessionLeaderId;
        }
        else
        {
            k_CachedSessions.Add(sessionId, 
                new CachedSession(){Id = sessionId, SessionLeaderUserId = sessionLeaderId});
        }
    }

    public static string GetJoinedSessionLeaderUserId()
    {
        if (!String.IsNullOrEmpty(_currentPlayerJoinedSessionId))
        {
            if (k_CachedSessions.TryGetValue(_currentPlayerJoinedSessionId, out var cachedSession))
            {
                return cachedSession.SessionLeaderUserId;
            }
        }
        return "";
    }
}

public struct CachedSession
{
    public CachedSession(string id, string sessionLeaderUserId)
    {
        Id = id;
        SessionLeaderUserId = sessionLeaderUserId;
    }
    public string Id;
    public string SessionLeaderUserId;
}
