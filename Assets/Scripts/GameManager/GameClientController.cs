using System.Linq;
using AccelByte.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameClientController : NetworkBehaviour
{
    private InGameState _serverGameState;
    public bool isGameSceneLoaded;
    [SerializeField] private PlayerInput playerInput;

    public void StartOnlineGame()
    {
        if (IsOwner)
        {
            LoadSceneServerRpc();
        }
    }

    [ServerRpc]
    private void LoadSceneServerRpc()
    {
        GameManager.Instance.StartOnlineGame();
    }

    void OnRotateShip(InputValue amount)
    {
        if (GameManager.Instance.InGameState == InGameState.GameOver)
            return;
        float rotateValue = amount.Get<float>();
        if (IsOwner && IsAlive())
        {
            RotateShipServerRpc(OwnerClientId, rotateValue);
            RotateShip(OwnerClientId, rotateValue);
        }
    }

    void OnFire(InputValue amount)
    {
        
        if (IsOwner && IsAlive())
        {
            FireMissileServerRpc(OwnerClientId);
        }
    }

    void OnChangePower(InputValue amount)
    {
        if (GameManager.Instance.InGameState == InGameState.GameOver)
            return;
        if (IsOwner && IsAlive())
        {
            var power = amount.Get<float>();
            ChangePowerServerRpc(OwnerClientId, power);
            ChangePower(OwnerClientId, power);
        }
    }

    private void ChangePower(ulong clientNetworkId, float amount)
    {
        var game = GameManager.Instance;
        if (game.Players.TryGetValue(clientNetworkId, out var player))
        {
            player.SetNormalisedPowerChangeSpeed(amount);
            if (amount == 0 && IsServer)
            {
                SyncPowerClientRpc(clientNetworkId, player.FirePowerLevel);
            }
        }
    }
    
    [ClientRpc]
    private void SyncPowerClientRpc(ulong clientNetworkId, float amount)
    {
        if (IsHost)
            return;
        if (GameManager.Instance.Players.TryGetValue(clientNetworkId, out var player))
        {
            player.ChangePowerLevelDirectly(amount);
        }
    }
    
    void OnOpenPauseMenu()
    {
        var gameState = GameManager.Instance.InGameState;
        if (IsOwner && 
            gameState is InGameState.Playing or InGameState.ShuttingDown)
        {
            var menuCanvas = MenuManager.Instance.ShowInGameMenu(AssetEnum.PauseMenuCanvas);
            if (menuCanvas is PauseMenuCanvas pauseMenuCanvas)
            {
                pauseMenuCanvas.DisableRestartBtn();
            }
        }
    }

    private bool IsPaused()
    {
        return MenuManager.Instance.GetCurrentMenu().GetAssetEnum() == AssetEnum.PauseMenuCanvas;
    }

    [ServerRpc]
    private void ChangePowerServerRpc(ulong clientNetworkId, float amount)
    {
        ChangePower(clientNetworkId, amount);
    }

    [ServerRpc]
    private void RotateShipServerRpc(ulong clientNetworkId, float amount)
    {
        RotateShip(clientNetworkId, amount);
    }
    private void RotateShip(ulong clientNetworkId, float amount)
    {
        var game = GameManager.Instance;
        if (game.Players.TryGetValue(clientNetworkId, out var player))
        {
            player.SetNormalisedRotateSpeed(amount);
            if (IsServer)
            {
                RotateShipClientRpc(clientNetworkId, amount);
                if (amount == 0)
                {
                    SyncShipRotationClientRpc(clientNetworkId, player.transform.rotation);
                }
            }
        }
    }

    [ClientRpc]
    private void RotateShipClientRpc(ulong clientNetworkId, float amount)
    {
        if (IsHost)
            return;
        RotateShip(clientNetworkId, amount);
    }
    
    [ClientRpc]
    private void SyncShipRotationClientRpc(ulong clientNetworkId, Quaternion rotation)
    {
        if (IsHost)
            return;
        if (GameManager.Instance.Players.TryGetValue(clientNetworkId, out var player))
        {
            player.transform.rotation = rotation;
        }
    }

    [ServerRpc]
    private void FireMissileServerRpc(ulong clientNetworkId)
    {
        FireMissile(clientNetworkId);
    }
    
    private void FireMissile(ulong clientNetworkId)
    {
        var game = GameManager.Instance;
        if (game.InGameState == InGameState.Playing)
        {
            if (game.Players.TryGetValue(clientNetworkId, out var player))
            {
                if (player.PlayerState.lives > 0)
                {
                    var missileState = player.LocalFireMissile();
                    if (missileState!=null && IsServer)
                    {
                        FireMissileClientRpc(clientNetworkId, missileState);
                    }
                }
            }
        }
    }
    
    [ClientRpc]
    private void FireMissileClientRpc(ulong clientNetworkId, MissileFireState missileFireState)
    {
        if (IsHost)
            return;
        if (GameManager.Instance.Players.TryGetValue(clientNetworkId, out var player)
            && GameManager.Instance.ConnectedPlayerStates.TryGetValue(clientNetworkId, out var playerState))
        {
            player.FireMissileClient(missileFireState, playerState);
        }
    }

    public override void OnNetworkSpawn()
    {
        playerInput.enabled = IsOwner;
        if (IsClient)
        {
            //client send user data to server
            if (TutorialModuleUtil.IsAccelbyteSDKInstalled())
            {
                UpdatePlayerStateServerRpc(NetworkManager.Singleton.LocalClientId, 
                GameData.CachedPlayerState, 
                MultiRegistry.GetApiClient().session.UserId);
            }
        }
        // Debug.Log($"GameClientController.OnNetworkSpawn IsServer:{IsServer} IsOwner:{IsOwner} OwnerClientId:{OwnerClientId}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerStateServerRpc(ulong clientNetworkId, PlayerState clientPlayerState, string clientUserId)
    {
        if (GameManager.Instance.ConnectedPlayerStates.TryGetValue(clientNetworkId, out var playerState))
        {
            playerState.avatarUrl = clientPlayerState.avatarUrl;
            playerState.playerName = clientPlayerState.playerName;
            playerState.playerId = clientUserId;//playerState.playerId = clientPlayerState.playerId;
            var g = GameManager.Instance;
            g.UpdatePlayerStatesClientRpc(g.ConnectedTeamStates.Values.ToArray(),
                g.ConnectedPlayerStates.Values.ToArray());
        }
    }
    
    

    private bool IsAlive()
    {
        if (!IsPaused() &&
            GameManager.Instance.ConnectedPlayerStates.TryGetValue(OwnerClientId, out var playerState))
        {
            return playerState.lives > 0;
        }
        return false;
    }
}
