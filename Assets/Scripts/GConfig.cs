using System.Collections.Generic;

public class GConfig
{
    private static Dictionary<GConfigType, bool> _bools = new Dictionary<GConfigType, bool>()
    {
        { GConfigType.AutoLoginSteam, true }
    };

    public static bool GetBool(GConfigType configType)
    {
        return _bools.TryGetValue(configType, out var value) && value;
    }
}

public enum GConfigType
{
    AutoLoginSteam
}
