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

    MainUse, //Ŭ��
    SubUse,
    SpecialUse,

    UltUse,
    CtrlUltUse,
    SubUltUse,

    EleUse,
    CtrlEleUse,
    SubEleUse,

    MainUseUp, //�ն���
    SubUseUp,
    SpecialUseUp,

    UltUp,
    CtrlUltUp,
    SubUltUp,

    EleUp,
    CtrlEleUp,
    SubEleUp,

    OppositeMainUse, //�ݴ�� Ŭ��
    OppositeSubUse,
    OppositeSpecialUse,

    OppositeMainUseUp, //�ݴ�� �ն���
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
    'WeaponScriot'�� �������ִ� gameObject�� �ʿ��Ѱ�
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
        //IK ���� �ܰ�
        IKTargetScript[] ikTargets = gameObject.GetComponentsInChildren<IKTargetScript>();

        foreach (IKTargetScript ikTarget in ikTargets)
        {
            IKTargetDesc desc = ikTarget._desc;

            if (desc._isRightSide != _isRightHandWeapon)
            {
                //IK �ߺ� ���� �ڵ��Դϴ�. �����տ� ������� �����ո� Ȱ��ȭ�ϰ�
                continue;
            }

            AvatarIKGoal goal = ikTarget.CalculateIKGoalType(_isRightHandWeapon);

            IKDesc newIKDesc = new IKDesc();
            newIKDesc._targetDesc = desc;
            newIKDesc._activated = false;

            newIKDesc._ikGoalTransform = ikTarget.transform;

            /*---------------------------------------------------
            |NOTI| IK�� Unity Humanoid Bone �ۿ� ������ ���մϴ�
            �ٵ� ������ �پ�� �Ұ�, ���Ⱑ ���� '����' �����Դϴ�
            �׷��� �߰����� ����� ���� _ikTargetTransform�� ���Դϴ�
            ---------------------------------------------------*/

            //������ �θ��
            newIKDesc._ikTargetTransform = _socketTranform.parent;

            //������ �ջ����� �̸�ŭ �������ִ�...���������� ����
            newIKDesc._ikPositionOffset = _socketTranform.localPosition;

            //������ �ջ����� �̸�ŭ ȸ�����ִ�...���������� ����
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
        Debug.Assert(weaponSocketScript != null, "Socket�� �ƴѰ��� ���⸦ �����Ϸ� �ϰ� �ִ�. �̷� �������� �߰��Ƿ��� �մϱ�?");
        switch (weaponSocketScript._sideType)
        {
            case WeaponSocketScript.SideType.Left:
                _isRightHandWeapon = false;
                break;
            case WeaponSocketScript.SideType.Right:
                _isRightHandWeapon = true;
                break;
            case WeaponSocketScript.SideType.Middle:
                Debug.Assert(false, "���� �߽ɹ���� ����");
                break;
        }
    }
}