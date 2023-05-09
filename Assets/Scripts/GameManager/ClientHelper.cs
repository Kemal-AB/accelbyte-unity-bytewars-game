using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientHelper
{
    private ulong _clientNetworkId;
    public ulong ClientNetworkId
    {
        get { return _clientNetworkId; }
    }
    public void SetClientNetworkId(ulong clientNetworkId)
    {
        _clientNetworkId = clientNetworkId;
    }
}
