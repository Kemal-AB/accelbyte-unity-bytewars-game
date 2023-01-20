using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PowerBarUIController m_powerBarUIPrefab;
    PowerBarUIController m_powerBarUI;
    public GameObject m_missilePrefab;
    public GameObject m_fireMissileEffectPrefab;
    public GameObject m_shipDestroyedEffectPrefab;
    public float m_minMissileSpeed = 1.5f;
    public float m_maxMissileSpeed = 9.0f;

    float m_normalisedRotateSpeed = 0.0f;
    float m_normalisedPowerChangeSpeed = 0.0f;

    float m_firePowerLevel = 0.5f;

    PlayerState m_playerState;

    void Start()
    {
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, Time.deltaTime * m_normalisedRotateSpeed * -100.0f);
        if( m_normalisedPowerChangeSpeed != 0.0f )
        {
            ChangePowerLevel(m_normalisedPowerChangeSpeed);
        }
    }

    public void SetPlayerState(PlayerState playerState)
    {
        m_playerState = playerState;
    }

    public void Init(Color colour)
    {
        m_powerBarUI = GameObject.Instantiate(m_powerBarUIPrefab, transform.position, Quaternion.identity,transform);
        m_powerBarUI.Init();
        m_powerBarUI.SetPosition(transform.position);
        gameObject.GetComponent<Renderer>().material.color = colour;        
        m_powerBarUI.SetColour(colour);
        m_powerBarUI.SetPercentageFraction(m_firePowerLevel,false);
    }

    public void FireMissile()
    {
        Vector3 missileSpawnPosition = transform.position + transform.up * 0.25f;
        GameObject missile = GameObject.Instantiate(m_missilePrefab, missileSpawnPosition, transform.rotation);
        GameObject.Instantiate(m_fireMissileEffectPrefab, missileSpawnPosition, transform.rotation);        

        MotionComponent motionComponent = missile.GetComponent<MotionComponent>();

        motionComponent.SetVelocity(transform.up * Mathf.Lerp(m_minMissileSpeed, m_maxMissileSpeed, m_firePowerLevel));
    }

    public void OnHitByObject(GameplayObjectComponent otherObject)
    {
        this.m_playerState.m_numLivesLeft--;

        GameObject.Instantiate(m_shipDestroyedEffectPrefab, transform.position, transform.rotation);                
    }

    public void SetNormalisedRotateSpeed(float normalisedRotateSpeed)
    {
        m_normalisedRotateSpeed = normalisedRotateSpeed;
    }
    public void SetNormalisedPowerChangeSpeed(float normalisedPowerChangeSpeed)
    {
        m_normalisedPowerChangeSpeed = normalisedPowerChangeSpeed;
    }

    void ChangePowerLevel(float normalisedChangeSpeed)
    {
        m_firePowerLevel = Mathf.Clamp01( m_firePowerLevel + normalisedChangeSpeed * Time.deltaTime );
        m_powerBarUI.SetPercentageFraction(m_firePowerLevel);
    }

}
