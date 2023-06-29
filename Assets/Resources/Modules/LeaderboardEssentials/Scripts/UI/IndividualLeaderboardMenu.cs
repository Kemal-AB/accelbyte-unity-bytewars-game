using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class IndividualLeaderboardMenu : MenuCanvas
{
    [SerializeField] private Transform rankingListPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject rankingItemPanelPrefab;
    
    private LeaderboardEssentialsWrapper _leaderboardWrapper;
    private AuthEssentialsWrapper _authWrapper;

    private string currentUserId;
    private string currentLeaderboardCode;
    private LeaderboardsPeriodMenu.LeaderboardPeriodType currentPeriodType;

    private const string DEFUSERNAME = "USER-";
    
    void Start()
    {
        // get leaderboard and auth's wrapper
        _leaderboardWrapper = TutorialModuleManager.Instance.GetModuleClass<LeaderboardEssentialsWrapper>();
        _authWrapper = TutorialModuleManager.Instance.GetModuleClass<AuthEssentialsWrapper>();
        
        backButton.onClick.AddListener(OnBackButtonClicked);

        currentUserId = MultiRegistry.GetApiClient().session.UserId;
        
        MenuCanvas leaderboardsMenuCanvas = MenuManager.Instance.GetMenu(AssetEnum.LeaderboardsMenuCanvas);
        LeaderboardsMenu leaderboardsMenuObject = leaderboardsMenuCanvas.GetComponent<LeaderboardsMenu>();
        currentLeaderboardCode = leaderboardsMenuObject.chosenLeaderboardCode;

        MenuCanvas leaderboardsPeriodMenuCanvas = MenuManager.Instance.GetMenu(AssetEnum.LeaderboardsPeriodMenuCanvas);
        LeaderboardsPeriodMenu leaderboardsPeriodMenu = leaderboardsPeriodMenuCanvas.GetComponent<LeaderboardsPeriodMenu>();
        currentPeriodType = leaderboardsPeriodMenu.chosenPeriod;
        
        DisplayRankingList();
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
            // Store the ranking result's userIds and points to a Dictionary
            Dictionary<string, float> userRankInfos = result.Value.data.ToDictionary(userPoint => userPoint.userId, userPoint => userPoint.point);
            
            // Get the players' display name from the provided user ids
            _authWrapper.BulkGetUserInfo(userRankInfos.Keys.ToArray(), authResult => OnBulkGetUserInfoCompleted(authResult, userRankInfos));
            
            if (userRankInfos.ContainsKey(currentUserId))
            {
                _leaderboardWrapper.GetUserRanking(currentUserId, currentLeaderboardCode, OnGetUserRankingCompleted);
            }
        }
    }

    private void OnBulkGetUserInfoCompleted(Result<ListBulkUserInfoResponse> result, Dictionary<string, float> userRankInfos)
    {
        // Dict key = userId, value = displayName
        Dictionary<string, string> userDisplayNames = new Dictionary<string, string>();
        foreach (BaseUserInfo userInfo in result.Value.data)
        {
            // If display name not exists, set to default format: "USER-<<5 char of userId>>"
            string displayName = (userInfo.displayName == "")? DEFUSERNAME + userInfo.userId.Substring(0,5) : userInfo.displayName;
            userDisplayNames.Add(userInfo.userId, displayName);
        }

        foreach (string userId in userRankInfos.Keys)
        {
            RankingItemPanel itemPanel = Instantiate(rankingItemPanelPrefab, rankingListPanel).GetComponent<RankingItemPanel>();
            itemPanel.ChangePlayerNameText(userDisplayNames[userId]);
            itemPanel.ChangeHighestScoreText(userRankInfos[userId].ToString());
        }
    }
    
    private void OnGetUserRankingCompleted(Result<UserRankingDataV3> result)
    {
        if (!result.IsError)
        {
            if (currentPeriodType == LeaderboardsPeriodMenu.LeaderboardPeriodType.AllTime)
            {
                RankingItemPanel itemPanel = Instantiate(rankingItemPanelPrefab, rankingListPanel).GetComponent<RankingItemPanel>();
                itemPanel.ChangeHighestScoreText(result.Value.AllTime.point.ToString());
                
                // If display name not exists, set to default format: "USER-<<5 char of userId>>"
                string displayName = (_authWrapper.userData.display_name == "")? DEFUSERNAME + currentUserId.Substring(0,5) : _authWrapper.userData.display_name;
                itemPanel.ChangePlayerNameText(displayName);

                Image itemPanelImage = itemPanel.gameObject.GetComponent<Image>();
                itemPanelImage.color = Color.grey;
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
