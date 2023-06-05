using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class StatsHelper : MonoBehaviour
{
    // statcodes' name configured in Admin Portal
    private const string SINGLEPLAYER_STATCODE = "highestscore-singleplayer";
    private const string ELIMINATION_STATCODE = "highestscore-elimination";
    private const string TEAMDEATHMATCH_STATCODE = "highestscore-teamdeathmatch";
	
    private StatsEssentialsWrapper _statsWrapper;
    
    // Start is called before the first frame update
    void Start()
    {
        _statsWrapper = TutorialModuleManager.Instance.GetModuleClass<StatsEssentialsWrapper>();

        GameManager.onGameOver += CheckHighestScoreStats;
    }

    #region AB Functions Call
    
    private void CheckHighestScoreStats(GameModeEnum gameMode, List<PlayerState> playerStates)
    {
        string[] statCodes = new[] {SINGLEPLAYER_STATCODE, ELIMINATION_STATCODE, TEAMDEATHMATCH_STATCODE};
        
        _statsWrapper.GetUserStatsFromClient(statCodes, null, result => OnCheckHighestScoreStatsCompleted(result, gameMode, playerStates));
    }

    private void UpdateStatsWithClientSdk(string statCode, PlayerState playerState)
    {
        _statsWrapper.UpdateUserStatsFromClient(statCode, playerState.score, "", OnUpdateStatsWithClientSdkCompleted);
    }
    
    private void UpdateStatsWithServerSdk(string statCode, List<PlayerState> playerStates)
    {
        // update only the current player
        string currentUserId = MultiRegistry.GetApiClient().session.UserId;
        foreach (PlayerState playerState in playerStates)
        {
            if (playerState.playerId == currentUserId)
            {
                Dictionary<string, float> statItems = new Dictionary<string, float>()
                {
                    {statCode, playerState.score}
                };
                _statsWrapper.UpdateUserStatsFromServer(currentUserId, statItems, "", OnUpdateStatsWithServerSdkCompleted);
            }
        }
    }
    
    #endregion

    #region Callback Functions

    private void OnCheckHighestScoreStatsCompleted(Result<PagedStatItems> result, GameModeEnum gameMode, List<PlayerState> playerStates)
    {
        if (!result.IsError)
        {
            Dictionary<string, float> currentHighestScore = new Dictionary<string, float>();
            foreach (StatItem statItem in result.Value.data)
            {
                currentHighestScore.Add(statItem.statCode, statItem.value);
            }
            
            if (gameMode is GameModeEnum.SinglePlayer && playerStates[0].score > currentHighestScore[SINGLEPLAYER_STATCODE])
            {
                UpdateStatsWithClientSdk(SINGLEPLAYER_STATCODE, playerStates[0]);
            }

            if (gameMode is GameModeEnum.OnlineMultiplayer)
            {
                string currentUserId = MultiRegistry.GetApiClient().session.UserId;
                foreach (PlayerState playerState in playerStates)
                {
                    if (playerState.playerId == currentUserId)
                    {
                        UpdateStatsWithServerSdk(ELIMINATION_STATCODE, playerStates);
                    }
                }
            }
        }
    }
    
    private void OnUpdateStatsWithClientSdkCompleted(Result<UpdateUserStatItemValueResponse> result)
    {
        if (!result.IsError)
        {
            Debug.Log("Successfully update stat value with Client SDK!");
        }
    }

    private void OnUpdateStatsWithServerSdkCompleted(Result<StatItemOperationResult[]> result)
    {
        if (!result.IsError)
        {
            Debug.Log("Successfully update stats values with Server SDK!");
        }
    }
    
    #endregion
}
