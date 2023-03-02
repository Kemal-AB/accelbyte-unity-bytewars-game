using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button playButton;
    public Button playOnlineButton;

    // Start is called before the first frame update
    void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonPressed);
        playOnlineButton.onClick.AddListener(OnPlayOnlineButtonPressed);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPlayButtonPressed()
    {
        // MenuManager.Instance.ChangeToMenu(MenuManager.MenuEnum.PlayMenuCanvas);
    }

    public void OnPlayOnlineButtonPressed()
    {
        // MenuManager.Instance.ChangeToMenu(MenuManager.MenuEnum.PlayOnlineMenuCanvas);
    }

    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }

}
