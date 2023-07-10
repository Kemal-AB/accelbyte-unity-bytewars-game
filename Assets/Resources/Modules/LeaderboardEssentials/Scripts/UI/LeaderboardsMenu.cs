using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardsMenu : MenuCanvas
{
    [SerializeField] private Transform leaderboardListPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject leaderboardItemButtonPrefab;

    [HideInInspector] public string chosenLeaderboardCode;
    
    private LeaderboardEssentialsWrapper _leaderboardWrapper;

    void Start()
    {
        // get leaderboard's wrapper
        _leaderboardWrapper = TutorialModuleManager.Instance.GetModuleClass<LeaderboardEssentialsWrapper>();
        
        backButton.onClick.AddListener(OnBackButtonClicked);
        
        DisplayLeaderboardList();
    }

    private void OnEnable()
    {
        if (_leaderboardWrapper)
        {
            DisplayLeaderboardList();
        }
    }

    private void DisplayLeaderboardList()
    {
        // ensure the Leaderboard List Panel children are empty
        LoopThroughTransformAndDestroy(leaderboardListPanel);
        
        _leaderboardWrapper.GetLeaderboardList(OnDisplayLeaderboardListCompleted);
    }

    private void OnDisplayLeaderboardListCompleted(Result<LeaderboardPagedListV3> result)
    {
        if (!result.IsError)
        {
            foreach (LeaderboardDataV3 leaderboardData in result.Value.Data)
            {
                if (leaderboardData.Name.Contains("Unity") && leaderboardData.LeaderboardCode != "board-unity-highestscore-singleplayer")
                {
                    Button leaderboardButton = Instantiate(leaderboardItemButtonPrefab, leaderboardListPanel).GetComponent<Button>();
                    TMP_Text leaderboardButtonText = leaderboardButton.GetComponentInChildren<TMP_Text>();
                    leaderboardButtonText.text = leaderboardData.Name.Replace("Unity Leaderboard ", "");
                    
                    leaderboardButton.onClick.AddListener(() => ChangeToLeaderboardsPeriodMenu(leaderboardData.LeaderboardCode));
                }
            }
        }
    }
    
    private void ChangeToLeaderboardsPeriodMenu(string newLeaderboardCode)
    {
        chosenLeaderboardCode = newLeaderboardCode;
        MenuManager.Instance.ChangeToMenu(AssetEnum.LeaderboardsPeriodMenuCanvas);
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
