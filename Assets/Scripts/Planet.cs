using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public float m_scale = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_scale = GetComponent<GameplayObjectComponent>().m_radius * 2.0f;
        transform.localScale = Vector3.one * m_scale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetRadius()
    {
        return m_scale * 0.5f;
    }
}
