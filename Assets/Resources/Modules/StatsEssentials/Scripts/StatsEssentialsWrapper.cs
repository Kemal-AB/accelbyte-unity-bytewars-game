using System.Collections;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using AccelByte.Server;
using UnityEngine;

public class StatsEssentialsWrapper : MonoBehaviour
{
    // AccelByte's Multi Registry references
    private Statistic statistic;
    private ServerStatistic serverStatistic;
    
    // Start is called before the first frame update
    void Start()
    {
        statistic = MultiRegistry.GetApiClient().GetStatistic();
        serverStatistic = MultiRegistry.GetServerApiClient().GetStatistic();
    }

    #region AB Service Functions
    
    /// <summary>
    /// Update User Statistics value from Client side
    /// </summary>
    /// <param name="statCode">list of stat codes of the desired stat items</param>
    /// <param name="statValue">desired value for stat item</param>
    /// <param name="resultCallback">callback function to get result from other script</param>
    /// <param name="additionalKey">additional custom key that will be added to the slot</param>
    public void UpdateUserStatsFromClient(string statCode, float statValue, ResultCallback<UpdateUserStatItemValueResponse> resultCallback, string additionalKey = null)
    {
        PublicUpdateUserStatItem userStatItem = new PublicUpdateUserStatItem
        {
            updateStrategy = StatisticUpdateStrategy.OVERRIDE,
            value = statValue
        };
        
        statistic.UpdateUserStatItemsValue(
            statCode,
            additionalKey,
            userStatItem,
            result => OnUpdateUserStatsFromClientCompleted(result, resultCallback)
        );
    }
    
    /// <summary>
    /// Update User Statistics value from Server side
    /// </summary>
    /// <param name="userId">user id of the desired user's stats</param>
    /// <param name="statCode">list of stat codes of the desired stat items</param>
    /// <param name="statValue">desired value for stat item</param>
    /// <param name="resultCallback">callback function to get result from other script</param>
    /// <param name="additionalKey">additional custom key that will be added to the slot</param>
    public void UpdateUserStatsFromServer(string userId, string statCode, float statValue, ResultCallback<StatItemOperationResult[]> resultCallback, string additionalKey = null)
    {
        StatItemUpdate userStatItem = new StatItemUpdate
        {
            statCode = statCode,
            updateStrategy = StatisticUpdateStrategy.OVERRIDE,
            value = statValue
        };
        StatItemUpdate[] bulkUpdateUserStatItems = { userStatItem };
        
        serverStatistic.UpdateUserStatItems(
            userId,
            bulkUpdateUserStatItems,
            result => OnUpdateUserStatsFromServerCompleted(result, resultCallback)
        );
    }
    
    /// <summary>
    /// Get User Statistics from Client side
    /// </summary>
    /// <param name="statCodes">list of stat codes of the desired stat items</param>
    /// <param name="tags">list of custom tags of the desired stat items</param>
    /// <param name="resultCallback">callback function to get result from other script</param>
    public void GetUserStatsFromClient(string[] statCodes, ResultCallback<PagedStatItems> resultCallback, string[] tags = null)
    {
        statistic.GetUserStatItems(
            statCodes, 
            tags, 
            result => OnGetUserStatsFromClientCompleted(result, resultCallback)
        );
    }

    /// <summary>
    /// Get User Statistics from Server side
    /// </summary>
    /// <param name="userId">user id of the desired user's stats</param>
    /// <param name="statCodes">list of stat codes of the desired stat items</param>
    /// <param name="tags">list of custom tags of the desired stat items</param>
    /// <param name="resultCallback">callback function to get result from other script</param>
    public void GetUserStatsFromServer(string userId, string[] statCodes, ResultCallback<PagedStatItems> resultCallback, string[] tags = null)
    {
        serverStatistic.GetUserStatItems(
            userId,
            statCodes,
            tags,
            result => OnGetUserStatItemsFromServerCompleted(result, resultCallback)
        );
    }

    #endregion

    #region Callback Functions

    /// <summary>
    /// Default Callback for Statistic's UpdateUserStatItems() function
    /// </summary>
    /// <param name="result">result of the GetUserStatItems() function call</param>
    /// <param name="customCallback">additional callback function that can be customized from other script</param>
    private void OnUpdateUserStatsFromClientCompleted(Result<UpdateUserStatItemValueResponse> result, ResultCallback<UpdateUserStatItemValueResponse> customCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log("Update User's Stat Items from Client successful.");
        }
        else
        {
            Debug.Log($"Update User's Stat Items from Client failed. Message: {result.Error.Message}");
            
            customCallback?.Invoke(result);
        }
    }

    /// <summary>
    /// Default Callback for ServerStatistic's UpdateUserStatItems() function
    /// </summary>
    /// <param name="result">result of the GetUserStatItems() function call</param>
    /// <param name="customCallback">additional callback function that can be customized from other script</param>
    private void OnUpdateUserStatsFromServerCompleted(Result<StatItemOperationResult[]> result, ResultCallback<StatItemOperationResult[]> customCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log("Update User's Stat Items from Server successful.");
        }
        else
        {
            Debug.Log($"Update User's Stat Items from Server failed. Message: {result.Error.Message}");
        }
        
        customCallback?.Invoke(result);
    }
    
    /// <summary>
    /// Default Callback for Statistic's GetUserStatItems() function
    /// </summary>
    /// <param name="result">result of the GetUserStatItems() function call</param>
    /// <param name="customCallback">additional callback function that can be customized from other script</param>
    private void OnGetUserStatsFromClientCompleted(Result<PagedStatItems> result, ResultCallback<PagedStatItems> customCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log("Get User's Stat Items from Client successful.");
        }
        else
        {
            Debug.Log($"Get User's Stat Items from Client failed. Message: {result.Error.Message}");
        }
        
        customCallback?.Invoke(result);
    }
    
    /// <summary>
    /// Default Callback for ServerStatistic's GetUserStatItems() function
    /// </summary>
    /// <param name="result">result of the GetUserStatItems() function call</param>
    /// <param name="customCallback">additional callback function that can be customized from other script</param>
    private void OnGetUserStatItemsFromServerCompleted(Result<PagedStatItems> result, ResultCallback<PagedStatItems> customCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log("Get User's Stat Items from Server successful.");
        }
        else
        {
            Debug.Log($"Get User's Stat Items from Server failed. Message: {result.Error.Message}");
        }
        
        customCallback?.Invoke(result);
    }

    #endregion
}
