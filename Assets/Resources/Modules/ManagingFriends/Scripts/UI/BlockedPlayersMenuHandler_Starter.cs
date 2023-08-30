using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AccelByte.Core;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BlockedPlayersMenuHandler_Starter : MenuCanvas
{
    [SerializeField] private RectTransform defaultPanel;
    [SerializeField] private RectTransform loadingSuccessPanel;
    [SerializeField] private RectTransform loadingFailedPanel;
    [SerializeField] private RectTransform loadingPanel;
    [SerializeField] private Button backButton;

    private List<RectTransform> _panels = new List<RectTransform>();
    private Dictionary<string, RectTransform> _blockedPlayers = new Dictionary<string, RectTransform>();
    
    private FriendEssentialsWrapper _friendEssentialsWrapper;


    enum BlockedFriendsView
    {
        Default,
        Loading,
        LoadingSuccess,
        LoadingFailed
    }
    
    
    private BlockedFriendsView CurrentView
    {
        get => CurrentView;
        set => ViewSwitcher(value);
    }
    
    private void ViewSwitcher(BlockedFriendsView value)
    {
        switch (value)
        {
            case BlockedFriendsView.Default:
                SwitcherHelper(defaultPanel);
                break;
            case BlockedFriendsView.Loading:
                SwitcherHelper(loadingPanel);
                break;
            case BlockedFriendsView.LoadingSuccess:
                SwitcherHelper(loadingSuccessPanel);
                break;
            case BlockedFriendsView.LoadingFailed:
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
        FriendEssentialsWrapper.OnIncomingAdded += UpdateFriendList;
    }
    
    private void Awake()
    {
        _friendEssentialsWrapper = TutorialModuleManager.Instance.GetModuleClass<FriendEssentialsWrapper>();
    }

    
    private void OnEnable()
    {
        GetBlockedPlayers();
    }
    
    private void OnDisable()
    {
        ClearBlockedPlayers();
    }
    
    //Put your code here
    private void GetBlockedPlayers()
    {
        Debug.LogWarning($"Get blocked player list is not yet implemented.");
    }
    
    #region Predefined Managing Friends
    
    private void UpdateFriendList()
    {
        if (transform.gameObject.activeSelf)
        {
            ClearBlockedPlayers();
            GetBlockedPlayers();
        }
    }
    
    private void ClearBlockedPlayers()
    {
        var tempFriendPanel = new List<GameObject>();
        foreach (var friendPanelHandler in _blockedPlayers)
        {
            if (friendPanelHandler.Value != null)
            {
                tempFriendPanel.Add(friendPanelHandler.Value.gameObject);
            }
        }
        
        tempFriendPanel.ForEach(Destroy);
        
        _blockedPlayers.Clear();
    }
    

    
    private void GenerateEntryResult(ListBulkUserInfoResponse friends)
    {
        var friendRequestComponent = loadingSuccessPanel.GetChild(0);
        Debug.Log(friendRequestComponent.name);
        foreach (var baseUserInfo in friends.data)
        {
            var resultPanel = Instantiate(friendRequestComponent, Vector3.zero, Quaternion.identity, loadingSuccessPanel);
            resultPanel.gameObject.SetActive(true);
            resultPanel.gameObject.name = baseUserInfo.userId;
            var friendEntryComponent = resultPanel.GetComponentInChildren<BlockedFriendEntryHandler>();
            friendEntryComponent.friendName.text = String.IsNullOrEmpty(baseUserInfo.displayName) ? "Bytewars Player Headless" : baseUserInfo.displayName;
            friendEntryComponent.unblockButton.onClick.AddListener(() =>
            {
                OnUnblockFriend(baseUserInfo.userId);
            });
            _blockedPlayers.TryAdd(baseUserInfo.userId, (RectTransform)resultPanel);
            RetrieveAvatar(baseUserInfo.userId);
        }
    }
    
    private void OnUnblockFriend(string userId)
    {
    }

    private void OnUnblockedCompleted(string userId, Result<UnblockPlayerResponse> result)
    {
        if (!result.IsError)
        {
            var target = GameObject.Find(userId);
            Destroy(target);
        }
    }

    private void UserInfo(BlockedList friends)
    {
        var blockedUsersId = friends.data.Select(x => x.blockedUserId).ToArray();
        _friendEssentialsWrapper.GetBulkUserInfo(blockedUsersId, OnGetBulkUserInfoCompleted);
    }

    private void OnGetBulkUserInfoCompleted(Result<ListBulkUserInfoResponse> result)
    {
        if (!result.IsError)
        {
            GenerateEntryResult(result.Value);
        }
    }

    private void RetrieveAvatar(string userId)
    {
        loadingPanel.gameObject.SetActive(true);
        _friendEssentialsWrapper.GetUserAvatar(userId, result => OnGetAvatarCompleted(userId, result));
    }
    
    private void OnGetAvatarCompleted(string userId, Result<Texture2D> result)
    {
        if (!result.IsError)
        {
            var blockedPlayerEntry = GameObject.Find(userId);
            blockedPlayerEntry.GetComponent<BlockedFriendEntryHandler>().friendImage.sprite = Sprite.Create(result.Value,
                new Rect(0f, 0f, result.Value.width, result.Value.height), Vector2.zero);
        }
        loadingPanel.gameObject.SetActive(false);
    }
    
    #endregion

    public override GameObject GetFirstButton()
    {
        return defaultPanel.gameObject;

    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.BlockedPlayersMenuCanvas_Starter;
    }
}
