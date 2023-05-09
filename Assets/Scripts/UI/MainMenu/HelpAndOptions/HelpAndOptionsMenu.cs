using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpAndOptionsMenu : MenuCanvas
{
    [SerializeField] private Button helpButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button backButton;

    // Start is called before the first frame update
    void Start()
    {
        
        helpButton.onClick.AddListener((() =>
        {
            MenuManager.Instance.ChangeToMenu(AssetEnum.HelpMenuCanvas);
        }));
        optionsButton.onClick.AddListener((() =>
        {
            MenuManager.Instance.ChangeToMenu(AssetEnum.OptionsMenuCanvas);
        }));
        creditsButton.onClick.AddListener((() =>
        {
            MenuManager.Instance.ChangeToMenu(AssetEnum.CreditsMenuCanvas);
        }));
        backButton.onClick.AddListener(() => MenuManager.Instance.OnBackPressed());
    }

    public override GameObject GetFirstButton()
    {
        return helpButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.HelpAndOptionsMenuCanvas;
    }
}
