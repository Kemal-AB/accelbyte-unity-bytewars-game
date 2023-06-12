using System;
using Unity.Netcode;
using UnityEngine;

public class ConnectionHelper 
{
    public ConnectionHelper()
    {
        
    }

    public ConnectionApprovalResult ConnectionApproval(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response, bool isServer, 
        InGameState inGameState, GameModeSO[] availableInGameMode, InGameMode inGameMode,
        ServerHelper serverHelper)
    {
        ConnectionApprovalResult result = null;
        var initialData = GameUtility.FromByteArray<InitialConnectionData>(request.Payload);
        int clientRequestedGameModeIndex = (int)initialData.inGameMode;
        Debug.Log($"ConnectionApprovalCallback IsServer:{isServer} requested game mode:{clientRequestedGameModeIndex} clientNetworkId{request.ClientNetworkId}");
        bool isNewPlayer = String.IsNullOrEmpty(initialData.sessionId);
        if (isNewPlayer && inGameState != InGameState.None)
        {
            string reason = $"game already on {inGameState} player can not join clientNetworkId:{request.ClientNetworkId}";
            RejectConnection(response, reason);
            return null;
        }

        GameModeSO gameModeSo = availableInGameMode[clientRequestedGameModeIndex];
        if (inGameMode==InGameMode.None)
        {
            result = new ConnectionApprovalResult()
            {
                InGameMode = (InGameMode)clientRequestedGameModeIndex,
                GameModeSo = gameModeSo
            };
        }
        else
        {
            InGameMode requestedGameMode = (InGameMode)clientRequestedGameModeIndex;
            if (inGameMode != requestedGameMode)
            {
                string reason = $"Game Mode did not match requested:{requestedGameMode} available:{inGameMode} clientNetworkId:{request.ClientNetworkId}";
                RejectConnection(response, reason);
                return null;
            }
        }
        if (isNewPlayer)
        {
            serverHelper.CreateNewPlayerState(request.ClientNetworkId, gameModeSo);
        }
        else
        {
            //handle reconnection
            if (inGameState != InGameState.GameOver)
            {
                Debug.Log($"player sessionId:{initialData.sessionId} try to reconnect");
                Player player = serverHelper.AddReconnectPlayerState(initialData.sessionId, 
                    request.ClientNetworkId, 
                    availableInGameMode[clientRequestedGameModeIndex]);
                if (player)
                {
                    if (result == null)
                    {
                        result = new ConnectionApprovalResult()
                        {
                            reconnectPlayer = player
                        };
                    }
                    else
                    {
                        result.reconnectPlayer = player;
                    }
                    Debug.Log($"player reconnect success sessionId:{initialData.sessionId}");
                }
                else
                {
                    RejectConnection(response, 
                        $"failed to reconnect, clientNetworkId already claimed by another player, sessionId:{initialData.sessionId} clientNetworkId:{request.ClientNetworkId}");
                    return result;
                }
            }
            else
            {
                RejectConnection(response,
                    $"failed to reconnect game is over, sessionId:{initialData.sessionId}");
                return result;
            }
            

        }
        //TODO verify client against IAM services before approving
        //spawns player controller
        response.CreatePlayerObject = true;
        response.Approved = true;
        response.Pending = false;
        return result;
    }
    
    private void RejectConnection(NetworkManager.ConnectionApprovalResponse response, string reason)
    {
        response.Approved = false;
        response.Pending = false;
        Debug.Log(reason);
        response.Reason = reason;
    }
    
}

public class ConnectionApprovalResult
{
    public InGameMode InGameMode;
    public GameModeSO GameModeSo;
    public Player reconnectPlayer;
}
