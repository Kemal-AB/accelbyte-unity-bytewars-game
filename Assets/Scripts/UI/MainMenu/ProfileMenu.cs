using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileMenu : MenuCanvas
{
    [SerializeField] private Button statsButton;
    [SerializeField] private Button backButton;
    
    // Start is called before the first frame update
    void Start()
    {
        statsButton.onClick.AddListener(OnStatsButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnStatsButtonClicked()
    {
        var statsEssentials = TutorialModuleManager.Instance.GetModule(TutorialType.StatsEssentials);
        if (statsEssentials != null)
        {
            MenuManager.Instance.ChangeToMenu(statsEssentials.mainPrefab.GetAssetEnum());
        }
    }

    private void OnBackButtonClicked()
    {
        MenuManager.Instance.OnBackPressed();
    }

    public override GameObject GetFirstButton()
    {
        return statsButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.ProfileMenuCanvas;
    }
}
