using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
{
    public int Compare(T x, T y)
    {
        // �⺻ �������� ������ ����� �������� ������������ ��ȯ
        return y.CompareTo(x);
    }
}

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


[Serializable]
public class WeaponStateDesc
{
    [Serializable]
    public class EachState
    {
        private bool _isEntry = false;
        public List<ConditionDesc> _entryConditions = new List<ConditionDesc>();

        public StateAsset _state = null;
        public List<ConditionDesc> _nextStateConditions = new List<ConditionDesc>();
        public List<StateLinkDesc> _linkedStates = new List<StateLinkDesc>();
    }

    public List<EachState> _states = new List<EachState>();
}

public class EntryState
{
    public State _state = null;
    public List<ConditionDesc> _entryCondition = null;
}


public class StateNodeDesc
{
    public class LinkedState
    {
        public State _state = null;
        public List<ConditionDesc> _multiConditions = null;
    }

    private HashSet<State> _linked = new HashSet<State>();
    private SortedDictionary<int, List<LinkedState>>  _linkedStates = new SortedDictionary<int, List<LinkedState>>(new DescendingComparer<int>());
    public SortedDictionary<int, List<LinkedState>> GetLinkecStates() { return _linkedStates; } 

    public void AddNode(int weight, LinkedState willBeLinkedState)
    {
        if (_linkedStates.ContainsKey(weight) == false) 
        {
            _linkedStates.Add(weight, new List<LinkedState>());
        }

        List<LinkedState> targetLinked = _linkedStates[weight];

        targetLinked.Add(willBeLinkedState);
        _linked.Add(willBeLinkedState._state);
    }

    public bool FindNode(State willBeLinkedState)
    {
        return _linked.Contains(willBeLinkedState);
    }
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
    private Dictionary<State, StateNodeDesc> _weaponStates = new Dictionary<State, StateNodeDesc>();
    private SortedDictionary<int, List<EntryState>> _entryStates = new SortedDictionary<int, List<EntryState>>(new DescendingComparer<int>());
    public StateNodeDesc FindLinkedStateNodeDesc(State targetState)
    {
        if (_weaponStates.ContainsKey(targetState) == false)
        {
            return null;
        }
        return _weaponStates[targetState];
    }

    private void Awake()
    {
        /*-----------------------------------------------------------------------------------------------------------------
        |NOTI| ������ �������� �⺻ PIVOT�� ����ִ�.
        ������ ��ġ�� �ڽ� Transform���� �����¸� �ȵȴ� : (IK�� �̿��� ���ɼ� ������)
        ���� _pivotPosition, _pivotRotation = ���⸶�� ����ִ� ���� �ǹ� ������ �ν����� â���� �̸� �����صд�
        -----------------------------------------------------------------------------------------------------------------*/
        GraphLinking();
    }

    public int CalculateConditionWeight(List<ConditionDesc> conditions)
    {
        int retWeight = 0;

        foreach (ConditionDesc condition in conditions)
        {
            //�⺻������ ������ �ϳ� �ɷ������� ����ġ +1�Դϴ�.
            //�޺� Ű, KeyInput��쿡�� Ű�� �������� ����ġ�� �����ϴ�.
            switch (condition._singleConditionType)
            {
                default:
                    retWeight++;
                    break;

                case ConditionType.KeyInput:
                    {
                        //�� Ű ���� ... ver 1
                        List<KeyInputConditionDesc> keys = condition._keyInputConditionTarget;
                        retWeight += keys.Count;
                    }
                    break;

                case ConditionType.ComboKeyCommand:
                    {
                        //����Ű�� �� ���� + �޺����� ... ver 1
                        List<ComboKeyCommandDesc> comboKeys = condition._commandInputConditionTarget;
                        foreach (ComboKeyCommandDesc command in comboKeys)
                        {
                            retWeight += command._targetCommandKeys.Count;
                        }
                    }
                    break;
            }
        }

        return retWeight;
    }


    protected void GraphLinking()
    {
        Dictionary<StateAsset, State> tempReadyAssets = new Dictionary<StateAsset, State>();

        //EntryState�� �̸� �����д�.
        {
            for (int i = 0; i < _weaponStateAssets.Count; i++)
            {
                StateAsset entryNode = _weaponStateAssets[i]._states[0]._state;

                if (tempReadyAssets.ContainsKey(entryNode) == false)//���� ��ȸ �� ����̴�.
                {
                    State newState = new State(entryNode);
                    tempReadyAssets.Add(entryNode, newState);
                    Debug.Assert(_weaponStateAssets[i]._states[0]._entryConditions != null, "Entry �ε� null�̸� �ȵ˴ϴ�.");
                    Debug.Assert(_weaponStateAssets[i]._states[0]._entryConditions.Count > 0 , "Entry �ε� Count�� 0�̸� �ȵ˴ϴ�.");
                    StateNodeDesc newStateNode = new StateNodeDesc();
                    _weaponStates.Add(newState, newStateNode);

                    int entryWeight = CalculateConditionWeight(_weaponStateAssets[i]._states[0]._entryConditions);

                    //Dictionary<int, List<State>>
                    if (_entryStates.ContainsKey(entryWeight) == false)
                    {
                        _entryStates.Add(entryWeight, new List<EntryState>());
                    }
                    EntryState newEntryState = new EntryState();
                    newEntryState._state = newState;
                    newEntryState._entryCondition = _weaponStateAssets[i]._states[0]._entryConditions;
                    _entryStates[entryWeight].Add(newEntryState);
                }
            }
        }


        for (int i = 0; i < _weaponStateAssets.Count; i++)
        {
            for (int j = 0; j < _weaponStateAssets[i]._states.Count; j++)
            {
                StateAsset node = _weaponStateAssets[i]._states[j]._state;

                State targetState = null;
                tempReadyAssets.TryGetValue(node, out targetState);

                if (targetState == null)//���� ��ȸ �� ����̴�.
                {
                    State newState = new State(node);
                    tempReadyAssets.Add(node, newState);
                    StateNodeDesc newStateNode = new StateNodeDesc();
                    _weaponStates.Add(newState, newStateNode);
                }

                //Next Combo State ���� ...  ���� �޺��� �ִٸ� ... 
                if ((j + 1) < _weaponStateAssets[i]._states.Count)
                {
                    StateAsset nextComboNode = _weaponStateAssets[i]._states[j + 1]._state;

                    State nextComboState = null;
                    tempReadyAssets.TryGetValue(nextComboNode, out nextComboState);

                    if (nextComboState == null)//���� ��ȸ �� ����̴�.
                    {
                        State newState = new State(nextComboNode);
                        tempReadyAssets.Add(nextComboNode, newState);
                        StateNodeDesc newStateNode = new StateNodeDesc();
                        _weaponStates.Add(newState, newStateNode);
                    }
                    nextComboState = tempReadyAssets[nextComboNode];

                    //   [[ --- targetState  --->>  stateWillBeLinked --- ]]
                    Debug.Assert(_weaponStates.ContainsKey(targetState) != false, "������ �ȵ˴ϴ�");
                    StateNodeDesc targetLinkedDesc = _weaponStates[targetState];

                    if (targetLinkedDesc.FindNode(nextComboState) == false)
                    {
                        StateNodeDesc.LinkedState linkingDesc = new StateNodeDesc.LinkedState();
                        linkingDesc._state = nextComboState;
                        linkingDesc._multiConditions = _weaponStateAssets[i]._states[j + 1]._nextStateConditions;
                        
                        int stateWeight = CalculateConditionWeight(linkingDesc._multiConditions);
                        targetLinkedDesc.AddNode(stateWeight, linkingDesc);
                    }
                }


                // Jumping Linking State ����
                {
                    for (int k = 0; k < _weaponStateAssets[i]._states[j]._linkedStates.Count; k++)
                    {
                        StateAsset linkedNode = _weaponStateAssets[i]._states[j]._linkedStates[k]._stateAsset;

                        State willBeLinkedState = null;
                        tempReadyAssets.TryGetValue(linkedNode, out willBeLinkedState);

                        if (willBeLinkedState == null)//���� ��ȸ �� ����̴�.
                        {
                            State newState = new State(linkedNode);
                            tempReadyAssets.Add(linkedNode, newState);
                            StateNodeDesc newStateNode = new StateNodeDesc();
                            _weaponStates.Add(newState, newStateNode);
                        }

                        willBeLinkedState = tempReadyAssets[linkedNode];


                        //   [[ --- targetState  --->>  stateWillBeLinked --- ]]
                        Debug.Assert(_weaponStates.ContainsKey(targetState) != false, "������ �ȵ˴ϴ�");
                        StateNodeDesc targetLinkedDesc = _weaponStates[targetState];

                        if (targetLinkedDesc.FindNode(willBeLinkedState) == false)
                        {
                            StateNodeDesc.LinkedState linkingDesc = new StateNodeDesc.LinkedState();
                            linkingDesc._state = willBeLinkedState;
                            linkingDesc._multiConditions = _weaponStateAssets[i]._states[j]._linkedStates[k]._multiConditionAsset;

                            int stateWeight = CalculateConditionWeight(linkingDesc._multiConditions);
                            targetLinkedDesc.AddNode(stateWeight, linkingDesc);
                        }
                    }
                }


                //Entry State ���� ... ���� �޺��� Entry�δ� �Ѿ �� ����.
                {
                    for (int k = 0; k < _weaponStateAssets.Count; k++)
                    {
                        List<WeaponStateDesc.EachState> temoComboList = _weaponStateAssets[k]._states;

                        if (k == i)
                        {
                            continue;
                        }

                        StateAsset EntryState = temoComboList[0]._state;
                        State willBeLinkedState = null;
                        tempReadyAssets.TryGetValue(EntryState, out willBeLinkedState);
                        Debug.Assert(willBeLinkedState != null, "������ �ȵ˴ϴ�");

                        if (targetState == willBeLinkedState)
                        {
                            continue;
                        }

                        //   [[ --- targetState  --->>  stateWillBeLinked --- ]]
                        Debug.Assert(_weaponStates.ContainsKey(targetState) != false, "������ �ȵ˴ϴ�");
                        Debug.Assert(_weaponStates.ContainsKey(willBeLinkedState) != false, "������ �ȵ˴ϴ�");

                        StateNodeDesc targetLinkedDesc = _weaponStates[targetState];
                        if (targetLinkedDesc.FindNode(willBeLinkedState) == false)
                        {
                            StateNodeDesc.LinkedState linkingDesc = new StateNodeDesc.LinkedState();
                            linkingDesc._state = willBeLinkedState;
                            linkingDesc._multiConditions = temoComboList[0]._entryConditions;

                            int stateWeight = CalculateConditionWeight(linkingDesc._multiConditions);
                            targetLinkedDesc.AddNode(stateWeight, linkingDesc);
                        }

                    }
                }
            }
        }
    }

    protected virtual void LateUpdate()
    {
        FollowSocketTransform();
    }

    virtual public void FollowSocketTransform()
    {
        transform.rotation = _socketTranform.rotation * Quaternion.Euler(_pivotRotation);
        transform.position = (transform.rotation * _pivotPosition) + _socketTranform.position;
    }



    virtual public void Equip(PlayerScript itemOwner, Transform followTransform)
    {
        _owner = itemOwner;
        _socketTranform = followTransform;
    }



    public SortedDictionary<int, List<EntryState>> GetEntryStates()
    {
        return _entryStates;
    }



    virtual public void TurnOnAim() { }
    virtual public void TurnOffAim() { }
    virtual public void UnEquip() { }
}