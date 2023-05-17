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
    private MatchmakingV2 _matchmakingV2;
    private static string _matchmakingV2TicketId;
    private static string _sessionId;
    private static bool _matchCanceled = false;
    private Session _matchmakingV2Session;
    private static DedicatedServerManager _dedicatedServerManager;
    

    // Start is called before the first frame update
    void Start()
    {
        _matchmakingV2 = MultiRegistry.GetApiClient().GetMatchmakingV2();
        _matchmakingV2Session = MultiRegistry.GetApiClient().GetSession();
        _dedicatedServerManager = MultiRegistry.GetServerApiClient().GetDedicatedServerManager();

    }
    
}

