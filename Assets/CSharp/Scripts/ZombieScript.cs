using UnityEngine;
using static AnimationFrameDataAsset;


public class ZombieScript : CharacterScript, IHitable
{
    public override LayerMask CalculateWeaponColliderExcludeLayerMask(ColliderAttachType type, GameObject targetObject)
    {
        //EnemyAIScript aiScript = gameObject.GetComponent<EnemyAIScript>();

        int ret = LayerMask.GetMask("Player");
        return ret;
    }
}
