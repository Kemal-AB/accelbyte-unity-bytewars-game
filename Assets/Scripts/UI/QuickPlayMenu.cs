using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class QuickPlayMenu : MonoBehaviour
{
    public Button backButton;
    public Button eliminationButton;
    public Button teamDeadmatchButton;

    // Start is called before the first frame update
    void Start()
    {
        eliminationButton.onClick.AddListener(OnEliminationButtonPressed);
        teamDeadmatchButton.onClick.AddListener(OnTeamDeadmatchButtonPressed);
    }

    public void OnEliminationButtonPressed()
    {
        // MenuManager.Instance.ChangeToMenu(MenuManager.MenuEnum.MatchLobbyCanvas);

    }

    public void OnTeamDeadmatchButtonPressed()
    {
        // MenuManager.Instance.ChangeToMenu(MenuManager.MenuEnum.MatchLobbyTeamCanvas);

    }



}
