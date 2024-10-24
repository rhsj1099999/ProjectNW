using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum StateAction
{
    Move,
    Jump,
    Attack, //현재 무기에 따라 달라질 수 있다.
}

[Serializable]
public struct StateLinkDesc
{
    public ConditionAsset _conditionAsset;
    public StateAsset _stateAsset;
}

[Serializable]
public struct StateDesc
{
    public string _stataName;
    public AnimationClip _stateAnimationClip;

    public List<StateAction> _EnterStateActions;
    public List<StateAction> _inStateActions;
    public List<StateAction> _ExitStateActions;

    public List<StateLinkDesc> _linkedStates;
}


public struct StateInitDesc
{
    public CharacterMoveScript2 _ownerCharacterComponent;
    public Animator _ownerAnimator;
    public InputController _ownerInputController;
}



public class State
{
    private StateDesc _stateDesc; //Copy From ScriptableObject
    private StateAsset _stateAssetCreateFrom = null;
    private Dictionary<State/*Another State*/, Condition> _linkedState = new Dictionary<State , Condition>();
    StateInitDesc _ownerActionComponent;


    public void Initialize(PlayerScript owner)
    {
        _ownerActionComponent = new StateInitDesc();

        InitPartial(ref _ownerActionComponent, _stateDesc._EnterStateActions, owner);
        InitPartial(ref _ownerActionComponent, _stateDesc._inStateActions, owner);
        InitPartial(ref _ownerActionComponent, _stateDesc._ExitStateActions, owner);
    }
    private void InitPartial(ref StateInitDesc _ownerActionComponent, List<StateAction> list, PlayerScript owner)
    {
        foreach (var actions in list)
        {
            switch (actions)
            {
                case StateAction.Move:
                    {
                        if (_ownerActionComponent._ownerCharacterComponent == null)
                        {
                            _ownerActionComponent._ownerCharacterComponent = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "Move행동이 있는데 이 컴포넌트가 없습니다");
                        }

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponent<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "Input행동이 있는데 이 컴포넌트가 없습니다");
                        }
                    }
                    break;

                case StateAction.Jump:
                    {
                        if (_ownerActionComponent._ownerCharacterComponent == null)
                        {
                            _ownerActionComponent._ownerCharacterComponent = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "Input행동이 있는데 이 컴포넌트가 없습니다");
                        }

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponent<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "Input행동이 있는데 이 컴포넌트가 없습니다");
                        }
                    }
                    break;
                case StateAction.Attack:
                    {
                        //현재 장착하고있는 무기에 영향을 받습니다.
                    }
                    break;
                default:
                    Debug.Assert(false, "데이터가 추가됐습니까?");
                    break;
            }
        }
    }

    public void LinkingStates(/*컨트롤러가 사용하는 스테이트들*/ref List<State> allStates /*딕셔너리로 바꾸세요*/, PlayerScript owner)
    {
        foreach (var linked in _stateDesc._linkedStates)
        {
            Condition newCondition = new Condition(linked._conditionAsset);
            newCondition.Initialize(owner);

            foreach (var state in allStates)
            {
                if (state == this)
                {
                    continue; //나 자신이다
                }

                if (state.GetStateAssetFrom() == linked._stateAsset)
                {
                    _linkedState.Add(state, newCondition);
                    break;
                }

                Debug.Assert(false, "없는 상태로 넘어가려 하는 조건이 있습니다");
            }
        }
    }


    public StateDesc? GetStateDesc() //구조체 커질수도 있으니까 참조로 줍시다
    {
        return _stateDesc;
    }

    public StateAsset GetStateAssetFrom()
    {
        return _stateAssetCreateFrom;
    }

    public State(StateAsset stateAsset)
    {
        _stateDesc = stateAsset._myState; //복사 완료
        _stateAssetCreateFrom = stateAsset;
    }

    public State CheckChangeState()
    {
        foreach(KeyValuePair<State, Condition> pair in _linkedState)
        {
            if(pair.Value.CheckCondition() == true) //이 상태로 넘어갈 수 있는 조건을 만족했다.
            {
                return pair.Key;
            }
        }
        return null;
    }


    public void DoActions(List<StateAction> actions)
    {
        foreach(var action in actions)
        {
            switch(action)
            {
                case StateAction.Move:
                    _ownerActionComponent._ownerCharacterComponent.CharacterRotate(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                    _ownerActionComponent._ownerCharacterComponent.CharacterMove(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                    break;

                case StateAction.Jump:
                    break;
                case StateAction.Attack:
                    break;
                default:
                    Debug.Assert(false, "데이터가 추가됐습니까?");
                    break;
            }
        }
    }
}
