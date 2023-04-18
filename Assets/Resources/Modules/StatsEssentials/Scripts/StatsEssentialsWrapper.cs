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
    /// <param name="statCode">stat code of the desired stat items</param>
    /// <param name="statValue">desired value for stat item</param>
    /// <param name="additionalKey">additional custom key that will be added to the slot</param>
    /// <param name="resultCallback">callback function to get result from other script</param>
    public void UpdateUserStatsFromClient(string statCode, float statValue, string additionalKey, ResultCallback<UpdateUserStatItemValueResponse> resultCallback = null)
    {
        PublicUpdateUserStatItem userStatItem = new PublicUpdateUserStatItem
        {
            updateStrategy = StatisticUpdateStrategy.INCREMENT,
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
    /// <param name="newStatItemsValue">dictionary of stat codes along with its new values</param>
    /// <param name="additionalKey">additional custom key that will be added to the slot</param>
    /// /// <param name="resultCallback">callback function to get result from other script</param>
    public void UpdateUserStatsFromServer(string userId, Dictionary<string, float> newStatItemsValue, string additionalKey, ResultCallback<StatItemOperationResult[]> resultCallback)
    {
        List<StatItemUpdate> bulkUpdateUserStatItems = new List<StatItemUpdate>();
        foreach (var newStatItem in newStatItemsValue)
        {
            StatItemUpdate userStatItem = new StatItemUpdate
            {
                statCode = newStatItem.Key,
                updateStrategy = StatisticUpdateStrategy.INCREMENT,
                value = newStatItem.Value
            };
            
            bulkUpdateUserStatItems.Add(userStatItem);
        }
        
        serverStatistic.UpdateUserStatItems(
            userId,
            bulkUpdateUserStatItems.ToArray(),
            result => OnUpdateUserStatsFromServerCompleted(result, resultCallback)
        );
    }
    
    /// <summary>
    /// Get User Statistics from Client side
    /// </summary>
    /// <param name="statCodes">list of stat codes of the desired stat items</param>
    /// <param name="tags">list of custom tags of the desired stat items</param>
    /// <param name="resultCallback">callback function to get result from other script</param>
    public void GetUserStatsFromClient(string[] statCodes, string[] tags, ResultCallback<PagedStatItems> resultCallback)
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
    public void GetUserStatsFromServer(string userId, string[] statCodes, string[] tags, ResultCallback<PagedStatItems> resultCallback)
    {
        serverStatistic.GetUserStatItems(
            userId,
            statCodes,
            tags,
            result => OnGetUserStatItemsFromServerCompleted(result, resultCallback)
        );
    }
    
    /// <summary>
    /// Reset User Stat Item's value from Client side
    /// </summary>
    /// <param name="statCode">stat code of the desired stat items</param>
    /// <param name="additionalKey">additional custom key that will be added to the slot</param>
    /// <param name="resultCallback">callback function to get result from other script</param>
    public void ResetUserStatsFromClient(string statCode, string additionalKey, ResultCallback<UpdateUserStatItemValueResponse> resultCallback = null)
    {
        PublicUpdateUserStatItem userStatItem = new PublicUpdateUserStatItem
        {
            updateStrategy = StatisticUpdateStrategy.OVERRIDE,
            value = 0
        };
        
        statistic.UpdateUserStatItemsValue(
            statCode,
            additionalKey,
            userStatItem,
            result => OnResetUserStatsFromClientCompleted(result, resultCallback)
        );
    }
    
    /// <summary>
    /// Reset User Stat Items values from Server side
    /// </summary>
    /// <param name="userId">user id of the desired user's stats</param>
    /// <param name="statCodes">list of desired stat code</param>
    /// <param name="additionalKey">additional custom key that will be added to the slot</param>
    /// /// <param name="resultCallback">callback function to get result from other script</param>
    public void ResetUserStatsFromServer(string userId, string[] statCodes, string additionalKey, ResultCallback<StatItemOperationResult[]> resultCallback)
    {
        List<StatItemUpdate> bulkUpdateUserStatItems = new List<StatItemUpdate>();
        foreach (string statCode in statCodes)
        {
            StatItemUpdate userStatItem = new StatItemUpdate
            {
                statCode = statCode,
                updateStrategy = StatisticUpdateStrategy.OVERRIDE,
                value = 0
            };
            
            bulkUpdateUserStatItems.Add(userStatItem);
        }
        
        serverStatistic.UpdateUserStatItems(
            userId,
            bulkUpdateUserStatItems.ToArray(),
            result => OnResetUserStatsFromServerCompleted(result, resultCallback)
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
    
    /// <summary>
    /// Default Callback for Reset Stat Items value from Client
    /// </summary>
    /// <param name="result">result of the GetUserStatItems() function call</param>
    /// <param name="customCallback">additional callback function that can be customized from other script</param>
    private void OnResetUserStatsFromClientCompleted(Result<UpdateUserStatItemValueResponse> result, ResultCallback<UpdateUserStatItemValueResponse> customCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log("Reset User Stat Item's value from Client successful.");
        }
        else
        {
            Debug.Log($"Reset User Stat Item's value from Client failed. Message: {result.Error.Message}");
            
            customCallback?.Invoke(result);
        }
    }

    /// <summary>
    /// Default Callback for Reset Stat Items values from Server
    /// </summary>
    /// <param name="result">result of the GetUserStatItems() function call</param>
    /// <param name="customCallback">additional callback function that can be customized from other script</param>
    private void OnResetUserStatsFromServerCompleted(Result<StatItemOperationResult[]> result, ResultCallback<StatItemOperationResult[]> customCallback = null)
    {
        if (!result.IsError)
        {
            Debug.Log("Reset User Stat Item's value from Server successful.");
        }
        else
        {
            Debug.Log($"Reset User Stat Item's value from Server failed. Message: {result.Error.Message}");
        }
        
        customCallback?.Invoke(result);
    }

    #endregion
}
