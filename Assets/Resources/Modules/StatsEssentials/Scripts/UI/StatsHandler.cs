using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsHandler : MenuCanvas
{
    [SerializeField] private TMP_Text singlePlayerStatValueText;
    [SerializeField] private TMP_Text eliminationStatValueText;
    [SerializeField] private TMP_Text teamDeathmatchStatValueText;
    [SerializeField] private Button backButton;

    // statcodes' name configured in Admin Portal
    private const string SINGLEPLAYER_STATCODE = "highestscore-singleplayer";
    private const string ELIMINATION_STATCODE = "highestscore-elimination";
    private const string TEAMDEATHMATCH_STATCODE = "highestscore-teamdeathmatch";
	
    private StatsEssentialsWrapper _statsWrapper;
    
    // Start is called before the first frame update
    void Start()
    {
        // get stats' wrapper
        _statsWrapper = TutorialModuleManager.Instance.GetModuleClass<StatsEssentialsWrapper>();
        
        // uncomment to Reset Stat Item's value
        // _statsWrapper.ResetUserStatsFromClient(SINGLEPLAYER_STATCODE, null, null);

        // UI initialization
        backButton.onClick.AddListener(OnBackButtonClicked);
		
        DisplayStats();
    }

    void OnEnable()
    {
        if (gameObject.activeSelf && _statsWrapper != null)
        {
            DisplayStats();
        }

        // GameOverScreenController.OnGameOver += playerState =>
        // {
        //     UpdateUserStats(playerState);
        // };
        // InGameGameMode.OnGameOverState += () =>
        // {
        //     UpdateAllUsersStats();
        // };
    }

    private void DisplayStats()
    {
        // set default values
        singlePlayerStatValueText.text = "0";
        eliminationStatValueText.text = "0";
        teamDeathmatchStatValueText.text = "0";
        
        // trying to get the stats values
        string[] statCodes =
        {
            SINGLEPLAYER_STATCODE,
            ELIMINATION_STATCODE,
            TEAMDEATHMATCH_STATCODE
        };
        _statsWrapper.GetUserStatsFromClient(statCodes, null, OnGetUserStatsCompleted);
    }
	
    private void UpdateUserStats(PlayerState playerState)
    {
        StatsEssentialsWrapper statsEssentialsWrapper = TutorialModuleManager.Instance.GetModuleClass<StatsEssentialsWrapper>();
        statsEssentialsWrapper.UpdateUserStatsFromClient(SINGLEPLAYER_STATCODE, playerState.score, "", OnUpdateUserStatsCompleted);
    }
    
    private void UpdateAllUsersStats()
    {
        // update statistic from server
        // if (GameDirector.Instance.GameMode == GameDirector.E_GameMode.MULTI_PLAYER)
        // {
        //     if (GameDirector.Instance.GameMode == GameDirector.E_GameMode.ELIMINATION)
        //     {
        //         UpdateUserStats(ELIMINATION_STATCODE);
        //     }
        //     else if (GameDirector.Instance.GameMode == GameDirector.E_GameMode.TEAMDEATHMATCH)
        //     {
        //         UpdateUserStats(TEAMDEATHMATCH_STATCODE);
        //     }
        // }
        
        // foreach (var player in GetGameState().m_players)
        // {
        //     PlayerState playerState = player.GetPlayerState();
        //     Dictionary<string, float> statItems = new Dictionary<string, float>()
        //     {
        //         {multiplayerMode, playerState.m_playerScore}
        //     };
        //     
        //     statsEssentialsWrapper.UpdateUserStatsFromServer(player.userId, statItems, "", null);
        // }
    }
    
    private void OnGetUserStatsCompleted(Result<PagedStatItems> result)
    {
        if (!result.IsError){
            foreach (StatItem statItem in result.Value.data)
            {
                Debug.Log("[STATS]" + statItem.statCode + " - " + statItem.value);
                switch (statItem.statCode)
                {
                    case SINGLEPLAYER_STATCODE:
                        singlePlayerStatValueText.text = statItem.value.ToString();
                        break;
                    case ELIMINATION_STATCODE:
                        eliminationStatValueText.text = statItem.value.ToString();
                        break;
                    case TEAMDEATHMATCH_STATCODE:
                        teamDeathmatchStatValueText.text = statItem.value.ToString();
                        break;
                }
            }
        }
    }

    private void OnUpdateUserStatsCompleted(Result<UpdateUserStatItemValueResponse> result)
    {
        if (!result.IsError)
        {
            Debug.Log($"Player's {SINGLEPLAYER_STATCODE} stat value updated!");
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
        return AssetEnum.StatsProfileMenuCanvas;
    }
}
