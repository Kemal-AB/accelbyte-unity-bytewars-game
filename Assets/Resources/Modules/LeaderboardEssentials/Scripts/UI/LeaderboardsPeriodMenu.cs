using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardsPeriodMenu : MenuCanvas
{
    [SerializeField] private Button allTimeButton;
    [SerializeField] private Button backButton;

    [HideInInspector] public LeaderboardPeriodType chosenPeriod;
    public enum LeaderboardPeriodType
    {
        AllTime
    }

    void Start()
    {
        allTimeButton.onClick.AddListener(ChangeToIndividualLeaderboardMenu);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void ChangeToIndividualLeaderboardMenu()
    {
        chosenPeriod = LeaderboardPeriodType.AllTime;
        MenuManager.Instance.ChangeToMenu(AssetEnum.IndividualLeaderboardMenuCanvas);
    }
    
    private void OnBackButtonClicked()
    {
        MenuManager.Instance.OnBackPressed();
    }

    public override GameObject GetFirstButton()
    {
        return allTimeButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.LeaderboardsPeriodMenuCanvas;
    }
}
