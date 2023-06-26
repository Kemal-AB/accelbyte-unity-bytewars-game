using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RankingItemPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text highestScoreText;

    public void ChangePlayerNameText(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void ChangeHighestScoreText(string rankScore)
    {
        highestScoreText.text = rankScore;
    }
}
