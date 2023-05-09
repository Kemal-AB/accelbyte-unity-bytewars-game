using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MissileFireState : INetworkSerializable
{
    public Vector3 spawnPosition;
    public Quaternion rotation;
    public Vector3 velocity;
    public Color color;
    public int id;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref spawnPosition);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref velocity);
        serializer.SerializeValue(ref color);
        serializer.SerializeValue(ref id);
    }
}
