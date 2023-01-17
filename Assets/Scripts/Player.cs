using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject m_missilePrefab;
    public float m_minMissileSpeed = 1.5f;
    public float m_maxMissileSpeed = 9.0f;

    float m_normalisedRotateSpeed = 0.0f;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, Time.deltaTime * m_normalisedRotateSpeed * -100.0f);
    }

    public void FireMissile()
    {
        GameObject missile = GameObject.Instantiate(m_missilePrefab, transform.position, transform.rotation);

        MotionComponent motionComponent = missile.GetComponent<MotionComponent>();

        motionComponent.SetVelocity(transform.up * m_maxMissileSpeed / 2.0f);
    }

    public void OnHitByObject(GameplayObjectComponent otherObject)
    {
        Destroy(this.gameObject);
    }

    public void SetNormalisedRotateSpeed(float normalisedRotateSpeed)
    {
        m_normalisedRotateSpeed = normalisedRotateSpeed;
    }

}
