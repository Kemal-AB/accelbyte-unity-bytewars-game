using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingEntryPanel : MonoBehaviour
{
    [SerializeField] private Image prefabImage;
    [SerializeField] private TMP_Text playerRankText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text highestScoreText;

    public void ChangeAllTextUIs(int playerRank, string playerName, float playerScore)
    {
        playerRankText.text = "#" + playerRank;
        playerNameText.text = playerName;
        highestScoreText.text = playerScore.ToString();
    }

    public void ChangePrefabColor(Color color)
    {
        prefabImage.color = color;
    }
}
