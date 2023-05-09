using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class MatchLobbyMenu : MenuCanvas
{
    [SerializeField] private PlayerLobby[] _playersLobby;
    public Button backButton;
    public Button inviteFriendsButton;
    public Button startButton;
    private ulong _clientNetworkId;
    private Dictionary<int, TeamState> _teamStates;
    private Dictionary<ulong, PlayerState> _playerStates;
    [SerializeField]
    private TextMeshProUGUI countdownLabel;

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObj)
        {
            var gameController = playerObj.GetComponent<GameClientController>();
            if (gameController)
            {
                gameController.StartOnlineGame();
            }
        }
    }

    private void Init()
    {
        foreach (var playerLobby in _playersLobby)
        {
            playerLobby.gameObject.SetActive(false);
        }
    }
    public void SpawnPlayer(TeamState teamState, PlayerState playerState, bool isCurrentPlayer)
    {
        foreach (var playerLobby in _playersLobby)
        {
            if (!playerLobby.gameObject.activeSelf)
            {
                playerLobby.Set(teamState, playerState, isCurrentPlayer);
                playerLobby.gameObject.SetActive(true);
                break;
            }
        }
    }

    public override GameObject GetFirstButton()
    {
        return startButton.gameObject;
    }

    private void OnEnable()
    {
        _clientNetworkId = GameManager.Instance.ClientNetworkId;
        _playerStates = GameManager.Instance.ConnectedPlayerStates;
        _teamStates = GameManager.Instance.ConnectedTeamStates;
        Refresh(_teamStates, _playerStates, _clientNetworkId);
    }

    public void Refresh(Dictionary<int, TeamState> teamStates, Dictionary<ulong, PlayerState> playerStates, 
        ulong clientNetworkId)
    {
        Init();
        foreach (var kvp in playerStates)
        {
            var playerState = kvp.Value;
            SpawnPlayer(teamStates[playerState.teamIndex], playerState, playerState.clientNetworkId==clientNetworkId);
        }
    }

    public void Countdown(int second)
    {
        countdownLabel.text = second.ToString();
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.MatchLobbyMenuCanvas;
    }
}
