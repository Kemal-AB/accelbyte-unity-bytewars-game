using System;
using System.Threading.Tasks;
using AccelByte.Core;
using AccelByte.Models;
using AccelByte.Server;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MatchSessionServerHelper
{
    private static DedicatedServerManager _dedicatedServerManager;
    private static ServerDSHub _serverDSHub;
    private static bool _isGameStarted;
    private static ServerMatchmakingV2 _matchmakingV2Server;
    private const string ClassName = "[MatchSessionServerHelper]";
    private static bool _isMatchMakingEssentialsEnabled;

    private static void Init()
    {
        if (_serverDSHub != null) return;
        _dedicatedServerManager = MultiRegistry.GetServerApiClient().GetDedicatedServerManager();
        _matchmakingV2Server = MultiRegistry.GetServerApiClient().GetMatchmakingV2();
        _serverDSHub = MultiRegistry.GetServerApiClient().GetDsHub();
        _isMatchMakingEssentialsEnabled = TutorialModuleManager.Instance.IsModuleActive(TutorialType.MatchmakingEssentials);
        ListenDsHubNotification();
    }

    private static void ListenDsHubNotification()
    {
        _serverDSHub.OnConnected += () =>
        {
            Debug.Log($"{ClassName} server connected to dedicated server hub");
        };

        _serverDSHub.MatchmakingV2ServerClaimed += (Result<ServerClaimedNotification> result) =>
        {
            if (!result.IsError)
            {
                var serverSession = result.Value.sessionId;
                GameData.ServerSessionID = serverSession;
                Debug.Log($"{ClassName} Server Claimed and Assigned to sessionId = {serverSession}");
            }
        };

        _serverDSHub.MatchmakingV2BackfillProposalReceived += (Result<MatchmakingV2BackfillProposalNotification> result) =>
        {
            if (!result.IsError)
            {
                if (!_isGameStarted)
                {
                    Debug.Log($"_isGameStarted {_isGameStarted}");
                    OnBackFillProposalReceived(result.Value, _isGameStarted );
                    Debug.Log($"{ClassName} Start back-filling process {result.Value.matchSessionId}");

                }
                else
                {
                    OnBackFillProposalRejected(result.Value);
                }
            }
        };
    }
    public static void LoginAndRegisterServer()
    {
        Init();
        if (_isMatchMakingEssentialsEnabled)
        {
            ConnectDSHub();
        }
        else
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
                    ConnectDSHub();
                }
            });
        }
    }

    private static void RegisterServer()
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
    
    public static void UnRegisterServer()
    {
        if (_isMatchMakingEssentialsEnabled) return;
        bool isLocal = ConnectionHandler.GetArgument();
        if (isLocal)
        {
            // Deregister Local Server from AMS
            _dedicatedServerManager.DeregisterLocalServer(result =>
            {
                if (result.IsError)
                {
                    Debug.Log($"{ClassName} Failed Deregister Local Server");
                }
                else
                {
                    Debug.Log($"{ClassName} Successfully Deregister Local Server");
                    #if UNITY_EDITOR
                    EditorApplication.ExitPlaymode();
                    #else
                    Application.Quit();
                    #endif
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
    private static async void ConnectDSHub()
    {
        // server must be registered first to DSMC to have a ServerName
        string serverName = _dedicatedServerManager.ServerName;
        while (String.IsNullOrEmpty(serverName))
        {
            await Task.Delay(1000);
            serverName = _dedicatedServerManager.ServerName;
            Debug.Log($"{ClassName} server name:{serverName}");
        }
        _serverDSHub.Connect(serverName);
    }
    

    private static void OnBackFillProposalReceived(MatchmakingV2BackfillProposalNotification proposal, bool isStopBackfilling)
    {
        _matchmakingV2Server.AcceptBackfillProposal(proposal, isStopBackfilling, result =>
        {
            if (!result.IsError)
            {
                Debug.Log($"{ClassName} Back Fill accepted {!isStopBackfilling}");
            }
        });
    }

    public static void OnBackFillRejected()
    {
        _isGameStarted = true;
    }
    private static void OnBackFillProposalRejected(MatchmakingV2BackfillProposalNotification proposal)
    {
        _matchmakingV2Server.RejectBackfillProposal(proposal, true, result =>
        {
            if (!result.IsError)
            {
                Debug.Log($"{ClassName} Back Fill rejected- Game already started");
            }
        });
    }
}
