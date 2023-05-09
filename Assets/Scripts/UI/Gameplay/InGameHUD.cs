using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class InGameHUD : MonoBehaviour
{
    [SerializeField] private PlayerHUD[] _playerHUDs;
    [SerializeField] private TMPro.TextMeshProUGUI _timeLabel;
    public TextMeshProUGUI gameStatusText;
    public RectTransform gameStatusContainer;

    public void SetTime(int timeInSecond)
    {
        _timeLabel.text = timeInSecond.ToString();
    }

    public void Init(TeamState[] teamStates, PlayerState[] playerStates)
    {
        for (int i = 0; i < teamStates.Length; i++)
        {
            var teamState = teamStates[i];
            var hud = _playerHUDs[teamState.teamIndex];
            hud.Init(teamState, playerStates);
        }
    }

    public void UpdateKillsAndScore(PlayerState playerState, PlayerState[] players)
    {
        var hud = _playerHUDs[playerState.teamIndex];
        float score = 0;
        int killCount = 0;
        foreach (var pState in players)
        {
            if (pState.teamIndex == playerState.teamIndex)
            {
                score += pState.score;
                killCount += pState.killCount;
            }
        }
        hud.SetKillsValue(killCount);
        hud.SetScoreValue((int)score);
    }

    public void SetLivesValue(int teamIndex, int lives)
    {
        _playerHUDs[teamIndex].SetLivesValue(lives);
    }

    public void Reset()
    {
        _timeLabel.text = "0";
        foreach (var playerHUD in _playerHUDs)
        {
            playerHUD.Reset();
        }
        gameStatusContainer.gameObject.SetActive(false);
    }

    readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);
    

    public void UpdatePreGameCountdown(int second)
    {
        if(!gameStatusContainer.gameObject.activeSelf)
            gameStatusContainer.gameObject.SetActive(true);
        if (second == 0)
        {
            gameStatusText.text = "Game Started";
            gameStatusContainer.gameObject.SetActive(false);
            // LeanTween.alpha(gameStatusContainer, 0, 1)
            //     .setOnComplete(OnGameStatusContainerFullyHidden);
        }
        else
        {
            gameStatusText.text = second.ToString();
        }
    }
    
    public void UpdateShutdownCountdown(string prefix, int second)
    {
        if(!gameStatusContainer.gameObject.activeSelf)
            gameStatusContainer.gameObject.SetActive(true);
        gameStatusText.text = prefix+second;
        if (second == 0)
        {
            gameStatusContainer.gameObject.SetActive(false);
        }
    }

    private void OnGameStatusContainerFullyHidden()
    {
        gameStatusContainer.gameObject.SetActive(false);
    }
}
