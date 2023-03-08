using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
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
        yield return new WaitUntil(()=>MenuManager.Instance.IsInitialized);
        //TODO show login with device id menu
        MenuManager.Instance.ChangeToMenu(AssetEnum.MainMenuCanvas);
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
