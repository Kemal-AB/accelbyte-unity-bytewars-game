using System;
using UnityEngine;

public class MissileTrail : FxEntity
{
    public const float FadeDelayAfterMissileDestruct = 7.0f;
    public const float FadeRate = 4.0f;
    [SerializeField] private TrailRenderer trailRenderer;

    private const float CurrentAlphaStart = 1;
    private const float WantedAlphaStart = 1;
    GameObject m_parentMissile;
    float m_currentAlpha = 1.0f;
    float m_wantedAlpha = 1.0f;
    float _timeWithoutMissile = 0.0f;

    public void Init(GameObject missile, Vector3 pos, Quaternion rot)
    {
        trailRenderer.Clear();
        transform.position = pos;
        transform.rotation = rot;
        trailRenderer.Clear();
        m_parentMissile = missile;
        m_wantedAlpha = WantedAlphaStart;
        trailRenderer.material.SetFloat("_Alpha", CurrentAlphaStart);
    }

    public void TriggerFadeOut()
    {
        m_wantedAlpha = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_parentMissile && m_parentMissile.activeSelf)
        {
            transform.position = m_parentMissile.transform.position;
        }
        else
        {
            _timeWithoutMissile += Time.deltaTime;
            if (_timeWithoutMissile > FadeDelayAfterMissileDestruct)
            {
                TriggerFadeOut();
            }
        }
        if(Math.Abs(m_wantedAlpha - WantedAlphaStart) < 0.1f)
            return;
        m_currentAlpha = Mathf.Lerp(m_currentAlpha, m_wantedAlpha, FadeRate * Time.deltaTime);
        trailRenderer.material.SetFloat("_Alpha", m_currentAlpha);

        // if the object is effectively faded out, destroy
        if (m_currentAlpha < 0.001f)
        {
            Reset();
        }
    }

    public override void Reset()
    {
        m_parentMissile = null;
        m_currentAlpha = CurrentAlphaStart;
        m_wantedAlpha = WantedAlphaStart;
        _timeWithoutMissile = 0;
        trailRenderer.material.SetFloat("_Alpha", CurrentAlphaStart);
        gameObject.SetActive(false);
    }
}
