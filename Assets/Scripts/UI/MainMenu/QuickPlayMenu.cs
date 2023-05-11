using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;


public class QuickPlayMenu : MenuCanvas
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button eliminationButton;
    [SerializeField] private Button teamDeadmatchButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button okButton;
    
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private GameObject findingMatchPanel;
    [SerializeField] private GameObject joiningMatchPanel;
    [SerializeField] private GameObject cancelingMatchPanel;
    [SerializeField] private GameObject failedPanel;
    [SerializeField] private GameObject footerButtonPanel;
    [SerializeField] private GameObject headerPanel;


    private List<GameObject> _panels = new List<GameObject>();

    private MatchmakingEssentialsWrapper _matchmakingEssentialsWrapper;

    #region QuickPlayView

    public enum QuickPlayView
    {
        ContentMatch,
        FindingMatch,
        JoiningMatch,
        CancelingMatch,
        Failed
    }

    private QuickPlayView currentView
    {
        get => currentView;
        set => viewSwitcher(value);
    }

    private void viewSwitcher(QuickPlayView value)
    {
        
        switch (value)
        {
            case QuickPlayView.FindingMatch:
                switcherHelper(findingMatchPanel, value);
                break;
            case QuickPlayView.JoiningMatch:
                switcherHelper(joiningMatchPanel, value);
                break;
            case QuickPlayView.CancelingMatch:
                switcherHelper(cancelingMatchPanel, value);
                break;
            case QuickPlayView.Failed:
                switcherHelper(failedPanel, value);
                break;
            case QuickPlayView.ContentMatch:
                switcherHelper(contentPanel, value);
                break;
        }
    }

    private void switcherHelper(GameObject panel, QuickPlayView value)
    {
        panel.SetActive(true);
        _panels.Except(new []{panel})
            .ToList().ForEach(x => x.SetActive(false));
        if (value != QuickPlayView.ContentMatch)
        {
            headerPanel.SetActive(false);
            footerButtonPanel.SetActive(false);
            return;
        }
        
        headerPanel.SetActive(true);
        footerButtonPanel.SetActive(true);
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _matchmakingEssentialsWrapper = TutorialModuleManager.Instance.GetModuleClass<MatchmakingEssentialsWrapper>();
        _matchmakingEssentialsWrapper.OnMatchFound += ChangeLoading;

        eliminationButton.onClick.AddListener(OnEliminationButtonPressed);
        teamDeadmatchButton.onClick.AddListener(OnTeamDeadmatchButtonPressed);
        backButton.onClick.AddListener(MenuManager.Instance.OnBackPressed);
        cancelButton.onClick.AddListener(ClickCancelMatchmaking);
        okButton.onClick.AddListener(OnOkFailedButtonPressed);
        
        _panels = new List<GameObject>()
        {
            contentPanel, 
            findingMatchPanel, 
            joiningMatchPanel, 
            cancelingMatchPanel, 
            failedPanel
        };
    }

    private void OnOkFailedButtonPressed()
    {
        currentView = QuickPlayView.ContentMatch;
    }

    private void ChangeLoading(Result<MatchmakingV2MatchTicketStatus> result)
    {
        if (!result.IsError)
        {
            currentView = QuickPlayView.JoiningMatch;
        }
    }
    public void OnEliminationButtonPressed()
    {
        currentView = QuickPlayView.FindingMatch;
        _matchmakingEssentialsWrapper.StartMatchmaking("elimination_unity", OnMatchmakingCreated);
    }

    private void OnMatchmakingCreated(Result<SessionV2GameSession> result)
    {
        if (!result.IsError)
        {
            Debug.Log(result.Value.dsInformation.server);
        
            if (result.Value.dsInformation.status == SessionV2DsStatus.AVAILABLE)
            {
                GameManager.Instance
                    .StartAsClient(result.Value.dsInformation.server.ip, (ushort)result.Value.dsInformation.server.port, 
                        InGameMode.OnlineEliminationGameMode);
            }
            else
            {
                currentView = QuickPlayView.Failed;
                Debug.Log("failed to matchmaking, please try again, error: ");
            }
        }
        else
        {
            Debug.Log($"error");
            currentView = QuickPlayView.Failed;
        }

    }
    
    private void ClickCancelMatchmaking()
    {
        _matchmakingEssentialsWrapper.CancelMatchMatch(OnMatchCanceled);
    }

    private void OnMatchCanceled(Result result)
    {
        if (!result.IsError)
        {
            Debug.Log($"Matchcanceled");
            currentView = QuickPlayView.ContentMatch;
            return;
        }
        else
        {
            Debug.Log($"Is Canceled error {result.IsError}, {result.Error.Message}");
        }
    }

    public void OnTeamDeadmatchButtonPressed()
    {
        currentView = QuickPlayView.FindingMatch;
        _matchmakingEssentialsWrapper.StartMatchmaking("teamdeathmatch_unity", OnTeamDeathMatchMatchmakingFinished);
        
    }

    private void OnTeamDeathMatchMatchmakingFinished(Result<SessionV2GameSession> result)    
    {
        if (!result.IsError)
        {
            Debug.Log(result.Value.dsInformation.server);
        
            if (result.Value.dsInformation.status == SessionV2DsStatus.AVAILABLE)
            {
                GameManager.Instance
                    .StartAsClient(result.Value.dsInformation.server.ip, (ushort)result.Value.dsInformation.server.port, 
                        InGameMode.OnlineDeathMatchGameMode);
            }
            else
            {
                currentView = QuickPlayView.Failed;
                Debug.Log("failed to matchmaking, please try again, error: ");
            }
        }
        else
        {
            Debug.Log($"error");
            currentView = QuickPlayView.Failed;
        }
    }

    public override GameObject GetFirstButton()
    {
        return eliminationButton.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.QuickPlayMenuCanvas;
    }
    
    private void OnEnable()
    {
        currentView = QuickPlayView.ContentMatch;
    }
}
