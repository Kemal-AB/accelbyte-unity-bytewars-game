using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTimeComponent : MonoBehaviour
{
    public float m_timeToLive = 2.0f;
    float m_timeAlive = 0.0f;

    void Update()
    {
        m_timeAlive += Time.deltaTime;

        if( m_timeAlive > m_timeToLive )
        {
            Destroy(gameObject);
        }
    }
}
