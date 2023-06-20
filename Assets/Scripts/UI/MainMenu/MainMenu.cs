using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MenuCanvas
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button playOnlineBtn;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button helpAndOptionsButton;
    [SerializeField] private Button quitButton;

    // Start is called before the first frame update
    void Start()
    {
        CheckModulesButtons();
        
        playButton.onClick.AddListener(OnPlayButtonPressed);
        playOnlineBtn.onClick.AddListener(OnPlayOnlineButtonPressed);
        profileButton.onClick.AddListener(OnProfileButtonPressed);
        helpAndOptionsButton.onClick.AddListener(OnHelpAndOptionsButtonPressed);
        quitButton.onClick.AddListener(OnQuitButtonPressed);
    }

    public void OnPlayButtonPressed()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.PlayMenuCanvas);
    }

    public void OnPlayOnlineButtonPressed()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.PlayOnlineMenuCanvas);
    }
    
    public void OnProfileButtonPressed()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.ProfileMenuCanvas);
    }

    public void OnHelpAndOptionsButtonPressed()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.HelpAndOptionsMenuCanvas);
    }

    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }

    public override GameObject GetFirstButton()
    {
        return playButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.MainMenuCanvas;
    }

    public void ShowOnlineBtn()
    {
        playOnlineBtn.gameObject.SetActive(true);
    }

    private void CheckModulesButtons()
    {
        #if !BYTEWARS_DEBUG
            bool isOnlineBtnActive = TutorialModuleManager.Instance.IsModuleActive(TutorialType.MatchmakingEssentials);
            playOnlineBtn.gameObject.SetActive(isOnlineBtnActive);
        #endif

        bool isStatsModuleActive = TutorialModuleManager.Instance.IsModuleActive(TutorialType.StatsEssentials);
        profileButton.gameObject.SetActive(isStatsModuleActive);
    }
}
