using System;
using UnityEngine;
using Random = System.Random;

public class LoginWithDeviceHandler : MonoBehaviour
{
    private TutorialModuleData _moduleData;
    private Action<bool> _onFinished;
    private bool _isLoggedIn = false;
    public bool IsLoggedIn
    {
        get { return _isLoggedIn; }
    }
    public void SetData(TutorialModuleData moduleData, Action<bool> onFinished)
    {
        _onFinished = onFinished;
        _moduleData = moduleData;
        for (int i = 0; i < _moduleData.menuCanvasData.buttons.Length; i++)
        {
            if (_moduleData.menuCanvasData.buttons[i].name.Equals("LoginWithDeviceIDButton"))
            {
                _moduleData.menuCanvasData.buttons[i].callback = ClickLoginWithDeviceId;
            }
        }
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
        if (_onFinished != null)
        {
            if(isSuccess)
                _onFinished(isSuccess);
            else
            {
                MenuManager.Instance.ShowRetrySkipQuitMenu(ClickLoginWithDeviceId, ()=>_onFinished(true));
            }
        }
    }
    
}
