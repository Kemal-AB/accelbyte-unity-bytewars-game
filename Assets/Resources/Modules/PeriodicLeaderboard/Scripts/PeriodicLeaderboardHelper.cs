using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PeriodicLeaderboardHelper : MonoBehaviour
{
    private PeriodicLeaderboardEssentialsWrapper _periodicLeaderboardWrapper;
    
    public static string chosenCycleId;
    
    void Start()
    {
        _periodicLeaderboardWrapper = TutorialModuleManager.Instance.GetModuleClass<PeriodicLeaderboardEssentialsWrapper>();

        LeaderboardsPeriodMenu.onLeaderboardsPeriodMenuActivated += DisplayPeriodList;
    }
    
    private void DisplayPeriodList(Transform leaderboardListPanel, GameObject leaderboardItemButtonPrefab){
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
}
