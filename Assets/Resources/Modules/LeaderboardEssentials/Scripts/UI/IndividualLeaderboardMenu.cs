using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndividualLeaderboardMenu : MenuCanvas
{
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject leaderboardItemPrefab;

    void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
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
        return AssetEnum.IndividualLeaderboardMenuCanvas;
    }
}
