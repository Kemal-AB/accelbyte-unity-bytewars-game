using System.Collections.Generic;

public class ClientHelper
{
    private ulong _clientNetworkId;
    public ulong ClientNetworkId => _clientNetworkId;
    public void SetClientNetworkId(ulong clientNetworkId)
    {
        _clientNetworkId = clientNetworkId;
    }
    public void PlaceObjectsOnClient(LevelObject[] levelObjects, ulong[] playersClientIds,
        ObjectPooling objectPooling, Dictionary<string, GameEntityAbs> gamePrefabs,
        Dictionary<int, Planet> planets, Dictionary<ulong, Player> players, 
        ServerHelper serverHelper, List<GameEntityAbs> activeGameEntities)
    {
        
        int playerClientIdIndex = 0;
        for (var i = 0; i < levelObjects.Length; i++)
        {
            var levelObject = levelObjects[i];
            var newObj = objectPooling.Get(gamePrefabs[levelObject.m_prefabName]);
            var transform1 = newObj.transform;
            transform1.position = levelObject.m_position;
            transform1.rotation = levelObject.m_rotation;
            newObj.SetId(levelObject.ID);
            if (newObj is Planet planet2)
            {
                planets.TryAdd(levelObject.ID, planet2);
            } 
            else if (newObj is Player player)
            {
                if (player)
                {
                    players ??= new Dictionary<ulong, Player>();
                    var clientNetworkId = playersClientIds[playerClientIdIndex];
                    var pState = serverHelper.ConnectedPlayerStates[clientNetworkId];
                    player.SetPlayerState(pState, GameData.GameModeSo.maxInFlightMissilesPerPlayer, 
                        serverHelper.ConnectedTeamStates[pState.teamIndex].teamColour);
                    players.TryAdd(clientNetworkId, player);
                    playerClientIdIndex++;
                }
            }
            activeGameEntities.Add(newObj);
        }
    }
}
