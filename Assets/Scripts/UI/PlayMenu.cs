using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public Button backButton;
    public Button singlePlayerButton;
    public Button multiplayerButton;
    
    void Start()
    {
        singlePlayerButton.onClick.AddListener(OnSinglePlayerButtonPressed);
    }
    
    public void OnSinglePlayerButtonPressed()
    {
        MenuManager.Instance.HideAnimate(OnHideAnimateComplete);
    }

    private void OnHideAnimateComplete()
    {
        GameDirector.Instance.StartSinglePlayer();
    }
}
