using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Player m_controlledPlayer;

    float m_rotateInputValue = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_controlledPlayer.transform.Rotate(Vector3.forward, Time.deltaTime * m_rotateInputValue * -100.0f);
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
        m_rotateInputValue = amount.Get<float>();
    }

    void OnOpenPauseMenu(InputValue amount)
    {
        GameObject.FindObjectOfType<InGameGameMode>().OnPausePressed();
    }

}
