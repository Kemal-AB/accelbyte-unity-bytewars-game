using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class InGameGameMode : MonoBehaviour
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
    public int m_startNumLives = 1;    

    public Color[] m_teamColours;

    InGameGameState m_gameState;
    GameObject m_levelParent;

    // Start is called before the first frame update
    void Start()
    {
        m_gameState = GameObject.FindObjectOfType<InGameGameState>();
        m_levelParent = new GameObject("LevelObjects");

        // Afif: Remove these lines when they're instantiated in the main menu instead
        PlayerInput spawnedPlayerController1 = PlayerInput.Instantiate(m_playerControllerPrefab, controlScheme: "Keyboard", pairWithDevice: Keyboard.current);
        PlayerInput spawnedPlayerController2 = PlayerInput.Instantiate(m_playerControllerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all.Count > 0 ? Gamepad.all[0] :  null);

        PlayerController[] playerControllers = GameObject.FindObjectsOfType<PlayerController>();

        SpawnLevelObjects();
        SpawnPlayers(playerControllers);
        SetupGame();


        SetGameState(InGameGameState.GameState.Playing);
    }

    void Update()
    {
        if( m_gameState.GetGameState() == InGameGameState.GameState.Playing )
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
        switch( newGameState )
        {
            case InGameGameState.GameState.GameOver:
            {
                m_hud.gameObject.SetActive(false);
                Time.timeScale = 0;
                m_gameOverUI.gameObject.SetActive(true);
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

    GameObject SpawnPlayer(PlayerController playerController, int playerIndex, Vector3 position)
    {
        GameObject newShip = GameObject.Instantiate(m_playerPrefab, position, Quaternion.identity, m_levelParent.transform);

        m_gameState.m_activeObjects.Add(newShip.GetComponent<GameplayObjectComponent>());

        Player playerComponent = newShip.GetComponent<Player>();
        
        if( playerController.gameObject.GetComponent<PlayerState>() != null )
        {
            Destroy(playerController.gameObject.GetComponent<PlayerState>());
        }

        PlayerState playerState = playerController.gameObject.AddComponent<PlayerState>();

        playerController.SetControlledPlayer(playerComponent);
        playerComponent.SetPlayerState(playerState);

        playerComponent.Init(m_teamColours[playerIndex]);

        m_hud.m_playerControllers[ playerIndex ].SetColour(m_teamColours[playerIndex]);

        return newShip;
    }

    void SpawnPlayers(PlayerController[] playerControllers)
    {
        List<GameObject> ships = new List<GameObject>();

        int playerIndex = 0;

        foreach(PlayerController playerController in playerControllers)
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
                    if( !GetHasLineOfSightToOtherShip(randomPosition, ships) )
                    {
                        GameObject newShip = SpawnPlayer(playerController, playerIndex, randomPosition);
                        
                        ships.Add(newShip);

                        playerIndex++;

                        break;
                    }
                }
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
                player.OnHitByObject(sourceObject);
            }
        }
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
        SceneManager.LoadScene("MainMenu",LoadSceneMode.Single);
    }
}
