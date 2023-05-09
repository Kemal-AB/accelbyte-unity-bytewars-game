

using System;

public interface IMatchmaking
{
    public void StartMatchmaking(Action<MatchmakingResult> onMatchmakingFinished);
    public void CancelMatchmaking();
}
