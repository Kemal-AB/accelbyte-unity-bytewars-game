using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkingTest : NetworkBehaviour
{

    public NetworkManager netManager;

    public TMP_Text ConsoleText;

    public NetworkVariable<Vector3> Player1Position = new NetworkVariable<Vector3>();
    public NetworkVariable<Vector3> Player2Position = new NetworkVariable<Vector3>();

    public int MaxAllowedPlayers = 2;

    private int _totalPlayersConnected = 0;
    // Start is called before the first frame update
    [SerializeField]
    private string _sceneName = "GalaxyWorld";
    private Scene m_LoadedScene;

    private void Start()
    {
        
    }
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

        NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;

        NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;

        WriteToConsole("Transport mode: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);

        base.OnNetworkSpawn();
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

    private void OnClientConnectedCallback(ulong clientId)
    {
        _totalPlayersConnected += 1;
        if (NetworkManager.Singleton.IsServer)
        {
            WriteToConsole("New Client connected- ClientID: " + clientId + " total Connected Clients: " + _totalPlayersConnected.ToString());

            if(_totalPlayersConnected == MaxAllowedPlayers)
            {
                WriteToConsole("Max players reached: " + MaxAllowedPlayers.ToString() + " Sending StartGameRPC to clients... ");

                var status = NetworkManager.SceneManager.LoadScene(_sceneName,UnityEngine.SceneManagement.LoadSceneMode.Single);
                if (status != SceneEventProgressStatus.Started)
                {
                    Debug.LogWarning($"Failed to load {_sceneName} " +
                          $"with a {nameof(SceneEventProgressStatus)}: {status}");
                }
                //StartGameClientRpc();
            }
        }
    }

    [ClientRpc]
    void StartGameClientRpc()
    {
        WriteToConsole("Starting Game!");
    }


    private void OnClientDisconnectCallback(ulong clientId)
    {
        _totalPlayersConnected -= 1;
        if (NetworkManager.Singleton.IsServer)
        {
            WriteToConsole("Client Disconnected - ClientID: " + clientId + " total Connected Clients: " + _totalPlayersConnected.ToString());
        }
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        var clientOrServer = sceneEvent.ClientId == NetworkManager.ServerClientId ? "server" : "client";
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.LoadComplete:
                {
                    // We want to handle this for only the server-side
                    if (sceneEvent.ClientId == NetworkManager.ServerClientId)
                    {
                        // *** IMPORTANT ***
                        // Keep track of the loaded scene, you need this to unload it
                        m_LoadedScene = sceneEvent.Scene;
                        
                    }
                    WriteToConsole(clientOrServer +" with ID: "+ sceneEvent.ClientId + " LoadSceneCompleted :" + m_LoadedScene.name);
                    break;
                }
            case SceneEventType.UnloadComplete:
                {
                    WriteToConsole(clientOrServer + " with ID: " + sceneEvent.ClientId + " UnloadSceneCompleted :" + m_LoadedScene.name);
                    break;
                }
            case SceneEventType.LoadEventCompleted:
            case SceneEventType.UnloadEventCompleted:
                {
                    var loadUnload = sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted ? "Load" : "Unload";
                    WriteToConsole($"{loadUnload} event completed for the following client " +
                        $"identifiers:({sceneEvent.ClientsThatCompleted})");
                    if (sceneEvent.ClientsThatTimedOut.Count > 0)
                    {
                        WriteToConsole($"{loadUnload} event timed out for the following client " +
                            $"identifiers:({sceneEvent.ClientsThatTimedOut})");
                    }
                    break;
                }
        }
    }

    public void WriteToConsole(string text)
    {
        ConsoleText.text += "\n" + text;
    }

    public override void OnDestroy()
    {
        NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
