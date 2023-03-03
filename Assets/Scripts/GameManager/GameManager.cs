using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private WaitForSeconds _wait = new WaitForSeconds(0.5f);
    private static GameManager _instance;
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
        yield return _wait;
        bool isABSDKInstalled = TutorialModuleUtil.IsAccelbyteSDKInstalled();
        LoginWithDeviceHandler loginWithDeviceHandler =
            TutorialModuleManager.Instance.GetModuleClass<LoginWithDeviceHandler>();
        var loginWithDeviceIDdata =
            AssetManager.Singleton.GetAsset(AssetEnum.LoginWithDeviceID) as TutorialModuleData;
        if (isABSDKInstalled && loginWithDeviceHandler!=null && 
            !loginWithDeviceHandler.IsLoggedIn && loginWithDeviceIDdata!=null 
            && loginWithDeviceIDdata.isActive)
        {
            loginWithDeviceHandler.SetData(loginWithDeviceIDdata, OnLoginWithDeviceIDFinished);
            MenuManager.Instance.AddMenu(loginWithDeviceIDdata.menuCanvasData);
            yield return _wait;
            MenuManager.Instance.ChangeToMenu(loginWithDeviceIDdata.menuCanvasData.name);
        }
        else
        {
            MenuManager.Instance.ChangeToMenu(AssetEnum.MainMenuCanvas);
        }
    }

    private void OnLoginWithDeviceIDFinished(bool isSuccess)
    {
        Debug.Log("login with device id isSuccess: "+isSuccess);
        if (isSuccess)
        {
            MenuManager.Instance.ChangeToMenu(AssetEnum.MainMenuCanvas);
        }
    }
}
