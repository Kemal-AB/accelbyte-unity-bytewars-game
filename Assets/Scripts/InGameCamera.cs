using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameCamera : MonoBehaviour
{
    public float m_minToMaxMultiplier = 2.5f;
    Vector2 m_minExtents = new Vector2();
    Vector2 m_maxExtents = new Vector2();
    Vector2 m_furthestPositionToFrame = new Vector2();

    InGameGameState m_gameState;

    float m_aspectRatio = 16.0f / 9.0f;

    void Start()
    {
        m_aspectRatio = GetComponent<Camera>().aspect;
        m_minExtents.y = GetComponent<Camera>().orthographicSize;
        m_minExtents.x = m_minExtents.y * m_aspectRatio;

        m_maxExtents = m_minExtents * m_minToMaxMultiplier;

        m_furthestPositionToFrame = m_minExtents;

        m_gameState = GameObject.FindObjectOfType<InGameGameState>();
    }

    void Update()
    {
        m_furthestPositionToFrame = m_minExtents;

        var bufferWidth = 1.0f;

        foreach( var activeObject in m_gameState.m_activeObjects )
        {
            Vector3 objPosition = activeObject.transform.position;
            m_furthestPositionToFrame = Vector2.Max(m_furthestPositionToFrame, new Vector2(Mathf.Abs(objPosition.x) + bufferWidth,Mathf.Abs(objPosition.y)+ bufferWidth));
        }

        foreach( var activeObject in GameObject.FindObjectsOfType<Missile>() )
        {
            Vector3 objPosition = activeObject.transform.position;
            m_furthestPositionToFrame = Vector2.Max(m_furthestPositionToFrame, new Vector2(Mathf.Abs(objPosition.x) + bufferWidth,Mathf.Abs(objPosition.y)+ bufferWidth));
        }

        m_furthestPositionToFrame = Vector2.Min(m_furthestPositionToFrame, m_maxExtents);

        float largestWidthToFrame = Mathf.Max( m_furthestPositionToFrame.y, m_furthestPositionToFrame.x / m_aspectRatio );

        GetComponent<Camera>().orthographicSize = largestWidthToFrame;
    }

}
