using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum WeaponUseType
{
    TargetingFront,
    TargetingBack,
    TargetingLeft,
    TargetingRight,
    MainUse, //Ŭ��
    SubUse,
    SpecialUse,
}


//[Serializable]
//public class StateLinkDesc
//{
//    public List<ConditionDesc> _multiConditionAsset; //MultiCondition
//    public StateAsset _stateAsset;
//    private int _autoLinkWeight = 0; //�� ���ǵ��� �ڵ����� ����ϴ� ����ġ ����
//}

[Serializable]
public class WeaponStateDesc
{
    [Serializable]
    public class JumpingState
    {
        public List<StateLinkDesc> _linkedStates = new List<StateLinkDesc>(); //���� ���� �Ѿ �� �ִ�
    }

    [Serializable]
    public class EachState
    {
        private bool _isEntry = false;
        public List<ConditionDesc> _entryConditions = new List<ConditionDesc>();

        public StateAsset _state = null;
        public List<ConditionDesc> _nextStateConditions = new List<ConditionDesc>();
        public List<JumpingState> _linkedStates = new List<JumpingState>();
    }

    public List<EachState> _states = new List<EachState>();

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
    public Transform _socketTranform = null;
    public Vector3 _pivotRotation = Vector3.zero;
    public Vector3 _pivotPosition = Vector3.zero;


    
    public PlayerScript _owner = null;
    public AnimationClip _handlingIdleAnimation = null;



    public bool _onlyTwoHand = false;
    public ItemInfo _itemInfo = null;
    public ItemInfo.WeaponType _weaponType = ItemInfo.WeaponType.NotWeapon;


    public List<WeaponStateDesc> _weaponStateAssets = new List<WeaponStateDesc>();


    ////����� ����� ���� �������ִ°͵�
    //public List<WeaponComboEntryDesc> _weaponStateAssets = new List<WeaponComboEntryDesc>();
    //////������ ������ �ν��Ͻ���
    //private List<State> _weaponStates = new List<State>();
    //private List<WeaponComboEntry> _weaponEntryStates = new List<WeaponComboEntry>();



    private void Awake()
    {
        /*-----------------------------------------------------------------------------------------------------------------
        |NOTI| ������ �������� �⺻ PIVOT�� ����ִ�.
        ������ ��ġ�� �ڽ� Transform���� �����¸� �ȵȴ� : (IK�� �̿��� ���ɼ� ������)
        ���� _pivotPosition, _pivotRotation = ���⸶�� ����ִ� ���� �ǹ� ������ �ν����� â���� �̸� �����صд�
        -----------------------------------------------------------------------------------------------------------------*/
        _pivotPosition = transform.position;
        _pivotRotation = transform.rotation.eulerAngles;




        //foreach (var stateAsset in _weaponStateAssets)
        //{
        //    State newState = new State(stateAsset._stateAsset);

        //    if (stateAsset._isEntry == true)
        //    {
        //        WeaponComboEntry comboEntry = new WeaponComboEntry();
        //        comboEntry._entryCondition = stateAsset._entryCondition;
        //        comboEntry._state = newState;

        //        _weaponEntryStates.Add(comboEntry);
        //    }


        //    foreach (State state in _weaponStates)
        //    {
        //        state.LinkingStates(ref _weaponStates);
        //    }
        //}
    }


    protected void GraphLinking()
    {

    }

    protected virtual void LateUpdate()
    {
        FollowSocketTransform();
    }

    virtual public void FollowSocketTransform()
    {
        transform.position = _pivotPosition + _socketTranform.position;
        transform.rotation = Quaternion.Euler(_pivotRotation) * _socketTranform.rotation;

        //Quaternion parentRotation = transform.parent.rotation;
        //Quaternion adjustedPivotRotation = parentRotation * Quaternion.Euler(_pivotRotation);
        //transform.rotation = _socketTranform.rotation * adjustedPivotRotation;
    }



    virtual public void Equip(PlayerScript itemOwner, Transform followTransform)
    {
        _owner = itemOwner;
        _socketTranform = followTransform;
    }


    public void CheckNextAttackStates(ref List<State> statesOut)
    {
        //���� ���Ⱑ ����� ������Ʈ�� �̿�(�̰� ���� ��������)
    }


    public State NextStateCheck()
    {
        //�÷��̾�� ���⿡�� �ܼ��� ��Ŭ��, ��Ŭ�� �� ���۸� �Ѱ��ٰ���

        //���Ⱑ �˾Ƽ� �Ǵ��ؼ� ���� ������Ʈ�� �Ѱ���� �Ѵ�.
        //���߿� ������ ���߰���, ���� �޺����� ���


        //���Ⱑ ��ȹ�� ���¸� ���� ���߿� ��ĵ ����� ��û�ϸ�?

        return null;
    }



    public virtual State CalculateNextState()
    {
        return null;
    }


    virtual public void TurnOnAim() { }
    virtual public void TurnOffAim() { }
    virtual public void UnEquip() { }
}
