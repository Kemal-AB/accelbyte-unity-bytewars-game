using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;

public class IndividualLeaderboardMenu : MenuCanvas
{
    [SerializeField] private Transform rankingListPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject rankingItemPanelPrefab;
    
    private LeaderboardEssentialsWrapper _leaderboardWrapper;
    private string currentLeaderboardCode;
    private LeaderboardsPeriodMenu.LeaderboardPeriodType currentPeriodType;
    
    void Start()
    {
        // get leaderboard's wrapper
        _leaderboardWrapper = TutorialModuleManager.Instance.GetModuleClass<LeaderboardEssentialsWrapper>();
        
        backButton.onClick.AddListener(OnBackButtonClicked);

        MenuCanvas leaderboardsMenuCanvas = MenuManager.Instance.GetMenu(AssetEnum.LeaderboardsMenuCanvas);
        LeaderboardsMenu leaderboardsMenuObject = leaderboardsMenuCanvas.GetComponent<LeaderboardsMenu>();
        currentLeaderboardCode = leaderboardsMenuObject.chosenLeaderboardCode;

        MenuCanvas leaderboardsPeriodMenuCanvas = MenuManager.Instance.GetMenu(AssetEnum.LeaderboardsPeriodMenuCanvas);
        LeaderboardsPeriodMenu leaderboardsPeriodMenu = leaderboardsPeriodMenuCanvas.GetComponent<LeaderboardsPeriodMenu>();
        currentPeriodType = leaderboardsPeriodMenu.chosenPeriod;
    }

    private void OnEnable()
    {
        if (_leaderboardWrapper)
        {
            DisplayRankingList();
        }
    }

    public void DisplayRankingList()
    {
        // ensure the Ranking List Panel children are empty
        LoopThroughTransformAndDestroy(rankingListPanel);
        
        if (currentPeriodType is LeaderboardsPeriodMenu.LeaderboardPeriodType.AllTime)
        {
            _leaderboardWrapper.GetRankings(currentLeaderboardCode, OnDisplayRankingListCompleted);
        }
    }

    private void OnDisplayRankingListCompleted(Result<LeaderboardRankingResult> result)
    {
        if (!result.IsError)
        {
            foreach (UserPoint userPoint in result.Value.data)
            {
                RankingItemPanel itemPanel = Instantiate(rankingItemPanelPrefab, rankingListPanel).GetComponent<RankingItemPanel>();
                itemPanel.ChangePlayerNameText(userPoint.userId);
                itemPanel.ChangeHighestScoreText(userPoint.point.ToString());
            }
        }
    }
    
    private void OnBackButtonClicked()
    {
        MenuManager.Instance.OnBackPressed();
    }

    public override GameObject GetFirstButton()
    {
        return backButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.IndividualLeaderboardMenuCanvas;
    }
    
    /// <summary>
    /// A utility function to Destroy all Children of the parent transform. Optionally do not remove a specific Transform
    /// </summary>
    /// <param name="parent">Parent Object to destroy children</param>
    /// <param name="doNotRemove">Optional specified Transform that should NOT be destroyed</param>
    private void LoopThroughTransformAndDestroy(Transform parent, Transform doNotRemove = null)
    {
        //Loop through all the children and add them to a List to then be deleted
        List<GameObject> toBeDeleted = new List<GameObject>();

        foreach (Transform t in parent)
        {
            //except the Do Not Remove transform if there is one
            if (t != doNotRemove)
            {
                toBeDeleted.Add(t.gameObject);
            }
        }
        //Loop through list and Delete all Children
        for (int i = 0; i < toBeDeleted.Count; i++)
        {
            Destroy(toBeDeleted[i]);
        }
    }
}
