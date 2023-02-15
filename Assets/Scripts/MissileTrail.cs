using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileTrail : MonoBehaviour
{
    public float m_FadeDelayAfterMissileDestruct = 7.0f;
    public float m_FadeRate = 4.0f;

    GameObject m_parentMissile;
    float m_currentAlpha = 1.0f;
    float m_wantedAlpha = 1.0f;
    float m_timeWithoutMissile = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(GameObject missile)
    {
        m_parentMissile = missile;
    }

    public void TriggerFadeOut()
    {
        m_wantedAlpha = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_parentMissile)
        {
            transform.position = m_parentMissile.transform.position;
        }
        else
        {
            m_timeWithoutMissile += Time.deltaTime;
            if (m_timeWithoutMissile > m_FadeDelayAfterMissileDestruct)
            {
                TriggerFadeOut();
            }
        }

        m_currentAlpha = Mathf.Lerp(m_currentAlpha, m_wantedAlpha, m_FadeRate * Time.deltaTime);

        GetComponent<Renderer>().material.SetFloat("_Alpha", m_currentAlpha);

        // if the object is effectively faded out, destroy
        if (m_currentAlpha < 0.001f)
        {
            Destroy(this.gameObject);
        }
    }
}
