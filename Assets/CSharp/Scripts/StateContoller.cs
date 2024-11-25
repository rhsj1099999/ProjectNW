using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static State;
using static StateNodeDesc;
using static UnityEngine.Rendering.DebugUI;

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
    AttackCommandCheck, //Action에 이것을 가지고 있다면, 무기에게 조작을 넘겨주어 공격을 시도할 수 있다.
    StateEndDesierdCheck,
    CheckBehaves,
    CalculateWeaponLayer_EnterAttack,
    CalculateWeaponLayer_ExitAttack,
}

public enum ConditionType
{
    MoveDesired,
    AnimationEnd,
    InAir,
    KeyInput,
    EquipWeaponByType,
    AnimationFrame, //~~초 이상으로 재생 됐습니다. -> Animator가 지원하는 normalizedTime을 쓰지 않습니다.
    RightHandWeaponSignaled, //무기로 공격하려했으며, 넘어갈 수 있습니다.
    LeftHandWeaponSignaled, //무기로 공격하려했으며, 넘어갈 수 있습니다.
    RightAttackTry, //한손무기를 양손으로 잡거나, 쌍수화를 진행하면 Right 기준으로 키조작을 잡습니다.
    LeftAttackTry,
    AttackTry,
    isTargeting,
    ComboKeyCommand,
    FocusedWeapon, //양손으로 잡아야만 한다.
    AnimatorLayerNotBusy,
}


public enum RepresentStateType
{
    Idle,
    Walk,
    Run,
    Sprint,
    Jump,
    Attack,
    InAir,
    Hit_L,
    Hit_H,
    Hit_Fall,
    Hit_FallDropDown,
    Hit_FallGround,
    RollFront,
    End,
}

[Serializable]
public class ComboKeyCommandDesc
{
    public List<WeaponUseType> _targetCommandKeys = new List<WeaponUseType>(); //같이 눌려야 하는 키

    //콤보 커맨드에 홀드 관련 집어넣지마라...아직은
    //public KeyPressType _targetState;
    //public bool _keyInpuyGoal;
    //public float _keyHoldGoal = 0.0f;
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
    public List<ComboKeyCommandDesc> _commandInputConditionTarget;
    public FrameDataType _animationFrameDataType = FrameDataType.End;
    public float _comboStrainedTime = -1.0f; //n초 내에 완성시켜야 하는 콤보
    public List<AnimatorLayerTypes> _mustNotBusyLayers;
    public int _mustNotBusyLayers_BitShift = 1;
}

[Serializable]
public class StateLinkDesc
{
    public List<ConditionDesc> _multiConditionAsset; //MultiCondition
    public StateAsset _stateAsset;
    private int _autoLinkWeight = 0; //각 조건들을 자동으로 계산하는 가중치 변수
}

[Serializable]
public class StateDesc
{
    public string _stataName;
    public AnimationClip _stateAnimationClip = null;
    public bool _rightWeaponOverride = true;
    public bool _leftWeaponOverride = true;
    public bool _isAttackState = false;
    public bool _isLocoMotionToAttackAction = false;
    public bool _isLoopState = false;

    public bool _stateLocked = false; //외부에서 상태변경이 들어와도 씹겠다.
    /*------------------------------------------------------------------------------
    |NOTI| !_isAttackState = _isLocoMotionToAttackAction의 개념일거같지만 지금은 아니다
    ------------------------------------------------------------------------------*/

    public bool _canUseItem = false;

    public List<RepresentStateType> _stateType = new List<RepresentStateType>();
    public List<StateActionType> _EnterStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _inStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _ExitStateActionTypes = new List<StateActionType>();
    public List<AdditionalBehaveType> _checkingBehaves = new List<AdditionalBehaveType>();

    public List<ConditionDesc> _breakLoopStateCondition = null;
    public List<StateLinkDesc> _linkedStates = new List<StateLinkDesc>();
    public AnimationClip _endStateIdleException = null; //상태의 애니메이션이 끝날때 예외 애니메이션
}



[Serializable]
public class StateInitial
{
    public string _stateName;
    public StateDesc _stateDesc;
}


[Serializable]
public class StateInitialPair
{
    public RepresentStateType _stateRepresentType = RepresentStateType.End;
    public StateAsset _stateAsset = null;
}

public class StateContoller : MonoBehaviour
{
    public class StateContollerComponentDesc
    {
        public PlayerScript _owner = null;
        public Animator _ownerAnimator = null;
        public GameObject _ownerModelObjectOrigin = null;
        public InputController _ownerInputController = null;
        public CharacterMoveScript2 _ownerMoveScript = null;
        public CharacterController _ownerCharacterComponent = null;
    }
    private StateContollerComponentDesc _ownerStateControllingComponent = new StateContollerComponentDesc();

    [SerializeField] private float _stateChangeTime = 0.085f;
    private float _stateChangeTimeAcc = 0.0f;
    private bool _stateChangeCoroutineStarted = false;
    private State _reservedNextWeaponState = null;


    private float _attackStateAutoChangeTime = 0.0f;
    private float _attackStateAutoChangeTimeAcc = 0.0f;
    private bool _attackStateAutoChangeTimeCoroutineStarted = false;


    [SerializeField] private List<StateInitialPair> _stateInitial = new List<StateInitialPair>();
    private Dictionary<RepresentStateType, State> _states = new Dictionary<RepresentStateType, State>();

    private State _currState;
    public State GetCurrState() { return _currState; }

    private float _currStateTime = 0.0f;
    private float _prevStateTime = 0.0f;

    private KeyCode _rightHandAttackKey = KeyCode.Mouse0;
    private KeyCode _leftHandAttackKey = KeyCode.Mouse1;


    private void Awake()
    {
        PlayerScript playerScript = GetComponent<PlayerScript>();

        _ownerStateControllingComponent._owner = playerScript;

        for (int i = 0; i < _stateInitial.Count; ++i)
        {
            Debug.Assert(_states.ContainsKey(_stateInitial[i]._stateRepresentType) == false, "StateRepresent가 겹칩니다");

            State newState = ResourceDataManager.Instance.GetState(_stateInitial[i]._stateAsset);
            
            newState.SettingOwnerComponent(playerScript, _ownerStateControllingComponent);

            _states.Add(_stateInitial[i]._stateRepresentType, newState);
        }

        foreach (KeyValuePair<RepresentStateType, State> statePair in _states)
        {
            statePair.Value.LinkingStates(ref _states);
        }

        ChangeState(_states[RepresentStateType.Idle]);
    }


    public void TryChangeState(RepresentStateType representType)
    {
        if (_states.ContainsKey(representType) == false)
        {
            Debug.Log("해당 상태를 사용하지 않습니다");
            return;
        }

        ChangeState(_states[representType]);
    }


    private void StatedWillBeChanged()
    {
        if (_stateChangeCoroutineStarted == true)
        {
            /*--------------------------------------------------------------
            |NOTI| 이 코루틴이 실행중이였다면 플레이어는 지연체크를 하려는거였고.
            피격같은 외부에서 강제로 변경한 상태여만 합니다.
            --------------------------------------------------------------*/
            StopCoroutine("AttackComboChangeCoroutine");
            _stateChangeCoroutineStarted = false;
            CustomKeyManager.Instance.SetAttackKeyRestrained(false);
        }

        if (_attackStateAutoChangeTimeCoroutineStarted == true)
        {
            /*--------------------------------------------------------------
            |NOTI| 공격이 끝나든, 피격돼서 끝나든, 플레이어 의지로 끝나든 상태가
            변경되면 이 코루틴은 끝나야하는게 맞습니다.
            --------------------------------------------------------------*/
            StopCoroutine("AttackStateAutoChangeCoroutine");
            _attackStateAutoChangeTimeCoroutineStarted = false;
        }

        _reservedNextWeaponState = null;
        _currStateTime = 0.0f;
    }

    private void ChangeState(State nextState)
    {
        //Debug.Assert(nextState != _currState, "같은 상태로 변경하려는 진입접이 있습니까?"); 네

        StatedWillBeChanged();

        Debug.Log("State Changed : " + nextState.GetStateDesc()._stataName);

        if (_currState != null)
        {
            DoActions(_currState.GetStateDesc()._ExitStateActionTypes);
        }

        _currState = nextState;

        DoActions(_currState.GetStateDesc()._EnterStateActionTypes);

        if (_currState.GetStateDesc()._isAttackState == true && //다음으로 넘어가려는 상태가 공격상태입니다.
            _currState.GetStateDesc()._isLoopState == false)  //반복상태도 아닙니다.
        {
            StartCoroutine("AttackStateAutoChangeCoroutine");
        }

        _ownerStateControllingComponent._owner.StateChanged();
    }



    public void DoWork()
    {
        Debug.Assert(_currState != null, "스테이트 null입니다");

        State nextState = (_reservedNextWeaponState != null)
            ? _reservedNextWeaponState
            : CheckChangeState_Recursion(_currState);

        if (nextState != null)
        {
            ChangeState(nextState);
        }

        //상태 변경이 완료됐고. 현재 상태들의 Action을 실행하려 합니다.

        //공격을 할 수 있는 상태에서 공격키가 아무거나 눌렸습니다. 0.1초 뒤 공격 애니메이션으로 전환을 시도할겁니다.
        if ((Input.GetKeyDown(_rightHandAttackKey) == true || Input.GetKeyDown(_leftHandAttackKey) == true) &&
            _stateChangeCoroutineStarted == false &&
            true/*넘어갈 수 있는 공격상태가 하나라도 존재한다*/)
        {
            StartCoroutine("AttackComboChangeCoroutine");
        }

        DoActions(_currState.GetStateDesc()._inStateActionTypes);

        _prevStateTime = _currStateTime;
        _currStateTime += Time.deltaTime;
    }


    private IEnumerator AttackComboChangeCoroutine()
    {
        _stateChangeTimeAcc = 0.0f;
        _stateChangeCoroutineStarted = true;
        Debug.Log("Attack Try Coroutine Started");
        CustomKeyManager.Instance.SetAttackKeyRestrained(true);

        while (true) 
        {
            _stateChangeTimeAcc += Time.deltaTime;
            if (_stateChangeTimeAcc >= _stateChangeTime)
            {
                Debug.Log("Attack Try Coroutine End Well");
                CalculateNextWeaponState(_currState.GetStateDesc()._isLocoMotionToAttackAction);
                break;
            }

            yield return null;
        }

        CustomKeyManager.Instance.SetAttackKeyRestrained(false);
        _stateChangeCoroutineStarted = false;
    }





    private IEnumerator AttackStateAutoChangeCoroutine()
    {
        _attackStateAutoChangeTimeAcc = 0.0f;
        _attackStateAutoChangeTime = _currState.GetStateDesc()._stateAnimationClip.length;
        _attackStateAutoChangeTimeCoroutineStarted = true;

        while (true)
        {
            _attackStateAutoChangeTimeAcc += Time.deltaTime;

            if (_attackStateAutoChangeTimeAcc >= _attackStateAutoChangeTime)
            {
                CalculateAfterAttackState();
                break;
            }

            yield return null;
        }

        _attackStateAutoChangeTimeCoroutineStarted = false;
    }







    private void CalculateAfterAttackState()
    {
        /*----------------------------------------------------
        |NOTI| 무기 공격을 끝내면 기본적으로 Idle로 간다고 처리하는 구조임
        이것은 불완전하다. 나중에 문제가 생길 수 있다
        -----------------------------------------------------*/

        State nextState = (_reservedNextWeaponState != null)
            ? _reservedNextWeaponState
            : CheckChangeState_Recursion(_states[0]);

        if (nextState == null) 
        {
            ChangeState(_states[0]);
        }

        else if (nextState != _currState)
        {
            ChangeState(nextState);
        }
    }







    public State CheckChangeState_Recursion(State currentState) //최종 상태를 결정할때까지 재귀적으로 실행할 함수
    {
        if (_stateChangeCoroutineStarted == true)
        {
            return null; //공격 콤보 체크가 진행중이라 아무것도 안할꺼다
        }

        State targetState = currentState;

        int debugChangeCount = 0;
        bool isStateChangeGuaranted = false;

        while (true)
        {
            if (debugChangeCount > 100)
            {
                Debug.Assert(false, "상태가 계속 바뀌려합니다. 로직에 문제가 있습니까?");
                Debug.Break();
                return null;
            }

            bool isSuccess = true;

            if (targetState.GetLinkedState().Count <= 0)
            {
                isSuccess = false;
            }

            foreach (KeyValuePair<State, List<ConditionDesc>> pair in targetState.GetLinkedState())
            {
                isSuccess = true;

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
                    targetState = pair.Key;
                    
                    {
                        /*--------------------------------------------------------
                        |TODO| 점프상태라면 연쇄 검사를 하지않도록 임시방편 처리. 이거 지우는 구조 생각해볼것
                        이유는 점프로 바뀌어야 y변화가 있는데 그전에 착지를 했다고 판정해버려서임
                        --------------------------------------------------------*/
                        if (targetState.GetStateDesc()._EnterStateActionTypes.Count > 0 &&
                            targetState.GetStateDesc()._EnterStateActionTypes[0] == StateActionType.Jump)
                        {
                            return targetState;
                        }
                    }

                    if (isStateChangeGuaranted == false)
                    {
                        StatedWillBeChanged();
                        isStateChangeGuaranted = true;
                    }

                    break; //다시 검사하러 간다
                }
            }

            if (targetState == currentState)
            {
                if (currentState.GetStateDesc()._isLoopState == true)
                {
                    foreach (ConditionDesc conditionDesc in currentState.GetStateDesc()._breakLoopStateCondition)
                    {
                        if (CheckCondition(conditionDesc) == false)
                        {
                            return null;
                        }
                    }

                    targetState = _states[0];
                    isSuccess = true;
                }

                if (isSuccess == false)
                {
                    return null;
                }
            }

            if (isSuccess == true)
            {
                debugChangeCount++;
                continue;
            }

            return targetState;
        }
    }






    public void DoActions(List<StateActionType> actions)
    {
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
                        float currentSecond = _currStateTime;

                        Vector3 currentUnityLocalHip = new Vector3
                            (
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveX.Evaluate(currentSecond),
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveY.Evaluate(currentSecond),
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveZ.Evaluate(currentSecond)
                            );

                        float prevSecond = _prevStateTime;

                        if (prevSecond > currentSecond)//애니메이션이 바뀌였나? 과거가 더 크다
                        {prevSecond = 0.0f;}

                        Vector3 prevUnityLocalHip = new Vector3
                            (
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveX.Evaluate(prevSecond),
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveY.Evaluate(prevSecond),
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveZ.Evaluate(prevSecond)
                            );

                        Vector3 deltaLocalHip = (currentUnityLocalHip - prevUnityLocalHip);
                        Vector3 worldDelta = _ownerStateControllingComponent._ownerCharacterComponent.transform.localToWorldMatrix * deltaLocalHip;

                        //Root 모션의 y값은 모델에 적용
                        {
                            Vector3 modelLocalPosition = _ownerStateControllingComponent._ownerModelObjectOrigin.transform.localPosition;
                            modelLocalPosition.y = worldDelta.y;
                            _ownerStateControllingComponent._ownerModelObjectOrigin.transform.localPosition = modelLocalPosition;
                        }

                        worldDelta.y = 0.0f;
                        _ownerStateControllingComponent._ownerCharacterComponent.Move(worldDelta);

                        _currState.GetStateAnimActionInfo()._prevReadedSecond = currentSecond;
                    }
                    break;

                case StateActionType.RotateWithoutInterpolate:
                    {
                        Vector3 convertedDirection = _ownerStateControllingComponent._ownerMoveScript.GetDirectionConvertedByCamera(_ownerStateControllingComponent._ownerInputController._pr_directionByInput);
                        gameObject.transform.LookAt(gameObject.transform.position + convertedDirection);
                    }
                    break;

                case StateActionType.RightHandWeaponSignal:
                    break;

                case StateActionType.LeftHandWeaponSignal:
                    break;

                case StateActionType.AttackCommandCheck:
                    break;

                case StateActionType.StateEndDesierdCheck:
                    break;

                case StateActionType.CheckBehaves:
                    {
                        foreach (var type in _currState.GetStateDesc()._checkingBehaves)
                        {
                            _ownerStateControllingComponent._owner.CheckBehave(type);
                        }
                    }
                    break;

                case StateActionType.CalculateWeaponLayer_EnterAttack:
                    {
                        _ownerStateControllingComponent._owner.WeaponLayerChange_EnterAttack(_currState);
                    }
                    break;

                case StateActionType.CalculateWeaponLayer_ExitAttack:
                    {
                        _ownerStateControllingComponent._owner.WeaponLayerChange_ExitAttack(_currState);
                    }
                    break;

                default:
                    Debug.Assert(false, "데이터가 추가됐습니까?");
                    break;
            }
        }
    }











    public bool CheckCondition(ConditionDesc conditionDesc, bool isRightHandWeapon = false/*TODO : 이 변수는 CommandCheck 하나때문에 있습니다. 빼야합니다*/)
    {
        bool ret = false;

        switch (conditionDesc._singleConditionType)
        {
            case ConditionType.MoveDesired:
                {
                    if(_ownerStateControllingComponent._ownerInputController == null)
                    {
                        _ownerStateControllingComponent._ownerInputController = _ownerStateControllingComponent._owner.GetComponent<InputController>();
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
                    //if (_ownerStateControllingComponent._owner.GetCurrAnimationLoopCount() >= 1)
                    //{
                    //    return true;
                    //}

                    float animationLength = _currState.GetStateDesc()._stateAnimationClip.length;
                    if (animationLength - Time.deltaTime < _currStateTime) 
                    {
                        return true;
                    }

                    return false;
                }

            case ConditionType.InAir:
                {
                    if (_ownerStateControllingComponent._ownerMoveScript == null)
                    {
                        _ownerStateControllingComponent._ownerMoveScript = _ownerStateControllingComponent._owner.GetComponent<CharacterMoveScript2>();
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
                    //ItemInfo ownerCurrWeapon = _ownerStateControllingComponent._owner.GetWeaponItem();

                    //if (ownerCurrWeapon == null)
                    //{ return false; } //무기를 끼고있지 않습니다.

                    //if (ownerCurrWeapon._weaponType == conditionDesc._weaponTypeGoal)
                    //{ return false; } //끼고있는 무기가 목표값과 다릅니다.

                    return true;
                }

            case ConditionType.AnimationFrame:
                {
                    StateDesc currStateDesc = _currState.GetStateDesc();
                    
                    int currOnwerAnimationFrame = (int)(_currStateTime * currStateDesc._stateAnimationClip.frameRate);

                    StateAnimActionInfo currStateAnimInfo = _currState.GetStateAnimActionInfo();

                    if (currStateAnimInfo._myFrameData == null)
                    {
                        //한번은 찾아본다
                        currStateAnimInfo._myFrameData = ResourceDataManager.Instance.GetAnimationFrameData(currStateDesc._stateAnimationClip, conditionDesc._animationFrameDataType);

                        Debug.Assert(currStateAnimInfo._myFrameData != null, "Condition이 AnimationFrame인데, FrameData가 null입니다.");
                    }

                    return currStateAnimInfo._myFrameData.FrameCheck(currOnwerAnimationFrame);
                }

            case ConditionType.RightHandWeaponSignaled:
                {

                }
                break;

            case ConditionType.LeftHandWeaponSignaled:
                {

                }
                break;

            case ConditionType.RightAttackTry:
                {
                    //bool rightHandGrabCondition = true;

                    //switch (_ownerStateControllingComponent._owner.GetGrabFocusType())
                    //{
                    //    case WeaponGrabFocus.Normal:
                    //        {
                    //            if (_ownerStateControllingComponent._owner.GetRightWeaponPrefab() == null)
                    //            {
                    //                rightHandGrabCondition = false;
                    //            }
                    //        }
                    //        break;
                    //    case WeaponGrabFocus.RightHandFocused:
                    //        {
                    //            if (_ownerStateControllingComponent._owner.GetRightWeaponPrefab() == null)
                    //            {
                    //                rightHandGrabCondition = false;
                    //            }
                    //        }
                    //        break;
                    //    case WeaponGrabFocus.LeftHandFocused:
                    //        {
                    //            if (_ownerStateControllingComponent._owner.GetLeftWeaponPrefab() == null)
                    //            {
                    //                rightHandGrabCondition = false;
                    //            }
                    //        }
                    //        break;
                    //    case WeaponGrabFocus.DualGrab:
                    //        {
                    //            if (_ownerStateControllingComponent._owner.GetRightWeaponPrefab() == null ||
                    //                _ownerStateControllingComponent._owner.GetLeftWeaponPrefab() == null)
                    //            {
                    //                rightHandGrabCondition = false;
                    //            }
                    //        }
                    //        break;

                    //    default:
                    //        break;
                    //}

                    //if (rightHandGrabCondition == false)
                    //{
                    //    return false;
                    //}

                    //if (CustomKeyManager.Instance.GetKeyInputDesc(_rightHandAttackKey)._pressType == KeyPressType.Pressed)
                    //{
                    //    return true;
                    //}
                }
                break;

            case ConditionType.LeftAttackTry:
                {
                    //if (CustomKeyManager.Instance.GetKeyInputDesc(_leftHandAttackKey)._pressType == KeyPressType.Pressed)
                    //{
                    //    //공격 키 둘중에 하나라도 눌렸다.
                    //    return true;
                    //}
                }
                break;

            case ConditionType.AttackTry:
                {

                }
                break;

            case ConditionType.isTargeting:
                break;

            case ConditionType.ComboKeyCommand:
                {
                    LinkedList<ComboCommandKeyDesc> currCommand = CustomKeyManager.Instance.GetComboCommandKeyDescs();
                    List<ComboKeyCommandDesc> stateComboKeyCommand = conditionDesc._commandInputConditionTarget;
                    if (stateComboKeyCommand.Count <= 0)
                    {
                        Debug.Assert(false, "CommandCondition인데 키가 하나도 없습니다");
                    }

                    int KeyRecoredeCount = currCommand.Count - 1;
                    int CommandCount = stateComboKeyCommand.Count - 1;

                    WeaponGrabFocus ownerGrabType = _ownerStateControllingComponent._owner.GetGrabFocusType();
                    WeaponUseType weaponComboType = WeaponUseType.MainUse;
                    ComboCommandKeyType recordedType = ComboCommandKeyType.TargetingBack;

                    int index = 0;

                    if (CommandCount > KeyRecoredeCount)
                    {
                        return false; //콤보를 확인할만큼 키가 없다.
                    }

                    for (LinkedListNode<ComboCommandKeyDesc> node = currCommand.Last; node != null; node = node.Previous)
                    {
                        if ((CommandCount - index) < 0)
                        {
                            break; //콤보를 다 검사했다. 이상이 없었다면 return을 안했음
                        }

                        recordedType = node.Value._type; //입력된 키

                        //조합 키 체크
                        for (int i = 0; i < stateComboKeyCommand[CommandCount - index]._targetCommandKeys.Count; i++)
                        {
                            weaponComboType = stateComboKeyCommand[CommandCount - index]._targetCommandKeys[i];

                            if (weaponComboType == WeaponUseType.MainUse || weaponComboType == WeaponUseType.SubUse || weaponComboType == WeaponUseType.SpecialUse)
                            {
                                if (recordedType == ComboCommandKeyType.TargetingBack || recordedType == ComboCommandKeyType.TargetingFront || recordedType == ComboCommandKeyType.TargetingLeft || recordedType == ComboCommandKeyType.TargetingRight)
                                { return false; }

                                ComboCommandKeyType targetType = KeyConvert(weaponComboType, ownerGrabType, isRightHandWeapon);

                                if (targetType <= ComboCommandKeyType.TargetingRight)
                                {
                                    return false; //치환에 실패했다
                                }

                                if (CustomKeyManager.Instance.AttackKeyRestrainedExist(targetType) == false)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if ((ComboCommandKeyType)weaponComboType != recordedType)
                                {
                                    return false;
                                }
                            }
                        }

                        index++;
                    }
                }
                return true;

            case ConditionType.FocusedWeapon:
                break;

            case ConditionType.AnimatorLayerNotBusy:
                {
                    if (conditionDesc._mustNotBusyLayers.Count <= 0)
                    {
                        return true;
                    }

                    if (conditionDesc._mustNotBusyLayers_BitShift == -1)
                    {
                        foreach (AnimatorLayerTypes item in conditionDesc._mustNotBusyLayers)
                        {
                            conditionDesc._mustNotBusyLayers_BitShift = (conditionDesc._mustNotBusyLayers_BitShift | (1 << (int)item));
                        }
                    }

                    int ownerBusyLayer_Bitshift = _ownerStateControllingComponent._owner.GetCurrentBusyAnimatorLayer_BitShift();

                    if ((ownerBusyLayer_Bitshift & conditionDesc._mustNotBusyLayers_BitShift) == 0)
                    {
                        return true;
                    }
                }
                return false;

            default:
                Debug.Assert(false, "데이터가 추가됐습니까?");
                break;
        }

        return false;
    }





    private ComboCommandKeyType KeyConvert(WeaponUseType target, WeaponGrabFocus ownerGrabType, bool isRightHandWeapon)
    {
        ComboCommandKeyType convertedRet = ComboCommandKeyType.TargetingFront;

        switch (target)
        {
            case WeaponUseType.MainUse: //무기의 콤보가 주사용 클릭이다
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.RightClick;
                            }
                            else
                            {
                                convertedRet = ComboCommandKeyType.LeftClick;
                            }
                        }
                        break;

                    case WeaponGrabFocus.RightHandFocused: //무기를 오른편에 양손으로 잡고있다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.RightClick;
                            }
                        }
                        break;

                    case WeaponGrabFocus.LeftHandFocused:
                        {
                            if (isRightHandWeapon == false)
                            {
                                convertedRet = ComboCommandKeyType.LeftClick;
                            }
                        }
                        break;
                }
                break;

            case WeaponUseType.SubUse:  //무기의 콤보가 보조 사용 클릭이다
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.SubRightClick;
                            }
                            else
                            {
                                convertedRet = ComboCommandKeyType.SubLeftClick;
                            }
                        }
                        break;

                    case WeaponGrabFocus.RightHandFocused: //무기를 오른편에 양손으로 잡고있다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.LeftClick;
                            }
                        }
                        break;

                    case WeaponGrabFocus.LeftHandFocused:
                        {
                            if (isRightHandWeapon == false)
                            {
                                convertedRet = ComboCommandKeyType.RightClick;
                            }
                        }
                        break;
                }
                break;

            case WeaponUseType.SpecialUse:  //무기의 콤보가 특수 사용 클릭이다
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.CtrlRightClick;
                            }
                            else
                            {
                                convertedRet = ComboCommandKeyType.CtrlLeftClick;
                            }
                        }
                        break;

                    case WeaponGrabFocus.RightHandFocused: //무기를 오른편에 양손으로 잡고있다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.CtrlRightClick;
                            }
                        }
                        break;

                    case WeaponGrabFocus.LeftHandFocused:
                        {
                            if (isRightHandWeapon == false)
                            {
                                convertedRet = ComboCommandKeyType.CtrlLeftClick;
                            }
                        }
                        break;
                }
                break;

            default:
                break;
        }

        return convertedRet;
    }



    




    //isCheckingWeaponEntry == T -> Weapon Motion 이 아닌데 Weapon Motion을 쓰려고하는경우
    //isCheckingWeaponEntry == F -> Weapon Motion 에서  Weapon Motion을 쓰려고하는경우
    private void CalculateNextWeaponState(bool isCheckingWeaponEntry) 
    {
        if (isCheckingWeaponEntry == true)
        {
            WeaponScript weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(true);

            SortedDictionary<int, List<LinkedState>> targetDict = null;

            //오른손 먼저 검사
            if (weaponScript != null)
            {
                targetDict = weaponScript.GetEntryStates();

                _reservedNextWeaponState = CheckNextWeaponState(targetDict, true);
            }

            if (_reservedNextWeaponState != null)
            {
                _ownerStateControllingComponent._owner.SetLatestWeaponUse(true);
                return;
            }

            weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(false);

            if (weaponScript == null)
            {
                return;
            }

            targetDict = weaponScript.GetEntryStates();

            _reservedNextWeaponState = CheckNextWeaponState(targetDict, false);
            _ownerStateControllingComponent._owner.SetLatestWeaponUse(false);
        }
        else 
        {
            //공격 -> 공격을 하려는 경우다

            bool isLatestRightHandUse = _ownerStateControllingComponent._owner.GetLatestWeaponUse();
            WeaponScript weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(isLatestRightHandUse);
            SortedDictionary<int, List<LinkedState>> targetDict = null;

            //최근공격을 했던 손의 연결 상태 먼저 검사
            {
                if (weaponScript != null)
                {
                    //targetDict = weaponScript.FindLinkedStateNodeDesc(_currState).GetLinkecStates();
                    targetDict = weaponScript.FindLinkedStateNodeDesc(_currState).GetLinkecStates();

                    _reservedNextWeaponState = CheckNextWeaponState(targetDict, isLatestRightHandUse);
                }

                if (_reservedNextWeaponState != null)
                {
                    _ownerStateControllingComponent._owner.SetLatestWeaponUse(isLatestRightHandUse);
                    return;
                }
            }


            //반대손의 Entry를 검사
            {
                weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(!isLatestRightHandUse);

                if (weaponScript != null)
                {
                    targetDict = weaponScript.GetEntryStates();

                    _reservedNextWeaponState = CheckNextWeaponState(targetDict, !isLatestRightHandUse);
                }

                if (_reservedNextWeaponState != null)
                {
                    _ownerStateControllingComponent._owner.SetLatestWeaponUse(!isLatestRightHandUse);
                    return;
                }
            }
        }
    }



    private State CheckNextWeaponState(SortedDictionary<int, List<LinkedState>> targetDict, bool isRightHandWeapon)
    {
        foreach (KeyValuePair<int, List<LinkedState>> stateList in targetDict)
        {
            foreach (LinkedState entryState in stateList.Value)
            {
                //이미 어려운거부터 정렬돼있다고 가정한다. 그렇지 않다면 정렬로직의 문제다. 여기서 신경쓰지 않는다
                bool stateCheckPassed = true;

                foreach (ConditionDesc condition in entryState._multiConditions)
                {
                    if (CheckCondition(condition, isRightHandWeapon) == false)
                    {
                        stateCheckPassed = false;
                        break;
                    }
                }

                if (stateCheckPassed == true)
                {
                    return entryState._state;
                }
            }
        }

        return null;
    }
















    //private State CalculateWeaponStateFromEntry(Dictionary<int, List<LinkedState>> weponEntryStates, bool isRightWeapon)
    //{
    //    //조건을 만족하면 _nextAttackStates에 집어넣습니다.

    //    foreach (KeyValuePair<int, List<LinkedState>> stateList in weponEntryStates)
    //    {
    //        foreach (LinkedState entryState in stateList.Value)
    //        {
    //            //CheckChangeState()
    //            foreach (ConditionDesc condition in entryState._multiConditions)
    //            {
    //                CheckCondition(condition, isRightWeapon);
    //            }
    //        }
    //    }

    //    return null;
    //}



    //private bool KeyConvert(ref WeaponUseType ret, WeaponGrabFocus ownerGrabType, ComboCommandKeyType recordedType, bool isRightHandWeapon)
    //{
    //    switch (recordedType)
    //    {
    //        case ComboCommandKeyType.LeftClick: //유저가 일반 왼클릭을 했따
    //            {
    //                switch (ownerGrabType)
    //                {
    //                    case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
    //                        {
    //                            if (isRightHandWeapon == false)
    //                            {
    //                                ret = WeaponUseType.MainUse;
    //                            }
    //                            else
    //                            {
    //                                return false;
    //                            }
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.RightHandFocused: //일반 왼클릭 했는데 오른손을 주로 잡고있었다
    //                        {
    //                            ret = WeaponUseType.SubUse;
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.LeftHandFocused: //일반 왼클릭 했는데 왼손을 주로 잡고있었다
    //                        {
    //                            ret = WeaponUseType.MainUse;
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.DualGrab:
    //                        {
    //                            ret = WeaponUseType.SubUse;
    //                        }
    //                        break;

    //                    default:
    //                        break;
    //                }
    //            }
    //            break;

    //        case ComboCommandKeyType.RightClick: //유저가 일반 우클릭을 했다
    //            {
    //                switch (ownerGrabType)
    //                {
    //                    case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
    //                        {
    //                            if (isRightHandWeapon == false)
    //                            {
    //                                return false;
    //                            }
    //                            else
    //                            {
    //                                ret = WeaponUseType.MainUse;
    //                            }
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.RightHandFocused: //일반 우클릭 했는데 오른손을 주로 잡고있었다
    //                        {
    //                            ret = WeaponUseType.MainUse;
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.LeftHandFocused: //일반 왼클릭 했는데 왼손을 주로 잡고있었다
    //                        {
    //                            ret = WeaponUseType.SubUse;
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.DualGrab:
    //                        {
    //                            ret = WeaponUseType.MainUse;
    //                        }
    //                        break;

    //                    default:
    //                        break;
    //                }
    //            }
    //            break;

    //        case ComboCommandKeyType.CtrlLeftClick: //유저가 스페셜 왼클릭을 했다
    //            {
    //                switch (ownerGrabType)
    //                {
    //                    case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
    //                        {
    //                            if (isRightHandWeapon == false)
    //                            {
    //                                ret = WeaponUseType.SpecialUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
    //                            }
    //                            else
    //                            {
    //                                return false;
    //                            }
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.RightHandFocused: //스페셜 왼클릭 했는데 오른손을 주로 잡고있었다
    //                        {
    //                            return false;
    //                        }

    //                    case WeaponGrabFocus.LeftHandFocused: //스페셜 왼클릭 했는데 왼손을 주로 잡고있었다
    //                        {
    //                            ret = WeaponUseType.SpecialUse;
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.DualGrab:
    //                        {
    //                            return false;
    //                        }

    //                    default:
    //                        break;
    //                }
    //            }
    //            break;

    //        case ComboCommandKeyType.CtrlRightClick: //유저가 스페셜 우클릭을 했다.
    //            {
    //                switch (ownerGrabType)
    //                {
    //                    case WeaponGrabFocus.Normal:
    //                        {
    //                            if (isRightHandWeapon == false)
    //                            {
    //                                return false;
    //                            }
    //                            else
    //                            {
    //                                ret = WeaponUseType.SpecialUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
    //                            }
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.RightHandFocused:
    //                        {
    //                            ret = WeaponUseType.SpecialUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.LeftHandFocused:
    //                        {
    //                            return false;
    //                        }

    //                    case WeaponGrabFocus.DualGrab:
    //                        {
    //                            ret = WeaponUseType.SpecialUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
    //                        }
    //                        break;

    //                    default:
    //                        break;
    //                }
    //            }
    //            break;

    //        case ComboCommandKeyType.SubLeftClick: //유저가 보조 왼클릭을 했다.
    //            {
    //                switch (ownerGrabType)
    //                {
    //                    case WeaponGrabFocus.Normal:
    //                        {
    //                            if (isRightHandWeapon == false)
    //                            {
    //                                ret = WeaponUseType.SubUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
    //                            }
    //                            else
    //                            {
    //                                return false;
    //                            }
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.RightHandFocused:
    //                        {
    //                            return false;
    //                        }

    //                    case WeaponGrabFocus.LeftHandFocused:
    //                        {
    //                            return false;
    //                        }

    //                    case WeaponGrabFocus.DualGrab:
    //                        {
    //                            return false;
    //                        }

    //                    default:
    //                        break;
    //                }
    //            }
    //            break;

    //        case ComboCommandKeyType.SubRightClick: //유저가 보조 우클릭을 했다.
    //            {
    //                switch (ownerGrabType)
    //                {
    //                    case WeaponGrabFocus.Normal:
    //                        {
    //                            if (isRightHandWeapon == false)
    //                            {
    //                                return false;
    //                            }
    //                            else
    //                            {
    //                                ret = WeaponUseType.SubUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
    //                            }
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.RightHandFocused:
    //                        {
    //                            return false;
    //                        }

    //                    case WeaponGrabFocus.LeftHandFocused:
    //                        {
    //                            return false;
    //                        }

    //                    case WeaponGrabFocus.DualGrab:
    //                        {
    //                            return false;
    //                        }

    //                    default:
    //                        break;
    //                }
    //            }
    //            break;

    //        default:
    //            return false;
    //    }

    //    return true;
    //}
}
