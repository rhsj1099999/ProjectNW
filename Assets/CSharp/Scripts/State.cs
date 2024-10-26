using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum StateActionType
{
    Move,
    Attack, //현재 무기에 따라 달라질 수 있다.
    SaveLatestVelocity,
    Jump,
    ForcedMove,
    ResetLatestVelocity,
}
public enum ConditionType
{
    MoveDesired,
    AnimationEnd,
    InAir,
    KeyInput,
}

[Serializable]
public class KeyInputConditionDesc
{
    public enum KeyPressType
    {
        Pressed,
        Hold,
        Released,
        None,
    };

    public KeyCode _targetKeyCode;
    public KeyPressType _targetState;
    public bool _keyInpuyGoal;
}

[Serializable]
public class ConditionDesc
{
    public ConditionType _singleConditionType;
    public bool _componentConditionGoal;
    public List<KeyInputConditionDesc> _keyInputConditionTarget;
}

[Serializable]
public class StateLinkDesc
{
    public List<ConditionDesc> _multiConditionAsset; //MultiCondition
    public StateAsset _stateAsset;
}

[Serializable]
public class StateDesc
{
    public string _stataName;
    public AnimationClip _stateAnimationClip;

    public List<StateActionType> _EnterStateActionTypes;
    public List<StateActionType> _inStateActionTypes;
    public List<StateActionType> _ExitStateActionTypes;

    public List<StateLinkDesc> _linkedStates;
}

public class State
{
    public State(StateAsset stateAsset)
    {
        _stateDesc = stateAsset._myState; //복사 완료
        _stateAssetCreateFrom = stateAsset;
    }

    public class StateInitDesc
    {
        public CharacterMoveScript2 _ownerMoveScript;
        public CharacterController _ownerCharacterController;
        public InputController _ownerInputController;
        public Animator _ownerAnimator;
    }


    private StateDesc _stateDesc; //Copy From ScriptableObject
    private StateAsset _stateAssetCreateFrom = null;
    private Dictionary<State/*Another State*/, List<Condition>> _linkedState = new Dictionary<State , List<Condition>>();
    private StateInitDesc _ownerActionComponent;

    public StateDesc GetStateDesc() {return _stateDesc;}
    public StateAsset GetStateAssetFrom() {return _stateAssetCreateFrom;}


    public void Initialize(PlayerScript owner)
    {
        _ownerActionComponent = new StateInitDesc();

        InitPartial(ref _ownerActionComponent, _stateDesc._EnterStateActionTypes, owner);
        InitPartial(ref _ownerActionComponent, _stateDesc._inStateActionTypes, owner);
        InitPartial(ref _ownerActionComponent, _stateDesc._ExitStateActionTypes, owner);
    }

    public void LinkingStates(/*컨트롤러가 사용하는 스테이트들*/ref List<State> allStates /*딕셔너리로 바꾸세요*/, PlayerScript owner)
    {
        foreach (var linked in _stateDesc._linkedStates)
        {
            State targetState = null;

            foreach (var state in allStates)
            {
                if (state == this)
                {
                    continue; //나 자신이다
                }

                if (state.GetStateAssetFrom() == linked._stateAsset)
                {
                    targetState = state;
                    break;
                }
                //Debug.Assert(false, "없는 상태로 넘어가려 하는 조건이 있습니다");
            }

            if (targetState == null) //연결된 상태가 플레이어(혹은몬스터)가 사용하지 않는 상태다
            {
                continue;
            }

            
            if (_linkedState.ContainsKey(targetState) == false)
            {
                _linkedState.Add(targetState, new List<Condition>());
            }

            List<Condition> existConditions = _linkedState[targetState];

            for (int i = 0; i < linked._multiConditionAsset.Count; i++)
            {
                Condition newCondition = new Condition(linked._multiConditionAsset[i]);
                newCondition.Initialize(owner);
                existConditions.Add(newCondition);
            }
        }
    }

    public State CheckChangeState()
    {
        foreach(KeyValuePair<State, List<Condition>> pair in _linkedState)
        {
            bool isSuccess = true;

            foreach (Condition condition in pair.Value) 
            {
                if (condition.CheckCondition() == false)
                {
                    isSuccess = false;
                    break; //멀티컨디션에서 하나라도 삑났다.
                }
            }

            if (isSuccess == true)
            {
                return pair.Key; //전부다 만족했다.
            }
        }

        return null; //만족한게 하나도 없다.
    }


    public void DoActions(List<StateActionType> actions)
    {
        foreach(var action in actions)
        {
            switch(action)
            {
                case StateActionType.Move:
                    _ownerActionComponent._ownerMoveScript.CharacterRotate(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                    _ownerActionComponent._ownerMoveScript.CharacterMove(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                    break;

                case StateActionType.Attack:
                    break;

                case StateActionType.SaveLatestVelocity:
                    break;

                case StateActionType.Jump:
                    _ownerActionComponent._ownerMoveScript.DoJump();
                    break;

                case StateActionType.ForcedMove:
                    Vector3 planeVelocity = _ownerActionComponent._ownerMoveScript.GetLatestVelocity();
                    planeVelocity.y = 0.0f;
                    _ownerActionComponent._ownerMoveScript.CharacterForcedMove(planeVelocity, 1.0f);
                    break;

                case StateActionType.ResetLatestVelocity:
                    //_ownerActionComponent._ownerMoveScript.ResetLatestVelocity();
                    break;

                default:
                    Debug.Assert(false, "데이터가 추가됐습니까?");
                    break;
            }
        }
    }




    private void InitPartial(ref StateInitDesc _ownerActionComponent, List<StateActionType> list, PlayerScript owner)
    {
        foreach (var actions in list)
        {
            switch (actions)
            {
                case StateActionType.Move:
                    {
                        if (_ownerActionComponent._ownerMoveScript == null)
                        {
                            _ownerActionComponent._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerMoveScript != null, "Move행동이 있는데 이 컴포넌트가 없습니다");
                        }

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponent<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "Input행동이 있는데 이 컴포넌트가 없습니다");
                        }
                    }
                    break;

                case StateActionType.Attack:
                    {
                        //현재 장착하고있는 무기에 영향을 받습니다.
                    }
                    break;

                case StateActionType.SaveLatestVelocity:
                    {
                        if (_ownerActionComponent._ownerMoveScript == null)
                        {
                            _ownerActionComponent._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerMoveScript != null, "SaveLatestVelocity행동이 있는데 이 컴포넌트가 없습니다");
                        }
                    }
                    break;

                case StateActionType.Jump:
                    {
                        if (_ownerActionComponent._ownerMoveScript == null)
                        {
                            _ownerActionComponent._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerMoveScript != null, "Jump행동이 있는데 이 컴포넌트가 없습니다");
                        }
                    }
                    break;

                case StateActionType.ForcedMove:
                    {
                        if (_ownerActionComponent._ownerMoveScript == null)
                        {
                            _ownerActionComponent._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerMoveScript != null, "ForcedMove행동이 있는데 이 컴포넌트가 없습니다");
                        }

                        if (_ownerActionComponent._ownerCharacterController == null)
                        {
                            _ownerActionComponent._ownerCharacterController = owner.GetComponent<CharacterController>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterController != null, "ForcedMove행동이 있는데 이 컴포넌트가 없습니다");
                        }
                    }
                    break;

                case StateActionType.ResetLatestVelocity:
                    {
                        if (_ownerActionComponent._ownerMoveScript == null)
                        {
                            _ownerActionComponent._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerMoveScript != null, "ResetLatestVelocity행동이 있는데 이 컴포넌트가 없습니다");
                        }
                    }
                    break;

                default:
                    Debug.Assert(false, "데이터가 추가됐습니까?");
                    break;
            }
        }
    }
}
