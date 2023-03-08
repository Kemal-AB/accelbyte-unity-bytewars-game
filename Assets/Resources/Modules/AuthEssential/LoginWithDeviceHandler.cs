using System;
using UnityEngine;
using UnityEngine.UI;

public class LoginWithDeviceHandler : MonoBehaviour
{
    [SerializeField] private Button loginBtn;
    private TutorialModuleData _moduleData;
    private bool _isLoggedIn = false;

    private void Start()
    {
        loginBtn.onClick.AddListener(ClickLoginWithDeviceId);
    }

    public bool IsLoggedIn
    {
        get { return _isLoggedIn; }
    }

    private int loginDataIndex = 0;
    private bool[] loginDataSequence = { false, true };
    private void ClickLoginWithDeviceId()
    {
        //int ranTest = UnityEngine.Random.Range(0, 2);
        OnLoginFinished(loginDataSequence[loginDataIndex]);
        loginDataIndex++;
    }

    private void OnLoginFinished(bool isSuccess)
    {
        _isLoggedIn = isSuccess;
        if (isSuccess)
        {
            MenuManager.Instance.ChangeToMenu(AssetEnum.MainMenuCanvas);
        }
        else
        {
            MenuManager.Instance.ShowRetrySkipQuitMenu(ClickLoginWithDeviceId, 
                ClickLoginWithDeviceId, "Fail to login with device ID");
        }
    }
    
}
