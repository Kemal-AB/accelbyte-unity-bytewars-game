using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Player m_controlledPlayer;

    void Start()
    {
        
    }

    public PlayerState GetPlayerState()
    {
        return GetComponent<PlayerState>();
    }

    public void SetControlledPlayer(Player player)
    {
        m_controlledPlayer = player;
    }

    void OnFire(InputValue amount)
    {
        m_controlledPlayer.FireMissile();
    }

    void OnRotateShip(InputValue amount)
    {
        m_controlledPlayer.SetNormalisedRotateSpeed(amount.Get<float>());
    }

    void OnOpenPauseMenu(InputValue amount)
    {
        GameObject.FindObjectOfType<InGameGameMode>().OnPausePressed();
    }

    void OnChangePower(InputValue amount)
    {
        m_controlledPlayer.SetNormalisedPowerChangeSpeed(amount.Get<float>());
    }
}
