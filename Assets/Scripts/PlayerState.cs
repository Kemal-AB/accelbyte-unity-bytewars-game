using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Networking;


public class PlayerState : MonoBehaviour, INetworkSerializable
{
    public Color m_teamColour;
    public int m_numLivesLeft;
    public float m_playerScore;
    public string m_playerName;
    public int m_playerIndex;
    public int m_killCount;
    public int m_numMissilesFired;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_teamColour);
        serializer.SerializeValue(ref m_numLivesLeft);
        serializer.SerializeValue(ref m_playerScore);
        serializer.SerializeValue(ref m_playerName);
        serializer.SerializeValue(ref m_playerIndex);
        serializer.SerializeValue(ref m_killCount);
        serializer.SerializeValue(ref m_numMissilesFired);
    }
}
