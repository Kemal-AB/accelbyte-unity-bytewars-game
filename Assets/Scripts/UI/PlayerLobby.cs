using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobby : MonoBehaviour
{
    [SerializeField] private Image shipAvatar;

    [SerializeField] private TextMeshProUGUI shipName;

    public void Set(TeamState teamState, PlayerState playerState, bool isCurrentPlayer)
    {
        shipAvatar.color = teamState.teamColour;
        shipName.color = teamState.teamColour;
        if (isCurrentPlayer)
        {
            shipName.text = "You: "+playerState.playerName;
        }
        else
        {
            shipName.text = playerState.playerName;
        }
    }

}
