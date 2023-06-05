// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using AccelByte.Server;
using UnityEngine;

public class MatchmakingEssentialsWrapper : MonoBehaviour
{
    private MatchmakingV2 _matchmakingV2;
    private static string _matchmakingV2TicketId;
    private static string _sessionId;
    private static bool _matchCanceled = false;
    private Session _matchmakingV2Session;
    private DedicatedServerManager _dedicatedServerManager;
    private ServerDSHub _serverDSHub;
    private ServerMatchmakingV2 _matchmakingV2Server;
    private ServerOauthLoginSession _serverOauthLoginSession;
    private bool _isGameStarted = false;

    public event Action<Result<MatchmakingV2MatchTicketStatus>> OnMatchFound;
    public event Action<Result<SessionV2GameSession>> OnSessionJoined;
    public event Action<Result<SessionV2GameSession>> OnServerTimeout;

    // Start is called before the first frame update
    void Start()
    {
        _matchmakingV2 = MultiRegistry.GetApiClient().GetMatchmakingV2();
        _matchmakingV2Session = MultiRegistry.GetApiClient().GetSession();
        _dedicatedServerManager = MultiRegistry.GetServerApiClient().GetDedicatedServerManager();
        _matchmakingV2Server = MultiRegistry.GetServerApiClient().GetMatchmakingV2();
        _serverDSHub = MultiRegistry.GetServerApiClient().GetDsHub();

        GameManager.Instance.OnClientLeaveSession += LeaveSession;
        GameManager.Instance.OnDeregisterServer += UnRegisterServer;
        GameManager.Instance.OnRegisterServer += LoginAndRegisterServer;
        GameManager.Instance.OnRejectBackfill += OnBackfillRejected;
        
    }

    #region ClientSide

    #region MatchmakingRelatedCode

        public void StartMatchmaking(string matchPoolName, ResultCallback<SessionV2GameSession> resultCallback)
    {
        if (_matchCanceled)
        {
            _matchCanceled = false;
        }
        
        bool isLocal = ConnectionHandler.GetArgument();
        if (isLocal)
        {
            string localServerName = ConnectionHandler.LocalServerName;
            MatchmakingV2CreateTicketRequestOptionalParams optionals = new MatchmakingV2CreateTicketRequestOptionalParams();
            optionals.attributes = new Dictionary<string, object>()
            {
                { "server_name", localServerName },
            };
            _matchmakingV2.CreateMatchmakingTicket(matchPoolName, optionals, result => OnCreateMatchmakingCompleted(result, resultCallback)); 
        }         
        else
        {
            _matchmakingV2.CreateMatchmakingTicket(matchPoolName, null, result => OnCreateMatchmakingCompleted(result, resultCallback));
        }
    }
    
    private void OnCreateMatchmakingCompleted(Result<MatchmakingV2CreateTicketResponse> result, ResultCallback<SessionV2GameSession> resultCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log($"Matchmaking ticket created: {result.Value.matchTicketId}");
            _matchmakingV2TicketId = result.Value.matchTicketId;
            CheckMatchmakingV2Status();
            OnMatchFound += OnStartJoinSession;
            OnServerTimeout += server => resultCallback?.Invoke(server);
            OnSessionJoined += server => resultCallback?.Invoke(server);
        }
        else
        {
            Debug.Log($"Failed to create matchmaking : {result.Error.Message}");
        }
        
    }
    
    private void OnStartJoinSession(Result<MatchmakingV2MatchTicketStatus> result)
    {
        IsMatchCanceled();
        Debug.Log($"Start to join the session {result.Value.matchFound}");
        _matchmakingV2Session.JoinGameSession(_sessionId, OnJoinGameSession);
    }

    public void CheckMatchmakingV2Status()
    {
        IsMatchCanceled(() => CancelInvoke(nameof(CheckMatchmakingV2Status)));

        _matchmakingV2.GetMatchmakingTicket(_matchmakingV2TicketId, OnGetMatchmakingV2Status);
    }
    
    private void CheckGameSessionDetail()
    {
        IsMatchCanceled(() => CancelInvoke(nameof(CheckGameSessionDetail)));
        
        _matchmakingV2Session.GetGameSessionDetailsBySessionId(_sessionId, OnSessionV2Callback);
    }

    
    private void OnGetMatchmakingV2Status(Result<MatchmakingV2MatchTicketStatus> result)
    {
        IsMatchCanceled();

        if (!result.IsError)
        {
            Debug.Log(JsonUtility.ToJson(result.Value));
            if (result.Value.matchFound)
            {
                _sessionId = result.Value.sessionId;
                Debug.Log($"Success to find a match, matchfound status: {result.Value.matchFound}");
                OnMatchFound?.Invoke(result);
            }
            else
            {
                Invoke(nameof(CheckMatchmakingV2Status), 1);
            }
        }
        else
        {
            Debug.Log("check matchmaking ticket status is error 'OnGetMatchmakingV2Status': "+result.Error.Message);
        }
    }
    
    
    private void OnJoinGameSession(Result<SessionV2GameSession> result)
    {
        
        if (!result.IsError)
        {
            Debug.Log($"on join game session 'OnJoinGameSesssion': {JsonUtility.ToJson(result.Value)}");
            Debug.Log(JsonUtility.ToJson(result.Value.dsInformation.status.ToString()));
            if (result.Value.dsInformation.status != SessionV2DsStatus.AVAILABLE)
            {
                CheckGameSessionDetail();
            }
            else
            {
                Debug.Log($"game session is ready 'OnJoinGameSesssion': {JsonUtility.ToJson(result.Value.dsInformation.server)}");
                OnSessionJoined?.Invoke(result);
            }

        }
        else
        {
            Debug.Log($"error join game session: {result.Error.Message}");
        }
    }
    
    private void OnSessionV2Callback(Result<SessionV2GameSession> result)
    {
        IsMatchCanceled();

        if (result.IsError)
        {
            Debug.Log($"get game session error 'OnSessionV2Callback': {result.Error.Message}");
            OnServerTimeout?.Invoke(result);
        }
        else
        {
            Debug.Log($"dedicated server information 'OnSessionV2Callback': {JsonUtility.ToJson(result.Value.dsInformation)}");

            if (result.Value.dsInformation.status == SessionV2DsStatus.FAILED_TO_REQUEST)
            {
                OnServerTimeout?.Invoke(result);
                return;
            }
            
            if (result.Value.dsInformation.status != SessionV2DsStatus.AVAILABLE)
            {
                Invoke(nameof(CheckGameSessionDetail), 1);
            }
            else
            {
                Debug.Log($"game session is ready 'OnSessionV2Callback': {JsonUtility.ToJson(result.Value.dsInformation.server)}");
                OnSessionJoined?.Invoke(result);
            }
        }
    }
    
    public void CancelMatchMatch(ResultCallback resultCallback)
    {
        if (_matchmakingV2TicketId == null) return;
        Debug.Log(_matchmakingV2TicketId);
        _matchmakingV2.DeleteMatchmakingTicket(_matchmakingV2TicketId, result => OnCancelMatchCompleted(result, resultCallback));

    }

    private void IsMatchCanceled(Action function = null)
    {
        if (_matchCanceled)
        {
            Debug.LogWarning($"Matchmaking canceled from IsMatchCanceled");
            function?.Invoke();
            return;
        }
    }

    private void OnCancelMatchCompleted(Result result, ResultCallback  customCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log($"Success to delete the ticket.");
            _matchCanceled = true;
        }
        else
        {
            Debug.Log($"Failed to delete matchmaking ticket. Message: {result.Error.Message}");
        }
        
        customCallback?.Invoke(result);
    }

    private void LeaveSession()
    {
        if (_sessionId != null)
        {
            _matchmakingV2Session.LeaveGameSession(_sessionId, OnLeaveCompleted);
        }
    }

    private void OnLeaveCompleted(Result<SessionV2GameSession> result)
    {
        if (!result.IsError)
        {
            Debug.Log($" leave session error = {result.IsError}");
        }
        else
        {
            Debug.Log($" leave session error = {result.IsError}");

        }
    }

    #endregion

    #endregion
    
    #region ServerSideCode

    #region ServerRegistrationRelatedCode

    private void LoginAndRegisterServer()
    {
        AccelByteServerPlugin.GetDedicatedServer().LoginWithClientCredentials(result =>
        {
            if (result.IsError)
            {
                Debug.Log($"Server login failed : {result.Error.Code}: {result.Error.Message}");
                Application.Quit();
            }
            else
            {
                Debug.Log("Server login successful");
                RegisterServer();
                ConnectAndListenDSHubNotification();
            }
        });
    }

    private void RegisterServer()
    {
        bool isLocal = ConnectionHandler.GetArgument();
        
        if (!isLocal)
        {
            // Register Server to DSM
            _dedicatedServerManager.RegisterServer((int)ConnectionHandler.LocalPort, registerResult =>
            {
                Debug.Log(
                    registerResult.IsError 
                        ? "Register Server to DSM failed" 
                        : "Register Server to DSM successful");

            });
        }
        else
        {
            string ip = ConnectionHandler.LocalServerIP;
            string name = ConnectionHandler.LocalServerName;
            uint portNumber = Convert.ToUInt32(ConnectionHandler.LocalPort);
            
            // Register Local Server to DSM
            _dedicatedServerManager.RegisterLocalServer(ip, portNumber, name, registerResult =>
            {
                Debug.Log(registerResult.IsError
                    ? "Register Local Server to DSM failed"
                    : "Register Local Server to DSM successful");

            });
        }
    }
    
    private void UnRegisterServer()
    {
        bool isLocal = ConnectionHandler.GetArgument();
        
        if (isLocal)
        {
            // Deregister Local Server from AMS
            _dedicatedServerManager.DeregisterLocalServer(result =>
            {
                if (result.IsError)
                {
                    Debug.Log("Failed Deregister Local Server");
                }
                else
                {
                    Debug.Log("Successfully Deregister Local Server");
                    Application.Quit();
                }
            });
        }
        else
        {
            // Shutdown Server from AMS
            _dedicatedServerManager.ShutdownServer(true, result =>
            {
                if (result.IsError)
                {
                    Debug.Log("Failed Shutdown Server");
                }
                else
                {
                    Debug.Log("Successfully Shutdown Server");
                    Application.Quit();
                }
            });
        }    
    }

    #endregion
    
    #region BackFillRelatedCode

    private void ConnectAndListenDSHubNotification()
    {
        _serverDSHub.OnConnected += () =>
        {
            Debug.Log($"Login TO DSHUB");
        };

        _serverDSHub.MatchmakingV2ServerClaimed += (Result<ServerClaimedNotification> result) =>
        {
            if (!result.IsError)
            {
                var serverSession = result.Value.sessionId;
                Debug.Log($"Server Claimed and Assigned to sessionId = {serverSession}");
            }
        };

        _serverDSHub.MatchmakingV2BackfillProposalReceived += (Result<MatchmakingV2BackfillProposalNotification> result) =>
        {
            if (!result.IsError)
            {
                if (!_isGameStarted)
                {
                    Debug.Log($"_isGameStarted {_isGameStarted}");
                    OnBackfillProposalReceived(result.Value, _isGameStarted );
                    Debug.Log($"Start back-filling process {result.Value.matchSessionId}");

                }
                else
                {
                    OnBackfillProposalRejected(result.Value);
                }
            }
        };

        // server must be registered first to DSMC to have a ServerName
        string serverName = _dedicatedServerManager.ServerName;
        _serverDSHub.Connect(serverName);
    }
    

    private void OnBackfillProposalReceived(MatchmakingV2BackfillProposalNotification proposal, bool isStopBackfilling)
    {
        _matchmakingV2Server.AcceptBackfillProposal(proposal, isStopBackfilling, result =>
        {
            if (!result.IsError)
            {
                Debug.Log($"Backfill accepted {!isStopBackfilling}");
            }
        });
    }

    private void OnBackfillRejected()
    {
        _isGameStarted = true;
    }
    private void OnBackfillProposalRejected(MatchmakingV2BackfillProposalNotification proposal)
    {
        _matchmakingV2Server.RejectBackfillProposal(proposal, true, result =>
        {
            if (!result.IsError)
            {
                Debug.Log($"Backfill rejected- Game already started");
            }
        });
    }

    #endregion
    
    #endregion
}

