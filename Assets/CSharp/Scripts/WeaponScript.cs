using System;
using System.Collections.Generic;
using UnityEngine;




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














    private void Awake()
    {
        /*-----------------------------------------------------------------------------------------------------------------
        |NOTI| 아이템 프리팹은 기본 PIVOT을 들고있다.
        무기의 위치는 자식 Transform으로 결정돠면 안된다 : (IK를 이용할 가능성 때문에)
        따라서 _pivotPosition, _pivotRotation = 무기마다 들고있는 고유 피벗 프리팹 인스펙터 창에서 미리 설정해둔다
        -----------------------------------------------------------------------------------------------------------------*/
    }

    public virtual void InitIK()
    {
        _ownerIKSkript = _owner.GetComponentInChildren<CharacterAnimatorScript>().gameObject.GetComponentInChildren<IKScript>();

        //IK 세팅 단계
        IKTargetScript[] ikTargets = gameObject.GetComponentsInChildren<IKTargetScript>();

        foreach (IKTargetScript ikTarget in ikTargets)
        {
            IKTargetDesc desc = ikTarget.GetDesc();

            if (desc._isRightSide != _isRightHandWeapon)
            {
                continue;
            }

            ikTarget.RegistIK(_ownerIKSkript, _isRightHandWeapon);

            AvatarIKGoal goal = AvatarIKGoal.LeftHand;
            
            if (_isRightHandWeapon == true)
            {
                if (ikTarget._isMainHandle == true)
                {
                    goal = AvatarIKGoal.RightHand;
                }
                else
                {
                    goal = AvatarIKGoal.LeftHand;
                }
            }
            else 
            {
                if (ikTarget._isMainHandle == true)
                {
                    goal = AvatarIKGoal.LeftHand;
                }
                else
                {
                    goal = AvatarIKGoal.RightHand;
                }
            }

            _createdIKTargets.Add(goal, ikTarget.GetDesc());
            _ownerIKSkript.OffIK(ikTarget.GetDesc());
        }
    }

    protected virtual void LateUpdate()
    {
        FollowSocketTransform();
    }



    virtual public void FollowSocketTransform()
    {
        if (_isRightHandWeapon == true)
        {
            transform.rotation = _socketTranform.rotation * Quaternion.Euler(_pivotRotation_Right);
            transform.position = (transform.rotation * _pivotPosition_Right) + _socketTranform.position;
        }
        else 
        {
            transform.rotation = _socketTranform.rotation * Quaternion.Euler(_pivotRotation_Left);
            transform.position = (transform.rotation * _pivotPosition_Left) + _socketTranform.position;
        }
    }



    virtual public void Equip(CharacterScript itemOwner, Transform followTransform)
    {
        _owner = itemOwner;
        Equip_OnSocket(followTransform);
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



    //public SortedDictionary<int, List<LinkedState>> GetEntryStates()
    //{
    //    return _entryStates;
    //}


    //public int CalculateConditionWeight(List<ConditionDesc> conditions)
    //{
    //    int retWeight = 0;

    //    foreach (ConditionDesc condition in conditions)
    //    {
    //        //기본적으로 조건이 하나 걸려있으면 가중치 +1입니다.
    //        //콤보 키, KeyInput경우에는 키가 어려울수록 가중치가 더들어갑니다.
    //        switch (condition._singleConditionType)
    //        {
    //            default:
    //                retWeight++;
    //                break;

    //            case ConditionType.KeyInput:
    //                {
    //                    //총 키 개수 ... ver 1
    //                    List<KeyInputConditionDesc> keys = condition._keyInputConditionTarget;
    //                    retWeight += keys.Count;
    //                }
    //                break;

    //            case ConditionType.ComboKeyCommand:
    //                {
    //                    //조합키들 총 개수 + 콤보개수 ... ver 1
    //                    List<ComboKeyCommandDesc> comboKeys = condition._commandInputConditionTarget;
    //                    foreach (ComboKeyCommandDesc command in comboKeys)
    //                    {
    //                        retWeight += command._targetCommandKeys.Count;
    //                    }
    //                    retWeight += condition._commandInputConditionTarget.Count;
    //                }
    //                break;
    //        }
    //    }

    //    return retWeight;
    //}


    //protected void GraphLinking()
    //{
    //    Dictionary<StateAsset, State> tempReadyAssets = new Dictionary<StateAsset, State>();

    //    //EntryState를 미리 만들어둔다.
    //    for (int i = 0; i < _weaponStateAssets.Count; i++)
    //    {
    //        StateAsset entryNode = _weaponStateAssets[i]._states[0]._state;

    //        if (tempReadyAssets.ContainsKey(entryNode) == false)//최초 순회 된 노드이다.
    //        {
    //            //State newState = new State(entryNode);
    //            State newState = ResourceDataManager.Instance.GetState(entryNode);
    //            tempReadyAssets.Add(entryNode, newState);
    //            Debug.Assert(_weaponStateAssets[i]._states[0]._entryConditions != null, "Entry 인데 null이면 안됩니다.");
    //            Debug.Assert(_weaponStateAssets[i]._states[0]._entryConditions.Count > 0, "Entry 인데 Count가 0이면 안됩니다.");
    //            StateNodeDesc newStateNode = new StateNodeDesc();
    //            _weaponStates.Add(newState, newStateNode);

    //            int entryWeight = CalculateConditionWeight(_weaponStateAssets[i]._states[0]._entryConditions);

    //            //Dictionary<int, List<State>>
    //            if (_entryStates.ContainsKey(entryWeight) == false)
    //            {
    //                _entryStates.Add(entryWeight, new List<LinkedState>());
    //            }
    //            LinkedState newEntryState = new LinkedState();
    //            newEntryState._state = newState;
    //            newEntryState._multiConditions = _weaponStateAssets[i]._states[0]._entryConditions;
    //            _entryStates[entryWeight].Add(newEntryState);
    //        }
    //    }

    //    //State Linking 단계
    //    for (int i = 0; i < _weaponStateAssets.Count; i++)
    //    {
    //        for (int j = 0; j < _weaponStateAssets[i]._states.Count; j++)
    //        {
    //            StateAsset node = _weaponStateAssets[i]._states[j]._state;

    //            State targetState = null;
    //            tempReadyAssets.TryGetValue(node, out targetState);

    //            if (targetState == null)//최초 순회 된 노드이다.
    //            {
    //                //State newState = new State(node);
    //                State newState = ResourceDataManager.Instance.GetState(node);
    //                tempReadyAssets.Add(node, newState);
    //                StateNodeDesc newStateNode = new StateNodeDesc();
    //                _weaponStates.Add(newState, newStateNode);
    //            }

    //            //Next Combo State 연결 ...  다음 콤보가 있다면 ... 
    //            if ((j + 1) < _weaponStateAssets[i]._states.Count)
    //            {
    //                StateAsset nextComboNode = _weaponStateAssets[i]._states[j + 1]._state;

    //                State nextComboState = null;
    //                tempReadyAssets.TryGetValue(nextComboNode, out nextComboState);

    //                if (nextComboState == null)//최초 순회 된 노드이다.
    //                {
    //                    //State newState = new State(nextComboNode);
    //                    State newState = ResourceDataManager.Instance.GetState(nextComboNode);
    //                    tempReadyAssets.Add(nextComboNode, newState);
    //                    StateNodeDesc newStateNode = new StateNodeDesc();
    //                    _weaponStates.Add(newState, newStateNode);
    //                }
    //                nextComboState = tempReadyAssets[nextComboNode];

    //                //   [[ --- targetState  --->>  stateWillBeLinked --- ]]
    //                Debug.Assert(_weaponStates.ContainsKey(targetState) != false, "없으면 안됩니다");
    //                StateNodeDesc targetLinkedDesc = _weaponStates[targetState];

    //                if (targetLinkedDesc.FindNode(nextComboState) == false)
    //                {
    //                    LinkedState linkingDesc = new LinkedState();
    //                    linkingDesc._state = nextComboState;
    //                    linkingDesc._multiConditions = _weaponStateAssets[i]._states[j + 1]._nextStateConditions;

    //                    int stateWeight = CalculateConditionWeight(linkingDesc._multiConditions);
    //                    targetLinkedDesc.AddNode(stateWeight, linkingDesc);
    //                }
    //            }


    //            // Jumping Linking State 연결
    //            {
    //                for (int k = 0; k < _weaponStateAssets[i]._states[j]._linkedStates.Count; k++)
    //                {
    //                    StateAsset linkedNode = _weaponStateAssets[i]._states[j]._linkedStates[k]._stateAsset;

    //                    State willBeLinkedState = null;
    //                    tempReadyAssets.TryGetValue(linkedNode, out willBeLinkedState);

    //                    if (willBeLinkedState == null)//최초 순회 된 노드이다.
    //                    {
    //                        //State newState = new State(linkedNode);
    //                        State newState = ResourceDataManager.Instance.GetState(linkedNode);
    //                        tempReadyAssets.Add(linkedNode, newState);
    //                        StateNodeDesc newStateNode = new StateNodeDesc();
    //                        _weaponStates.Add(newState, newStateNode);
    //                    }

    //                    willBeLinkedState = tempReadyAssets[linkedNode];


    //                    //   [[ --- targetState  --->>  stateWillBeLinked --- ]]
    //                    Debug.Assert(_weaponStates.ContainsKey(targetState) != false, "없으면 안됩니다");
    //                    StateNodeDesc targetLinkedDesc = _weaponStates[targetState];

    //                    if (targetLinkedDesc.FindNode(willBeLinkedState) == false)
    //                    {
    //                        LinkedState linkingDesc = new LinkedState();
    //                        linkingDesc._state = willBeLinkedState;
    //                        linkingDesc._multiConditions = _weaponStateAssets[i]._states[j]._linkedStates[k]._multiConditionAsset;

    //                        int stateWeight = CalculateConditionWeight(linkingDesc._multiConditions);
    //                        targetLinkedDesc.AddNode(stateWeight, linkingDesc);
    //                    }
    //                }
    //            }


    //            //Entry State 연결 ... 동일 콤보의 Entry로는 넘어갈 수 없다.
    //            {
    //                for (int k = 0; k < _weaponStateAssets.Count; k++)
    //                {
    //                    List<WeaponStateDesc.EachState> temoComboList = _weaponStateAssets[k]._states;

    //                    if (k == i)
    //                    {
    //                        continue;
    //                    }

    //                    StateAsset EntryState = temoComboList[0]._state;
    //                    State willBeLinkedState = null;
    //                    tempReadyAssets.TryGetValue(EntryState, out willBeLinkedState);
    //                    Debug.Assert(willBeLinkedState != null, "없으면 안됩니다");

    //                    if (targetState == willBeLinkedState)
    //                    {
    //                        continue;
    //                    }

    //                    //   [[ --- targetState  --->>  stateWillBeLinked --- ]]
    //                    Debug.Assert(_weaponStates.ContainsKey(targetState) != false, "없으면 안됩니다");
    //                    Debug.Assert(_weaponStates.ContainsKey(willBeLinkedState) != false, "없으면 안됩니다");

    //                    StateNodeDesc targetLinkedDesc = _weaponStates[targetState];
    //                    if (targetLinkedDesc.FindNode(willBeLinkedState) == false)
    //                    {
    //                        LinkedState linkingDesc = new LinkedState();
    //                        linkingDesc._state = willBeLinkedState;
    //                        linkingDesc._multiConditions = temoComboList[0]._entryConditions;

    //                        int stateWeight = CalculateConditionWeight(linkingDesc._multiConditions);
    //                        targetLinkedDesc.AddNode(stateWeight, linkingDesc);
    //                    }

    //                }
    //            }
    //        }
    //    }
    //}
}