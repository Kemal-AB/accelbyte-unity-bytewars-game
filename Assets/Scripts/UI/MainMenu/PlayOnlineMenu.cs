using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayOnlineMenu : MenuCanvas
{
    public Button backButton;
    public Button browseMatchButton;
    public Button createMatchButton;
    public Button quickPlayButton;


    // Start is called before the first frame update
    void Start()
    {
        SetModuleButtonVisibility();
        browseMatchButton.onClick.AddListener(OnBrowserMatchButtonPressed);
        createMatchButton.onClick.AddListener(OnCreateMatchButtonPressed);
        quickPlayButton.onClick.AddListener(OnQuickPlayButtonPressed);
        backButton.onClick.AddListener(MenuManager.Instance.OnBackPressed);
    }

    private void SetModuleButtonVisibility()
    {
        #if !BYTEWARS_DEBUG
        var isQuickPlayBtnActive = TutorialModuleManager.Instance.IsModuleActive(TutorialType.MatchmakingEssentials);
        quickPlayButton.gameObject.SetActive(isQuickPlayBtnActive);
        var isCreateBrowseMatchBtnActive = TutorialModuleManager.Instance.IsModuleActive(TutorialType.MatchSession);
        createMatchButton.gameObject.SetActive(isCreateBrowseMatchBtnActive);
        browseMatchButton.gameObject.SetActive(isCreateBrowseMatchBtnActive);
        #endif
    }


    private void OnQuickPlayButtonPressed()
    {
        // MenuManager.Instance.ChangeToMenu(AssetEnum.ServerTypeSelection);
        //TODO delete this and uncomment code above to enable peer to peer server selection
        GameData.ServerType = ServerType.OnlineDedicatedServer;
        MenuManager.Instance.ChangeToMenu(AssetEnum.QuickPlayMenuCanvas);
    }

    public void OnCreateMatchButtonPressed()
    {
       MenuManager.Instance.ChangeToMenu(AssetEnum.CreateMatchMenuCanvas);

    }

    public void OnBrowserMatchButtonPressed()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.BrowseMatchMenuCanvas);
    }

    public override GameObject GetFirstButton()
    {
        return quickPlayButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.PlayOnlineMenuCanvas;
    }
}
