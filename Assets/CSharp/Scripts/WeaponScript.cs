using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
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

    UltUse,
    CtrlUltUse,
    SubUltUse,

    EleUse,
    CtrlEleUse,
    SubEleUse,

    MainUseUp, //손떼기
    SubUseUp,
    SpecialUseUp,

    UltUp,
    CtrlUltUp,
    SubUltUp,

    EleUp,
    CtrlEleUp,
    SubEleUp,

    OppositeMainUse, //반대손 클릭
    OppositeSubUse,
    OppositeSpecialUse,

    OppositeMainUseUp, //반대손 손떼기
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
    /*-----------------------------------------------
    |NOTI| Object Need.
    'WeaponScriot'를 가지고있는 gameObject가 필요한것
    -----------------------------------------------*/
    protected IKScript _ownerIKSkript = null;
    protected Dictionary<AvatarIKGoal, IKTargetDesc> _createdIKTargets = new Dictionary<AvatarIKGoal, IKTargetDesc>();

    public Transform _socketTranform = null;
    private Quaternion _addRotation = quaternion.identity;
    private Vector3 _addPosition = Vector3.zero;

    /*------------------------------------------
    StoreInfo Need.
    ------------------------------------------*/
    protected ItemStoreDesc_Weapon _itemStoreInfo = null;
    public ItemStoreDesc_Weapon _ItemStoreInfo => _itemStoreInfo;

    public ItemAsset_Weapon GetItemAsset() { return (ItemAsset_Weapon)_itemStoreInfo._itemAsset; }
    public CharacterScript _owner = null;
    protected bool _isRightHandWeapon = false;



    public virtual bool isWeaponUseReady() {return false;}
    public virtual void UseWeapon() {}
    public virtual bool isUsingMe() {return false;}

    virtual public void TurnOnAim() { }
    virtual public void TurnOffAim() { }
    virtual public void UnEquip() { }

    public virtual void Init(ItemStoreDesc_Weapon info) { _itemStoreInfo = info;}

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

        string targetName = (_isRightHandWeapon == true)
            ? "HandlingAbsolute_Right"
            : "HandlingAbsolute_Left";

        Transform target = transform.Find(targetName);

        Transform targetTransform = target.transform;

        _addRotation = targetTransform.localRotation;
        _addPosition = targetTransform.localPosition;
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
}