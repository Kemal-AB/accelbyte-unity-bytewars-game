using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;

public class FriendDetailsMenuHandler_Starter : MenuCanvas
{
    public RectTransform friendDetailsPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private Button blockButton;
    [SerializeField] private Button unfriendButton;

    private string _userId;
    public string UserID { private get => _userId;
        set
        {
            _userId = value;
            Debug.Log(UserID);
        }
    }
        
    // Start is called before the first frame update
    void Start()
    {
        EnableButton(blockButton, TutorialType.ManagingFriends);
        EnableButton(unfriendButton, TutorialType.ManagingFriends);
        
        backButton.onClick.AddListener(MenuManager.Instance.OnBackPressed);
        blockButton.onClick.AddListener(OnBlockCliked);
        unfriendButton.onClick.AddListener(OnUnfriendClicked);
    }
    
    private void EnableButton(Button button, TutorialType tutorialType)
    {
        var module = TutorialModuleManager.Instance.GetModule(tutorialType);
        if (module.isActive)
        {
            button.gameObject.SetActive(true);
        }
    }

    private void OnUnfriendClicked()
    {
        Debug.LogWarning($"Unfriend a friend is not yet implemented.");
    }
    
    private void OnBlockCliked()
    {
        Debug.LogWarning($"Block a player is not yet implemented.");
    }
    
    public override GameObject GetFirstButton()
    {
        return backButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.FriendDetailsMenuCanvas_Starter;
    }
}
