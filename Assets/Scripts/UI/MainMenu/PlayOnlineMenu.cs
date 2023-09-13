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
        browseMatchButton.onClick.AddListener(OnBrowserMatchButtonPressed);
        createMatchButton.onClick.AddListener(OnCreateMatchButtonPressed);
        quickPlayButton.onClick.AddListener(OnQuickPlayButtonPressed);
        backButton.onClick.AddListener(MenuManager.Instance.OnBackPressed);
    }


    private void OnQuickPlayButtonPressed()
    {
        // MenuManager.Instance.ChangeToMenu(AssetEnum.ServerTypeSelection);
        //TODO delete this and uncomment code above to enable peer to peer server selection
        GameData.ServerType = ServerType.OnlineDedicatedServer;
        MenuManager.Instance.ChangeToMenu(AssetEnum.QuickPlayGameMenu);
    }

    public void OnCreateMatchButtonPressed()
    {
       // MenuManager.Instance.ChangeToMenu(MenuManager.MenuEnum.CreateMatchMenuCanvas);

    }

    public void OnBrowserMatchButtonPressed()
    {
        // MenuManager.Instance.ChangeToMenu(MenuManager.MenuEnum.BrowseMatchesMenuCanvas);
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
