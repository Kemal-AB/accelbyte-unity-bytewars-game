using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverScreenController : MonoBehaviour
{
    public GameObject m_leaderboardEntryPrefab;
    public GameObject m_leaderboardParentObject;

    public TextMeshProUGUI m_winningPlayerTextUI;

    public void SetPlayers(PlayerController[] playerControllers)
    {
        List<PlayerState> playerStates = new List<PlayerState>();

        foreach( var playerController in playerControllers )
        {
            playerStates.Add(playerController.GetPlayerState());
        }

        playerStates.Sort((a,b)=>{
            return b.m_playerScore.CompareTo(a.m_playerScore);
        });

        SetWinningPlayer(playerStates[0].m_playerName,playerStates[0].m_teamColour);

        foreach( var playerState in playerStates )
        {
            GameObject leaderboardEntryObject = GameObject.Instantiate(m_leaderboardEntryPrefab,Vector3.zero,Quaternion.identity,m_leaderboardParentObject.transform);
            LeaderboardEntryController leaderboardEntryController = leaderboardEntryObject.GetComponent<LeaderboardEntryController>();

            leaderboardEntryController.SetDetails(playerState.m_playerName, playerState.m_teamColour, playerState.m_killCount, (int)playerState.m_playerScore);
        }
    }

    public void SetWinningPlayer(string playerName, Color playerColour)
    {
        m_winningPlayerTextUI.text = playerName;
        m_winningPlayerTextUI.color = playerColour;
    }
}
