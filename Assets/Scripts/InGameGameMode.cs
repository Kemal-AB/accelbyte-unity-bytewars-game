using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class InGameGameMode : NetworkBehaviour
{
    public InGameHUDController m_hud;
    public GameObject m_gameOverUI;
    public GameObject m_pauseMenuGameObject;


    public GameObject[] m_objectsToSpawn;
    public GameObject m_playerPrefab;
    public GameObject m_playerControllerPrefab;
    public Bounds m_bounds;
    public int m_numLevelObjectsToSpawn = 7;
    public int m_numRetriesToPlaceLevelObject = 5;
    public int m_numRetriesToPlacePlayer = 100;
    public float m_gameDuration = 600.0f;
    public float m_baseKillScore = 500.0f;
    public int m_startNumLives = 2;    

    public Color[] m_teamColours;
    public string[] m_playerNames;

    InGameGameState m_gameState;
    GameObject m_levelParent;

    bool m_gameInitialized = false;
    // Start is called before the first frame update
    void Start()
    {

        m_gameState = GameObject.FindObjectOfType<InGameGameState>();
        m_levelParent = new GameObject("LevelObjects");

        if (GameDirector.Instance.GameMode == GameDirector.E_GameMode.SINGLE_PLAYER)
        {
            Random.InitState(Random.Range(0, 1000));

            PlayerInput spawnedPlayerController1 = PlayerInput.Instantiate(m_playerControllerPrefab, controlScheme: "Keyboard", pairWithDevice: Keyboard.current);
            PlayerInput spawnedPlayerController2 = PlayerInput.Instantiate(m_playerControllerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all.Count > 0 ? Gamepad.all[0] : null);

            m_gameState.m_playerControllers = GameObject.FindObjectsOfType<PlayerController>();

        }
        else if (GameDirector.Instance.GameMode == GameDirector.E_GameMode.MULTI_PLAYER)
        {
            Random.InitState(GameDirector.Instance.RandomMasterSeed.Value);

            if (NetworkManager.Singleton.IsServer)
            {
                PlayerInput spawnedPlayerController1 = PlayerInput.Instantiate(m_playerControllerPrefab, controlScheme: "Keyboard", pairWithDevice: Keyboard.current);
                PlayerInput spawnedPlayerController2 = PlayerInput.Instantiate(m_playerControllerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all.Count > 0 ? Gamepad.all[0] : null);

                m_gameState.m_playerControllers = GameObject.FindObjectsOfType<PlayerController>();
            }
        }

        GameDirector.Instance.WriteToConsole("Init the Random MasterSeed to:" + GameDirector.Instance.RandomMasterSeed.Value);

        SpawnLevelObjects();

        SpawnLocalPlayers(m_gameState.m_playerControllers);

        SetupGame();

        SetGameState(InGameGameState.GameState.Playing);

        m_gameInitialized = true;

        if (GameDirector.Instance.GameMode == GameDirector.E_GameMode.MULTI_PLAYER && NetworkManager.Singleton.IsServer)
            InitPlayerControllerClientRPC();
    }

    public void ServerStartGame()
    {
        GameDirector.Instance.WriteToConsole("Server Launching Game");
    }

    void Update()
    {
        if(m_gameInitialized && m_gameState.GetGameState() == InGameGameState.GameState.Playing )
        {
            m_gameState.m_timeLeft = Mathf.Max(0.0f, m_gameState.m_timeLeft - Time.deltaTime );

            m_hud.m_timeController.SetScoreValue( Mathf.CeilToInt(m_gameState.m_timeLeft) );

            if( m_gameState.m_timeLeft <= 0.0f )
            {
                SetGameState( InGameGameState.GameState.GameOver );
            }
        }
    }

    void SetGameState(InGameGameState.GameState newGameState)
    {
        if( newGameState == m_gameState.GetGameState() )
        {
            return;
        }

        switch( newGameState )
        {
            case InGameGameState.GameState.GameOver:
            {
                m_hud.gameObject.SetActive(false);
                Time.timeScale = 0;
                m_gameOverUI.gameObject.SetActive(true);
                //PlayerList_Here
                m_gameOverUI.gameObject.GetComponent<GameOverScreenController>().SetPlayers(GetGameState().m_players);
            }
            break;
        };

        if( newGameState == InGameGameState.GameState.Paused )
        {
            Time.timeScale = 0;
            m_pauseMenuGameObject.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            m_pauseMenuGameObject.SetActive(false);
        }

        m_gameState.SetGameState(newGameState);
    }

    void SetupGame()
    {
        m_gameState.m_timeLeft = m_gameDuration;
    }

    void SpawnLevelObjects()
    {
        for( int i = 0; i < m_numLevelObjectsToSpawn; i++)
        {
            GameObject randomObject = m_objectsToSpawn[Random.Range(0, m_objectsToSpawn.Length)];

            for (int r = 0; r < m_numRetriesToPlaceLevelObject; r++)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(m_bounds.min.x, m_bounds.max.x),
                    Random.Range(m_bounds.min.y, m_bounds.max.y),
                    0.0f
                );

                if (!IsTooCloseToOtherObject(randomPosition, randomObject))
                {
                    GameObject planet = GameObject.Instantiate(randomObject, randomPosition, Quaternion.identity, m_levelParent.transform);
                    m_gameState.m_activeObjects.Add(planet.GetComponent<GameplayObjectComponent>());

                    break;
                }
            }
        }
    }

    void SpawnLocalPlayers(PlayerController[] playerControllers)
    {
        List<GameObject> ships = new List<GameObject>();

        int playerIndex = 0;

        foreach (PlayerController playerController in playerControllers)
        {
            for (int i = 0; i < m_numRetriesToPlacePlayer; i++)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(m_bounds.min.x, m_bounds.max.x),
                    Random.Range(m_bounds.min.y, m_bounds.max.y),
                    0.0f
                );

                if (!IsTooCloseToOtherObject(randomPosition, m_playerPrefab))
                {
                    if (!GetHasLineOfSightToOtherShip(randomPosition, ships))
                    {
                        GameObject newShip = SpawnPlayer(playerController, playerIndex, randomPosition);

                        if (newShip != null)
                            ships.Add(newShip);

                        playerIndex++;

                        break;
                    }
                }
            }
        }
    }

    GameObject SpawnPlayer(PlayerController playerController, int playerIndex, Vector3 position)
    {
        GameObject newShip = null;

        if (GameDirector.Instance.GameMode == GameDirector.E_GameMode.SINGLE_PLAYER)
        {
            newShip = GameObject.Instantiate(m_playerPrefab, position, Quaternion.identity, m_levelParent.transform);

            m_gameState.m_activeObjects.Add(newShip.GetComponent<GameplayObjectComponent>());

            Player playerComponent = newShip.GetComponent<Player>();

            if (playerController.gameObject.GetComponent<PlayerState>() != null)
            {
                Destroy(playerController.gameObject.GetComponent<PlayerState>());
            }

            PlayerState playerState = playerController.gameObject.AddComponent<PlayerState>();

            playerController.SetControlledPlayer(playerComponent);
            playerComponent.SetPlayerState(playerState);
            playerState.m_numLivesLeft = m_startNumLives;
            playerState.m_teamColour = m_teamColours[playerIndex];
            playerState.m_playerName = m_playerNames[playerIndex];

            playerComponent.Init(playerState.m_teamColour);

            m_gameState.m_players.Add(playerComponent);

            m_hud.m_playerControllers[playerIndex].SetColour(playerState.m_teamColour);
        }
        else
        {

            if (NetworkManager.Singleton.IsServer)
            {
                ++playerIndex;

                GameDirector.Instance.WriteToConsole("Server SpawningPlayer: " + playerIndex);

                newShip = GameObject.Instantiate(m_playerPrefab, position, Quaternion.identity, m_levelParent.transform);

                newShip.GetComponent<NetworkObject>().SpawnAsPlayerObject((ulong)(playerIndex), true);

                m_gameState.m_activeObjects.Add(newShip.GetComponent<GameplayObjectComponent>());

                Player playerComponent = newShip.GetComponent<Player>();

                if (playerController.gameObject.GetComponent<PlayerState>() != null)
                {
                    Destroy(playerController.gameObject.GetComponent<PlayerState>());
                }

                PlayerState playerState = playerController.gameObject.AddComponent<PlayerState>();

                playerController.SetControlledPlayer(playerComponent);
                playerComponent.SetPlayerState(playerState);
                playerState.m_numLivesLeft = m_startNumLives;
                playerState.m_teamColour = m_teamColours[playerIndex];
                playerState.m_playerName = m_playerNames[playerIndex];

                playerComponent.InitServerPlayer(playerState.m_teamColour);

                m_gameState.m_players.Add(playerComponent);
            }

        }

        return newShip;
    }

    [ClientRpc]
    void InitPlayerControllerClientRPC()
    {
        if (NetworkManager.Singleton.IsServer) return;

        m_gameState = GameObject.FindObjectOfType<InGameGameState>();
        m_levelParent = new GameObject("LevelObjects");

        GameDirector.Instance.WriteToConsole("InitPlayer - ClientID: " + NetworkManager.Singleton.LocalClientId);
        
        Player myPlayerComponent = NetworkManager.LocalClient.PlayerObject.GetComponent<Player>();
        GameplayObjectComponent myGoComponent  = NetworkManager.LocalClient.PlayerObject.GetComponent<GameplayObjectComponent>();
       
        m_gameState.m_activeObjects.Add(myGoComponent);
        PlayerInput spawnedPlayerController1 = PlayerInput.Instantiate(m_playerControllerPrefab, controlScheme: "Keyboard", pairWithDevice: Keyboard.current);
        m_gameState.m_playerControllers = GameObject.FindObjectsOfType<PlayerController>();
        m_gameState.m_players.Add(myPlayerComponent);

        if (m_gameState.m_playerControllers[0].gameObject.GetComponent<PlayerState>() != null)
        {
            Destroy(m_gameState.m_playerControllers[0].gameObject.GetComponent<PlayerState>());
        }

        PlayerState playerState = m_gameState.m_playerControllers[0].gameObject.AddComponent<PlayerState>();

        m_gameState.m_playerControllers[0].SetControlledPlayer(myPlayerComponent);
        myPlayerComponent.SetPlayerState(playerState);
        playerState.m_numLivesLeft = m_startNumLives;
        playerState.m_teamColour = m_teamColours[0];
        playerState.m_playerName = m_playerNames[0];


        myPlayerComponent.Init(playerState.m_teamColour);

        m_hud.m_playerControllers[0].SetColour(playerState.m_teamColour);


        foreach (Player OtherPlayerComponent in GameObject.FindObjectsOfType<Player>())
        {
            //Add the remote player to GameObjects list too
            if (OtherPlayerComponent != myPlayerComponent)
            {
                m_gameState.m_activeObjects.Add(OtherPlayerComponent.gameObject.GetComponent<GameplayObjectComponent>());

                PlayerState OtherplayerState = m_gameState.m_playerControllers[0].gameObject.AddComponent<PlayerState>();

                OtherPlayerComponent.SetPlayerState(OtherplayerState);
                OtherplayerState.m_numLivesLeft = m_startNumLives;
                OtherplayerState.m_teamColour = m_teamColours[1];
                OtherplayerState.m_playerName = m_playerNames[1];

                OtherPlayerComponent.Init(OtherplayerState.m_teamColour);
                m_hud.m_playerControllers[1].SetColour(OtherplayerState.m_teamColour);
            }
        }
    }

    bool IsTooCloseToOtherObject(Vector3 testPosition, GameObject testObject)
    {
        float testObjectRadius = testObject.GetComponent<Planet>()?.m_scale ?? 1.0f;

        for (int i = 0; i < m_levelParent.transform.childCount; i++)
        {
            Transform existingObject = m_levelParent.transform.GetChild(i);
            float distToExistingObject = Vector3.Distance(testPosition, existingObject.position);

            float existingObjectRadius = existingObject.GetComponent<Planet>()?.GetRadius() ?? 1.0f;

            float combinedObjectRadius = testObjectRadius + existingObjectRadius;

            if (distToExistingObject < combinedObjectRadius + 2.0f)
            {
                return true;                 
            }
        }

        return false;
    }

    bool GetHasLineOfSightToOtherObject(Vector3 shipPosition, Vector3 otherPosition, float testRadius)
    {
        Vector3 toOtherPosition = otherPosition - shipPosition;
        Vector3 linePerp = Vector3.Cross(toOtherPosition, Vector3.forward).normalized;

        for (int i = 0; i < m_levelParent.transform.childCount; i++)
        {
            Transform existingObject = m_levelParent.transform.GetChild(i);

            if( existingObject.tag == "Player" )
            {
                continue;
            }

            float existingObjectRadius = existingObject.GetComponent<Planet>()?.GetRadius() ?? 1.0f;
            float combinedRadius = existingObjectRadius + testRadius;

            float distToLine = Mathf.Abs(Vector3.Dot(existingObject.transform.position - shipPosition, linePerp));

            if( distToLine < combinedRadius )
            {
                float distAlongLine = Vector3.Dot(existingObject.transform.position - shipPosition, toOtherPosition.normalized);

                if( distAlongLine >= 0.0f && distAlongLine <= toOtherPosition.magnitude)
                {
                    return false;
                }
            }

            if( Vector3.Distance(shipPosition,existingObject.transform.position) < testRadius)
            {
                return false;
            }
        }

        return true;
    }

    bool GetHasLineOfSightToOtherShip(Vector3 shipPosition, List<GameObject> otherShips)
    {
        foreach (GameObject ship in otherShips)
        {
            if (GetHasLineOfSightToOtherObject(shipPosition, ship.transform.position, 0.5f))
            {
                return true;
            }
        }

        return false;
    }
    bool GetHasLineOfSightToOtherShip(Vector3 shipPosition, List<Player> otherShips)
    {
        foreach (Player player in otherShips)
        {
            GameObject ship = player.gameObject;
            if (GetHasLineOfSightToOtherObject(shipPosition, ship.transform.position, 0.5f))
            {
                return true;
            }
        }

        return false;
    }
    public InGameGameState GetGameState()
    {
        return m_gameState;
    }

    public void OnObjectHit(GameplayObjectComponent hitObject, GameplayObjectComponent sourceObject)
    {
        if( hitObject.tag == "Player" )
        {
            Player player = hitObject.GetComponent<Player>();

            if( player != null )
            {
                Missile missile = sourceObject.GetComponent<Missile>();
                if( missile != null )
                {
                    PlayerState owningPlayerState = missile.GetOwningPlayerState();

                    if( owningPlayerState != player.GetPlayerState() )
                    {
                        owningPlayerState.m_playerScore += m_baseKillScore + missile.GetScore();
                        owningPlayerState.m_killCount ++;

                        m_hud.m_playerControllers[ owningPlayerState.m_playerIndex ].SetScoreValue( (int)owningPlayerState.m_playerScore );
                        m_hud.m_playerControllers[ owningPlayerState.m_playerIndex ].SetKillsValue( owningPlayerState.m_killCount );
                    }
                }
                
                player.OnHitByObject(sourceObject);

                if(player.GetPlayerState().m_numLivesLeft <= 0)
                {
                    //Plaer Dead, destroyed
                    m_gameState.OnObjectRemovedFromWorld(hitObject);

                    if (GameDirector.Instance.GameMode == GameDirector.E_GameMode.SINGLE_PLAYER)
                        Destroy(player.gameObject);
                    else
                        if (NetworkManager.Singleton.IsServer)
                            NetworkManager.Destroy(player.gameObject);
                }else
                {//Reposition Player
                 // if ((GameDirector.Instance.GameMode == GameDirector.E_GameMode.SINGLE_PLAYER) || (GameDirector.Instance.GameMode == GameDirector.E_GameMode.MULTI_PLAYER && NetworkManager.Singleton.IsServer))
                 // {
                    bool playerPlaced = false;
                    for (int i = 0; i < m_numRetriesToPlacePlayer; i++)
                    {
                        Vector3 randomPosition = new Vector3(
                            Random.Range(m_bounds.min.x, m_bounds.max.x),
                            Random.Range(m_bounds.min.y, m_bounds.max.y),
                            0.0f
                        );

                        if (!IsTooCloseToOtherObject(randomPosition, m_playerPrefab))
                        {
                            if (!GetHasLineOfSightToOtherShip(randomPosition, m_gameState.m_players))
                            {
                                //player.transform.position = randomPosition;
                                player.gameObject.transform.position = randomPosition;
                                playerPlaced = true;
                            }
                        }

                        if(playerPlaced) break; 
                    }
                   // }
                }
            }
        }
        else if (hitObject.tag == "Planet")
        {
            Planet planet = hitObject.GetComponent<Planet>();
            if (planet)
            {
                planet.OnHitByMissile();
            }
        }

        CheckForGameOverCondition();
    }

    public void OnMissileDestroyed(Missile missile)
    {

    }

    public void OnMissileScoreUpdated(Missile missile,PlayerState owningPlayerState, float score, float scoreIncrement)
    {

    }

    public void CheckForGameOverCondition()
    {

        int numPlayersAlive = 0;
        if (GameDirector.Instance.GameMode == GameDirector.E_GameMode.SINGLE_PLAYER)
        {
            foreach (Player player in m_gameState.m_players)
            {
                if (player.GetPlayerState().m_numLivesLeft > 0)
                {
                    numPlayersAlive++;
                }
            }

            if (numPlayersAlive <= 1)
            {
                EndGame();
            }
        }else
        {
            if (NetworkManager.Singleton.IsServer)
            {
                foreach (Player player in m_gameState.m_players)
                {
                    if (player.GetPlayerState().m_numLivesLeft > 0)
                    {
                        numPlayersAlive++;
                    }
                }

                if (numPlayersAlive <= 1)
                {
                    GameDirector.Instance.WriteToConsole("Server Game Ended");
                    EndGameClientRPC();
                    EndGame();
                    
                }
            }
        }
    }

    void EndGame()
    {
        GameDirector.Instance.WriteToConsole("Local Game Ended");
        SetGameState(InGameGameState.GameState.GameOver);
    }
    [ClientRpc]
    void EndGameClientRPC()
    {
        if (IsOwner) return;
        GameDirector.Instance.WriteToConsole("CLient Game Ended");
        SetGameState(InGameGameState.GameState.GameOver);
    }

    public void OnPausePressed()
    {
        if( m_gameState.GetGameState() == InGameGameState.GameState.Playing)
        {
            SetGameState(InGameGameState.GameState.Paused);
        }
    }

    public void OnResumePressed()
    {
        SetGameState(InGameGameState.GameState.Playing);
    }

    public void OnRestartPressed()
    {
        SceneManager.LoadScene("GalaxyWorld",LoadSceneMode.Single);        
    }

    public void OnQuitPressed()
    {
        string sceneName = "MainMenu";
        // SceneManager.LoadScene("MainMenu",LoadSceneMode.Single);
        MenuManager.Instance.ChangeToMainMenu(sceneName);
    }
}
