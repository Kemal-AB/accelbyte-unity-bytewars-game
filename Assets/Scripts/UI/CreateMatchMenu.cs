using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CreateMatchMenu : MonoBehaviour
{
    public Button backButton;
    public Button createEliminationButton;
    public Button createTeamDeadmatchButton;
    public Button createCustomMatchButton;


    // Start is called before the first frame update
    void Start()
    {
        createCustomMatchButton.onClick.AddListener(OnCustomMatchButtonPressed);
        backButton.onClick.AddListener(OnBackButtonPressed);
    }

    public void OnBackButtonPressed()
    {
        MenuManager.Instance.OnBackPressed();
    }

    public void OnCustomMatchButtonPressed()
    {
        // MenuManager.Instance.ChangeToMenu(MenuManager.MenuEnum.CreateCustomMatchMenuCanvas);
    }
}
