using System;
using UnityEngine;
using System.Collections.Generic;



[CreateAssetMenu(fileName = "AnimationAttackFrameAsset", menuName = "Scriptable Object/CreateAnimationAttackFrameDesc", order = int.MinValue)]
public class AnimationAttackFrameAsset : ScriptableObject
{
    public enum ColliderAttachType
    {
        //애니메이션에 따라서 활성/비활성화 할 충돌체들의 하나의 공통적 이름
        //캐릭터마다 가지고있는 고유한 뼈 까지 다 여기에 집어넣어야한다.
        HumanoidLeftHand,
        HumanoidRightHand,

        HumanoidLeftLeg,
        HumanoidRightLeg,

        HumanoidLeftHead,

        HumanoidRightHandWeapon,
        HumanoidLeftHandWeapon,

        ENEND,
    }

    [Serializable]
    public class AttackFrameDesc
    {
        public ColliderAttachType _attachType = ColliderAttachType.ENEND;
        public int _upFrame = -1;
        public int _underFrame = -1;
    }

    public AnimationClip _animation = null;
    public List<AttackFrameDesc> _frameDesc = new List<AttackFrameDesc>();
}
