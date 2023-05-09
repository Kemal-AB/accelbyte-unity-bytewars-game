using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelObject : INetworkSerializable
{
    public string m_prefabName;
    public Vector3 m_position;
    public Quaternion m_rotation;
    public int ID;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_prefabName);
        serializer.SerializeValue(ref m_position);
        serializer.SerializeValue(ref m_rotation);
        serializer.SerializeValue(ref ID);
    }
}
