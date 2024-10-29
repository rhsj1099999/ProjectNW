using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum StateActionType
{
    Move,
    Attack, //현재 무기에 따라 달라질 수 있다.
    SaveLatestVelocity,
    Jump,
    ForcedMove,
    ResetLatestVelocity,
    RootMove,
    RotateWithoutInterpolate,
}

public enum ConditionType
{
    MoveDesired,
    AnimationEnd,
    InAir,
    KeyInput,
    EquipWeaponByType,
    AnimationFrameUp, //~~초 이상으로 재생 됐습니다. -> Animator가 지원하는 normalizedTime을 쓰지 않습니다.
    AnimationFrameUnder, //~~초 이하로 재생 됐습니다. -> Animator가 지원하는 normalizedTime을 쓰지 않습니다.
}

[Serializable]
public class KeyInputConditionDesc
{


    public KeyCode _targetKeyCode;
    public KeyPressType _targetState;
    public bool _keyInpuyGoal;
    public float _keyHoldGoal = 0.0f; //n초 이상 이 키를 눌렀다 근데 어디 저장하냐
}

[Serializable]
public class ConditionDesc
{
    public ConditionType _singleConditionType;
    public bool _componentConditionGoal;
    public ItemInfo.WeaponType _weaponTypeGoal;
    public List<KeyInputConditionDesc> _keyInputConditionTarget;
    public float _animationFrameUpGoal;
    public float _animationFrameUnderGoal;
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
        public PlayerScript _owner = null;
        public CharacterMoveScript2 _ownerMoveScript = null;
        public CharacterController _ownerCharacterController = null;
        public InputController _ownerInputController = null;
        public Animator _ownerAnimator = null;

        public AnimationCurve _animationHipCurveX = null;
        public AnimationCurve _animationHipCurveY = null;
        public AnimationCurve _animationHipCurveZ = null;

        public float _currStateSecond = 0.0f;
        public float _prevStateSecond = 0.0f;
        public float _prevReadedSecond = 0.0f;
    }


    private StateDesc _stateDesc; //Copy From ScriptableObject
    private StateAsset _stateAssetCreateFrom = null;
    private Dictionary<State/*Another State*/, List<Condition>> _linkedState = new Dictionary<State , List<Condition>>();
    private StateInitDesc _ownerActionComponent;
    private string _unityName_HipBone = "Hips";
    private string _unityName_HipBoneLocalPositionX = "RootT.x";
    private string _unityName_HipBoneLocalPositionY = "RootT.y";
    private string _unityName_HipBoneLocalPositionZ = "RootT.z";


    public StateDesc GetStateDesc() {return _stateDesc;}
    public StateAsset GetStateAssetFrom() {return _stateAssetCreateFrom;}
    


    public void Initialize(PlayerScript owner)
    {
        _ownerActionComponent = new StateInitDesc();

        _ownerActionComponent._owner = owner;

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
        _ownerActionComponent._currStateSecond += Time.deltaTime;

        foreach (var action in actions)
        {
            switch(action)
            {
                case StateActionType.Move:
                    {
                        _ownerActionComponent._ownerMoveScript.CharacterRotate(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                        _ownerActionComponent._ownerMoveScript.CharacterMove(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                    }

                    break;

                case StateActionType.Attack:
                    break;

                case StateActionType.SaveLatestVelocity:
                    break;

                case StateActionType.Jump:
                    {
                        _ownerActionComponent._ownerMoveScript.DoJump();
                    }
                    
                    break;

                case StateActionType.ForcedMove:
                    {
                        Vector3 planeVelocity = _ownerActionComponent._ownerMoveScript.GetLatestVelocity();
                        planeVelocity.y = 0.0f;
                        _ownerActionComponent._ownerMoveScript.CharacterForcedMove(planeVelocity, 1.0f);
                    }

                    break;

                case StateActionType.ResetLatestVelocity:
                    break;

                case StateActionType.RootMove:
                    {
                        float currentSecond = _ownerActionComponent._owner.GetCurrAnimationClipSecond();
                        //float currentSecond = _ownerActionComponent._currStateSecond;

                        Vector3 currentUnityLocalHip = new Vector3
                            (
                            _ownerActionComponent._animationHipCurveX.Evaluate(currentSecond),
                            _ownerActionComponent._animationHipCurveY.Evaluate(currentSecond),
                            _ownerActionComponent._animationHipCurveZ.Evaluate(currentSecond)
                            );

                        float prevSecond = _ownerActionComponent._prevReadedSecond;
                        //float prevSecond = _ownerActionComponent._prevStateSecond;

                        if (prevSecond > currentSecond)//애니메이션이 바뀌였나? 과거가 더 크다
                        {
                            prevSecond = 0.0f;
                        }

                        Vector3 prevUnityLocalHip = new Vector3
                            (
                            _ownerActionComponent._animationHipCurveX.Evaluate(prevSecond),
                            _ownerActionComponent._animationHipCurveY.Evaluate(prevSecond),
                            _ownerActionComponent._animationHipCurveZ.Evaluate(prevSecond)
                            );

                        Vector3 deltaLocalHip = (currentUnityLocalHip - prevUnityLocalHip);
                        Vector3 worldDelta = _ownerActionComponent._ownerCharacterController.transform.localToWorldMatrix * deltaLocalHip;
                        _ownerActionComponent._ownerCharacterController.Move(worldDelta);

                        //_ownerActionComponent._prevStateSecond = currentSecond;
                        _ownerActionComponent._prevReadedSecond = currentSecond;
                    }
                    break;

                case StateActionType.RotateWithoutInterpolate:
                    {

                    }
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

                case StateActionType.RootMove:
                    {
                        if (_ownerActionComponent._ownerAnimator == null)
                        {
                            _ownerActionComponent._ownerAnimator = owner.GetComponentInChildren<Animator>();
                            Debug.Assert(_ownerActionComponent._ownerAnimator != null, "RootMove행동이 있는데 이 컴포넌트가 없습니다");
                        }

                        if (_ownerActionComponent._ownerCharacterController == null)
                        {
                            _ownerActionComponent._ownerCharacterController = owner.GetComponentInChildren<CharacterController>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterController != null, "RootMove행동이 있는데 이 컴포넌트가 없습니다");
                        }

                        //애니메이션 커브 찾아놓기
                        {
                            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(_stateDesc._stateAnimationClip);

                            bool curveXFind = false, curveYFind = false, curveZFind = false;

                            foreach (var binding in bindings)
                            {
                                if (curveXFind == true && curveYFind == true && curveZFind == true)
                                { break; } //다 찾았습니다.

                                //if (binding.path == _unityName_HipBone && binding.propertyName == _unityName_HipBoneLocalPositionX)
                                //{_ownerActionComponent._animationHipCurveX = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveXFind = true; }

                                //if (binding.path == _unityName_HipBone && binding.propertyName == _unityName_HipBoneLocalPositionY)
                                //{_ownerActionComponent._animationHipCurveY = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveYFind = true; }

                                //if (binding.path == _unityName_HipBone && binding.propertyName == _unityName_HipBoneLocalPositionZ)
                                //{_ownerActionComponent._animationHipCurveZ = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveZFind = true; }

                                if (binding.propertyName == _unityName_HipBoneLocalPositionX)
                                { _ownerActionComponent._animationHipCurveX = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveXFind = true; }

                                if (binding.propertyName == _unityName_HipBoneLocalPositionY)
                                { _ownerActionComponent._animationHipCurveY = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveYFind = true; }

                                if (binding.propertyName == _unityName_HipBoneLocalPositionZ)
                                { _ownerActionComponent._animationHipCurveZ = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveZFind = true; }
                            }

                            Debug.Assert((curveXFind == true && curveYFind == true && curveZFind == true), "커브가 존재하지 않습니다");
                        }

                    }
                    break;

                case StateActionType.RotateWithoutInterpolate:
                    {
                        if (_ownerActionComponent._ownerCharacterController == null)
                        {
                            _ownerActionComponent._ownerCharacterController = owner.GetComponentInChildren<CharacterController>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterController != null, "RotateWithoutInterpolate행동이 있는데 이 컴포넌트가 없습니다");
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
