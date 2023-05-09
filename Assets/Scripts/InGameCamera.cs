using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameCamera : MonoBehaviour
{
    [SerializeField]
    private float m_minToMaxMultiplier;
    Vector2 m_minExtents = new Vector2();
    Vector2 m_maxExtents = new Vector2();
    Vector2 m_furthestPositionToFrame = new Vector2();
    float m_aspectRatio = 16.0f / 9.0f;
    [SerializeField]
    private Camera _camera;

    private float largestWidthToFrame = 0;

    [SerializeField] private GameManager _gm;
    void Start()
    {
        m_aspectRatio = _camera.aspect;
        m_minExtents.y = _camera.orthographicSize;
        m_minExtents.x = m_minExtents.y * m_aspectRatio;

        m_maxExtents = m_minExtents * m_minToMaxMultiplier;

        m_furthestPositionToFrame = m_minExtents;
    }

    void Update()
    {
        m_furthestPositionToFrame = m_minExtents;

        var bufferWidth = 1.0f;

        foreach( var activeObject in _gm.ActiveGEs )
        {
            if(activeObject && activeObject is Missile)
            {
                Vector3 objPosition = activeObject.transform.position;
                m_furthestPositionToFrame = Vector2.Max(m_furthestPositionToFrame, 
                    new Vector2(Mathf.Abs(objPosition.x) + bufferWidth, 
                        Mathf.Abs(objPosition.y) + bufferWidth));
            }
            
        }

        // foreach( var activeObject in GameObject.FindObjectsOfType<Missile>() )
        // {
        //     if (activeObject)
        //     {
        //         Vector3 objPosition = activeObject.transform.position;
        //         m_furthestPositionToFrame = Vector2.Max(m_furthestPositionToFrame, new Vector2(Mathf.Abs(objPosition.x) + bufferWidth,Mathf.Abs(objPosition.y)+ bufferWidth));
        //     }
        // }

        m_furthestPositionToFrame = Vector2.Min(m_furthestPositionToFrame, m_maxExtents);

        largestWidthToFrame = Mathf.Max( m_furthestPositionToFrame.y, m_furthestPositionToFrame.x / m_aspectRatio );

        _camera.orthographicSize = largestWidthToFrame;
    }
}
