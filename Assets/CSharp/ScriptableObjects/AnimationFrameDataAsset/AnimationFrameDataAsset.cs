using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "AnimationFrameDataAsset", menuName = "Scriptable Object/Create_AnimationFrameDataAsset", order = (int)MyUtil.CustomToolOrder.AnimationFrameDataAsset)]
public class AnimationFrameDataAsset : ScriptableObject
{
    public enum FrameCheckType
    {
        Up, Under, Between, End,
    }

    public enum FrameDataWorkType
    {
        ChangeToIdle,             //������ Idle�� �ٲ۴�
        StateChangeReady,         //Act Like IdleState
        NextAttackMotion,         //�޺��� �̾�� ��ȸ�����̴�.
        Attack,                   //������ �ִ�
        DeadCall,                 //���� ���
        AddBuff,                  //������ �ɾ��ش�
        RemoveBuff,               //������ �����Ѵ�
        RipositeAttack,           //ġ��Ÿ ����
        End = 8,
    }

    public enum ColliderAttachType
    {
        HumanoidLeftHand,
        HumanoidRightHand,
        HumanoidLeftLeg,
        HumanoidRightLeg,
        HumanoidHead,
        HumanoidRightHandWeapon,
        HumanoidLeftHandWeapon,
        ENEND,
    }

    [Serializable]
    public class AEachFrameData
    {
        public FrameCheckType _frameCheckType = FrameCheckType.Up;
        public ColliderAttachType _colliderAttachType = ColliderAttachType.HumanoidLeftHand;

        public int _frameUp = -1;
        public int _frameUnder = -1;
        public List<BuffAssetBase> _buffs = new List<BuffAssetBase>();
    }


    [Serializable]
    public class AFrameData
    {
        public FrameDataWorkType _frameWorkType = FrameDataWorkType.NextAttackMotion;
        public List<AEachFrameData> _frameDatas = new List<AEachFrameData>();
    }

    [SerializeField] private AnimationClip _animationClip = null;
    public AnimationClip _AnimationClip => _animationClip;

    [SerializeField] private List<AFrameData> _frameDataList = new List<AFrameData>();
    public List<AFrameData> _FrameDataList => _frameDataList;


}