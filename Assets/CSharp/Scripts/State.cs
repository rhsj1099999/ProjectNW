using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using static StateContoller;

public class State
{
    private string _unityName_HipBone = "Hips";
    private string _unityName_HipBoneLocalPositionX = "RootT.x";
    private string _unityName_HipBoneLocalPositionY = "RootT.y";
    private string _unityName_HipBoneLocalPositionZ = "RootT.z";

    public class StateAnimActionInfo
    {
        public AnimationCurve _animationHipCurveX = null;
        public AnimationCurve _animationHipCurveY = null;
        public AnimationCurve _animationHipCurveZ = null;

        public float _currStateSecond = 0.0f;
        public float _prevStateSecond = 0.0f;
        public float _prevReadedSecond = 0.0f;
    }


    public State(StateAsset stateAsset)
    {
        _stateDesc = stateAsset._myState; //복사 완료
        _stateAssetCreateFrom = stateAsset;
    }


    private PlayerScript _owner = null;
    private StateDesc _stateDesc;
    private StateAsset _stateAssetCreateFrom = null;
    public StateDesc GetStateDesc() { return _stateDesc; }
    public StateAsset GetStateAssetFrom() { return _stateAssetCreateFrom; }


    private StateAnimActionInfo _stateAnimActionInfo = null;
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
    public void LinkingStates(ref List<State> allStates/*Change To Dic*/)
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

                        if (_ownerActionComponent._ownerCharacterComponent == null)
                        {
                            _ownerActionComponent._ownerCharacterComponent = owner.GetComponentInChildren<CharacterController>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "RootMove행동이 있는데 이 컴포넌트가 없습니다");
                        }

                        //애니메이션 커브 찾아놓기
                        {
                            _stateAnimActionInfo = new StateAnimActionInfo();

                            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(_stateDesc._stateAnimationClip);

                            bool curveXFind = false, curveYFind = false, curveZFind = false;

                            foreach (var binding in bindings)
                            {
                                if (curveXFind == true && curveYFind == true && curveZFind == true)
                                { break; } //다 찾았습니다.

                                if (binding.propertyName == _unityName_HipBoneLocalPositionX)
                                { _stateAnimActionInfo._animationHipCurveX = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveXFind = true; }

                                if (binding.propertyName == _unityName_HipBoneLocalPositionY)
                                { _stateAnimActionInfo._animationHipCurveY = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveYFind = true; }

                                if (binding.propertyName == _unityName_HipBoneLocalPositionZ)
                                { _stateAnimActionInfo._animationHipCurveZ = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveZFind = true; }
                            }

                            Debug.Assert((curveXFind == true && curveYFind == true && curveZFind == true), "커브가 존재하지 않습니다");
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

                default:
                    Debug.Assert(false, "데이터가 추가됐습니까?");
                    break;
            }
        }
    }




    //public void SettingOwnerComponent_LinkedCondition(PlayerScript owner)
    //{
    //    //오류검사
    //    switch (_conditionDesc._singleConditionType)
    //    {
    //        case ConditionType.KeyInput:
    //            Debug.Assert(_conditionDesc._keyInputConditionTarget.Count != 0, "키인풋인데 정보가 없다");
    //            break;
    //        case ConditionType.EquipWeaponByType:
    //            Debug.Assert(_conditionDesc._weaponTypeGoal != ItemInfo.WeaponType.NotWeapon, "WeaponType인데 Goal이 None이다. 오류는 아닐수도 있다");
    //            break;
    //        case ConditionType.AnimationFrameUp:
    //            Debug.Assert(_conditionDesc._animationFrameUpGoal > float.Epsilon, "AnimSecondType인데 Goal이 0.0이다. 오류는 아닐수도 있다");
    //            break;

    //        case ConditionType.AnimationFrameUnder:
    //            Debug.Assert(_conditionDesc._animationFrameUnderGoal > float.Epsilon, "AnimSecondType인데 Goal이 0.0이다. 오류는 아닐수도 있다");
    //            break;

    //        default:
    //            break;
    //    }

    //    switch (_conditionDesc._singleConditionType)
    //    {
    //        case ConditionType.MoveDesired:
    //            _ownerComponents._ownerInputController = owner.GetComponent<InputController>();
    //            Debug.Assert(_ownerComponents._ownerInputController != null, "Input조건이 있는데 이 컴포넌트가 없습니다");
    //            break;

    //        case ConditionType.AnimationEnd:
    //            _ownerComponents._ownerAnimator = owner.GetComponentInChildren<Animator>();
    //            Debug.Assert(_ownerComponents._ownerAnimator != null, "Animation조건이 있는데 이 컴포넌트가 없습니다");
    //            break;

    //        case ConditionType.InAir:
    //            _ownerComponents._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
    //            Debug.Assert(_ownerComponents._ownerMoveScript != null, "GroundLoss조건이 있는데 이 컴포넌트가 없습니다");
    //            break;

    //        case ConditionType.KeyInput:
    //            _ownerComponents._ownerInputController = owner.GetComponent<InputController>();
    //            Debug.Assert(_ownerComponents._ownerInputController != null, "Jump조건이 있는데 이 컴포넌트가 없습니다");
    //            break;

    //        case ConditionType.EquipWeaponByType:
    //            //|TODO| 실시간으로 무기 장착 정보를 알 수 있는 인벤토리 컴포넌트를 알아야합니다.
    //            _ownerComponents._ownerCurrWeapon = owner.GetComponent<WeaponScript>();
    //            Debug.Assert(_ownerComponents._ownerCurrWeapon != null, "Jump조건이 있는데 이 컴포넌트가 없습니다");
    //            break;

    //        case ConditionType.AnimationFrameUp:
    //            //|TODO| 지금은 owner에게서 직접 재생시간을 받아옵니다. 추후 Animator가 이동하면 이곳에 작업이 필요합니다.
    //            break;

    //        case ConditionType.AnimationFrameUnder:
    //            //|TODO| 지금은 owner에게서 직접 재생시간을 받아옵니다. 추후 Animator가 이동하면 이곳에 작업이 필요합니다.
    //            break;

    //        case ConditionType.RightHandWeaponSignaled:
    //            break;

    //        case ConditionType.LeftHandWeaponSignaled:
    //            break;

    //        default:
    //            Debug.Assert(false, "데이터가 추가됐습니까?");
    //            break;
    //    }
    //}


}
