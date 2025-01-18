using System;
using UnityEngine;
using System.Collections.Generic;
using static ItemAsset_Weapon;

//아이템을 사용할때 필요한 정보들

[CreateAssetMenu(fileName = "ItemSubInfo_HandlingAnimationInfo", menuName = "Scriptable Object/Create_ItemSubInfo_HandlingAnimationInfo", order = (int)MyUtil.CustomToolOrder.CreateItemSubInfo)]
public class ItemSubInfo_HandlingAnimationInfo : ScriptableObject
{
    [SerializeField] private WeaponType _targetWeaponType;
    public WeaponType _TargetWeaponType => _targetWeaponType;


    /*------------------------------------------
    PutAway/Draw AnimationClips Section.
    ------------------------------------------*/
    [SerializeField] private AnimationClip _putawayAnimation = null;
    public AnimationClip _PutawayAnimation => _putawayAnimation;

    [SerializeField] private AnimationClip _drawAnimation = null;
    public AnimationClip _DrawAnimation => _drawAnimation;

    [SerializeField] private AnimationClip _putawayAnimation_Mirrored = null;
    public AnimationClip _PutawayAnimation_Mirrored => _putawayAnimation_Mirrored;

    [SerializeField] private AnimationClip _drawAnimation_Mirrored = null;
    public AnimationClip _DrawAnimation_Mirrored => _drawAnimation_Mirrored;

    [SerializeField] private AnimationClip _handlingIdleAnimation_OneHand = null;
    public AnimationClip _HandlingIdleAnimation_OneHand => _handlingIdleAnimation_OneHand;

    [SerializeField] private AnimationClip _handlingIdleAnimation_TwoHand = null;
    public AnimationClip _HandlingIdleAnimation_TwoHand => _handlingIdleAnimation_TwoHand;

    [SerializeField] private AnimationClip _handlingIdleAnimation_OneHand_Mirrored = null;
    public AnimationClip _HandlingIdleAnimation_OneHand_Mirrored => _handlingIdleAnimation_OneHand_Mirrored;

    [SerializeField] private AnimationClip _handlingIdleAnimation_TwoHand_Mirrored = null;
    public AnimationClip _HandlingIdleAnimation_TwoHand_Mirrored => _handlingIdleAnimation_TwoHand_Mirrored;

    public AnimationClip GetDrawAnimation(AnimatorLayerTypes layerType)
    {
        if (layerType == AnimatorLayerTypes.RightHand)
        {
            return _drawAnimation;
        }
        else if (layerType == AnimatorLayerTypes.LeftHand)
        {
            return _drawAnimation_Mirrored;
        }
        else
        {
            Debug.Assert(false, "무기를 꺼내는 애니메이션이 설정돼있지 않습니다");
            Debug.Break();
        }
        return null;
    }
    public AnimationClip GetDrawAnimation(bool isRightHand)
    {
        if (isRightHand == true)
        {
            return _drawAnimation;
        }
        else
        {
            return _drawAnimation_Mirrored;
        }
    }
    public AnimationClip GetPutawayAnimation(AnimatorLayerTypes layerType)
    {
        if (layerType == AnimatorLayerTypes.RightHand)
        {
            return _putawayAnimation;
        }
        else if (layerType == AnimatorLayerTypes.LeftHand)
        {
            return _putawayAnimation_Mirrored;
        }
        else
        {
            Debug.Assert(false, "무기를 집어넣는 애니메이션이 설정돼있지 않습니다");
            Debug.Break();
        }
        return null;
    }
    public AnimationClip GetPutawayAnimation(bool isRightHand)
    {
        if (isRightHand == true)
        {
            return _putawayAnimation;
        }
        else
        {
            return _putawayAnimation_Mirrored;
        }
    }

    public AnimationClip GetOneHandHandlingAnimation(AnimatorLayerTypes layerType)
    {
        if (layerType == AnimatorLayerTypes.RightHand)
        {
            return _handlingIdleAnimation_OneHand;
        }
        else if (layerType == AnimatorLayerTypes.LeftHand)
        {
            return _handlingIdleAnimation_OneHand_Mirrored;
        }

        return null;
    }
    public AnimationClip GetOneHandHandlingAnimation(bool isRightHand)
    {
        if (isRightHand == true)
        {
            return _handlingIdleAnimation_OneHand;
        }
        else
        {
            return _handlingIdleAnimation_OneHand_Mirrored;
        }
    }
    public AnimationClip GetTwoHandHandlingAnimation(AnimatorLayerTypes layerType)
    {
        if (layerType == AnimatorLayerTypes.RightHand)
        {
            return _handlingIdleAnimation_TwoHand;
        }
        else if (layerType == AnimatorLayerTypes.LeftHand)
        {
            return _handlingIdleAnimation_TwoHand_Mirrored;
        }
        return null;
    }
    public AnimationClip GetTwoHandHandlingAnimation(bool isRightHand)
    {
        if (isRightHand == true)
        {
            return _handlingIdleAnimation_TwoHand;
        }
        else
        {
            return _handlingIdleAnimation_TwoHand_Mirrored;
        }
    }
}
