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

    public float m_timeLeft = 600.0f;

    public List<GameplayObjectComponent> m_activeObjects = new List<GameplayObjectComponent>();

    public void SetGameState(GameState newGameState)
    {
        m_gameState = newGameState;
    }

    public GameState GetGameState()
    {
        return m_gameState;
    }

    void Update()
    {
        
    }
}
