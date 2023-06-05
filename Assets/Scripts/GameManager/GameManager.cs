using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccelByte.Core;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] private InGameHUD _hud;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameEntityAbs[] _gamePrefabs;
    [SerializeField] private FxEntity[] _FxPrefabs;
    [SerializeField] private GameModeSO[] availableInGameMode;
    [SerializeField] private Reconnect reconnect;
    //[SerializeField] private GameClientController clientPrefab;
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    private const string GameSceneName = "GalaxyWorld";
    private const string MenuSceneName = "MainMenu";
    private GameModeEnum _gameMode = GameModeEnum.MainMenu;
    private MenuManager _menuManager;
    private Dictionary<string, GameEntityAbs> _gamePrefabDict = new Dictionary<string, GameEntityAbs>();

    public delegate void GameOverDelegate(GameModeEnum gameMode, List<PlayerState> playerStates);
    public static event GameOverDelegate onGameOver = delegate {};
    
    [SerializeField]
    private Transform _container;

    [SerializeField] private InGameCamera _inGameCamera;
    //private List<PlayerController> _instantiatedPlayerControllers = new List<PlayerController>();
    private ObjectPooling _objectPooling;
    public InGameState InGameState
    {
        get { return _inGameState; }
    }
    public List<GameEntityAbs> ActiveGEs
    {
        get { return _activeGEs; }
    }
    public Dictionary<ulong, Player> Players
    {
        get { return _players; }
    }
    public ObjectPooling Pool
    {
        get { return _objectPooling; }
    }
    public Camera MainCamera
    {
        get { return _camera; }
    }
    public TeamState[] TeamStates
    {
        get { return _serverHelper.GetTeamStates(); }
    }
    public Dictionary<int, TeamState> ConnectedTeamStates
    {
        get { return _serverHelper.ConnectedTeamStates; }
    }
    public Dictionary<ulong, PlayerState> ConnectedPlayerStates
    {
        get { return _serverHelper.ConnectedPlayerStates; }
    }
    public ulong ClientNetworkId
    {
        get { return _clientHelper.ClientNetworkId; }
    }
    private readonly List<GameEntityAbs> _activeGEs = new List<GameEntityAbs>();
    private Dictionary<ulong, Player> _players = new Dictionary<ulong, Player>();
    private readonly Dictionary<int, Planet> _planets = new Dictionary<int, Planet>();
    private InGameState _inGameState = InGameState.None;
    private readonly ServerHelper _serverHelper = new ServerHelper();
    private readonly ClientHelper _clientHelper = new ClientHelper();
    private InGameMode _inGameMode = InGameMode.None;
    private Dictionary<ulong, GameClientController> connectedClients = new Dictionary<ulong, GameClientController>();
    private const int MinPlayerForOnlineGame = 2;
    private DebugImplementation debug;
    private readonly ConnectionHelper connectionHelper = new ConnectionHelper();
    private List<Vector3> availablePositions;
    public void TriggerPauseLocalGame()
    {
        if (_inGameState==InGameState.LocalPause)
        {
            SetInGameState(InGameState.Playing);
            _menuManager.HideAnimate(null);
        }
        else if(_inGameState==InGameState.Playing)
        {
            _menuManager.ShowInGameMenu(AssetEnum.PauseMenuCanvas);
            SetInGameState(InGameState.LocalPause);
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        foreach (var gamePrefab in _gamePrefabs)
        {
            _gamePrefabDict.Add(gamePrefab.name, gamePrefab);
        }
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientStopped += OnClientStopped;
        if (_unityTransport == null)
            _unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        #if UNITY_SERVER
        StartServer();
        #else
        if (debug == null)
            debug = new DebugImplementation();
        #endif
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        _hud.Reset();
        StartWait();
    }

    private void OnClientStopped(bool isHost)
    {
        //TODO check is in game scene, check whether client intentionally click quit button
        if (_inGameState != InGameState.GameOver)
        {
            if (_serverHelper.ConnectedPlayerStates.TryGetValue(_clientHelper.ClientNetworkId, out var playerState))
            {
                var initialData = new InitialConnectionData()
                {
                    inGameMode = _inGameMode,
                    sessionId = playerState.sessionId
                };
                // reconnect.TryReconnect(initialData);
                RemoveConnectedClient(_clientHelper.ClientNetworkId);
            }
        }

        bool isInMainMenu = MenuSceneName.Equals(SceneManager.GetActiveScene().name);
        if (isInMainMenu)
        {
            var menuCanvas = _menuManager.GetCurrentMenu();
            if (menuCanvas && menuCanvas is MatchLobbyMenu lobby)
            {
                lobby.ShowStatus("disconnected from server, trying to reconnect...");
            }
        }
        Debug.Log($"OnClientStopped isHost:{isHost} clientNetworkId:{_clientHelper.ClientNetworkId}");
    }

    private void StartServer()
    {
        if (_unityTransport)
        {
            //this might not necessary, setting server ip and host
            _unityTransport.ConnectionData.Address = "127.0.0.1";
            _unityTransport.ConnectionData.Port = 7778;
            _unityTransport.ConnectionData.ServerListenAddress = "0.0.0.0";
            NetworkManager.Singleton.StartServer();
            NetworkManager.SceneManager.OnSceneEvent += OnNetworkSceneEvent;
            Debug.Log(_unityTransport.ConnectionData.ServerListenAddress.ToString());
            Debug.Log("server started");
        }
    }
    
    

    private void OnNetworkSceneEvent(SceneEvent sceneEvent)
    {
        bool isServer = sceneEvent.ClientId == NetworkManager.ServerClientId;
        bool isGameScene = !String.IsNullOrEmpty(sceneEvent.SceneName) && sceneEvent.SceneName.Equals(GameSceneName);
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.LoadComplete:
                if (!isServer && isGameScene)
                {
                    if (connectedClients.TryGetValue(sceneEvent.ClientId, out var connectedClient))
                    {
                        connectedClient.isGameSceneLoaded = true;
                    }
                }
                break;
        }
        Debug.Log($"OnNetworkSceneEvent isServer {isServer} type {sceneEvent.SceneEventType.ToString()} isGameScene: {isGameScene}");
        if (isServer && sceneEvent.SceneEventType==SceneEventType.LoadComplete && 
            isGameScene)
        {
            OnRejectBackfill?.Invoke();
            MenuManager.Instance.CloseMenuPanel();
            if (_objectPooling == null)
                _objectPooling = new ObjectPooling(_container, _gamePrefabs, _FxPrefabs);
            SetupGame();
        }
        if (isServer && sceneEvent.SceneEventType==SceneEventType.LoadEventCompleted && 
            isGameScene && AllClientsSceneLoaded())
        {
            SetInGameState(InGameState.PreGameCountdown);
        }
    }

    private bool AllClientsSceneLoaded()
    {
        bool allClientsSceneLoaded = true;
        foreach (var kvp in connectedClients)
        {
            var client = kvp.Value;
            if (!client.isGameSceneLoaded)
            {
                allClientsSceneLoaded = false;
            }
        }

        return allClientsSceneLoaded;
    }

    private void OnClientDisconnected(ulong clientNetworkId)
    {
        string reason = "";
        string activeSceneName = SceneManager.GetActiveScene().name;
        bool isInMainMenu = activeSceneName.Equals(MenuSceneName);
        bool isInGameScene = activeSceneName.Equals(GameSceneName);
        if (!String.IsNullOrEmpty(NetworkManager.Singleton.DisconnectReason))
        {
            reason = NetworkManager.Singleton.DisconnectReason;
        }
        Debug.Log($"OnClientDisconnected client id {clientNetworkId} disconnected reason:{reason} active entity count:{_activeGEs.Count} IsServer:{IsServer}");
        if (isInMainMenu)
        {
            if (_menuManager.IsLoading)
            {
                _menuManager.HideLoading();
            }
            else
            {
                if (IsServer)
                {
                    RemoveConnectedClient(clientNetworkId);
                }
            }
        } 
        else if (isInGameScene)
        {
            if (IsServer && _inGameState != InGameState.GameOver)
            {
                //player might reconnect in the middle of game, missile will not reset
                RemoveConnectedClient(clientNetworkId, false);
                if (connectedClients.Count < MinPlayerForOnlineGame)
                {
                    SetInGameState(InGameState.ShuttingDown);
                }
            }
        }
            
        if (IsClient)
        {
            QuitToMainMenu();
        }
    }

    private void RemoveConnectedClient(ulong clientNetworkId, bool isResetMissile=true)
    {
        connectedClients.Remove(clientNetworkId);
        if (_players.TryGetValue(clientNetworkId, out var player))
        {
            if (IsServer)
            {
                _serverHelper.DisconnectPlayerState(clientNetworkId, player);
            }
            else
            {
                player.gameObject.SetActive(false);
                _players.Remove(clientNetworkId);
            }
        }
        RemoveConnectedClientRpc(clientNetworkId, _serverHelper.ConnectedTeamStates.Values.ToArray(), 
            _serverHelper.ConnectedPlayerStates.Values.ToArray(), isResetMissile);
    }

    [ClientRpc]
    private void RemoveConnectedClientRpc(ulong clientNetworkId, TeamState[] teamStates, 
        PlayerState[] playerStates, bool isResetMissile)
    {
        UpdateInGamePlayerState(teamStates, playerStates);
        connectedClients.Remove(clientNetworkId);
        if (_players.Remove(clientNetworkId, out var player))
        {
            if (isResetMissile)
            {
                player.Reset();
            }
            else
            {
                player.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// this is only called in server/hosting
    /// </summary>
    /// <param name="request">client information</param>
    /// <param name="response">set whether the client is allowed to connect or not</param>
    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, 
        NetworkManager.ConnectionApprovalResponse response)
    {
        var result = connectionHelper.ConnectionApproval(request, response, IsServer, _inGameState, availableInGameMode,
            _inGameMode, _serverHelper);
        if (result != null)
        {
            if (_inGameMode == InGameMode.None)
            {
                _inGameMode = result.InGameMode;
                GameData.GameModeSo = result.GameModeSo;
            }

            if (result.reconnectPlayer != null)
            {
                _players.TryAdd(request.ClientNetworkId, result.reconnectPlayer);
            }
        }
    }

    /// <summary>
    /// this is where the magic begin, most variable exist only on IsServer bracket, but not on IsClient
    /// </summary>
    /// <param name="clientNetworkId"></param>
    private void OnClientConnected(ulong clientNetworkId)
    {
        Debug.Log($"OnClientConnected IsServer:{IsServer} IsOwner:{IsOwner} clientNetworkId:{clientNetworkId}");
        if (IsOwner && IsServer)
        {
            _serverHelper.StartCoroutineCountdown(this, GameData.GameModeSo.lobbyCountdownSecond, OnLobbyCountdownServerUpdated);
            //most variable exists only on IsServer bracket
            bool isInGameScene = GameSceneName.Equals(SceneManager.GetActiveScene().name);
            SendConnectedPlayerStateClientRpc( _serverHelper.ConnectedTeamStates.Values.ToArray(), 
                _serverHelper.ConnectedPlayerStates.Values.ToArray(), _inGameMode, isInGameScene);
            var playerObj = NetworkManager.ConnectedClients[clientNetworkId].PlayerObject;
            var gameClient = playerObj.GetComponent<GameClientController>();
            if (gameClient)
            {
                connectedClients.Add(clientNetworkId, gameClient);
                Debug.Log($"clientNetworkId: {clientNetworkId} connected");
                if (isInGameScene && _inGameState!=InGameState.GameOver)
                {
                    if (_players.TryGetValue(clientNetworkId, out var serverPlayer))
                    {
                        serverPlayer.UpdateMissilesState();
                        ReAddReconnectedPlayerClientRpc(clientNetworkId, serverPlayer.GetFiredMissilesId(),
                            _serverHelper.ConnectedTeamStates.Values.ToArray(), 
                            _serverHelper.ConnectedPlayerStates.Values.ToArray());
                    }
                    //gameplay already started
                    if (_gameTimeLeft != 0)
                    {
                        SetInGameState(InGameState.Playing);
                    }
                    else
                    {
                        SetInGameState(InGameState.PreGameCountdown);
                    }
                }
            }
        }
        if (IsClient)
        {
            //most variable does not exists on IsClient bracket
            _clientHelper.SetClientNetworkId(clientNetworkId);
            var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (playerObj)
            {
                var gameController = playerObj.GetComponent<GameClientController>();
                if (gameController)
                {
                    connectedClients.TryAdd(clientNetworkId, gameController);
                }
            }
        }
    }

    private void ReAddReconnectedPlayerOnClient(ulong clientNetworkId, int[] firedMissilesId, 
        TeamState[] teamStates, PlayerState[] playerStates)
    {
        Debug.Log($"ReAddReconnectedPlayerOnClient IsServer:{IsServer} clientNetworkId:{clientNetworkId}");
        _clientHelper.SetClientNetworkId(clientNetworkId);
        _hud.HideGameStatusContainer();
        var player = InGameFactory.SpawnReconnectedShip(clientNetworkId, _serverHelper, _objectPooling);
        if (player)
        {
            player.SetFiredMissilesID(firedMissilesId);
            _players.TryAdd(clientNetworkId, player);
        }
        _serverHelper.UpdatePlayerStates(teamStates, playerStates);
    }

    [ClientRpc]
    private void ReAddReconnectedPlayerClientRpc(ulong clientNetworkId, int[] firedMissilesId, 
        TeamState[] teamStates, PlayerState[] playerStates)
    {
        ReAddReconnectedPlayerOnClient(clientNetworkId, firedMissilesId, teamStates, playerStates);
    }
    private void OnLobbyCountdownServerUpdated(int countdown)
    {
        UpdateLobbyCountdownClientRpc(countdown);
        if (countdown <= 0)
        {
            StartOnlineGame();
        }
    }

    [ClientRpc]
    private void UpdateLobbyCountdownClientRpc(int countdown)
    {
        _menuManager.UpdateLobbyCountdown(countdown);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner && !IsServer)
        {
            ClientConnectedServerRpc(NetworkObjectId);
        }
        base.OnNetworkSpawn();
    }

    async void StartWait()
    {
        await Task.Delay(150);
        while (!MenuManager.Instance.IsInitiated)
        {
            await Task.Delay(50);
        }
        _menuManager = MenuManager.Instance;
        _menuManager.SetEventSystem(_eventSystem);
        if (IsServer)
        {
            #if UNITY_SERVER
                    OnRegisterServer?.Invoke();
            #endif
            _menuManager.CloseMenuPanel();
        }
    }

    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        bool isInGameScene = next.name.Equals(GameSceneName);
        bool isInMainMenuScene = next.name.Equals(MenuSceneName);
        #if UNITY_SERVER
        if (next.name.Equals(MenuSceneName))
        {
            //TODO: ADD server shutdown
            // Debug.Log("server shutdown");
            NetworkManager.Singleton.Shutdown();
            DeregisterServer();
        }
        #else
        if (NetworkManager.Singleton.IsListening)
            return;
        if (isInGameScene)
        {
            AudioManager.Instance.PlayGameplayBGM();
            MenuManager.Instance.CloseMenuPanel();
            SetupGame();
        }

        if (isInMainMenuScene)
        {
            _objectPooling?.ResetAll();
        }
        #endif
    }

    void SetupGame()
    {
        if (!SetInGameState(InGameState.Initializing))
            return;
        _objectPooling ??= new ObjectPooling(_container, _gamePrefabs, _FxPrefabs);
        Debug.Log(("Setup Game"));
        _activeGEs.RemoveAll((GameEntityAbs ge) => !ge);
        _players.Clear();
        InGameStateResult states = new InGameStateResult();
        bool isOnlineGame = _gameMode == GameModeEnum.OnlineMultiplayer;
        if (isOnlineGame)
        {
            if (IsServer)
            {
                _hud.gameObject.SetActive(true);
                _hud.Init(_serverHelper.ConnectedTeamStates.Values.ToArray(), _serverHelper.ConnectedPlayerStates.Values.ToArray());
                var createLevelResult = InGameFactory.CreateLevel(GameData.GameModeSo, _activeGEs, 
                    _players, _objectPooling, _serverHelper.ConnectedTeamStates, _serverHelper.ConnectedPlayerStates);
                availablePositions = createLevelResult.AvailablePositions;
                PlaceObjectsClientRpc(createLevelResult.LevelObjects, _players.Keys.ToArray(), 
                    createLevelResult.AvailablePositions.ToArray(), _serverHelper.ConnectedTeamStates.Values.ToArray(),
                    _serverHelper.ConnectedPlayerStates.Values.ToArray());
            }
        }
        else
        {
            //create local offline teamStates and playerStates based on GameModeSO player per team count
            states = InGameFactory.CreateLocalGameState(GameData.GameModeSo);
            _serverHelper.SetTeamAndPlayerState(states);
            var result = InGameFactory.CreateLevel(GameData.GameModeSo, _activeGEs, 
                _players, _objectPooling, states.m_teamStates, states.m_playerStates);
            availablePositions = result.AvailablePositions;
            if (_hud)
            {
                _hud.gameObject.SetActive(true);
                _hud.Init(states.m_teamStates.Values.ToArray(), states.m_playerStates.Values.ToArray());
            }
            SetInGameState(InGameState.PreGameCountdown);
            
        }
        
    }

    private void OnPreGameTimerUpdated(int timerSecond)
    {
        _hud.UpdatePreGameCountdown(timerSecond);
        if (timerSecond == 0)
        {
            if (NetworkManager.Singleton.IsListening)
            {
                if (NetworkManager.Singleton.ConnectedClients.Count < MinPlayerForOnlineGame)
                {
                    SetInGameState(InGameState.ShuttingDown);
                }
                else
                {
                    SetInGameState(InGameState.Playing);
                }
            }
            else
            {
                SetInGameState(InGameState.Playing);
            }
        }
        if(NetworkManager.Singleton.IsListening)
            UpdatePreGameCountdownClientRpc(timerSecond);
    }

    [ClientRpc]
    private void UpdatePreGameCountdownClientRpc(int second)
    {
        _hud.UpdatePreGameCountdown(second);
    }

    [ClientRpc]
    private void PlaceObjectsClientRpc(LevelObject[] levelObjects, ulong[] playersClientIds, 
        Vector3[] availablePositionsP, TeamState[] teamStates, PlayerState[] playerStates)
    {
        _serverHelper.UpdatePlayerStates(teamStates, playerStates);
        Debug.Log("PlaceObjectsClientRpc");
        _objectPooling ??= new ObjectPooling(_container, _gamePrefabs, _FxPrefabs);
        availablePositions = new List<Vector3>(availablePositionsP);
        AudioManager.Instance.PlayGameplayBGM();
        int playerClientIdIndex = 0;
        for (var i = 0; i < levelObjects.Length; i++)
        {
            var levelObject = levelObjects[i];
            var newObj = _objectPooling.Get(_gamePrefabDict[levelObject.m_prefabName]);
            var transform1 = newObj.transform;
            transform1.position = levelObject.m_position;
            transform1.rotation = levelObject.m_rotation;
            newObj.SetId(levelObject.ID);
            if (newObj is Planet planet2)
            {
                _planets.TryAdd(levelObject.ID, planet2);
            } 
            else if (newObj is Player player)
            {
                if (player)
                {
                    _players ??= new Dictionary<ulong, Player>();
                    var clientNetworkId = playersClientIds[playerClientIdIndex];
                    var pState = _serverHelper.ConnectedPlayerStates[clientNetworkId];
                    player.SetPlayerState(pState, GameData.GameModeSo.maxInFlightMissilesPerPlayer, 
                        _serverHelper.ConnectedTeamStates[pState.teamIndex].teamColour);
                    _players.TryAdd(clientNetworkId, player);
                    playerClientIdIndex++;
                }
            }
            _activeGEs.Add(newObj);
        }
        _menuManager.CloseMenuPanel();
        _hud.gameObject.SetActive(true);
        _hud.Init(_serverHelper.ConnectedTeamStates.Values.ToArray(), 
            _serverHelper.ConnectedPlayerStates.Values.ToArray());
    }

    private int _gameTimeLeft = 0;
    private void OnGameTimeUpdated(int remainingTime)
    {
        if (InGameState == InGameState.Playing)
        {
            _gameTimeLeft = remainingTime;
            _hud.SetTime(remainingTime);
            UpdateGameTimeClientRpc(remainingTime);
            if (remainingTime <= 0)
            {
                SetInGameState(InGameState.GameOver);
            }
        }
    }

    [ClientRpc]
    private void UpdateGameTimeClientRpc(int remainingTimeSecond)
    {
        _hud.SetTime(remainingTimeSecond);
    }

    public void StartGame(GameModeSO gameModeSo)
    {
        //TODO change to switch to in game menu
        //hide current shown menu
        _menuManager.CloseMenuPanel();
        GameData.GameModeSo = gameModeSo;
        _gameMode = gameModeSo.gameMode;
        SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    public void RestartLocalGame()
    {
        _menuManager.HideAnimate(null);
        ResetLevel();
        SetupGame();
    }

    public void QuitToMainMenu()
    {
        _serverHelper.CancelCountdown();
        if (!NetworkManager.Singleton.ShutdownInProgress)
        {
            NetworkManager.Singleton.Shutdown();
        }
        SetInGameState(InGameState.None);
        _menuManager.HideAnimate(null);
        _menuManager.ChangeToMainMenu(MenuSceneName);
        OnClientLeaveSession?.Invoke();

    }
    public void OnObjectHit(Player player, Missile missile)
    {
        var owningPlayerState = missile.GetOwningPlayerState();

        if( owningPlayerState.teamIndex != player.PlayerState.teamIndex )
        {
            var score = GameData.GameModeSo.baseKillScore + missile.GetScore();
            var owningPlayer = _players[owningPlayerState.clientNetworkId];
            owningPlayer.AddKillScore(score);
            var playerStates = _serverHelper.ConnectedPlayerStates.Values.ToArray();
            _hud.UpdateKillsAndScore(owningPlayerState, playerStates);
            UpdateScoreClientRpc(owningPlayerState, playerStates);
        }
        player.OnHitByMissile();
        int teamIndex = player.PlayerState.teamIndex;
        int affectedTeamLive = _serverHelper.GetTeamLive(teamIndex);
        _hud.SetLivesValue(teamIndex, affectedTeamLive);
        UpdateLiveClientRpc(teamIndex, affectedTeamLive);
        if(player.PlayerState.lives <= 0)
        {
            _activeGEs.Remove(player);
            if (_gameMode is 
                GameModeEnum.LocalMultiplayer or GameModeEnum.SinglePlayer)
            {
                player.Reset();
                //TODO workaround, player can't pause game if first player (using keyboard PlayerInput) dead
                if (player.PlayerState.playerIndex == 0)
                {
                    //_playerInput.enabled = true;
                }
            }
            else
            {
                if (IsServer)
                {
                    ResetPlayerClientRpc(player.PlayerState.clientNetworkId);
                    player.Reset();
                }
            }
        }
        else
        {
            bool playerPlaced = false;
            for (int i = 0; i < GameData.GameModeSo.numRetriesToPlacePlayer; i++)
            {
                int randomIndex = Random.Range(0, availablePositions.Count);
                var randomPosition = availablePositions[randomIndex];
                availablePositions.RemoveAt(randomIndex);
                if (!GameUtility.HasLineOfSightToOtherShip(_activeGEs, randomPosition, _players))
                {
                    var teamColor = _serverHelper.ConnectedTeamStates[player.PlayerState.teamIndex]
                        .teamColour;
                    player.PlayerState.position = randomPosition;
                    player.Init(GameData.GameModeSo.maxInFlightMissilesPerPlayer, teamColor);
                    RepositionPlayerClientRpc(player.PlayerState.clientNetworkId, randomPosition, 
                        GameData.GameModeSo.maxInFlightMissilesPerPlayer, teamColor, player.transform.rotation);
                    playerPlaced = true;
                }
                if(playerPlaced) break; 
            }
        }
        CheckForGameOverCondition(_serverHelper.IsGameOver());
    }

    [ClientRpc]
    private void RepositionPlayerClientRpc(ulong clientNetworkId, Vector3 position, 
        int maxInFlightMissile, Vector4 teamColor, Quaternion rotation)
    {
        if (_players.TryGetValue(clientNetworkId, out var player))
        {
            player.Reset();
            player.transform.rotation = rotation;
            player.PlayerState.position = position;
            player.Init(maxInFlightMissile, teamColor);

        }
    }

    [ClientRpc]
    private void ResetPlayerClientRpc(ulong clientNetworkId)
    {
        if (_players.TryGetValue(clientNetworkId, out var player))
        {
            player.Reset();
        }
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(PlayerState playerState, PlayerState[] playerStates)
    {
        _hud.UpdateKillsAndScore(playerState, playerStates);
    }
    
    [ClientRpc]
    private void UpdateLiveClientRpc(int teamIndex, int lives)
    {
        _hud.SetLivesValue(teamIndex, lives);
    }
    
    public void CheckForGameOverCondition(bool isGameOver)
    {
        if (_gameMode is GameModeEnum.SinglePlayer or GameModeEnum.LocalMultiplayer)
        {
            if (isGameOver)
            {
                EndGame();
            }
        }
        else
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (isGameOver)
                {
                    UpdatePlayerStatesClientRpc(_serverHelper.ConnectedTeamStates.Values.ToArray(),
                        _serverHelper.ConnectedPlayerStates.Values.ToArray());
                    EndGame();
                }
            }
        }
    }

    [ClientRpc]
    public void UpdatePlayerStatesClientRpc(TeamState[] teamStates, PlayerState[] playerStates)
    {
        _serverHelper.UpdatePlayerStates(teamStates, playerStates);
        //TODO update game clients UI
    }
    
    private void EndGame()
    {
        SetInGameState(InGameState.GameOver);
    }
    
    private bool SetInGameState(InGameState state)
    {
        Debug.Log("try SetInGameState: "+state);
        if(_inGameState==state)
                return false;
        if(_hud)
            _hud.HideGameStatusContainer();
        _inGameState = state;
        switch (state)
        {
            case InGameState.None:
                _hud.gameObject.SetActive(false);
                ResetLevel();
                break;
            case InGameState.Initializing:
                if(_hud)
                    _hud.gameObject.SetActive(false);
                break;
            case InGameState.PreGameCountdown:
                if(_hud)_hud.gameObject.SetActive(true);
                _serverHelper.CancelCountdown();
                if(this)_serverHelper.StartCoroutineCountdown(this, GameData.GameModeSo.beforeGameCountdownSecond, 
                    OnPreGameTimerUpdated);
                break;
            case InGameState.Playing:
                _hud.gameObject.SetActive(true);
                _serverHelper.CancelCountdown();
                int remainingGameDuration = GameData.GameModeSo.gameDuration;
                if (_gameTimeLeft > 0)
                {
                    remainingGameDuration = _gameTimeLeft;
                }
                _serverHelper.StartCoroutineCountdown(this, remainingGameDuration, OnGameTimeUpdated);
                break;
            case InGameState.ShuttingDown:
                _hud.gameObject.SetActive(true);
                _serverHelper.CancelCountdown();
                _serverHelper.StartCoroutineCountdown(this, GameData.GameModeSo.beforeShutDownCountdownSecond, 
                    OnShutdownCountdownUpdate);
                break;
            case InGameState.LocalPause:
                _hud.gameObject.SetActive(false);
                break;
            case InGameState.GameOver:
                onGameOver.Invoke(_gameMode, ConnectedPlayerStates.Values.ToList());
                
                _serverHelper.CancelCountdown();
                if (NetworkManager.Singleton.IsListening)
                {
                    _serverHelper.StartCoroutineCountdown(this, GameData.GameModeSo.gameOverShutdownCountdown, 
                        OnGameOverShutDownCountdown);
                }
                _hud.gameObject.SetActive(false);
                _menuManager.ShowInGameMenu(AssetEnum.GameOverMenuCanvas);
                break;
        }

        if (state == InGameState.LocalPause)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
        if(!NetworkManager.Singleton.IsListening && _inGameCamera)
            _inGameCamera.enabled = state == InGameState.Playing;
        if(IsServer)
            UpdateInGameStateClientRpc(_inGameState);
        return true;
    }

    private void OnGameOverShutDownCountdown(int countdownSeconds)
    {
        bool willShutdown = countdownSeconds <= 0;
        _menuManager.UpdateGameOverCountdown(countdownSeconds);
        GameOverCountdownClientRpc(countdownSeconds);
        if (willShutdown)
        {
            QuitToMainMenu();
        }
    }

    [ClientRpc]
    private void GameOverCountdownClientRpc(int seconds)
    {
        _menuManager.UpdateGameOverCountdown(seconds);
    }

    private const string NotEnoughPlayer = "Not enough players, shutting down DS in: ";
    private void OnShutdownCountdownUpdate(int countdownSecond)
    {
        bool willShutdown = countdownSecond <= 0;
        
        _hud.UpdateShutdownCountdown(NotEnoughPlayer, countdownSecond);
        //this will kick player too if countdown is 0
        if (IsServer)
        {
            ShutdownClientRpc(countdownSecond);
        }
        if (willShutdown)
        {
            if (!NetworkManager.Singleton.ShutdownInProgress)
            {
                NetworkManager.Singleton.Shutdown();
                DeregisterServer();
                // Application.Quit();
            }
        }
    }

    [ClientRpc]
    private void ShutdownClientRpc(int countdownSecond)
    {
        _hud.UpdateShutdownCountdown(NotEnoughPlayer, countdownSecond);
        if (countdownSecond <= 0)
        {
            //TODO clear/reset level
            //kick player using NetworkManager.Singleton.Shutdown();
            QuitToMainMenu();
        }
    }

    [ClientRpc]
    private void UpdateInGameStateClientRpc(InGameState inGameState)
    {
        if(_hud)
            _hud.HideGameStatusContainer();
        _inGameState = inGameState;
        if (inGameState == InGameState.GameOver)
        {
            _hud.gameObject.SetActive(false);
            _menuManager.ShowInGameMenu(AssetEnum.GameOverMenuCanvas);
        }
        _inGameCamera.enabled = _inGameState == InGameState.Playing;
    }

    private void ResetLevel()
    {
        _gameTimeLeft = 0;
        // Debug.Log($"active game entity count:{_activeGEs.Count}");
        foreach (var ge in _activeGEs)
        {
            ge.Reset();
        }
        _activeGEs.Clear();
        _players.Clear();
        _planets.Clear();
        _hud.Reset();
    }

    [RuntimeInitializeOnLoadMethod]
    private static void SingletonInstanceChecker()
    {
        if (SceneManager.GetActiveScene().name.Equals(MenuSceneName) && _instance == null)
        {
            _instance = Instantiate(AssetManager.Singleton.GameManagerPrefab);
        }
    }

    public void OnPause()
    {
        //TODO if this is online game what happened when a connected player pause the game
        TriggerPauseLocalGame();
    }

    private UnityTransport _unityTransport;

    public void StartAsClient(string address, ushort port, InGameMode inGameMode)
    {
        var initialData = new InitialConnectionData(){ inGameMode = inGameMode };
        reconnect.ConnectAsClient(_unityTransport, address, port, initialData);
    }

    /// <summary>
    /// called by server to notify client that the playerstate and teamstate for the player are updated
    /// </summary>
    /// <param name="teamStates"></param>
    /// <param name="playerStates"></param>
    /// <param name="inGameMode"></param>
    /// <param name="isInGameScene"></param>
    [ClientRpc]
    private void SendConnectedPlayerStateClientRpc(TeamState[] teamStates, 
        PlayerState[] playerStates, InGameMode inGameMode, bool isInGameScene)
    {
        //client side, because the previous playerState only exists in server, clientrpc is called on client
        Debug.Log($"update player state lobby playerState count:{playerStates.Length} teamState count:{teamStates.Length}");

        foreach (PlayerState playerState in playerStates)
        {
            if (playerState.clientNetworkId == NetworkManager.Singleton.LocalClientId)
            {
                playerState.playerId = MultiRegistry.GetApiClient().session.UserId;
            }
        }
        
        _serverHelper.UpdatePlayerStates(teamStates, playerStates);
        _inGameMode = inGameMode;
        GameData.GameModeSo = availableInGameMode[(int)inGameMode];
        
        if (!isInGameScene)
        {
            var lobby = (MatchLobbyMenu)_menuManager.ChangeToMenu(AssetEnum.MatchLobbyMenuCanvas);
            lobby.Refresh(_serverHelper.ConnectedTeamStates, 
                _serverHelper.ConnectedPlayerStates, _clientHelper.ClientNetworkId);
            _menuManager.HideLoading(false);   
        }

    }

    private void UpdateInGamePlayerState(TeamState[] teamStates, PlayerState[] playerStates)
    {
        _serverHelper.UpdatePlayerStates(teamStates, playerStates);
        if (SceneManager.GetActiveScene().name.Equals(MenuSceneName))
        {
            var lobby = (MatchLobbyMenu)_menuManager.ChangeToMenu(AssetEnum.MatchLobbyMenuCanvas);
            lobby.Refresh(_serverHelper.ConnectedTeamStates, 
                _serverHelper.ConnectedPlayerStates, _clientHelper.ClientNetworkId);
        }
    }

    [ServerRpc]
    private void ClientConnectedServerRpc(ulong networkObjectId)
    {
        Debug.Log($"ClientConnectedServerRpc IsServer{IsServer} networkObjectId:{networkObjectId}");
    }
    
    public void StartOnlineGame()
    {
        _serverHelper.CancelCountdown();
        _menuManager.CloseMenuPanel();
        _gameMode = GameData.GameModeSo.gameMode;
        SetGameModeClientRpc(_gameMode);
        if (IsServer && IsOwner)
        {
            var status = NetworkManager.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {GameSceneName} " +
                                 $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }
    }

    [ClientRpc]
    private void SetGameModeClientRpc(GameModeEnum gameMode)
    {
        _gameMode = gameMode;
    }

    public void OnDisable()
    {
        _objectPooling?.DestroyAll();
        _objectPooling?.ClearAll();
        _players?.Clear();
        Debug.Log("GameManager OnDisable");
        _serverHelper.CancelCountdown();
    }

    [ClientRpc]
    public void MissileHitClientRpc(ulong playerClientNetworkId, 
        int missileId, int planetId, Vector3 missileExpPos, Quaternion missileExpRot)
    {
        // Debug.Log($"MissileHitPlanetClientRpc clientNetorkId:{playerClientNetworkId} missileId:{missileId} planetId:{planetId}");
        if (_players.TryGetValue(playerClientNetworkId, out var player))
        {
            player.ExplodeMissile(missileId, missileExpPos, missileExpRot);
        }

        if (planetId>-1 && _planets.TryGetValue(planetId, out var planet))
        {
            planet.OnHitByMissile();
        }
    }

    [ClientRpc]
    public void MissileSyncClientRpc(ulong playerClientNetworkId, int missileID, Vector3 velocity,
        Vector3 position, Quaternion rotation)
    {
        if (_players.TryGetValue(playerClientNetworkId, out var player))
        {
            player.SyncMissile(missileID, velocity, position, rotation);
        }
    }
    public event Action OnClientLeaveSession;
    public event Action OnDeregisterServer;
    public event Action OnRegisterServer;
    public event Action OnRejectBackfill; 
    private async void DeregisterServer()
    {
#if UNITY_SERVER
        OnDeregisterServer?.Invoke();
#endif
        await Task.Delay(150);
        // Application.Quit();
    }
}
