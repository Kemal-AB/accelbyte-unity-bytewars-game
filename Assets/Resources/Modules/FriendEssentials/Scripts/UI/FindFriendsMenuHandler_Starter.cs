using System;
using System.Collections.Generic;
using System.Linq;
using AccelByte.Core;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = System.Reflection.MethodBase;

public class FindFriendsMenuHandler_Starter : MenuCanvas
{
    //Predefined 8a
    [SerializeField] private TMP_InputField friendSearchBar;
    [SerializeField] private RectTransform friendListContent;
    
    //Predefined 8a
    [SerializeField] private RectTransform defaultPanel;
    [SerializeField] private RectTransform loadingPanel;
    [SerializeField] private RectTransform loadingFailedPanel;
    [SerializeField] private RectTransform loadingSuccessPanel;
    [SerializeField] private Button backButton;

    //Predefined 8a
    private List<RectTransform> _panels = new List<RectTransform>();
    private Dictionary<string, RectTransform> _usersResult = new Dictionary<string, RectTransform>();
    
    //copy from Putting It All Together step 1

    
    #region ViewportContent

    //Predefined 8a
    enum FindFriendsView
    {
        Default,
        Loading,
        LoadFailed,
        LoadSuccess
    }
    //Predefined 8a
    private FindFriendsView CurrentView
    {
        get => CurrentView;
        set => ViewSwitcher(value);
    }
    //Predefined 8a
    private void ViewSwitcher(FindFriendsView value)
    {
        switch (value)
        {
            case FindFriendsView.Default:
                SwitcherHelper(defaultPanel);
                break;
            case FindFriendsView.Loading:
                SwitcherHelper(loadingPanel);
                break;
            case FindFriendsView.LoadFailed:
                SwitcherHelper(loadingFailedPanel);
                break;
            case FindFriendsView.LoadSuccess:
                SwitcherHelper(loadingSuccessPanel);
                break;
        }
    }
    //Predefined 8a
    private void SwitcherHelper(Transform panel)
    {
        panel.gameObject.SetActive(true);
        _panels.Except(new []{panel})
            .ToList().ForEach(x => x.gameObject.SetActive(false));
    }

    #endregion
    
    void Start()
    {
        //Predefined 8a
        _panels = new List<RectTransform>()
        {
            defaultPanel, 
            loadingPanel, 
            loadingFailedPanel, 
            loadingSuccessPanel, 
        };
        
        backButton.onClick.AddListener(OnBackButtonClicked);
        //copy from Ready The UI step 1
        //copy from Putting It All Together step 2
    }
    
    #region ButtonAction

    private void OnBackButtonClicked()
    {
        MenuManager.Instance.OnBackPressed();
    }
    
    private void OnDisable()
    {
        ClearSearchPanel();
    }

    /// <summary>
    /// Clean search panel
    /// </summary>
    private void ClearSearchPanel()
    {
        friendSearchBar.text = "";
        GetComponentsInChildren<FriendResultPanelHandler>().ToList().ForEach(x => Destroy(x.gameObject));
        _usersResult.Clear();
    }

    private void OnFriendButtonClicked()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.FindFriendsMenuCanvas);
    }

    #endregion
    
    #region 8a
    

    
    #endregion

    #region MenuCanvasOverride

    public override GameObject GetFirstButton()
    {
        return friendSearchBar.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.FindFriendsMenuCanvas_Starter;
    }

    #endregion

}
