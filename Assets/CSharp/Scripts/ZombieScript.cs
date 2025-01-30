using UnityEngine;
using static AnimationFrameDataAsset;


public class ZombieScript : CharacterScript, IHitable
{
    public override LayerMask CalculateWeaponColliderIncludeLayerMask()
    {
        int ret = LayerMask.GetMask("Player");
        return ret;
    }
}
