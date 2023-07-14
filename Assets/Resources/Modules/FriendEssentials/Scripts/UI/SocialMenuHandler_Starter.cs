using UnityEngine;
using UnityEngine.UI;

public class SocialMenuHandler_Starter : MenuCanvas
{
    [SerializeField] private Button findFriendsButton;
    [SerializeField] private Button friendsButton;
    [SerializeField] private Button friendRequestsButton;
    [SerializeField] private Button sendFriendRequestsButton;
    [SerializeField] private Button backButton;
    
    void Start()
    {
        findFriendsButton.onClick.AddListener(OnFindFriendButtonClicked);
        friendsButton.onClick.AddListener(OnFriendsButtonClicked);
        friendRequestsButton.onClick.AddListener(OnFriendRequestClicked);
        sendFriendRequestsButton.onClick.AddListener(OnSentFriendRequestClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
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
