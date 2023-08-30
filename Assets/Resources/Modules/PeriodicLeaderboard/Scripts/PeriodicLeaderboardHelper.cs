using System.Collections;
using System.Collections.Generic;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PeriodicLeaderboardHelper : MonoBehaviour
{
    private PeriodicLeaderboardEssentialsWrapper _periodicLeaderboardWrapper;
    private AuthEssentialsWrapper _authWrapper;
    
    private TokenData currentUserData; 
    private string chosenCycleId;
    private const int RESULTOFFSET = 0;
    private const int RESULTLIMIT = 10;
    
    void Start()
    {
        _periodicLeaderboardWrapper = TutorialModuleManager.Instance.GetModuleClass<PeriodicLeaderboardEssentialsWrapper>();
        _authWrapper = TutorialModuleManager.Instance.GetModuleClass<AuthEssentialsWrapper>();
        currentUserData = _authWrapper.userData;
        
        LeaderboardsPeriodMenu.onLeaderboardsPeriodMenuActivated += DisplayCyclePeriodButtons;
        IndividualLeaderboardMenu.onDisplayRankingListEvent += DisplayCycleRankingList;
        IndividualLeaderboardMenu.onDisplayUserRankingEvent += DisplayUserCycleRanking;
    }
    
    private void DisplayCyclePeriodButtons(Transform leaderboardListPanel, GameObject leaderboardItemButtonPrefab){
        string[] cycleIds = LeaderboardsMenu.leaderboardCycleIds[LeaderboardsMenu.chosenLeaderboardCode];

        foreach (string cycleId in cycleIds)
        {
            _periodicLeaderboardWrapper.GetStatCycleConfig(cycleId, result =>
            {
                if (!result.IsError)
                {
                    Button leaderboardButton = Instantiate(leaderboardItemButtonPrefab, leaderboardListPanel).GetComponent<Button>();
                    TMP_Text leaderboardButtonText = leaderboardButton.GetComponentInChildren<TMP_Text>();
                    leaderboardButtonText.text = result.Value.Name;
                    leaderboardButton.onClick.AddListener(() => LeaderboardsPeriodMenu.ChangeToIndividualLeaderboardMenu(LeaderboardsPeriodMenu.LeaderboardPeriodType.Cycle));

                    chosenCycleId = cycleId;
                }
            });
        }
    }

    private void DisplayCycleRankingList(IndividualLeaderboardMenu individualLeaderboardMenu, UserCycleRanking[] userCycleRankings)
    {
        if (LeaderboardsPeriodMenu.chosenPeriod is LeaderboardsPeriodMenu.LeaderboardPeriodType.Cycle)
        {
            _periodicLeaderboardWrapper.GetRankingsByCycle(LeaderboardsMenu.chosenLeaderboardCode, chosenCycleId, individualLeaderboardMenu.OnDisplayRankingListCompleted, RESULTOFFSET, RESULTLIMIT);
        }
    }

    private void DisplayUserCycleRanking(IndividualLeaderboardMenu individualLeaderboardMenu, UserCycleRanking[] userCycleRankings)
    {
        if (LeaderboardsPeriodMenu.chosenPeriod is LeaderboardsPeriodMenu.LeaderboardPeriodType.Cycle)
        {
            foreach (UserCycleRanking cycleRanking in userCycleRankings)
            {
                if (cycleRanking.CycleId == chosenCycleId)
                {
                    individualLeaderboardMenu.InstantiateRankingItem(currentUserData.user_id, cycleRanking.Rank, currentUserData.display_name, cycleRanking.Point);
                    break;
                }
            }
        }
    }
}
