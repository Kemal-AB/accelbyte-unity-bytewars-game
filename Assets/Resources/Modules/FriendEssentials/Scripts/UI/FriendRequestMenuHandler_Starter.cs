using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AccelByte.Core;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestMenuHandler_Starter : MenuCanvas
{
    [SerializeField] private RectTransform defaultPanel;
    [SerializeField] private RectTransform loadingSuccessPanel;
    [SerializeField] private RectTransform loadingFailedPanel;
    [SerializeField] private RectTransform loadingPanel;
    [SerializeField] private Button backButton;

    private List<RectTransform> _panels = new List<RectTransform>();
    private Dictionary<string, RectTransform> _friendRequest = new Dictionary<string, RectTransform>();
    
    
    enum FriendRequestsView
    {
        Default,
        Loading,
        LoadingSuccess,
        LoadingFailed
    }
    
    
    private FriendRequestsView CurrentView
    {
        get => CurrentView;
        set => ViewSwitcher(value);
    }
    
    private void ViewSwitcher(FriendRequestsView value)
    {
        switch (value)
        {
            case FriendRequestsView.Default:
                SwitcherHelper(defaultPanel);
                break;
            case FriendRequestsView.Loading:
                SwitcherHelper(loadingPanel);
                break;
            case FriendRequestsView.LoadingSuccess:
                SwitcherHelper(loadingSuccessPanel);
                break;
            case FriendRequestsView.LoadingFailed:
                SwitcherHelper(loadingFailedPanel);
                break;
        }
    }
    
    
    private void SwitcherHelper(RectTransform panel)
    {
        panel.gameObject.SetActive(true);
        _panels.Except(new []{panel})
            .ToList().ForEach(x => x.gameObject.SetActive(false));
    }


    // Start is called before the first frame update
    void Start()
    {
        _panels = new List<RectTransform>()
        {
            defaultPanel,
            loadingPanel,
            loadingSuccessPanel,
            loadingFailedPanel
        };

        backButton.onClick.AddListener(MenuManager.Instance.OnBackPressed);
    }
    
    private void Awake()
    {
        
    }
    
    private void OnEnable()
    {
        
    }
    
    
    private void OnDisable()
    {
        ClearFriendRequestList();
    }
    
    #region 8B
    private void ClearFriendRequestList()
    {
        var tempFriendPanel = new List<GameObject>();
        foreach (var friendPanelHandler in _friendRequest)
        {
            if (friendPanelHandler.Value != null)
            {
                tempFriendPanel.Add(friendPanelHandler.Value.gameObject);
            }
        }
        
        tempFriendPanel.ForEach(Destroy);
        
        _friendRequest.Clear();
    }
    
    
    #endregion

    public override GameObject GetFirstButton()
    {
        return defaultPanel.gameObject;

    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.FriendRequestMenuCanvas_Starter;
    }
}
