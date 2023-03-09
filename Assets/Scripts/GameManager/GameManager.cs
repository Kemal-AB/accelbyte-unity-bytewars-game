using System.Collections;
using UnityEngine;

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
}
