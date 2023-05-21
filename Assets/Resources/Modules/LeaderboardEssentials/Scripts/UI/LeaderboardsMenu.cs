using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardsMenu : MenuCanvas
{
    [SerializeField] private Button singlePlayerButton;
    [SerializeField] private Button eliminationButton;
    [SerializeField] private Button teamDeathmatchButton;
    [SerializeField] private Button backButton;

    private void Start()
    {
        singlePlayerButton.onClick.AddListener(ChangeToLeaderboardsPeriodMenu);
        eliminationButton.onClick.AddListener(ChangeToLeaderboardsPeriodMenu);
        teamDeathmatchButton.onClick.AddListener(ChangeToLeaderboardsPeriodMenu);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void ChangeToLeaderboardsPeriodMenu()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.LeaderboardsPeriodMenuCanvas);
    }
    
    private void OnBackButtonClicked()
    {
        MenuManager.Instance.OnBackPressed();
    }

    public override GameObject GetFirstButton()
    {
        return singlePlayerButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.LeaderboardsMenuCanvas;
    }
}
