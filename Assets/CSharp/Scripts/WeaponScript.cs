using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public enum WeaponUseType
{
    TargetingMoveStart_DontUseThis,
    TargetingFront,
    TargetingBack,
    TargetingLeft,
    TargetingRight,
    TargetingMoveEnd_DontUseThis,

    WeaponUseStart_DontUseThis,
    MainUse, //클릭
    SubUse,
    SpecialUse,
    MainUseUp, //손떼기
    SubUseUp,
    SpecialUseUp,
    OppositeMainUse, //반대손 클릭
    OppositeSubUse,
    OppositeSpecialUse,
    OppositeMainUseUp, //반대손 클릭
    OppositeSubUseUp,
    OppositeSpecialUseUp,
    WeaponUseEnd_DontUseThis,
}


[Serializable]
public class WeaponComboEntryDesc
{
    public bool _isEntry = false;
    public ConditionDesc _entryCondition = null;
    public StateAsset _stateAsset = null;
}

[Serializable]
public class WeaponComboEntry
{
    public ConditionDesc _entryCondition = null;
    public State _state = null;
}

public class WeaponScript : MonoBehaviour
{
    /*------------------------------------------
    Pivot Section.
    ------------------------------------------*/
    public Vector3 _pivotRotation_Right = Vector3.zero;
    public Vector3 _pivotPosition_Right = Vector3.zero;
    public Vector3 _pivotRotation_Left = Vector3.zero;
    public Vector3 _pivotPosition_Left = Vector3.zero;

    //[SerializeField] private Transform _socketAbsoluteHandlingTransform = null;
    private Quaternion _addRotation = quaternion.identity;
    private Vector3 _addPosition = Vector3.zero;

    /*------------------------------------------
    Item Spec Section.
    ------------------------------------------*/
    public bool _onlyTwoHand = false;
    public ItemInfo _itemInfo = null;
    public ItemInfo.WeaponType _weaponType = ItemInfo.WeaponType.NotWeapon;
    public DamageDesc _weaponDamageDesc = new DamageDesc();



    /*------------------------------------------
    PutAway/Draw AnimationClips Section.
    ------------------------------------------*/
    public AnimationClip _putawayAnimation = null;
    public AnimationClip _drawAnimation = null;
    public AnimationClip _putawayAnimation_Mirrored = null;
    public AnimationClip _drawAnimation_Mirrored = null;
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

    public virtual void UseWeapon()
    {

    }

    public virtual bool isUsingMe()
    {
        return false;
    }

    public virtual bool isWeaponUseReady()
    {
        return false;
    }


    public AnimationClip _handlingIdleAnimation_OneHand = null;
    public AnimationClip _handlingIdleAnimation_TwoHand = null;
    public AnimationClip _handlingIdleAnimation_OneHand_Mirrored = null;
    public AnimationClip _handlingIdleAnimation_TwoHand_Mirrored = null;
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



    /*------------------------------------------
    IK Section.
    ------------------------------------------*/
    protected IKScript _ownerIKSkript = null;
    protected Dictionary<AvatarIKGoal, IKTargetDesc> _createdIKTargets = new Dictionary<AvatarIKGoal, IKTargetDesc>();
    


    /*------------------------------------------
    State Section.
    ------------------------------------------*/
    public StateGraphAsset _weaponStateGraph = null;




    /*------------------------------------------
    런타임중 정보저장용 변수들
    ------------------------------------------*/
    public Transform _socketTranform = null;
    protected bool _isRightHandWeapon = false;
    public CharacterScript _owner = null;





    public virtual void InitIK()
    {
        //IK 세팅 단계
        IKTargetScript[] ikTargets = gameObject.GetComponentsInChildren<IKTargetScript>();

        foreach (IKTargetScript ikTarget in ikTargets)
        {
            IKTargetDesc desc = ikTarget._desc;

            if (desc._isRightSide != _isRightHandWeapon)
            {
                //IK 중복 방지 코드입니다. 오른손에 쥐었을때 오른손만 활성화하게
                continue;
            }

            AvatarIKGoal goal = ikTarget.CalculateIKGoalType(_isRightHandWeapon);

            IKDesc newIKDesc = new IKDesc();
            newIKDesc._targetDesc = desc;
            newIKDesc._activated = false;

            newIKDesc._ikGoalTransform = ikTarget.transform;

            /*---------------------------------------------------
            |NOTI| IK는 Unity Humanoid Bone 밖에 지원을 안합니다
            근데 실제로 붙어야 할건, 무기가 붙을 '소켓' 기준입니다
            그래서 추가적인 계산을 위해 _ikTargetTransform에 붙입니다
            ---------------------------------------------------*/

            //소켓의 부모뼈
            newIKDesc._ikTargetTransform = _socketTranform.parent;

            //소켓은 손뼈부터 이만큼 떨어져있다...오프셋으로 지정
            newIKDesc._ikPositionOffset = _socketTranform.localPosition;

            //소켓은 손뼈부터 이만큼 회전해있다...오프셋으로 지정
            newIKDesc._ikRotationOffset = _socketTranform.localRotation;

            _ownerIKSkript.RegistIK(this, goal, newIKDesc);
        }
    }
    



    protected virtual void LateUpdate()
    {
        FollowSocketTransform();
    }



    virtual public void FollowSocketTransform()
    {
        transform.rotation = _socketTranform.rotation * Quaternion.Inverse(_addRotation);
        transform.position = _socketTranform.position + (transform.rotation * (-_addPosition));
    }



    virtual public void Equip(CharacterScript itemOwner, Transform followTransform)
    {
        _owner = itemOwner;
        Equip_OnSocket(followTransform);


        {
            //피벗 구하기

            //Weapon과 Socket에는 AbsoluteHandling 이 있다
            //반드시 있어야한다.

            //둘의 위치가 같기 위한 rotation 과 포지션이 같아지기 위한
            //중간 변수가 있을것. 그걸 피벗으로 결정한다

            string targetName = (_isRightHandWeapon == true)
                ? "HandlingAbsolute_Right"
                : "HandlingAbsolute_Left";

            Transform targetTransform = transform.Find(targetName).transform;

            _addRotation = targetTransform.localRotation;
            _addPosition = targetTransform.localPosition;
        }
    }

    public void Equip_OnSocket(Transform followTransform)
    {
        _socketTranform = followTransform;
        WeaponSocketScript weaponSocketScript = _socketTranform.gameObject.GetComponent<WeaponSocketScript>();
        Debug.Assert(weaponSocketScript != null, "Socket이 아닌곳에 무기를 장착하려 하고 있다. 이런 컨텐츠가 추가되려고 합니까?");
        switch (weaponSocketScript._sideType)
        {
            case WeaponSocketScript.SideType.Left:
                _isRightHandWeapon = false;
                break;
            case WeaponSocketScript.SideType.Right:
                _isRightHandWeapon = true;
                break;
            case WeaponSocketScript.SideType.Middle:
                Debug.Assert(false, "아직 중심무기는 없다");
                break;
        }
    }

    virtual public void TurnOnAim() { }
    virtual public void TurnOffAim() { }
    virtual public void UnEquip() { }
}