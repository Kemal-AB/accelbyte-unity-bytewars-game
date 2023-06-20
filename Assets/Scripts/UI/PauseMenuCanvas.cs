using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuCanvas : MenuCanvas
{
    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button restartBtn;
    [SerializeField] private Button quitBtn;
    void Start()
    {
        resumeBtn.onClick.AddListener(OnClickResumeBtn);
        restartBtn.onClick.AddListener(GameManager.Instance.RestartLocalGame);
        quitBtn.onClick.AddListener(GameManager.Instance.QuitToMainMenu);
    }

    private void OnClickResumeBtn()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            MenuManager.Instance.CloseMenuPanel();
        }
        else
        {
            GameManager.Instance.TriggerPauseLocalGame();
        }
    }

    public override GameObject GetFirstButton()
    {
        return resumeBtn.gameObject;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.PauseMenuCanvas;
    }

    private bool isRestartBtnShown=true;
    private void OnEnable()
    {
        restartBtn.gameObject.SetActive(isRestartBtnShown);
    }

    private void OnDisable()
    {
        isRestartBtnShown = true;
    }

    public void DisableRestartBtn()
    {
        isRestartBtnShown = false;
    }
}
