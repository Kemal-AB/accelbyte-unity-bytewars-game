using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public GameObject m_popupScoreTextPrefab;
    public float m_expiryTime = 0.0f;
    public float m_skimDeltaForIncrementSeconds = 0.25f;
    public float m_additionalSkimScoreMultiplier = 2.0f;
    public float m_scoreIncrement = 100.0f;
    public float m_maxTimeAlive = 20.0f;

    public float m_skimDistanceThreshold = 1.0f;

    float m_timeSkimmingPlanet = 0.0f;
    float m_timeSkimmingPlanetReward = 0.0f;
    float m_timeAlive = 0.0f;
    float m_score = 0.0f;
    //bool m_expiring = false;

    InGameGameMode m_gameMode;

    PlayerController m_owningPlayerController;

    GameplayObjectComponent m_gameplayObjectComponent;

    void Start()
    {
        m_gameMode = GameObject.FindObjectOfType<InGameGameMode>();
        m_gameplayObjectComponent = GetComponent<GameplayObjectComponent>();
    }

    void Update()
    {
        m_timeAlive += Time.deltaTime;

        if( m_timeAlive > 1.0f )
        {
            if( GetIsSkimmingObject() )
            {
                m_timeSkimmingPlanet += Time.deltaTime;
                m_timeSkimmingPlanetReward += Time.deltaTime;
            }
            else
            {
                m_timeSkimmingPlanet = 0.0f;
                m_timeSkimmingPlanetReward = 0.0f;
            }

            if( m_timeSkimmingPlanetReward > m_skimDeltaForIncrementSeconds )
            {
                m_score += m_scoreIncrement;
                GameObject popupText = GameObject.Instantiate(m_popupScoreTextPrefab);

                PopupTextUIController popupController = popupText.GetComponent<PopupTextUIController>();

                popupController.SetPosition(transform.position);
                
                m_gameMode.OnMissileScoreUpdated(this,m_owningPlayerController, m_score, m_scoreIncrement);
                m_timeSkimmingPlanetReward = 0.0f;
                m_scoreIncrement *= m_additionalSkimScoreMultiplier;
            }
        }
    }

    bool GetIsSkimmingObject()
    {
        foreach( var gameplayObject in m_gameMode.GetGameState().m_activeObjects)
        {
            if( gameplayObject.gameObject != gameObject )
            {
                float distance = Vector3.Distance( gameplayObject.transform.position, gameObject.transform.position );
                float combinedRadius = m_gameplayObjectComponent.m_radius + gameplayObject.m_radius;

                if( distance - combinedRadius < m_skimDistanceThreshold )
                {
                    return true;
                }
            }
        }

        return false;
    }
}
