using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Player m_controlledPlayer;

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
        if( m_controlledPlayer != null )
        {
            m_controlledPlayer.FireMissile();
        }
    }

    void OnRotateShip(InputValue amount)
    {
        if( m_controlledPlayer != null )
        {
            m_controlledPlayer.SetNormalisedRotateSpeed(amount.Get<float>());
        }
    }

    void OnChangePower(InputValue amount)
    {
        if( m_controlledPlayer != null )
        {
            m_controlledPlayer.SetNormalisedPowerChangeSpeed(amount.Get<float>());
        }
    }

    void OnOpenPauseMenu(InputValue amount)
    {
        GameObject.FindObjectOfType<InGameGameMode>().OnPausePressed();
    }
}
