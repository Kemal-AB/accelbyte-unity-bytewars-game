using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionComponent : MonoBehaviour
{
    public GameObject m_spawnOnDestroy;

    Vector3 m_velocity = Vector3.zero;
    float m_mass = 1.0f;
    InGameGameMode m_gameMode;
    GameplayObjectComponent m_gameplayObjectComponent;

    public enum State
    {
        ClearShip,
        Alive,
        FlaggedForDestruction
    }

    State m_state = State.ClearShip;

    void Start()
    {
        m_gameplayObjectComponent = GetComponent<GameplayObjectComponent>();
        m_gameMode = GameObject.FindObjectOfType<InGameGameMode>();
        m_mass = m_gameplayObjectComponent.m_mass;
    }

    void Update()
    {
        Vector3 totalForceThisFrame = GetTotalForceOnObject();
        Vector3 acceleration = totalForceThisFrame / m_mass;
        m_velocity += acceleration * Time.deltaTime;
        transform.position += m_velocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(m_velocity, Vector3.forward) * Quaternion.AngleAxis(90.0f, Vector3.right);

        GameplayObjectComponent intersectingObject = GetIntersectingObject();

        
        if ( m_state == State.ClearShip )
        {
            if (intersectingObject == null)
            {
                m_state = State.Alive;
            }
        }
        else if( intersectingObject != null )
        {
            Debug.LogWarning("FlaggedForDestruction " + m_state.ToString());
            m_gameMode.OnObjectHit(intersectingObject, m_gameplayObjectComponent);
            m_state = State.FlaggedForDestruction;
        }
    }

    public State GetMotionState()
    {
        return m_state;
    }

    public void SetVelocity(Vector3 velocity)
    {
        m_velocity = velocity;
    }
    Vector3 GetTotalForceOnObject()
    {
        Vector3 totalForce = Vector3.zero;

        foreach (GameplayObjectComponent activeObject in m_gameMode.GetGameState().m_activeObjects)
        {
            if (activeObject.transform.position == m_gameplayObjectComponent.transform.position)
            {
                continue;
            }

            float force = 50.0f * m_gameplayObjectComponent.m_mass * activeObject.m_mass;
            float distanceBetween = Vector3.Distance(transform.position, activeObject.transform.position);
            force /= Mathf.Pow(distanceBetween * 100.0f, 1.5f);

            Vector3 direction = (activeObject.transform.position - transform.position).normalized;

            totalForce += direction * 0.01f * force;
        }

        return totalForce;        
    }

    GameplayObjectComponent GetIntersectingObject()
    {
        foreach (GameplayObjectComponent activeObject in m_gameMode.GetGameState().m_activeObjects)
        {
            if (activeObject == m_gameplayObjectComponent)
            {
                continue;
            }

            float distanceBetween = Vector3.Distance(transform.position, activeObject.transform.position);
            float combinedRadius = activeObject.m_radius + m_gameplayObjectComponent.m_radius;

            if (distanceBetween <= combinedRadius)
            {
                return activeObject;
            }
        }

        return null;
    }
}
