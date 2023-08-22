using AccelByte.Core;
using Debugger;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugImplementation
{
    public DebugImplementation()
    {
        DebugConsole.AddButton("enable play online btn", OnEnablePlayOnlineBtn);
        DebugConsole.AddButton("shutdown netcode", OnShutdownNetcode);
        DebugConsole.AddButton("disconnect", OnTestDisconnect);
        DebugConsole.AddButton("check match session", MatchSessionWrapper.GetDetail);
#if UNITY_SERVER
        DebugConsole.AddButton("Deregister Local Server", OnDisconnectLocalDs);
#endif
    }

    private void OnDisconnectLocalDs()
    {
        MultiRegistry.GetServerApiClient()
            .GetDedicatedServerManager()
            .DeregisterLocalServer(result =>
            {
                if (result.IsError)
                {
                    Debug.Log("Failed Deregister Local Server");
                }
                else
                {
                    Debug.Log("Successfully Deregister Local Server");
#if UNITY_EDITOR
                    EditorApplication.ExitPlaymode();
#else
                    Application.Quit();
#endif
                }
            });
    }
    private void OnTestDisconnect()
    {
        var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        transport.ConnectionData.Port = 1234;
    }

    private void OnShutdownNetcode()
    {
        if (!NetworkManager.Singleton.ShutdownInProgress)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void OnEnablePlayOnlineBtn()
    {
        var menu = MenuManager.Instance.AllMenu[AssetEnum.MainMenuCanvas] as MainMenu;
        menu.ShowOnlineBtn();
    }
}
