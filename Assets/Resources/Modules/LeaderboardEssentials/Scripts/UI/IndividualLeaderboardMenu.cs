using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class IndividualLeaderboardMenu : MenuCanvas
{
    [SerializeField] private Transform rankingListPanel;
    [SerializeField] private Transform defaultText;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject rankingItemPanelPrefab;

    private string currentUserId;
    private string currentLeaderboardCode;
    private LeaderboardsPeriodMenu.LeaderboardPeriodType currentPeriodType;

    private const string DEFUSERNAME = "PLAYER-";
    private const int RESULTOFFSET = 0;
    private const int RESULTLIMIT = 10;
    
    private LeaderboardEssentialsWrapper _leaderboardWrapper;
    private AuthEssentialsWrapper _authWrapper;
    
    void Start()
    {
        // get leaderboard and auth's wrapper
        _leaderboardWrapper = TutorialModuleManager.Instance.GetModuleClass<LeaderboardEssentialsWrapper>();
        _authWrapper = TutorialModuleManager.Instance.GetModuleClass<AuthEssentialsWrapper>();
        
        backButton.onClick.AddListener(OnBackButtonClicked);

        GetLeaderboardCategoryValues();
        DisplayRankingList();
    }

    private void OnEnable()
    {
        if (_leaderboardWrapper)
        {
            GetLeaderboardCategoryValues();
            DisplayRankingList();
        }
    }

    private void GetLeaderboardCategoryValues()
    {
        currentUserId = MultiRegistry.GetApiClient().session.UserId;
        
        MenuCanvas leaderboardsMenuCanvas = MenuManager.Instance.GetMenu(AssetEnum.LeaderboardsMenuCanvas);
        LeaderboardsMenu leaderboardsMenuObject = leaderboardsMenuCanvas.GetComponent<LeaderboardsMenu>();
        currentLeaderboardCode = leaderboardsMenuObject.chosenLeaderboardCode;

        MenuCanvas leaderboardsPeriodMenuCanvas = MenuManager.Instance.GetMenu(AssetEnum.LeaderboardsPeriodMenuCanvas);
        LeaderboardsPeriodMenu leaderboardsPeriodMenu = leaderboardsPeriodMenuCanvas.GetComponent<LeaderboardsPeriodMenu>();
        currentPeriodType = leaderboardsPeriodMenu.chosenPeriod;
    }
    
    public void DisplayRankingList()
    {
        // ensure the Ranking List Panel children are empty
        LoopThroughTransformAndDestroy(rankingListPanel, defaultText);
        
        if (currentPeriodType is LeaderboardsPeriodMenu.LeaderboardPeriodType.AllTime)
        {
            _leaderboardWrapper.GetRankings(currentLeaderboardCode, OnDisplayRankingListCompleted, RESULTOFFSET, RESULTLIMIT);
        }
    }

    private void OnDisplayRankingListCompleted(Result<LeaderboardRankingResult> result)
    {
        if (!result.IsError)
        {
            // set default text to active (true) if list empty
            defaultText.gameObject.SetActive(result.Value.data.Length <= 0);
            
            // Store the ranking result's userIds and points to a Dictionary
            Dictionary<string, float> userRankInfos = result.Value.data.ToDictionary(userPoint => userPoint.userId, userPoint => userPoint.point);
            
            // Get the players' display name from the provided user ids
            _authWrapper.BulkGetUserInfo(userRankInfos.Keys.ToArray(), authResult => OnBulkGetUserInfoCompleted(authResult, userRankInfos));
            
            if (!userRankInfos.ContainsKey(currentUserId))
            {
                _leaderboardWrapper.GetUserRanking(currentUserId, currentLeaderboardCode, OnGetUserRankingCompleted);
            }
        }
        else
        {
            defaultText.gameObject.SetActive(true);
        }
    }

    private void OnBulkGetUserInfoCompleted(Result<ListBulkUserInfoResponse> result, Dictionary<string, float> userRankInfos)
    {
        // Dict key = userId, value = displayName
        Dictionary<string, string> userDisplayNames = result.Value.data.ToDictionary(userInfo => userInfo.userId, userInfo => userInfo.displayName);
        foreach (string userId in userRankInfos.Keys)
        {
            InstantiateRankingItem(userId, userDisplayNames[userId], userRankInfos[userId]);
        }
    }
    
    private void OnGetUserRankingCompleted(Result<UserRankingDataV3> result)
    {
        if (!result.IsError)
        {
            if (currentPeriodType == LeaderboardsPeriodMenu.LeaderboardPeriodType.AllTime)
            {
                InstantiateRankingItem(result.Value.UserId, _authWrapper.userData.display_name, result.Value.AllTime.point);
            }
        }
    }

    private void InstantiateRankingItem(string userId, string playerName, float playerScore)
    {
        RankingItemPanel itemPanel = Instantiate(rankingItemPanelPrefab, rankingListPanel).GetComponent<RankingItemPanel>();
        itemPanel.ChangeHighestScoreText(playerScore.ToString());

        // If display name not exists, set to default format: "PLAYER-<<5 char of userId>>"
        string displayName = (playerName == "") ? DEFUSERNAME + userId.Substring(0, 5) : playerName;
        itemPanel.ChangePlayerNameText(displayName);
        
        if (userId == currentUserId)
        {
            itemPanel.ChangePrefabColor(Color.gray);
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
