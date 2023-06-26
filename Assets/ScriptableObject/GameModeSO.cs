using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObjects/GameModeSO", order = 1)]
public class GameModeSO : ScriptableObject
{


    public GameEntityAbs[] objectsToSpawn;
    public GameEntityAbs playerPrefab;
    public Bounds bounds;
    public int numLevelObjectsToSpawn;
    public int numRetriesToPlaceLevelObject;
    public int numRetriesToPlacePlayer;
    public int gameDuration;
    public float baseKillScore;
    public int playerStartLives;    

    public Color[] teamColours;
    public GameModeEnum gameMode;
    public int playerPerTeamCount;
    public int teamCount;
    public int maxInFlightMissilesPerPlayer;
    public int lobbyCountdownSecond;
    public int beforeGameCountdownSecond;
    public int beforeShutDownCountdownSecond;
    public int gameOverShutdownCountdown;
    public int lobbyShutdownCountdown;

}
