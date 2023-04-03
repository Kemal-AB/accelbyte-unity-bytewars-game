using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileMenu : MonoBehaviour
{
    [SerializeField] private Button statsButton;
    [SerializeField] private Button backButton;
    
    // Start is called before the first frame update
    void Start()
    {
        statsButton.onClick.AddListener(OnStatsButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnStatsButtonClicked()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.StatsProfileMenuCanvas);
    }

    private void OnBackButtonClicked()
    {
        MenuManager.Instance.OnBackPressed();
    }
}
