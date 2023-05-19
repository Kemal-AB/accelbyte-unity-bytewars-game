using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class MatchLobbyMenu : MenuCanvas
{
    [SerializeField] private PlayerEntry[] _playersEntries;
    public Button backButton;
    public Button inviteFriendsButton;
    public Button startButton;
    private ulong _clientNetworkId;
    private Dictionary<int, TeamState> _teamStates;
    private Dictionary<ulong, PlayerState> _playerStates;
    [SerializeField]
    private TextMeshProUGUI statusLabel;
    [SerializeField]
    private GameObject statusContainer;

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
        foreach (var playerEntry in _playersEntries)
        {
            playerEntry.gameObject.SetActive(false);
        }
    }
    public void SpawnPlayer(TeamState teamState, PlayerState playerState, bool isCurrentPlayer)
    {
        foreach (var playerEntry in _playersEntries)
        {
            if (!playerEntry.gameObject.activeSelf)
            {
                playerEntry.Set(teamState, playerState, isCurrentPlayer);
                playerEntry.gameObject.SetActive(true);
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

    private const string CountDownPrefix = "MATCH START IN: ";
    public void Countdown(int second)
    {
        ShowStatus(CountDownPrefix+second);
    }

    public void ShowStatus(string status)
    {
        if(!statusContainer.activeSelf)
            statusContainer.SetActive(true);
        statusLabel.text = status;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.MatchLobbyMenuCanvas;
    }
}
