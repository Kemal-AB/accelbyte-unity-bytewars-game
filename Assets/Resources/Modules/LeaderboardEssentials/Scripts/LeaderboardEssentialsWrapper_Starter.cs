using System.Collections;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class LeaderboardEssentialsWrapper_Starter : MonoBehaviour
{
    // AccelByte's Multi Registry references
    private Leaderboard leaderboard;
    
    // Start is called before the first frame update
    void Start()
    {
        leaderboard = MultiRegistry.GetApiClient().GetLeaderboard();
    }
    
    
}
