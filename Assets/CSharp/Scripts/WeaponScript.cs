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
    MainUse, //클릭
    SubUse,
    SpecialUse,
}


//[Serializable]
//public class StateLinkDesc
//{
//    public List<ConditionDesc> _multiConditionAsset; //MultiCondition
//    public StateAsset _stateAsset;
//    private int _autoLinkWeight = 0; //각 조건들을 자동으로 계산하는 가중치 변수
//}

[Serializable]
public class WeaponStateDesc
{
    [Serializable]
    public class JumpingState
    {
        public List<StateLinkDesc> _linkedStates = new List<StateLinkDesc>(); //각각 전부 넘어갈 수 있다
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


    ////사용할 무브셋 전부 가지고있는것들
    //public List<WeaponComboEntryDesc> _weaponStateAssets = new List<WeaponComboEntryDesc>();
    //////실제로 생성된 인스턴스들
    //private List<State> _weaponStates = new List<State>();
    //private List<WeaponComboEntry> _weaponEntryStates = new List<WeaponComboEntry>();



    private void Awake()
    {
        /*-----------------------------------------------------------------------------------------------------------------
        |NOTI| 아이템 프리팹은 기본 PIVOT을 들고있다.
        무기의 위치는 자식 Transform으로 결정돠면 안된다 : (IK를 이용할 가능성 때문에)
        따라서 _pivotPosition, _pivotRotation = 무기마다 들고있는 고유 피벗 프리팹 인스펙터 창에서 미리 설정해둔다
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
        //위에 무기가 사용할 스테이트를 이용(이건 전부 진입점임)
    }


    public State NextStateCheck()
    {
        //플레이어는 무기에게 단순히 좌클릭, 우클릭 등 조작만 넘겨줄거임

        //무기가 알아서 판단해서 다음 스테이트를 넘겨줘야 한다.
        //공중에 있으면 공중공격, 다음 콤보공격 등등


        //무기가 계획된 상태를 연출 도중에 평캔 등등을 요청하면?

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
