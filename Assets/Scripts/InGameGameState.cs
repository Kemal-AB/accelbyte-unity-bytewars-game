using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class InGameGameState : MonoBehaviour
{
    public enum GameState
    {
        None,
        Playing,
        Paused,
        GameOver
    }

    GameState m_gameState = GameState.None;

    public float m_timeLeft = 600.0f;

    public List<GameplayObjectComponent> m_activeObjects = new List<GameplayObjectComponent>();
    public PlayerController[] m_playerControllers;
    public List<Player> m_players = new List<Player>();

    public void SetGameState(GameState newGameState)
    {
        m_gameState = newGameState;
    }

    public GameState GetGameState()
    {
        return m_gameState;
    }

    public void OnObjectRemovedFromWorld(GameplayObjectComponent objectToRemove)
    {
        m_activeObjects.Remove(objectToRemove);
    }

    void Update()
    {
        
    }
}
