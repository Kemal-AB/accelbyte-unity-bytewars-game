using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public GameObject m_spawnOnDestroy;
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

    PlayerState m_owningPlayerState;

    GameplayObjectComponent m_gameplayObjectComponent;
    UnityEngine.Color m_colour;
    MotionComponent m_motionComponent;

    void Start()
    {
        List<Vector3> outerVerts = new List<Vector3>();
        outerVerts.Add(new Vector3(0, 20, 0));
        outerVerts.Add(new Vector3(10, 0, 0));
        outerVerts.Add(new Vector3(10, -20, 0));
        outerVerts.Add(new Vector3(0, -20, 0));


        List<Vector3> innerVerts = new List<Vector3>();
        innerVerts.Add(new Vector3(0, 10, 0));
        innerVerts.Add(new Vector3(5, 0, 0));
        innerVerts.Add(new Vector3(5, -15, 0));
        innerVerts.Add(new Vector3(0, -15, 0));

        NeonObject playerGeometry = new NeonObject(outerVerts, innerVerts);

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.Clear();
        mesh.vertices = playerGeometry.vertexList.ToArray();
        mesh.uv = playerGeometry.uvList.ToArray();
        mesh.triangles = playerGeometry.indexList.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    public void Init(PlayerState owningPlayerState)
    {
        m_owningPlayerState = owningPlayerState;
        m_gameMode = GameObject.FindObjectOfType<InGameGameMode>();
        m_gameplayObjectComponent = GetComponent<GameplayObjectComponent>();
        m_motionComponent = GetComponent<MotionComponent>();
        m_colour = m_owningPlayerState.m_teamColour;
        GetComponent<Renderer>().material.SetVector("_PlayerColour", m_colour);

        // Tie the particle system lifetime to missile Max Time Alive so the particle emission falloff curve lines up to when the missile dies.
        // Note: the curve is tuned so the effect dies out across the range [0.85, 1], which works well for a max lifetime of ~20 seconds but may need readjusted if that value changes.
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();
        if (particleSystem)
        {
            // particle system has to stop to change duration
            particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            {
                var main = particleSystem.main;
                main.duration = m_maxTimeAlive;
            }
            particleSystem.Play(false);
        }
    }

    void Update()
    {
        m_timeAlive += Time.deltaTime;

        if(m_motionComponent == null) GameDirector.Instance.WriteToConsole("Missie m_motionComponent is NULL");
        if ( m_motionComponent.GetMotionState() == MotionComponent.State.FlaggedForDestruction )
        {
            GameDirector.Instance.WriteToConsole("Destroying missile:");
            OnDestroyMissile();            
        }
        else if( m_timeAlive > 1.0f )
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

                popupController.Init(transform.position,m_colour,m_scoreIncrement.ToString());
                
                m_gameMode.OnMissileScoreUpdated(this,m_owningPlayerState, m_score, m_scoreIncrement);
                m_timeSkimmingPlanetReward = 0.0f;
                m_scoreIncrement *= m_additionalSkimScoreMultiplier;
            }

            if( m_timeAlive > m_maxTimeAlive )
            {
                OnDestroyMissile();
            }
        }
    }

    void OnDestroyMissile()
    {
        if(m_spawnOnDestroy != null )
        {
            GameObject explosion = GameObject.Instantiate(m_spawnOnDestroy, transform.position, transform.rotation);
            explosion.GetComponent<Renderer>().material.SetVector("_Colour", m_colour);
        }
        m_gameMode.OnMissileDestroyed(this);
        Destroy(this.gameObject);
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

    public float GetScore()
    {
        return m_score;
    }

    public PlayerState GetOwningPlayerState()
    {
        return m_owningPlayerState;
    }
}
