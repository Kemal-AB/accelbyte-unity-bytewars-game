using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class GameEntityAbs : MonoBehaviour
{
    public abstract float GetScale();
    public abstract float GetRadius();
    public abstract float GetMass();
    public abstract void OnHitByMissile();
    public abstract void Reset();
    public abstract void SetId(int id);
    public abstract int GetId();
}
