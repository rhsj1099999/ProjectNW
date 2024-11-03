using System;
using System.Collections.Generic;
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
    RootMove,
    RotateWithoutInterpolate,
    RightHandWeaponSignal,
    LeftHandWeaponSignal,
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
    RightHandWeaponSignaled, //무기로 공격하려했으며, 넘어갈 수 있습니다.
    LeftHandWeaponSignaled, //무기로 공격하려했으며, 넘어갈 수 있습니다.
}

[Serializable]
public class KeyInputConditionDesc
{
    public KeyCode _targetKeyCode;
    public KeyPressType _targetState;
    public bool _keyInpuyGoal;
    public float _keyHoldGoal = 0.0f;
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
    public bool _rightWeaponOverride = true;
    public bool _leftWeaponOverride = true;

    public List<StateActionType> _EnterStateActionTypes;
    public List<StateActionType> _inStateActionTypes;
    public List<StateActionType> _ExitStateActionTypes;

    public List<StateLinkDesc> _linkedStates;
}

[Serializable]
public class StateInitial
{
    public string _stateName;
    public StateDesc _stateDesc;
}

public class StateContoller : MonoBehaviour
{
    public class StateContollerComponentDesc
    {
        public PlayerScript _owner;
        public CharacterController _ownerCharacterComponent;
        public Animator _ownerAnimator = null;
        public InputController _ownerInputController;
        public CharacterMoveScript2 _ownerMoveScript;
        public WeaponScript _ownerCurrWeapon;
    }

    private StateContollerComponentDesc _ownerStateControllingComponent = new StateContollerComponentDesc();

    [SerializeField] private List<StateAsset> _stateInitial = new List<StateAsset>(); //그냥 여기에 들어있는거만큼 어디선가 복사해오면 좋겠다
    private List<State> _states = new List<State>();

    private State _currState;
    public State GetCurrState() { return _currState; }

    private PlayerScript _owner = null;

    private float _currStateTime = 0.0f;

    private void Awake()
    {
        PlayerScript playerScript = GetComponent<PlayerScript>();

        _owner = playerScript;

        for (int i = 0; i < _stateInitial.Count; ++i)
        {
            State newState = new State(_stateInitial[i]);
            
            newState.SettingOwnerComponent(playerScript, _ownerStateControllingComponent);

            _states.Add(newState);
        }

        foreach (State state in _states)
        {
            state.LinkingStates(ref _states);
        }

        ChangeState(_states[0]);
    }

    private void ChangeState(State nextState)
    {
        if (nextState == null)
        {
            return;
        }
        
        if (nextState != _currState)  //상태가 달라졌다.
        {
            //Debug.Log(nextState.GetStateDesc()._stataName);

            _owner.StateChanged();

            if (_currState != null) 
            {
                DoActions(_currState.GetStateDesc()._ExitStateActionTypes);
            }

            _currState = nextState;

            DoActions(_currState.GetStateDesc()._EnterStateActionTypes);
        }
    }


    public void DoWork()
    {
        Debug.Assert(_currState != null, "스테이트 null입니다");

        State nextState = CheckChangeState();

        ChangeState(nextState);

        DoActions(_currState.GetStateDesc()._inStateActionTypes);
    }



    public State CheckChangeState()
    {
        foreach (KeyValuePair<State, List<ConditionDesc>> pair in _currState.GetLinkedState())
        {
            bool isSuccess = true;

            foreach (ConditionDesc conditionDesc in pair.Value)
            {
                if (CheckCondition(conditionDesc) == false)
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
        _currStateTime += Time.deltaTime;

        foreach (var action in actions)
        {
            switch (action)
            {
                case StateActionType.Move:
                    {
                        _ownerStateControllingComponent._ownerMoveScript.CharacterRotate(_ownerStateControllingComponent._ownerInputController._pr_directionByInput, 1.0f);
                        _ownerStateControllingComponent._ownerMoveScript.CharacterMove(_ownerStateControllingComponent._ownerInputController._pr_directionByInput, 1.0f);
                    }

                    break;

                case StateActionType.Attack:
                    break;

                case StateActionType.SaveLatestVelocity:
                    break;

                case StateActionType.Jump:
                    {
                        _ownerStateControllingComponent._ownerMoveScript.DoJump();
                    }

                    break;

                case StateActionType.ForcedMove:
                    {
                        Vector3 planeVelocity = _ownerStateControllingComponent._ownerMoveScript.GetLatestVelocity();
                        planeVelocity.y = 0.0f;
                        _ownerStateControllingComponent._ownerMoveScript.CharacterForcedMove(planeVelocity, 1.0f);
                    }

                    break;

                case StateActionType.ResetLatestVelocity:
                    break;

                case StateActionType.RootMove:
                    {
                        float currentSecond = _ownerStateControllingComponent._owner.GetCurrAnimationClipSecond();

                        Vector3 currentUnityLocalHip = new Vector3
                            (
                            _currState.GetStateAnimActionInfo()._animationHipCurveX.Evaluate(currentSecond),
                            _currState.GetStateAnimActionInfo()._animationHipCurveY.Evaluate(currentSecond),
                            _currState.GetStateAnimActionInfo()._animationHipCurveZ.Evaluate(currentSecond)
                            );

                        float prevSecond = _currState.GetStateAnimActionInfo()._prevReadedSecond;

                        if (prevSecond > currentSecond)//애니메이션이 바뀌였나? 과거가 더 크다
                        {prevSecond = 0.0f;}

                        Vector3 prevUnityLocalHip = new Vector3
                            (
                            _currState.GetStateAnimActionInfo()._animationHipCurveX.Evaluate(prevSecond),
                            _currState.GetStateAnimActionInfo()._animationHipCurveY.Evaluate(prevSecond),
                            _currState.GetStateAnimActionInfo()._animationHipCurveZ.Evaluate(prevSecond)
                            );

                        Vector3 deltaLocalHip = (currentUnityLocalHip - prevUnityLocalHip);
                        Vector3 worldDelta = _ownerStateControllingComponent._ownerCharacterComponent.transform.localToWorldMatrix * deltaLocalHip;

                        _ownerStateControllingComponent._ownerCharacterComponent.Move(worldDelta);

                        _currState.GetStateAnimActionInfo()._prevReadedSecond = currentSecond;
                    }
                    break;

                case StateActionType.RotateWithoutInterpolate:
                    break;

                case StateActionType.RightHandWeaponSignal:
                    break;

                case StateActionType.LeftHandWeaponSignal:
                    break;

                default:
                    Debug.Assert(false, "데이터가 추가됐습니까?");
                    break;
            }
        }
    }


    public bool CheckCondition(ConditionDesc conditionDesc)
    {
        bool ret = false;

        switch (conditionDesc._singleConditionType)
        {
            case ConditionType.MoveDesired:
                {
                    if(_ownerStateControllingComponent._ownerInputController == null)
                    {
                        _ownerStateControllingComponent._ownerInputController = _owner.GetComponent<InputController>();
                    }

                    Vector3 desiredMoved = _ownerStateControllingComponent._ownerInputController._pr_directionByInput;
                    if (desiredMoved != Vector3.zero)
                    {
                        ret = true;
                    }

                    return (ret == conditionDesc._componentConditionGoal);
                }

            case ConditionType.AnimationEnd:
                {
                    if (_ownerStateControllingComponent._owner.GetCurrAnimationLoopCount() >= 1)
                    {
                        return true;
                    }
                    return false;
                }

            case ConditionType.InAir:
                {
                    if (_ownerStateControllingComponent._ownerMoveScript == null)
                    {
                        _ownerStateControllingComponent._ownerMoveScript = _owner.GetComponent<CharacterMoveScript2>();
                    }

                    if (_ownerStateControllingComponent._ownerMoveScript.GetIsInAir() == true)
                    {
                        ret = true;
                    }

                    return (ret == conditionDesc._componentConditionGoal);
                }

            case ConditionType.KeyInput:
                {
                    bool isSuccess = true;

                    for (int i = 0; i < conditionDesc._keyInputConditionTarget.Count; ++i)
                    {
                        switch (conditionDesc._keyInputConditionTarget[i]._targetState)
                        {
                            case KeyPressType.Pressed:
                                if (Input.GetKeyDown(conditionDesc._keyInputConditionTarget[i]._targetKeyCode) != conditionDesc._keyInputConditionTarget[i]._keyInpuyGoal)
                                {
                                    isSuccess = false;
                                }
                                break;

                            case KeyPressType.Hold:
                                if (Input.GetKey(conditionDesc._keyInputConditionTarget[i]._targetKeyCode) != conditionDesc._keyInputConditionTarget[i]._keyInpuyGoal ||
                                    CustomKeyManager.Instance.GetKeyInputDesc(conditionDesc._keyInputConditionTarget[i]._targetKeyCode)._holdedSecond < conditionDesc._keyInputConditionTarget[i]._keyHoldGoal)
                                {
                                    isSuccess = false;
                                }
                                break;

                            case KeyPressType.Released:
                                if (Input.GetKeyUp(conditionDesc._keyInputConditionTarget[i]._targetKeyCode) != conditionDesc._keyInputConditionTarget[i]._keyInpuyGoal)
                                {
                                    isSuccess = false;
                                }
                                break;

                            default:
                                Debug.Log("KeyState목표값이 없습니다.");
                                break;
                        }

                        if (isSuccess == false) { return false; }
                    }

                    return isSuccess;
                }

            case ConditionType.EquipWeaponByType:
                {
                    ItemInfo ownerCurrWeapon = _ownerStateControllingComponent._owner.GetWeaponItem();

                    if (ownerCurrWeapon == null)
                    { return false; } //무기를 끼고있지 않습니다.

                    if (ownerCurrWeapon._weaponType == conditionDesc._weaponTypeGoal)
                    { return false; } //끼고있는 무기가 목표값과 다릅니다.

                    return true;
                }

            case ConditionType.AnimationFrameUp:
                {
                    //|TODO| 지금은 owner에게서 직접 재생시간을 받아옵니다. 추후 Animator가 이동하면 이곳에 작업이 필요합니다.
                    if (_ownerStateControllingComponent._owner.GetCurrAnimationClipFrame() < conditionDesc._animationFrameUpGoal)
                    { return false; }

                    return true;
                }

            case ConditionType.AnimationFrameUnder:
                {
                    //|TODO| 지금은 owner에게서 직접 재생시간을 받아옵니다. 추후 Animator가 이동하면 이곳에 작업이 필요합니다.
                    if (_ownerStateControllingComponent._owner.GetCurrAnimationClipFrame() > conditionDesc._animationFrameUnderGoal)
                    { return false; }

                    return true;
                }

            case ConditionType.RightHandWeaponSignaled:
                {

                }
                break;

            case ConditionType.LeftHandWeaponSignaled:
                {

                }
                break;

            default:
                Debug.Assert(false, "데이터가 추가됐습니까?");
                break;
        }

        return false;
    }
}
