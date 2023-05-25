// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using AccelByte.Server;
using UnityEngine;

public class MatchmakingEssentialsWrapper_Starter : MonoBehaviour
{
    //3a. Predefined Code
    private MatchmakingV2 _matchmakingV2;
    private static string _matchmakingV2TicketId;
    private static string _sessionId;
    private static bool _matchCanceled = false;
    private Session _matchmakingV2Session;
    
    //3b. predefined code
    private DedicatedServerManager _dedicatedServerManager;
    private ServerDSHub _serverDSHub;
    private ServerMatchmakingV2 _matchmakingV2Server;
    private bool _isGameStarted = false;

    
    //Copy 3a connecting-game-mode-selection-ui-with-matchmaking here
    
    

    // Start is called before the first frame update
    void Start()
    {
        // 3a predefined code
        _matchmakingV2 = MultiRegistry.GetApiClient().GetMatchmakingV2();
        _matchmakingV2Session = MultiRegistry.GetApiClient().GetSession();
        
        //Copy 3a connecting-game-mode-selection-ui-with-matchmaking here
        
        //3b predefined code
        _dedicatedServerManager = MultiRegistry.GetServerApiClient().GetDedicatedServerManager();
        _matchmakingV2Server = MultiRegistry.GetServerApiClient().GetMatchmakingV2();
        _serverDSHub = MultiRegistry.GetServerApiClient().GetDsHub();
        
        //Copy 3b Wrap it up here
        

        
    }
    
    // 3a predefined code
    private void IsMatchCanceled(Action function = null)
    {
        if (_matchCanceled)
        {
            Debug.LogWarning($"Matchmaking canceled from IsMatchCanceled");
            function?.Invoke();
            return;
        }
    }


}

