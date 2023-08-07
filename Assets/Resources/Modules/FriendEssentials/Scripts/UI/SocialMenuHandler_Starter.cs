using UnityEngine;
using UnityEngine.UI;

public class SocialMenuHandler_Starter : MenuCanvas
{
    [SerializeField] private Button findFriendsButton;
    [SerializeField] private Button friendsButton;
    [SerializeField] private Button friendRequestsButton;
    [SerializeField] private Button sendFriendRequestsButton;
    [SerializeField] private Button blockedPlayersButton;
    [SerializeField] private Button backButton;
    
    void Start()
    {
        EnableButton(blockedPlayersButton, TutorialType.ManagingFriends);

        findFriendsButton.onClick.AddListener(OnFindFriendButtonClicked);
        friendsButton.onClick.AddListener(OnFriendsButtonClicked);
        friendRequestsButton.onClick.AddListener(OnFriendRequestClicked);
        sendFriendRequestsButton.onClick.AddListener(OnSentFriendRequestClicked);
        blockedPlayersButton.onClick.AddListener(OnBlockedPlayersClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }
    
    private void EnableButton(Button button, TutorialType tutorialType)
    {
        var module = TutorialModuleManager.Instance.GetModule(tutorialType);
        if (module.isActive)
        {
            button.gameObject.SetActive(true);
        }
    }
    
    private void OnSentFriendRequestClicked()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.SentFriendRequestMenuCanvas_Starter);
    }

    private void OnFriendRequestClicked()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.FriendRequestMenuCanvas_Starter);
    }

    #region ButtonAction

    private void OnBackButtonClicked()
    {
        MenuManager.Instance.OnBackPressed();
    }

    private void OnFindFriendButtonClicked()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.FindFriendsMenuCanvas_Starter);
    }

    private void OnFriendsButtonClicked()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.FriendMenuCanvas_Starter);
    }
    
    private void OnBlockedPlayersClicked()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.BlockedPlayersMenuCanvas_Starter);
    }

    #endregion

    

    #region MenuCanvasOverride

    public override GameObject GetFirstButton()
    {
        return findFriendsButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.SocialMenuCanvas_Starter;
    }

    #endregion

}
