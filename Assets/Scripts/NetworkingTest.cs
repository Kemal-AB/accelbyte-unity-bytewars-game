using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkingTest : NetworkBehaviour
{

    public NetworkManager netManager;

    public TMP_Text ConsoleText;

    public NetworkVariable<Vector3> Player1Position = new NetworkVariable<Vector3>();
    public NetworkVariable<Vector3> Player2Position = new NetworkVariable<Vector3>();
    // Start is called before the first frame update

    
    public override void OnNetworkSpawn()
    {
        if (!IsServer) //Only send an RPC to the server on the client that owns the NetworkObject that owns this NetworkBehaviour instance
        {
            WriteToConsole("OnNetworkSpawn as a Client!");
            //TestServerRpc(0);
        }else
        {
            WriteToConsole("OnNetworkSpawn as a Server!");
            //TestClientRpc(3);
        }
    }

    public void StartServer()
    {
        WriteToConsole("Starting as a Server...");
        netManager.StartServer();
    }
    public void StartClient()
    {
        WriteToConsole("Starting as a Client...");
        netManager.StartClient();
    }

    public void SendClientTest()
    {
        TestClientRpc(10);
    }
    public void SendServerTest()
    {
        TestServerRpc(10);
    }

    [ClientRpc]
    void TestClientRpc(int value)
    {
        WriteToConsole("TestClientRPC with InValue: " + value.ToString());
    }

    [ServerRpc(RequireOwnership = false)]
    void TestServerRpc(int value)
    {
        WriteToConsole("TestServerRPC with InValue: " + value.ToString());
    }

    public void WriteToConsole(string text)
    {
        ConsoleText.text += "\n" + text;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
