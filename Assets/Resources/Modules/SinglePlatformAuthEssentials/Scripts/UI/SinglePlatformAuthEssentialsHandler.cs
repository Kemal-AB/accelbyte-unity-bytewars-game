using UnityEngine;

public class SinglePlatformAuthEssentialsHandler : MenuCanvas
{
    public override GameObject GetFirstButton()
    {
        return null;
    }

    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.SinglePlatformAuthEssentials;
    }
}
