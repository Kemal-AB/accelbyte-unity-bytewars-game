using System;
using UnityEngine;
using UnityEngine.UI;


public class QuickPlayMenu : MenuCanvas
{
    public Button backButton;
    public Button eliminationButton;
    public Button teamDeadmatchButton;
    private readonly IMatchmaking matchmaking = new MatchmakingWrapper();
    void Start()
    {
        eliminationButton.onClick.AddListener(OnEliminationButtonPressed);
        teamDeadmatchButton.onClick.AddListener(OnTeamDeadmatchButtonPressed);
        backButton.onClick.AddListener(MenuManager.Instance.OnBackPressed);
    }

    public void OnEliminationButtonPressed()
    {
        MenuManager.Instance.ShowLoading("Finding Match...", ClickCancelMatchmakingElimination);
        //call dummy Accelbyte Game Services for matchmaking to get server ip address and port
        matchmaking.StartMatchmaking(OnMatchmakingFinished);
        
    }

    private void OnMatchmakingFinished(MatchmakingResult result)
    {
        if (result.m_isSuccess)
        {
            MenuManager.Instance.ShowLoading("JOINING MATCH");
            GameManager.Instance
                .StartAsClient(result.m_serverIpAddress, result.m_serverPort, 
                    InGameMode.OnlineEliminationGameMode);
        }
        else
        {
            MenuManager.Instance.HideLoading();
            Debug.Log("failed to matchmaking, please try again, error: "+result.m_errorMessage);
        }
    }

    private void OnJoinedDedicatedServer(string errorMessage)
    {
        MenuManager.Instance.HideLoading();
        if (String.IsNullOrEmpty(errorMessage))
        {
            //show lobby and set its data in MatchLobbyMenu
            MenuManager.Instance.ChangeToMenu(AssetEnum.MatchLobbyMenuCanvas);
        }
        else
        {
            Debug.Log("failed to join dedicated server (DS) reason: "+errorMessage);
        }
    }


    private void ClickCancelMatchmakingElimination()
    {
        //TODO cancel matchmaking using SDK too
        matchmaking.CancelMatchmaking();
        MenuManager.Instance.HideLoading();
    }

    public void OnTeamDeadmatchButtonPressed()
    {
        MenuManager.Instance.ShowLoading("Finding Match...", ClickCancelMatchmakingElimination);
        //call dummy Accelbyte Game Services for matchmaking to get server ip address and port
        matchmaking.StartMatchmaking(OnTeamDeathMatchMatchmakingFinished);
    }

    private void OnTeamDeathMatchMatchmakingFinished(MatchmakingResult result)
    {
        if (result.m_isSuccess)
        {
            MenuManager.Instance.ShowLoading("JOINING MATCH");
            GameManager.Instance
                .StartAsClient(result.m_serverIpAddress, result.m_serverPort, 
                    InGameMode.OnlineDeathMatchGameMode);
        }
        else
        {
            MenuManager.Instance.HideLoading();
            MenuManager.Instance.ShowInfo(result.m_errorMessage, "Error");
            Debug.Log("failed to matchmaking, please try again, error: "+result.m_errorMessage);
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
}
