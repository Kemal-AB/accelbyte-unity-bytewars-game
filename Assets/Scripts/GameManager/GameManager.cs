using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private WaitForSeconds _wait = new WaitForSeconds(0.5f);
    private static GameManager _instance;
    private static bool _isLoggedIn = false;
    private void Awake()
    {

        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        StartCoroutine(StartWait());
    }

    IEnumerator StartWait()
    {
        if (_isLoggedIn) yield break;
        yield return _wait;
        var loginWithDeviceIdButton = new MenuButtonData
        {
            label = "Login with Device ID",
            name = "LoginWithDeviceIDButton",
            callback = ClickLoginWithDeviceID
        };
        var loginWithDeviceIDCanvasData = new MenuCanvasData
        {
            buttons = new Dictionary<string, MenuButtonData>()
            {
                {loginWithDeviceIdButton.name, loginWithDeviceIdButton}
            },
            name = "LoginWithDeviceIDMenuCanvas"
        };
        MenuManager.Instance.AddMenu(loginWithDeviceIDCanvasData);
        yield return _wait;
        MenuManager.Instance.ChangeToMenu(loginWithDeviceIDCanvasData.name);
    }

    private void ClickLoginWithDeviceID()
    {
        MenuManager.Instance.ChangeToMenu(AssetEnum.LoadingMenuCanvas);
        Debug.Log("call 3rdparty API, login with device id is clicked");
        StartCoroutine(PretendApiCallFinishedSuccessfully());
    }

    IEnumerator PretendApiCallFinishedSuccessfully()
    {
        _isLoggedIn = true;
        yield return _wait;
        MenuManager.Instance.ChangeToMenu(AssetEnum.MainMenuCanvas);
    }
}
