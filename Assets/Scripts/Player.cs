using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public PowerBarUIController m_powerBarUIPrefab;
    PowerBarUIController m_powerBarUI;
    public GameObject m_missilePrefab;
    public GameObject m_fireMissileEffectPrefab;
    public GameObject m_shipDestroyedEffectPrefab;
    public float m_minMissileSpeed = 1.5f;
    public float m_maxMissileSpeed = 9.0f;
    public int m_maxMissilesInFlight = 1;

    float m_normalisedRotateSpeed = 0.0f;
    float m_normalisedPowerChangeSpeed = 0.0f;

    float m_firePowerLevel = 0.5f;

    List<GameObject> m_firedMissiles = new List<GameObject>();

    PlayerState m_playerState;
    UnityEngine.Color m_colour;

    void Start()
    {
        GameDirector.Instance.WriteToConsole("PlayerSpawned! ");
        List<Vector3> outerVerts = new List<Vector3>();
        outerVerts.Add(new Vector3(0, 40, 0));
        outerVerts.Add(new Vector3(40, -45, 0));
        outerVerts.Add(new Vector3(25, -55, 0));
        outerVerts.Add(new Vector3(0, -45, 0));


        List<Vector3> innerVerts = new List<Vector3>();
        innerVerts.Add(new Vector3(0, 30, 0));
        innerVerts.Add(new Vector3(31.5f, -42, 0));
        innerVerts.Add(new Vector3(24, -47, 0));
        innerVerts.Add(new Vector3(0, -37, 0));

        NeonObject playerGeometry = new NeonObject(outerVerts, innerVerts);

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.Clear();
        mesh.vertices = playerGeometry.vertexList.ToArray();
        mesh.uv = playerGeometry.uvList.ToArray();
        mesh.triangles = playerGeometry.indexList.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

    }

    void Update()
    {
        
        transform.Rotate(Vector3.forward, Time.deltaTime * m_normalisedRotateSpeed * -100.0f);
        if( m_normalisedPowerChangeSpeed != 0.0f )
        {
            ChangePowerLevel(m_normalisedPowerChangeSpeed);
        }
    }

    public void SetPlayerState(PlayerState playerState)
    {
        m_playerState = playerState;
    }

    public PlayerState GetPlayerState()
    {
        return m_playerState;
    }

    public void Init(Color colour)
    {
        m_colour = colour;
        m_powerBarUI = GameObject.Instantiate(m_powerBarUIPrefab, transform.position, Quaternion.identity,transform);
        m_powerBarUI.Init();
        m_powerBarUI.SetPosition(transform.position);
        gameObject.GetComponent<Renderer>().material.SetVector("_PlayerColour", m_colour);
        m_powerBarUI.SetColour(colour);
        m_powerBarUI.SetPercentageFraction(m_firePowerLevel,false);
    }

    public void LocalFireMissile()
    {
        /*m_firedMissiles.RemoveAll(x=>x==null);

        if( m_firedMissiles.Count >= m_maxMissilesInFlight )
        {
            return;
        }*/

        Vector3 missileSpawnPosition = transform.position + transform.up * 0.25f;
       /* GameObject missile = GameObject.Instantiate(m_missilePrefab, missileSpawnPosition, transform.rotation);

        missile.GetComponent<Missile>().Init(GetPlayerState());

        MotionComponent motionComponent = missile.GetComponent<MotionComponent>();

        motionComponent.SetVelocity(transform.up * Mathf.Lerp(m_minMissileSpeed, m_maxMissileSpeed, m_firePowerLevel));

        m_firedMissiles.Add(missile);*/

        PlayerFireMissileServerRPC(missileSpawnPosition, transform.up, transform.rotation, m_firePowerLevel, GetPlayerState());
    }

    [ServerRpc]
    public void PlayerFireMissileServerRPC(Vector3 MissilePosition, Vector3 transformUP, Quaternion rotation, float firePowerLevel, PlayerState PState )
    {
        GameDirector.Instance.WriteToConsole("Server RPC Missile from Client: " + OwnerClientId);
        if (NetworkManager.ConnectedClients.ContainsKey(OwnerClientId))
        {
            //var client = NetworkManager.ConnectedClients[OwnerClientId];

            m_firedMissiles.RemoveAll(x => x == null);

            if (m_firedMissiles.Count >= m_maxMissilesInFlight)
            {
                return;
            }

            Vector3 missileSpawnPosition = MissilePosition;
            GameObject missile = GameObject.Instantiate(m_missilePrefab, missileSpawnPosition, rotation);

            missile.GetComponent<Missile>().Init(PState);

            MotionComponent motionComponent = missile.GetComponent<MotionComponent>();

            motionComponent.SetVelocity(transformUP * Mathf.Lerp(m_minMissileSpeed, m_maxMissileSpeed, firePowerLevel));

            m_firedMissiles.Add(missile);

            RemoteFireMissileClientRPC(MissilePosition, transformUP, rotation, firePowerLevel, PState);
        }
    }

    [ClientRpc]
    public void RemoteFireMissileClientRPC(Vector3 MissilePosition, Vector3 transformUP, Quaternion rotation, float firePowerLevel, PlayerState PState)
    {
        //GameDirector.Instance.WriteToConsole("RemoteFireMissileClientRPC febore Owner Check OwnerClientId: " + OwnerClientId);
        //if (IsOwner) return;

        GameDirector.Instance.WriteToConsole("RemoteFireMissileClientRPC OwnerClientId: " + OwnerClientId);
        Vector3 missileSpawnPosition = MissilePosition;
        GameObject missile = GameObject.Instantiate(m_missilePrefab, missileSpawnPosition, rotation);

        missile.GetComponent<Missile>().Init(PState);

        MotionComponent motionComponent = missile.GetComponent<MotionComponent>();

        motionComponent.SetVelocity(transformUP * Mathf.Lerp(m_minMissileSpeed, m_maxMissileSpeed, firePowerLevel));

    }
    public void OnHitByObject(GameplayObjectComponent otherObject)
    {
        this.m_playerState.m_numLivesLeft--;

        GameObject explosion = GameObject.Instantiate(m_shipDestroyedEffectPrefab, transform.position, transform.rotation);
        explosion.GetComponent<Renderer>().material.SetVector("_Colour", m_colour);
    }

    public void SetNormalisedRotateSpeed(float normalisedRotateSpeed)
    {
        m_normalisedRotateSpeed = normalisedRotateSpeed;
    }
    public void SetNormalisedPowerChangeSpeed(float normalisedPowerChangeSpeed)
    {
        m_normalisedPowerChangeSpeed = normalisedPowerChangeSpeed;
    }

    void ChangePowerLevel(float normalisedChangeSpeed)
    {
        m_firePowerLevel = Mathf.Clamp01( m_firePowerLevel + normalisedChangeSpeed * Time.deltaTime );
        m_powerBarUI.SetPercentageFraction(m_firePowerLevel);
    }

}
