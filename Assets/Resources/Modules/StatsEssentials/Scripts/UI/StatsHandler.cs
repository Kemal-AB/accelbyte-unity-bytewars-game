using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text singlePlayerText;
    [SerializeField] private TMP_Text eliminationText;
    [SerializeField] private TMP_Text teamDeathmatchText;
    [SerializeField] private Button backButton;
	
    private StatsEssentialsWrapper _statsWrapper;
	
    // statcodes' name configured in Admin Portal
    private const string SINGLEPLAYER_STATCODE = "highestscore-singleplayer";
    private const string ELIMINATION_STATCODE = "highestscore-elimination";
    private const string TEAMDEATHMATCH_STATCODE = "highestscore-teamdeathmatch";
	
    private void Start(){
        // get stats' wrapper
        _statsWrapper = TutorialModuleManager.Instance.GetModuleClass<StatsEssentialsWrapper>();
		
        // UI initialization
        backButton.onClick.AddListener(OnBackButtonClicked);
		
        DisplayStats();
    }

    private void OnEnable()
    {
        if (gameObject.activeSelf && _statsWrapper != null)
        {
            DisplayStats();
        }
    }

    private void DisplayStats()
    {
        // set default values
        singlePlayerText.text = "0";
        eliminationText.text = "0";
        teamDeathmatchText.text = "0";
        
        // trying to get the stats values
        string[] statCodes =
        {
            SINGLEPLAYER_STATCODE,
            ELIMINATION_STATCODE,
            TEAMDEATHMATCH_STATCODE
        };
        _statsWrapper.GetUserStatsFromClient(statCodes, null, OnGetUserStatsCompleted);
    }
	
    private void OnGetUserStatsCompleted(Result<PagedStatItems> result){
        if (!result.IsError){
            foreach (StatItem statItem in result.Value.data)
            {
                Debug.Log("[STATS]" + statItem.statCode + " - " + statItem.value);
                switch (statItem.statCode)
                {
                    case SINGLEPLAYER_STATCODE:
                        singlePlayerText.text = statItem.value.ToString();
                        break;
                    case ELIMINATION_STATCODE:
                        eliminationText.text = statItem.value.ToString();
                        break;
                    case TEAMDEATHMATCH_STATCODE:
                        teamDeathmatchText.text = statItem.value.ToString();
                        break;
                }
            }
        }
    }
	
    private void OnBackButtonClicked(){
        MenuManager.Instance.OnBackPressed();
    }
}
