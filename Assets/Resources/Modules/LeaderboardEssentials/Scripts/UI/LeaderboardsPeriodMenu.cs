using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardsPeriodMenu : MenuCanvas
{
    [SerializeField] private Transform leaderboardListPanel;
    [SerializeField] private Button allTimeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject leaderboardItemButtonPrefab;

    [HideInInspector] public LeaderboardPeriodType chosenPeriod;
    public enum LeaderboardPeriodType
    {
        AllTime,
        Cycle
    }

    [HideInInspector] public string chosenCycleId;

    private PeriodicLeaderboardEssentialsWrapper _periodicLeaderboardWrapper;

    void Start()
    {
        _periodicLeaderboardWrapper = TutorialModuleManager.Instance.GetModuleClass<PeriodicLeaderboardEssentialsWrapper>();
        
        allTimeButton.onClick.AddListener(() => ChangeToIndividualLeaderboardMenu(LeaderboardPeriodType.AllTime));
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void DisplayPeriodList(){
        MenuCanvas leaderboardsMenuCanvas = MenuManager.Instance.GetMenu(AssetEnum.LeaderboardsMenuCanvas);
        LeaderboardsMenu leaderboardsMenuObject = leaderboardsMenuCanvas.GetComponent<LeaderboardsMenu>();
        string[] cycleIds = leaderboardsMenuObject.leaderboardCycleIds[leaderboardsMenuObject.chosenLeaderboardCode];

        foreach (string cycleId in cycleIds)
        {
            _periodicLeaderboardWrapper.GetStatCycleConfig(cycleId, result =>
            {
                if (!result.IsError)
                {
                    Button leaderboardButton = Instantiate(leaderboardItemButtonPrefab, leaderboardListPanel).GetComponent<Button>();
                    TMP_Text leaderboardButtonText = leaderboardButton.GetComponentInChildren<TMP_Text>();
                    leaderboardButtonText.text = result.Value.Name;
                    
                    leaderboardButton.onClick.AddListener(() => ChangeToIndividualLeaderboardMenu(LeaderboardPeriodType.Cycle));
                }
            });
        }
    }

    private void OnGetStatCycleConfigCompleted()
    {
        
    }
    
    private void ChangeToIndividualLeaderboardMenu(LeaderboardPeriodType periodType)
    {
        chosenPeriod = periodType;
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
