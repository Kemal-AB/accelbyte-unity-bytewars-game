using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardsPeriodMenu_Starter : MenuCanvas
{
    [SerializeField] private Button allTimeButton;
    [SerializeField] private Button backButton;

    private void OnBackButtonClicked()
    {
        MenuManager.Instance.OnBackPressed();
    }

    public override GameObject GetFirstButton()
    {
        return allTimeButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.LeaderboardsPeriodMenuCanvas;
    }
}
