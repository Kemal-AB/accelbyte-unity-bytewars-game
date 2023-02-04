using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public float m_scale = 1.0f;
    public float m_glowPulseRate = 2.0f;
    public float m_glowPulseScale = 3.0f;
    float m_baseGlow = 1.0f;
    float m_glowMultiplier = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_scale = GetComponent<GameplayObjectComponent>().m_radius * 2.0f;
        transform.localScale = Vector3.one * m_scale;

        m_baseGlow = GetComponent<Renderer>().material.GetFloat("_Glow");
    }

    // Update is called once per frame
    void Update()
    {
        m_glowMultiplier = Mathf.Lerp(m_glowMultiplier, 1.0f, m_glowPulseRate * Time.deltaTime);        
        GetComponent<Renderer>().material.SetFloat("_Glow", m_baseGlow * m_glowMultiplier);
    }

    public float GetRadius()
    {
        return m_scale * 0.5f;
    }

    public void OnHitByMissile()
    {        
        m_glowMultiplier = m_glowPulseScale;
    }
}
