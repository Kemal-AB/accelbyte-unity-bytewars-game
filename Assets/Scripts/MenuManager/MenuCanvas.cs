using System;
using UnityEngine;


[Serializable]
public abstract class MenuCanvas : MonoBehaviour
{
    public abstract GameObject GetFirstButton();
    public abstract AssetEnum GetAssetEnum();
}