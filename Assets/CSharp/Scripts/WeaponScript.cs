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
    MainUse, //Ŭ��
    SubUse,
    SpecialUse,
    MainUseUp, //�ն���
    SubUseUp,
    SpecialUseUp,
    OppositeMainUse, //�ݴ�� Ŭ��
    OppositeSubUse,
    OppositeSpecialUse,
    OppositeMainUseUp, //�ݴ�� Ŭ��
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
            Debug.Assert(false, "���⸦ ������ �ִϸ��̼��� ���������� �ʽ��ϴ�");
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
            Debug.Assert(false, "���⸦ ����ִ� �ִϸ��̼��� ���������� �ʽ��ϴ�");
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
    ��Ÿ���� ��������� ������
    ------------------------------------------*/
    public Transform _socketTranform = null;
    protected bool _isRightHandWeapon = false;
    public CharacterScript _owner = null;





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


        {
            //�ǹ� ���ϱ�

            //Weapon�� Socket���� AbsoluteHandling �� �ִ�
            //�ݵ�� �־���Ѵ�.

            //���� ��ġ�� ���� ���� rotation �� �������� �������� ����
            //�߰� ������ ������. �װ� �ǹ����� �����Ѵ�

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

    virtual public void TurnOnAim() { }
    virtual public void TurnOffAim() { }
    virtual public void UnEquip() { }
}