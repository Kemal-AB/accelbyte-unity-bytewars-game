using System;
using System.Linq;


public class TutorialModuleUtil
{
    public static bool IsAccelbyteSDKInstalled()
    {
        var typ = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where type.Name == "AccelBytePlugin"
            select type);
        int classCount = typ.Count();
        if (classCount == 1)
        {
            return true;
        }
        return false;
    }
}
