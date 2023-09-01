using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardsPeriodMenu : MenuCanvas
{
    [SerializeField] private Transform leaderboardListPanel;
    [SerializeField] private Button allTimeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject leaderboardItemButtonPrefab;
    
    public static LeaderboardPeriodType chosenPeriod;
    public enum LeaderboardPeriodType
    {
        AllTime,
        Cycle
    }
    
    public delegate void LeaderboardsPeriodMenuDelegate(Transform leaderboardListPanel, GameObject leaderboardItemButtonPrefab);
    public static event LeaderboardsPeriodMenuDelegate onLeaderboardsPeriodMenuActivated = delegate { };

    void Start()
    {
        allTimeButton.onClick.AddListener(() => ChangeToIndividualLeaderboardMenu(LeaderboardPeriodType.AllTime));
        backButton.onClick.AddListener(OnBackButtonClicked);
        
        onLeaderboardsPeriodMenuActivated.Invoke(leaderboardListPanel, leaderboardItemButtonPrefab);
    }
    
    public static void ChangeToIndividualLeaderboardMenu(LeaderboardPeriodType periodType)
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
