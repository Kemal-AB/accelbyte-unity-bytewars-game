using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class Reconnect : MonoBehaviour
{
    public void ConnectAsClient(UnityTransport unityTransport, string address, ushort port, InitialConnectionData initialConnectionData)
    {
        if (unityTransport)
        {
            unityTransport.ConnectionData.Address = address;
            unityTransport.ConnectionData.Port = port;
            var connectionData = GameUtility.ToByteArray(initialConnectionData);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = connectionData;
            NetworkManager.Singleton.StartClient();
            //after called StartClient() NetworkManager will call OnClientConnected if the client connected to server successfully
            //TODO set error connect to server callback
        }
        else
        {
            Debug.Log("can't start online game, unity transport is null");
        }
    }

    private IEnumerator reconnectCoroutine;

    public void TryReconnect(InitialConnectionData initialConnectionData)
    {
        if (reconnectCoroutine == null)
        {
            if (reconnectCoroutine == null)
            {
                reconnectCoroutine = ReconnectWait(initialConnectionData);
                StartCoroutine((reconnectCoroutine));
            }
        }
    }

    private WaitForSeconds waitOneSecond = new WaitForSeconds(1);
    private IEnumerator ReconnectWait(InitialConnectionData initialConnectionData)
    {
        if (NetworkManager.Singleton.IsListening && 
            NetworkManager.Singleton.IsClient && 
            !NetworkManager.Singleton.ShutdownInProgress)
        {
            NetworkManager.Singleton.Shutdown();
            yield return new WaitUntil(() => !NetworkManager.Singleton.ShutdownInProgress);
        }

        yield return waitOneSecond;
        var connectionData = GameUtility.ToByteArray(initialConnectionData);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = connectionData;
        NetworkManager.Singleton.StartClient();
    }
}
