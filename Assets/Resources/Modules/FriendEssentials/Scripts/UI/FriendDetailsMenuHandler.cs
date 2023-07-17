using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendDetailsMenuHandler : MenuCanvas
{
    public RectTransform friendDetailsPanel;
    [SerializeField] private Button backButton;
 
    // Start is called before the first frame update
    void Start()
    {
        backButton.onClick.AddListener(MenuManager.Instance.OnBackPressed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override GameObject GetFirstButton()
    {
        return backButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.FriendDetailsMenuCanvas;
    }
}
