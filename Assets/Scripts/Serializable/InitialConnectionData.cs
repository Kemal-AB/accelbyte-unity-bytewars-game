using System;

[Serializable]
public class InitialConnectionData
{
    public InGameMode inGameMode;
    //unity netcode gameobject connection session id, for reconnection purpose
    public string sessionId;
    //accelbyte match session id/party session id
    public string serverSessionId="";
}
