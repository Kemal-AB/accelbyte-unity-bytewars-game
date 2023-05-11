// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
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
    private static DedicatedServerManager _dedicatedServerManager;

    public event Action<Result<MatchmakingV2MatchTicketStatus>> OnMatchFound;
    public event Action<Result<SessionV2GameSession>> OnSessionJoined;
    public event Action<Result<SessionV2GameSession>> OnServerTimeout;

    // Start is called before the first frame update
    void Start()
    {
        _matchmakingV2 = MultiRegistry.GetApiClient().GetMatchmakingV2();
        _matchmakingV2Session = MultiRegistry.GetApiClient().GetSession();
        _dedicatedServerManager = MultiRegistry.GetServerApiClient().GetDedicatedServerManager();

        GameManager.Instance.OnClientLeaveSession += LeaveSession;
        GameManager.Instance.OnDeregisterServer += DeregisterServer;
        GameManager.Instance.OnRegisterServer += RegisterServer;
    }
    
    private void RegisterServer()
    {
        Debug.Log("MatchmakingEssentialsWrapper Register Server");
        bool isLocal = ConnectionHandler.GetArgument();
        
        AccelByteServerPlugin.GetDedicatedServer().LoginWithClientCredentials(result =>
        {
            if (result.IsError)
            {
                // If we error, grab the Error Code and Message to print in the Log
                Debug.Log($"Server login failed : {result.Error.Code}: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Server login successful");

                if (!isLocal)
                {
                    // Register Server to DSM
                    _dedicatedServerManager.RegisterServer((int)ConnectionHandler.LocalPort, registerResult =>
                    {
                        if (registerResult.IsError)
                        {
                            Debug.Log("Register Server to DSM failed");
                        }
                        else
                        {
                            Debug.Log("Register Server to DSM successful");
                        }
                    });
                }
                else
                {
                    string ip = ConnectionHandler.LocalServerIP;
                    string name = ConnectionHandler.LocalServerName;
                    // string name = "localds-unity";
                    uint portNumber = Convert.ToUInt32(ConnectionHandler.LocalPort);
                    Debug.Log(ip);
                    Debug.Log(name);
                    
                    // Register Local Server to DSM
                    _dedicatedServerManager.RegisterLocalServer(ip, portNumber, name, registerResult =>
                    {
                        if (registerResult.IsError)
                        {
                            Debug.Log("Register Local Server to DSM failed");
                        }
                        else
                        {
                            Debug.Log("Register Local Server to DSM successful");
                        }
                    });
                }
            }
        });
    }

    public void StartMatchmaking(string matchPoolName, ResultCallback<SessionV2GameSession> resultCallback)
    {
        if (_matchCanceled)
        {
            _matchCanceled = false;
        }
        
        bool isLocal = ConnectionHandler.GetArgument();
        // bool isLocal = true;
        if (isLocal)
        {
            string localServerName = ConnectionHandler.LocalServerName;
            // string localServerName = "testserver";
            MatchmakingV2CreateTicketRequestOptionalParams Optionals = new MatchmakingV2CreateTicketRequestOptionalParams();
            Optionals.attributes = new Dictionary<string, object>()
            {
                { "server_name", localServerName },
            };
            _matchmakingV2.CreateMatchmakingTicket(matchPoolName, Optionals, result => OnCreateMatchmakingCompleted(result, resultCallback)); 
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
            Debug.Log($"matchmaking ticket created: {result.Value.matchTicketId}");
            _matchmakingV2TicketId = result.Value.matchTicketId;
            CheckMatchmakingV2Status();
            OnMatchFound += JoinSession;
            OnServerTimeout += server => resultCallback?.Invoke(server);
            OnSessionJoined += server => resultCallback?.Invoke(server);
        }
        else
        {
            Debug.Log($"create matchmaking failed: {result.Error.Message}");
        }
        
    }
    
    private void JoinSession(Result<MatchmakingV2MatchTicketStatus> result)
    {
        if (_matchCanceled)
        {
            Debug.Log($"Matchmaking canceled");

            return;
        }
        
        Debug.Log($"start join session JoinSession {result.Value.matchFound}");
        _matchmakingV2Session.JoinGameSession(_sessionId, OnJoinGameSession);

    }

    public void CheckMatchmakingV2Status()
    {
        if (_matchCanceled)
        {
            Debug.Log($"Matchmaking canceled");
            CancelInvoke(nameof(CheckMatchmakingV2Status));
            return;
        }
        _matchmakingV2.GetMatchmakingTicket(_matchmakingV2TicketId, OnGetMatchmakingV2Status);
    }
    
    private void CheckGameSessionDetail()
    {
        if (_matchCanceled)
        {
            Debug.Log($"Matchmaking canceled");
            CancelInvoke(nameof(CheckGameSessionDetail));

            return;
        }
        
        _matchmakingV2Session.GetGameSessionDetailsBySessionId(_sessionId, OnSessionV2Callback);
    }

    
    private void OnGetMatchmakingV2Status(Result<MatchmakingV2MatchTicketStatus> result)
    {
        if (_matchCanceled)
        {
            Debug.Log($"Matchmaking canceled");

            return;
        }
        
        if (!result.IsError)
        {
            Debug.Log(JsonUtility.ToJson(result.Value));
            if (result.Value.matchFound)
            {
                _sessionId = result.Value.sessionId;
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
                Invoke(nameof(CheckGameSessionDetail),1);
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
        if (_matchCanceled)
        {
            Debug.Log($"Matchmaking canceled");

            return;
        }
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

    private void OnCancelMatchCompleted(Result result, ResultCallback  customCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log($"success delete ticket");
            _matchCanceled = true;
        }
        else
        {
            Debug.Log($"failed delete matchmaking ticket. Message: {result.Error.Message}");
        }
        
        customCallback?.Invoke(result);

    }

    private void LeaveSession()
    {
        _matchmakingV2Session.LeaveGameSession(_sessionId, OnLeaveCompleted);
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
    
    private void DeregisterServer()
    {
        bool isLocal = ConnectionHandler.GetArgument();

        Debug.Log("start DeregisterServer");
        
        if (isLocal)
        {
            Debug.Log("Deregister Local Server to DSM");

            // Deregister Local Server to DSM
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
            Debug.Log("Shutdown Server to DSM");

            // Shutdown Server to DSM
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
}

