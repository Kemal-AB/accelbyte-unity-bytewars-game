using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingItemPanel : MonoBehaviour
{
    [SerializeField] private Image prefabImage;
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

    public void ChangePrefabColor(Color color)
    {
        prefabImage.color = color;
    }
}
