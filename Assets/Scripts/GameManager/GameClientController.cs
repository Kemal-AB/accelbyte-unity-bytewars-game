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
        if (IsOwner && IsAlive())
        {
            var power = amount.Get<float>();
            ChangePowerServerRpc(OwnerClientId, power);
            ChangePower(OwnerClientId, power);
        }
    }
    
    public void ChangePower(ulong clientNetworkId, float amount)
    {
        if (GameManager.Instance.InGameState == InGameState.Playing)
        {
            if (GameManager.Instance.Players.TryGetValue(clientNetworkId, out var player))
            {
                player.SetNormalisedPowerChangeSpeed(amount);
                if (amount == 0 && IsServer)
                {
                    SyncPowerClientRpc(clientNetworkId, player.FirePowerLevel);
                }
            }
        }
    }
    
    [ClientRpc]
    private void SyncPowerClientRpc(ulong clientNetworkId, float amount)
    {
        if (GameManager.Instance.Players.TryGetValue(clientNetworkId, out var player))
        {
            player.ChangePowerLevelDirectly(amount);
        }
    }
    
    void OnOpenPauseMenu()
    {
        if (IsOwner && GameManager.Instance.InGameState==InGameState.Playing)
            MenuManager.Instance.ShowInGameMenu(AssetEnum.PauseMenuCanvas);
        
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
        if (GameManager.Instance.InGameState == InGameState.Playing )
        {
            if (GameManager.Instance.Players.TryGetValue(clientNetworkId, out var player))
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
    }

    [ClientRpc]
    private void RotateShipClientRpc(ulong clientNetworkId, float amount)
    {
        RotateShip(clientNetworkId, amount);
    }
    
    [ClientRpc]
    private void SyncShipRotationClientRpc(ulong clientNetworkId, Quaternion rotation)
    {
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
        if (GameManager.Instance.InGameState == InGameState.Playing)
        {
            if (GameManager.Instance.Players.TryGetValue(clientNetworkId, out var player))
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
        if (GameManager.Instance.Players.TryGetValue(clientNetworkId, out var player)
            && GameManager.Instance.ConnectedPlayerStates.TryGetValue(clientNetworkId, out var playerState))
        {
            player.FireMissileClient(missileFireState, playerState);
        }
    }

    public override void OnNetworkSpawn()
    {
        playerInput.enabled = IsOwner;
    }

    private bool IsAlive()
    {
        if (GameManager.Instance.ConnectedPlayerStates.TryGetValue(OwnerClientId, out var playerState))
        {
            return playerState.lives > 0;
        }
        return false;
    }
}
