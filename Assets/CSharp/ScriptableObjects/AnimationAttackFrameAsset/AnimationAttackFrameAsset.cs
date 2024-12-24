using System;
using UnityEngine;
using System.Collections.Generic;



[CreateAssetMenu(fileName = "AnimationAttackFrameAsset", menuName = "Scriptable Object/CreateAnimationAttackFrameDesc", order = int.MinValue)]
public class AnimationAttackFrameAsset : ScriptableObject
{
    public enum ColliderAttachType
    {
        //�ִϸ��̼ǿ� ���� Ȱ��/��Ȱ��ȭ �� �浹ü���� �ϳ��� ������ �̸�
        //ĳ���͸��� �������ִ� ������ �� ���� �� ���⿡ ����־���Ѵ�.
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
