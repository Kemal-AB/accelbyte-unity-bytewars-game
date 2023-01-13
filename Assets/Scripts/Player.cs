using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject m_missilePrefab;
    public float m_minMissileSpeed = 1.5f;
    public float m_maxMissileSpeed = 9.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void FireMissile()
    {
        GameObject missile = GameObject.Instantiate(m_missilePrefab, transform.position, transform.rotation);

        MotionComponent motionComponent = missile.GetComponent<MotionComponent>();

        motionComponent.SetVelocity(transform.up * m_maxMissileSpeed / 2.0f);
    }
}
