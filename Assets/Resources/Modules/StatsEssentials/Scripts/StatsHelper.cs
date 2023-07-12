using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class StatsHelper : MonoBehaviour
{
    // statcodes' name configured in Admin Portal
    private const string SINGLEPLAYER_STATCODE = "unity-highestscore-singleplayer";
    private const string ELIMINATION_STATCODE = "unity-highestscore-elimination";
    private const string TEAMDEATHMATCH_STATCODE = "unity-highestscore-teamdeathmatch";
	
    private StatsEssentialsWrapper _statsWrapper;
    private string currentUserId;
    private string currentStatCode;
    
    // Start is called before the first frame update
    void Start()
    {
        _statsWrapper = TutorialModuleManager.Instance.GetModuleClass<StatsEssentialsWrapper>();

        GameManager.onGameOver += CheckHighestScoreStats;
    }

    #region AB Functions Call
    
    private void CheckHighestScoreStats(GameModeEnum gameMode, InGameMode inGameMode, List<PlayerState> playerStates)
    {
        currentUserId = MultiRegistry.GetApiClient().session.UserId;
        
        #if UNITY_SERVER
            if (inGameMode is InGameMode.OnlineEliminationGameMode)
            {
                currentStatCode = ELIMINATION_STATCODE;
            }
            else if (inGameMode is InGameMode.OnlineDeathMatchGameMode)
            {
                currentStatCode = TEAMDEATHMATCH_STATCODE;
            }

            Dictionary<string, float> userStats = playerStates.ToDictionary(state => state.playerId, state => state.score);

            _statsWrapper.BulkGetUsersStatFromServer(userStats.Keys.ToArray(), currentStatCode, result => OnBulkGetUserStatFromServer(result, userStats));
        #endif
        
        if (gameMode is GameModeEnum.SinglePlayer)
        {
            currentStatCode = SINGLEPLAYER_STATCODE;
            PlayerState playerState = null;
            foreach (PlayerState currentPlayerState in playerStates)
            {
                if (currentPlayerState.playerId == currentUserId)
                {
                    playerState = currentPlayerState;
                }
            }
            _statsWrapper.GetUserStatsFromClient(new string[]{currentStatCode}, null, result => OnGetUserStatsFromClient(result, playerState));
        }
    }

    #endregion

    #region Callback Functions

    private void OnBulkGetUserStatFromServer(Result<FetchUserStatistic> result, Dictionary<string, float> userStats)
    {
        if (!result.IsError)
        {
            Dictionary<string, float> bulkUserStats = result.Value.UserStatistic.ToDictionary(stat => stat.UserId, stat => stat.Value);

            foreach (string userId in userStats.Keys)
            {
                if (bulkUserStats.ContainsKey(userId) && userStats[userId] > bulkUserStats[userId])
                {
                    _statsWrapper.UpdateManyUserStatsFromServer(currentStatCode, userStats, OnUpdateStatsWithServerSdkCompleted);
                }
                else
                {
                    _statsWrapper.UpdateManyUserStatsFromServer(currentStatCode, userStats, OnUpdateStatsWithServerSdkCompleted);
                }
            }
        }
        else
        {
            _statsWrapper.UpdateManyUserStatsFromServer(currentStatCode, userStats, OnUpdateStatsWithServerSdkCompleted);
        }
    }
    
    private void OnGetUserStatsFromClient(Result<PagedStatItems> result, PlayerState playerState)
    {
        // if query success
        if (!result.IsError)
        {
            foreach (StatItem statItem in result.Value.data)
            {
                if (statItem.statCode == currentStatCode && playerState.score > statItem.value)
                {
                    _statsWrapper.UpdateUserStatsFromClient(currentStatCode, playerState.score, "", OnUpdateStatsWithClientSdkCompleted);
                }
            }
        }
        else
        {
            // Create a new stat item since the stat doesn't exist yet
            _statsWrapper.UpdateUserStatsFromClient(currentStatCode, playerState.score, "", OnUpdateStatsWithClientSdkCompleted);
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
