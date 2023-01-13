using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionComponent : MonoBehaviour
{
    Vector3 m_velocity = Vector3.zero;
    float m_mass = 1.0f;
    InGameGameState m_gameState;
    GameplayObjectComponent m_gameplayObjectComponent;

    void Start()
    {
        m_gameplayObjectComponent = GetComponent<GameplayObjectComponent>();
        m_gameState = GameObject.FindObjectOfType<InGameGameState>();
        m_mass = m_gameplayObjectComponent.m_mass;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 totalForceThisFrame = GetTotalForceOnObject();
        Vector3 acceleration = totalForceThisFrame / m_mass;
        m_velocity += acceleration * Time.deltaTime;
        transform.position += m_velocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(m_velocity, Vector3.forward) * Quaternion.AngleAxis(90.0f, Vector3.right);
    }

    public void SetVelocity(Vector3 velocity)
    {
        m_velocity = velocity;
    }
    Vector3 GetTotalForceOnObject()
    {
        Vector3 totalForce = Vector3.zero;

        foreach(GameplayObjectComponent activeObject in m_gameState.m_activeObjects)
        {
            if( activeObject.transform.position == m_gameplayObjectComponent.transform.position)
            {
                continue;
            }

            float force = 50.0f * m_gameplayObjectComponent.m_mass * activeObject.m_mass;
            float distanceBetween = Vector3.Distance(transform.position, activeObject.transform.position) * 100.0f;
            force /= Mathf.Pow(distanceBetween, 1.5f);

            Vector3 direction = (activeObject.transform.position - transform.position).normalized;

            totalForce += direction * 0.01f * force;
        }

        return totalForce;
    }
}
