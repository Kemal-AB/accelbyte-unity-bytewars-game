using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public static GameModeSO GameModeSo;
    public static PlayerState CachedPlayerState = new PlayerState();
    public static ServerType ServerType = ServerType.Offline;
}

public enum ServerType
{
    Offline,
    OnlineDedicatedServer,
    OnlinePeer2Peer
}
