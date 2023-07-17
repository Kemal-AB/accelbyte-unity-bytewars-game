using System.Collections.Generic;
using System.Linq;
using AccelByte.Core;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class FriendMenuHandler_Starter : MenuCanvas
{
    [SerializeField] private RectTransform defaultPanel;
    [SerializeField] private RectTransform loadingSuccessPanel;
    [SerializeField] private RectTransform loadingFailedPanel;
    [SerializeField] private RectTransform loadingPanel;
    [SerializeField] private Button backButton;
    
    private Dictionary<string, RectTransform> _friendEntries = new Dictionary<string, RectTransform>();
    private List<RectTransform> _panels = new List<RectTransform>();
    

    enum FriendsView
    {
        Default,
        Loading,
        LoadingSuccess,
        LoadingFailed
    }
    
    private FriendsView CurrentView
    {
        get => CurrentView;
        set => ViewSwitcher(value);
    }
    
    private void ViewSwitcher(FriendsView value)
    {
        switch (value)
        {
            case FriendsView.Default:
                SwitcherHelper(defaultPanel);
                break;
            case FriendsView.Loading:
                SwitcherHelper(loadingPanel);
                break;
            case FriendsView.LoadingSuccess:
                SwitcherHelper(loadingSuccessPanel);
                break;
            case FriendsView.LoadingFailed:
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
            loadingSuccessPanel,
            loadingFailedPanel,
            loadingPanel
        };

        backButton.onClick.AddListener(MenuManager.Instance.OnBackPressed);
    }

    #region Predefined Code
    
    private void Awake()
    {
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        ClearFriendList();
    }
    
    private void ClearFriendList()
    {
        foreach (var friendPanelHandler in _friendEntries)
        {
            Destroy(friendPanelHandler.Value.gameObject);
        }
        
        _friendEntries.Clear();
    }

    private void ResultError(string errorMessage)
    {
        CurrentView = FriendsView.LoadingFailed;
        var messageText = loadingFailedPanel.GetChild(0);
        messageText.GetComponent<TMP_Text>().text = errorMessage;
    }

    #endregion

    
    





    public override GameObject GetFirstButton()
    {
        return GameObject.Find("HeaderPanel").gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.FriendMenuCanvas_Starter;
    }
}
