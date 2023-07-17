using System;
using System.Collections.Generic;
using System.Linq;
using AccelByte.Core;
using AccelByte.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = System.Reflection.MethodBase;

public class FindFriendsMenuHandler : MenuCanvas
{
    // Start is called before the first frame update
    [SerializeField] private TMP_InputField friendSearchBar;
    [SerializeField] private RectTransform friendListContent;
    
    [SerializeField] private RectTransform defaultPanel;
    [SerializeField] private RectTransform loadingPanel;
    [SerializeField] private RectTransform loadingFailedPanel;
    [SerializeField] private RectTransform loadingSuccessPanel;
    [SerializeField] private Button backButton;

    private List<RectTransform> _panels = new List<RectTransform>();

    private FriendEssentialsWrapper _friendEssentialsWrapper;

    private Dictionary<string, RectTransform> _usersResult = new Dictionary<string, RectTransform>();
    #region ViewportContent

    enum FindFriendsView
    {
        Default,
        Loading,
        LoadFailed,
        LoadSuccess
    }
    
    private FindFriendsView currentView
    {
        get => currentView;
        set => ViewSwitcher(value);
    }

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

    private void SwitcherHelper(Transform panel)
    {
        panel.gameObject.SetActive(true);
        _panels.Except(new []{panel})
            .ToList().ForEach(x => x.gameObject.SetActive(false));
    }

    #endregion
    
    void Start()
    {
        _panels = new List<RectTransform>()
        {
            defaultPanel, 
            loadingPanel, 
            loadingFailedPanel, 
            loadingSuccessPanel, 
        };
        
        backButton.onClick.AddListener(OnBackButtonClicked);
        friendSearchBar.onEndEdit.AddListener(FindFriend);
        _friendEssentialsWrapper = TutorialModuleManager.Instance.GetModuleClass<FriendEssentialsWrapper>();
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
    
    #region Main Function
    /// <summary>
    /// Find user/friend
    /// </summary>
    /// <param name="displayName"></param>
    private void FindFriend(string displayName)
    {
        if (string.IsNullOrEmpty(friendSearchBar.text) || string.IsNullOrEmpty(friendSearchBar.text))
        {
            return;
        }
        
        ClearSearchPanel();
        _friendEssentialsWrapper.GetUserByDisplayName(displayName, OnUsersRetrieved);
    }

    /// <summary>
    /// send friend invitation
    /// </summary>
    /// <param name="userId"></param>
    private void SendFriendInvitation(string userId)
    {
        _friendEssentialsWrapper.SendFriendRequest(userId, result => OnSendRequestComplete(result, userId));
    }
    
    /// <summary>
    /// Get user avatar
    /// </summary>
    /// <param name="userId"></param>
    private void RetrievedUserAvatar(string userId)
    {
        _friendEssentialsWrapper.GetUserAvatar(userId, result => OnGetAvatarComplete(result, userId));
    }
    
    #endregion

    #region Callback

    /// <summary>
    /// FindFriend's callback
    /// </summary>
    /// <param name="result"></param>
    private void OnUsersRetrieved(Result<PagedPublicUsersInfo> result)
    {
        if (result.Value.data.Length > 0)
        {
            //Generate user list along with status and send invitation button
            GenerateFriendResult(result.Value.data.Length, result.Value);
        }
    }
    
    /// <summary>
    /// SendFriendInvitation's callback
    /// </summary>
    /// <param name="result"></param>
    /// <param name="userId"></param>
    private void OnSendRequestComplete(Result result, string userId)
    {
        _usersResult.TryGetValue(userId, out var tempFiendHandler);
        if (!result.IsError)
        {
            if (tempFiendHandler != null)
            {
                tempFiendHandler.GetComponent<FriendResultPanelHandler>().sendInviteButton.interactable = false;
                tempFiendHandler.GetComponent<FriendResultPanelHandler>().sendInviteButton.GetComponentInChildren<TMP_Text>().text = "Request Sent";
            }
        }
    }

    /// <summary>
    /// RetrievedUserAvatar's callback
    /// </summary>
    /// <param name="result"></param>
    /// <param name="userId"></param>
    private void OnGetAvatarComplete(Result<Texture2D> result, string userId)
    {
        if (!result.IsError)
        {
            if (result.Value != null)
            {
                _usersResult.TryGetValue(userId, out var tempFiendHandler);
                if (tempFiendHandler != null)
                {
                    tempFiendHandler.GetComponent<FriendResultPanelHandler>().friendImage.sprite = Sprite.Create(result.Value,
                        new Rect(0f, 0f, result.Value.width, result.Value.height), Vector2.zero);
                }
            }
        }
        else
        {
            Debug.LogWarning($"{result.Error.Message}");
        }
        loadingPanel.gameObject.SetActive(false);
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Generate friend result panel
    /// </summary>
    /// <param name="length"></param>
    /// <param name="publicUsersInfo"></param>
    private void GenerateFriendResult(int length, PagedPublicUsersInfo publicUsersInfo)
    {
        currentView = FindFriendsView.LoadSuccess;
        loadingPanel.gameObject.SetActive(true);
        for (int i = 0; i < length; i++)
        {
            var userId = publicUsersInfo.data[i].userId;
            if (userId == _friendEssentialsWrapper.PlayerUserId)
            {
                Debug.LogWarning(_friendEssentialsWrapper.PlayerUserId);
                continue;
            }
            var resultPanel = Instantiate(loadingSuccessPanel, Vector3.zero, Quaternion.identity, friendListContent);
            resultPanel.name = $"{userId}";
            _usersResult.TryAdd(publicUsersInfo.data[i].userId, resultPanel);
            resultPanel.GetComponent<FriendResultPanelHandler>().friendName.text = publicUsersInfo.data[i].displayName;
            resultPanel.GetComponent<FriendResultPanelHandler>().sendInviteButton.onClick.AddListener(() => SendFriendInvitation(userId));
            CheckFriendshipStatus(userId);
            RetrievedUserAvatar(userId);
        }

        loadingSuccessPanel.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Helper function to get friendship status and Outgoing friend request\
    /// will disable button and change text label
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="resultPanelHandler"></param>
    /// <returns></returns>
    private string CheckFriendStatusAndOutgoingRequest(string userId, FriendResultPanelHandler resultPanelHandler)
    {
        var alreadySendRequest = _friendEssentialsWrapper.CheckOutGoingFriendRequest(userId);
        var alreadyFriend = _friendEssentialsWrapper.IsAlreadyFriend(userId);
        if (alreadySendRequest || alreadyFriend)
        {
            _usersResult.TryGetValue(userId, out var result);
            if (result != null)
            {
                result.GetComponent<FriendResultPanelHandler>().sendInviteButton.interactable = false;
                if (alreadyFriend)
                {
                    return "Already Friend";
                }
                result.GetComponent<FriendResultPanelHandler>().sendInviteButton.GetComponentInChildren<TMP_Text>().text = "Request Sent";
            }
        }
        return "Not a friend";
    }
    
    /// <summary>
    /// Helper function to get friendship status and Outgoing friend request\
    /// will disable button and change text label
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="resultPanelHandler"></param>
    /// <returns></returns>
    private void CheckFriendshipStatus(string userId)
    {

        _friendEssentialsWrapper.GetFriendshipStatus(userId, result => OnGetFriendshipStatusCompleted(userId, result));
    }

    private void OnGetFriendshipStatusCompleted(string userId, Result<FriendshipStatus> result)
    {
        _usersResult.TryGetValue(userId, out var resultPanel);

        if (!result.IsError)
        {
            if (resultPanel != null)
            {
                resultPanel.GetComponent<FriendResultPanelHandler>().friendStatus.text = "Not Friend";

                if (result.Value.friendshipStatus == RelationshipStatusCode.Outgoing)
                {
                    resultPanel.GetComponent<FriendResultPanelHandler>().friendStatus.text = "Waiting for response";
                    resultPanel.GetComponent<FriendResultPanelHandler>().sendInviteButton.GetComponentInChildren<TMP_Text>().text = "Request Sent";
                    resultPanel.GetComponent<FriendResultPanelHandler>().sendInviteButton.interactable = false;
                }

                if (result.Value.friendshipStatus == RelationshipStatusCode.Friend)
                {
                    resultPanel.GetComponent<FriendResultPanelHandler>().friendStatus.text = "Already Friend";
                    resultPanel.GetComponent<FriendResultPanelHandler>().sendInviteButton.interactable = false;
                }

                if (result.Value.friendshipStatus == RelationshipStatusCode.Incoming)
                {
                    resultPanel.GetComponent<FriendResultPanelHandler>().friendStatus.text = "Waiting for your response";
                    resultPanel.GetComponent<FriendResultPanelHandler>().sendInviteButton.interactable = false;
                }
            }
        }
        else
        {
            resultPanel.GetComponent<FriendResultPanelHandler>().friendStatus.text = "Error to get friendship status";
            Debug.LogWarning($"{Logger.GetCurrentMethod()}");
        }
    }

    #endregion
    
    #endregion

    #region MenuCanvasOverride

    public override GameObject GetFirstButton()
    {
        return friendSearchBar.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.FindFriendsMenuCanvas;
    }

    #endregion

}
