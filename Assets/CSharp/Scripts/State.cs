using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static StateContoller;


public class State
{
    private bool _isTimerHandleAnimation = false;

    public class StateAnimActionInfo
    {
        public AnimationHipCurve _myAnimationCurve = null;
        public FrameData _myFrameData = null;

        public float _currStateSecond = 0.0f;
        public float _prevStateSecond = 0.0f;
        public float _prevReadedSecond = 0.0f;
    }

    public State(StateAsset stateAsset)
    {
        _stateDesc = stateAsset._myState; //복사 완료
        _stateAssetCreateFrom = stateAsset;

        //HipCurve Data
        {
            ResourceDataManager.Instance.AddHipCurve(_stateDesc._stateAnimationClip);
            _stateAnimActionInfo._myAnimationCurve = ResourceDataManager.Instance.GetHipCurve(_stateDesc._stateAnimationClip);
        }
    }

    private PlayerScript _owner = null;
    private StateDesc _stateDesc;
    private StateAsset _stateAssetCreateFrom = null;
    public StateDesc GetStateDesc() { return _stateDesc; }
    public StateAsset GetStateAssetFrom() { return _stateAssetCreateFrom; }

    private StateAnimActionInfo _stateAnimActionInfo = new StateAnimActionInfo();
    public StateAnimActionInfo GetStateAnimActionInfo() { return _stateAnimActionInfo; }

    private Dictionary<State, List<ConditionDesc>> _linkedState = new Dictionary<State , List<ConditionDesc>>();
    public Dictionary<State, List<ConditionDesc>> GetLinkedState() { return _linkedState; }


    public void SettingOwnerComponent(PlayerScript owner, StateContollerComponentDesc ownerComponent)
    {
        _owner = owner;
        InitPartial(ownerComponent, _stateDesc._EnterStateActionTypes, owner);
        InitPartial(ownerComponent, _stateDesc._inStateActionTypes, owner);
        InitPartial(ownerComponent, _stateDesc._ExitStateActionTypes, owner);
    }


    //Caller Must Have 'List<State>'                    /*Change To Dic*/
    public void LinkingStates(ref Dictionary<RepresentStateType, State> allStates/*Change To Dic*/)
    {
        foreach (var linked in _stateDesc._linkedStates)
        {
            State targetState = null;

            
            foreach (KeyValuePair<RepresentStateType, State> statePair in allStates)
            {
                if (statePair.Value == this)
                {
                    continue; //나 자신이다
                }

                if (statePair.Value.GetStateAssetFrom() == linked._stateAsset)
                {
                    targetState = statePair.Value;
                    break;
                }
            }


            if (targetState == null) //연결된 상태가 플레이어(혹은몬스터)가 사용하지 않는 상태다
            {
                Debug.Log("없는 상태로 넘어가려 하는 조건이 있습니다. 오류는 아닐수도 있지만 실수일수도 있습니다.");
                continue;
            }


            if (_linkedState.ContainsKey(targetState) == false)
            {
                _linkedState.Add(targetState, linked._multiConditionAsset);
            }
        }
    }


    private void InitPartial(StateContollerComponentDesc _ownerActionComponent, List<StateActionType> list, PlayerScript owner)
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

                        if (_ownerActionComponent._ownerCharacterComponent == null)
                        {
                            _ownerActionComponent._ownerCharacterComponent = owner.GetComponent<CharacterController>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "ForcedMove행동이 있는데 이 컴포넌트가 없습니다");
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

                case StateActionType.RootMove:
                    {
                        if (_ownerActionComponent._ownerAnimator == null)
                        {
                            _ownerActionComponent._ownerAnimator = owner.GetComponentInChildren<Animator>();
                            Debug.Assert(_ownerActionComponent._ownerAnimator != null, "RootMove행동이 있는데 이 컴포넌트가 없습니다");
                        }

                        if (_ownerActionComponent._ownerModelObjectOrigin == null)
                        {
                            _ownerActionComponent._ownerModelObjectOrigin = _ownerActionComponent._ownerAnimator.gameObject;
                            Debug.Assert(_ownerActionComponent._ownerModelObjectOrigin != null, "RootMove행동이 있는데 모델이 없습니다");
                        }

                        if (_ownerActionComponent._ownerCharacterComponent == null)
                        {
                            _ownerActionComponent._ownerCharacterComponent = owner.GetComponentInChildren<CharacterController>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "RootMove행동이 있는데 이 컴포넌트가 없습니다");
                        }
                    }
                    break;

                case StateActionType.RotateWithoutInterpolate:
                    {
                        if (_ownerActionComponent._ownerCharacterComponent == null)
                        {
                            _ownerActionComponent._ownerCharacterComponent = owner.GetComponentInChildren<CharacterController>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "RotateWithoutInterpolate행동이 있는데 이 컴포넌트가 없습니다");
                        }

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponentInChildren<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "RotateWithoutInterpolate행동이 있는데 이 컴포넌트가 없다");
                        }

                        if (_ownerActionComponent._ownerMoveScript == null)
                        {
                            _ownerActionComponent._ownerMoveScript = owner.GetComponentInChildren<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerMoveScript != null, "RotateWithoutInterpolate행동이 있는데 이 컴포넌트가 없다");
                        }
                    }
                    break;

                case StateActionType.RightHandWeaponSignal:
                    {
                        //_ownerActionComponent._owner.RightHandWeaponSignal();
                    }
                    break;

                case StateActionType.LeftHandWeaponSignal:
                    {
                        //_ownerActionComponent._owner.LeftHandWeaponSignal();
                    }
                    break;

                case StateActionType.AttackCommandCheck:
                    break;

                case StateActionType.StateEndDesierdCheck:
                    break;

                case StateActionType.CheckBehaves:
                    break;

                case StateActionType.CalculateWeaponLayer_EnterAttack:
                    break;

                case StateActionType.CalculateWeaponLayer_ExitAttack:
                    break;

                default:
                    Debug.Assert(false, "데이터가 추가됐습니까?");
                    break;
            }
        }
    }
}
