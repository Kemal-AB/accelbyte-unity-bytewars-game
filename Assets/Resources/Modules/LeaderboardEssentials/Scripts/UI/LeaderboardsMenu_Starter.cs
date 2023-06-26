using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardsMenu_Starter : MenuCanvas
{
    [SerializeField] private Transform leaderboardListPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject leaderboardItemButtonPrefab;

    [HideInInspector]
    public string currentLeaderboardCode;
    
    private LeaderboardEssentialsWrapper _leaderboardWrapper;

    private void Start()
    {
        // get leaderboard's wrapper
        _leaderboardWrapper = TutorialModuleManager.Instance.GetModuleClass<LeaderboardEssentialsWrapper>();
        
        backButton.onClick.AddListener(OnBackButtonClicked);
        
        DisplayLeaderboardList();
    }

    private void DisplayLeaderboardList()
    {
        Debug.Log(_leaderboardWrapper == null);
        _leaderboardWrapper.GetLeaderboardList(OnDisplayLeaderboardListCompleted);
    }

    private void OnDisplayLeaderboardListCompleted(Result<LeaderboardPagedListV3> result)
    {
        if (!result.IsError)
        {
            // ensure the Leaderboard List Panel children are empty
            LoopThroughTransformAndDestroy(leaderboardListPanel);
            
            foreach (LeaderboardDataV3 leaderboardData in result.Value.Data)
            {
                Button leaderboardButton = Instantiate(leaderboardItemButtonPrefab, leaderboardListPanel).GetComponent<Button>();
                TMP_Text leaderboardButtonText = leaderboardButton.GetComponentInChildren<TMP_Text>();
                leaderboardButtonText.text = leaderboardData.Name;

                currentLeaderboardCode = leaderboardData.LeaderboardCode;
                leaderboardButton.onClick.AddListener(ChangeToLeaderboardsPeriodMenu);
            }
        }
    }
    
    private void ChangeToLeaderboardsPeriodMenu()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.LeaderboardsPeriodMenuCanvas);
        Debug.Log($"[LEADERBOARRRRRRRRR] current code: {currentLeaderboardCode}");
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
        return AssetEnum.LeaderboardsMenuCanvas;
    }
    
    /// <summary>
    /// A utility function to Destroy all Children of the parent transform. Optionally do not remove a specific Transform
    /// </summary>
    /// <param name="parent">Parent Object to destroy children</param>
    /// <param name="doNotRemove">Optional specified Transform that should NOT be destroyed</param>
    private void LoopThroughTransformAndDestroy(Transform parent, Transform doNotRemove = null)
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
