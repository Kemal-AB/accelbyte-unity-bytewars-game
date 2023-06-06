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
    
    private void CheckHighestScoreStats(GameModeEnum gameMode, InGameMode inGameMode, List<PlayerState> playerStates)
    {
        string[] statCodes = new[] {SINGLEPLAYER_STATCODE, ELIMINATION_STATCODE, TEAMDEATHMATCH_STATCODE};
        
        _statsWrapper.GetUserStatsFromClient(statCodes, null, result => OnCheckHighestScoreStatsCompleted(result, gameMode, inGameMode, playerStates));
    }

    private void UpdateStatsWithClientSdk(string statCode, PlayerState playerState)
    {
        _statsWrapper.UpdateUserStatsFromClient(statCode, playerState.score, "", OnUpdateStatsWithClientSdkCompleted);
    }
    
    private void UpdateStatsWithServerSdk(string statCode, PlayerState playerState)
    {
        // update only the current player
        string currentUserId = MultiRegistry.GetApiClient().session.UserId;
        Dictionary<string, float> statItems = new Dictionary<string, float>()
        {
            {statCode, playerState.score}
        };
        _statsWrapper.UpdateUserStatsFromServer(currentUserId, statItems, "", OnUpdateStatsWithServerSdkCompleted);
    }
    
    #endregion

    #region Callback Functions

    private void OnCheckHighestScoreStatsCompleted(Result<PagedStatItems> result, GameModeEnum gameMode, InGameMode inGameMode, List<PlayerState> playerStates)
    {
        Dictionary<string, float> currentHighestScore = new Dictionary<string, float>();
        string currentUserId = MultiRegistry.GetApiClient().session.UserId;
        string currentStatCode = "";

        // set stat code based on the current Game Mode
        if (gameMode is GameModeEnum.SinglePlayer)
        {
            currentStatCode = SINGLEPLAYER_STATCODE;
        }
        else if (inGameMode is InGameMode.OnlineEliminationGameMode)
        {
            currentStatCode = ELIMINATION_STATCODE;
        }
        else if (inGameMode is InGameMode.OnlineDeathMatchGameMode)
        {
            currentStatCode = TEAMDEATHMATCH_STATCODE;
        }

        // if query success
        if (!result.IsError)
        {
            foreach (StatItem statItem in result.Value.data)
            {
                currentHighestScore.Add(statItem.statCode, statItem.value);
            }
            
            // Only update stats if the score is higher than the current highest score stat
            foreach (PlayerState playerState in playerStates)
            {
                if (playerState.playerId == currentUserId && playerState.score > currentHighestScore[currentStatCode])
                {
                    UpdateStatSDKUsageChecker(gameMode, currentStatCode, playerState);
                }
            }
        }
        else
        {
            // Create a new stat item since the stat doesn't exist yet
            foreach (PlayerState playerState in playerStates)
            {
                if (playerState.playerId == currentUserId)
                {
                    UpdateStatSDKUsageChecker(gameMode, currentStatCode, playerState);
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

    private void UpdateStatSDKUsageChecker(GameModeEnum gameMode, string currentStatCode, PlayerState playerState)
    {
        if (gameMode is GameModeEnum.SinglePlayer)
        {
            UpdateStatsWithClientSdk(currentStatCode, playerState);
        }
        if (gameMode is GameModeEnum.OnlineMultiplayer)
        {
            UpdateStatsWithServerSdk(currentStatCode, playerState);
        }
    }
}
