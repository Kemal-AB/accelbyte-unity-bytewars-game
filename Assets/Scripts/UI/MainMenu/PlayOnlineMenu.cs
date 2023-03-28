using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayOnlineMenu : MonoBehaviour
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
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnQuickPlayButtonPressed()
    {
        // MenuManager.Instance.ChangeToMenu(MenuManager.MenuEnum.QuickPlayMenuCanvas);
    }

    public void OnCreateMatchButtonPressed()
    {
       // MenuManager.Instance.ChangeToMenu(MenuManager.MenuEnum.CreateMatchMenuCanvas);

    }

    public void OnBrowserMatchButtonPressed()
    {
        // MenuManager.Instance.ChangeToMenu(MenuManager.MenuEnum.BrowseMatchesMenuCanvas);
    }

}
