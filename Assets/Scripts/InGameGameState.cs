using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameGameState : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    GameState m_gameState = GameState.Playing;
    public GameObject m_pauseMenuGameObject;

    public float m_timeLeft = 600.0f;

    public List<GameplayObjectComponent> m_activeObjects = new List<GameplayObjectComponent>();

    public void SetGameState(GameState newGameState)
    {
        m_gameState = newGameState;

        if( m_gameState == GameState.Paused )
        {
            Time.timeScale = 0;
            m_pauseMenuGameObject.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            m_pauseMenuGameObject.SetActive(false);
        }
    }

    public GameState GetGameState()
    {
        return m_gameState;
    }

    void Update()
    {
        
    }
}
