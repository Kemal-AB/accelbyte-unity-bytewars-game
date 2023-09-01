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
    [SerializeField] private GameObject rankingEntryPanelPrefab;
    [SerializeField] private RankingEntryPanel userRankingPanel;

    private TokenData currentUserData;
    private string currentLeaderboardCode;
    private LeaderboardsPeriodMenu.LeaderboardPeriodType currentPeriodType;

    private const string DEFUSERNAME = "PLAYER-";
    private const int RESULTOFFSET = 0;
    private const int RESULTLIMIT = 10;
    
    private LeaderboardEssentialsWrapper _leaderboardWrapper;
    private AuthEssentialsWrapper _authWrapper;
    
    public delegate void IndividualLeaderboardMenuDelegate(IndividualLeaderboardMenu individualLeaderboardMenu, UserCycleRanking[] userCycleRankings = null);
    public static event IndividualLeaderboardMenuDelegate onDisplayRankingListEvent = delegate {};
    public static event IndividualLeaderboardMenuDelegate onDisplayUserRankingEvent = delegate {};
    
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
        currentUserData = _authWrapper.userData;
        currentLeaderboardCode = LeaderboardsMenu.chosenLeaderboardCode;
        currentPeriodType = LeaderboardsPeriodMenu.chosenPeriod;
    }
    
    public void DisplayRankingList()
    {
        // ensure the Ranking List Panel children are empty
        LoopThroughTransformAndDestroy(rankingListPanel, defaultText);
        
        if (currentPeriodType is LeaderboardsPeriodMenu.LeaderboardPeriodType.AllTime)
        {
            _leaderboardWrapper.GetRankings(currentLeaderboardCode, OnDisplayRankingListCompleted, RESULTOFFSET, RESULTLIMIT);
        }
        
        onDisplayRankingListEvent.Invoke(this);
    }

    public void OnDisplayRankingListCompleted(Result<LeaderboardRankingResult> result)
    {
        if (!result.IsError)
        {
            // set default text to active (true) if list empty
            defaultText.gameObject.SetActive(result.Value.data.Length <= 0);
            
            // Store the ranking result's userIds and points to a Dictionary
            Dictionary<string, float> userRankInfos = result.Value.data.ToDictionary(userPoint => userPoint.userId, userPoint => userPoint.point);
            
            // Get the players' display name from the provided user ids
            _authWrapper.BulkGetUserInfo(userRankInfos.Keys.ToArray(), authResult => OnBulkGetUserInfoCompleted(authResult, userRankInfos));
            
            if (!userRankInfos.ContainsKey(currentUserData.user_id))
            {
                _leaderboardWrapper.GetUserRanking(currentUserData.user_id, currentLeaderboardCode, OnGetUserRankingCompleted);
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
        int rankOrder = 0;
        foreach (string userId in userRankInfos.Keys)
        {
            rankOrder += 1;
            InstantiateRankingItem(userId, rankOrder, userDisplayNames[userId], userRankInfos[userId]);
        }
    }
    
    private void OnGetUserRankingCompleted(Result<UserRankingDataV3> result)
    {
        if (!result.IsError)
        {
            if (currentPeriodType == LeaderboardsPeriodMenu.LeaderboardPeriodType.AllTime)
            {
                UserRanking allTimeUserRank = result.Value.AllTime;
                InstantiateRankingItem(result.Value.UserId, allTimeUserRank.rank, currentUserData.display_name, allTimeUserRank.point);
            }
            
            onDisplayUserRankingEvent.Invoke(this, result.Value.Cycles);
        }
    }

    public void InstantiateRankingItem(string userId, int playerRank, string playerName, float playerScore)
    {
        // If display name not exists, set to default format: "PLAYER-<<5 char of userId>>"
        string displayName = (playerName == "") ? DEFUSERNAME + userId.Substring(0, 5) : playerName;

        // update user rank entry panel if player is not in the leaderboard list
        if (playerRank > 10 && userId == currentUserData.user_id)
        {
            userRankingPanel.ChangeAllTextUIs(playerRank, displayName, playerScore);
            return;
        }
        
        RankingEntryPanel itemPanel = Instantiate(rankingEntryPanelPrefab, rankingListPanel).GetComponent<RankingEntryPanel>();
        itemPanel.ChangeAllTextUIs(playerRank, displayName, playerScore);
        if (userId == currentUserData.user_id)
        {
            itemPanel.ChangePanelColor(new Color(1.0f, 1.0f, 1.0f, 0.098f)); //rgba 255,255,255,25
            userRankingPanel.ChangeAllTextUIs(playerRank, displayName, playerScore);
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
    public void LoopThroughTransformAndDestroy(Transform parent, Transform doNotRemove = null)
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
