using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static State;
using static StateNodeDesc;

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
}


public enum RepresentStateType
{
    Idle = 1 << 0,
    Walk = 1 << 1,
    Run = 1 << 2,
    Sprint = 1 << 3,
    Attack = 1 << 4,
    InAir = 1 << 5,
    Hit = 1 << 6,
    End = 1 << 7,
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
    public AnimationClip _stateAnimationClip;
    public bool _rightWeaponOverride = true;
    public bool _leftWeaponOverride = true;
    public bool _isAttackState = false;
    public bool _isLocoMotionToAttackAction = false;
    /*------------------------------------------------------------------------------
    |NOTI| !_isAttackState = _isLocoMotionToAttackAction의 개념일거같지만 지금은 아니다
    ------------------------------------------------------------------------------*/

    public List<RepresentStateType> _stateType = new List<RepresentStateType>();

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
    [SerializeField] private float _stateChangeTime = 0.085f;
    private float _stateChangeTimeAcc = 0.0f;
    private bool _stateChangeCoroutineStarted = false;
    private State _reservedNextWeaponState = null;


    private float _attackStateAutoChangeTime = 0.0f;
    private float _attackStateAutoChangeTimeAcc = 0.0f;
    private bool _attackStateAutoChangeTimeCoroutineStarted = false;



    public class StateContollerComponentDesc
    {
        public PlayerScript _owner;
        public Animator _ownerAnimator = null;
        public InputController _ownerInputController;
        public CharacterMoveScript2 _ownerMoveScript;
        public CharacterController _ownerCharacterComponent;
    }

    private StateContollerComponentDesc _ownerStateControllingComponent = new StateContollerComponentDesc();

    [SerializeField] private List<StateAsset> _stateInitial = new List<StateAsset>(); //그냥 여기에 들어있는거만큼 어디선가 복사해오면 좋겠다
    private List<State> _states = new List<State>();

    private State _currState;
    public State GetCurrState() { return _currState; }

    private float _currStateTime = 0.0f;
    private KeyCode _rightHandAttackKey = KeyCode.Mouse0;
    private KeyCode _leftHandAttackKey = KeyCode.Mouse1;

    private List<State> _nextAttackStates = new List<State>();

    private void Awake()
    {
        PlayerScript playerScript = GetComponent<PlayerScript>();

        _ownerStateControllingComponent._owner = playerScript;

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
        Debug.Assert(nextState != _currState, "같은 상태로 변경하려는 진입접이 있습니까?");

        if (_stateChangeCoroutineStarted == true)
        {
            /*--------------------------------------------------------------
            |NOTI| 이 코루틴이 실행중이였다면 플레이어는 지연체크를 하려는거였고.
            피격같은 외부에서 강제로 변경한 상태여만 합니다.
            --------------------------------------------------------------*/

            StopCoroutine("AttackComboChangeCoroutine");
            _stateChangeCoroutineStarted = false;
            CustomKeyManager.Instance.SetAttackKeyRestrained(false);
            Debug.Log("||--State Intercepted!!--||" + nextState.GetStateDesc()._stataName);
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

        //Debug.Log(nextState.GetStateDesc()._stataName);

        _ownerStateControllingComponent._owner.StateChanged();

        if (_currState != null)
        {
            DoActions(_currState.GetStateDesc()._ExitStateActionTypes);
        }

        _currState = nextState;

        DoActions(_currState.GetStateDesc()._EnterStateActionTypes);

        if (_currState.GetStateDesc()._isAttackState == true) //다음으로 넘어가려는 상태가 공격상태입니다.
        {
            StartCoroutine("AttackStateAutoChangeCoroutine");
        }

        _reservedNextWeaponState = null;
        _currStateTime = 0.0f;
    }






    public void DoWork()
    {
        Debug.Assert(_currState != null, "스테이트 null입니다");

        State nextState = (_reservedNextWeaponState != null)
            ? _reservedNextWeaponState
            : CheckChangeState();

        if (nextState != null)
        {
            ChangeState(nextState);
        }


        //상태 변경이 완료됐고. 현재 상태들의 Action을 실행하려 합니다.
        {
            //공격을 할 수 있는 상태에서 공격키가 아무거나 눌렸습니다. 0.1초 뒤 공격 애니메이션으로 전환을 시도할겁니다.
            if ((Input.GetKeyDown(_rightHandAttackKey) == true || Input.GetKeyDown(_leftHandAttackKey) == true) &&
                _stateChangeCoroutineStarted == false &&
                true/*넘어갈 수 있는 공격상태가 하나라도 존재한다*/)
            {
                StartCoroutine("AttackComboChangeCoroutine");
            }

            DoActions(_currState.GetStateDesc()._inStateActionTypes);
        }

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
        //State nextState = null;
        //{
        //    //공격이 끝난 후, 상태결정 로직
        //}
        //Debug.Assert(nextState != null, "공격이 끝난 후 다음상태를 결정할 수 없습니까?");
        //ChangeState(nextState);
        
        /*-----------------------------------------------------
        |NOTI| 원래는 위에처럼 할려했는데.
        Idle로 바꾼뒤 Idle에서 한번 더 체크를 하는게 일단 간단함
        -----------------------------------------------------*/

        ChangeState(_states[0]);

        State nextState = (_reservedNextWeaponState != null)
            ? _reservedNextWeaponState
            : CheckChangeState();

        if (nextState != null && nextState != _currState)
        {
            ChangeState(nextState);
        }
    }











    public State CheckChangeState()
    {
        if (_stateChangeCoroutineStarted == true)
        {
            return null; //현재 공격을 시도해서 0.085초 뒤에 지연체크를 할겁니다. 아무것도 하지마세요...?
        }

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
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveX.Evaluate(currentSecond),
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveY.Evaluate(currentSecond),
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveZ.Evaluate(currentSecond)
                            );

                        float prevSecond = _currState.GetStateAnimActionInfo()._prevReadedSecond;

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

                case StateActionType.AttackCommandCheck:
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
                    bool rightHandGrabCondition = true;

                    switch (_ownerStateControllingComponent._owner.GetGrabFocusType())
                    {
                        case WeaponGrabFocus.Normal:
                            {
                                if (_ownerStateControllingComponent._owner.GetRightWeaponPrefab() == null)
                                {
                                    rightHandGrabCondition = false;
                                }
                            }
                            break;
                        case WeaponGrabFocus.RightHandFocused:
                            {
                                if (_ownerStateControllingComponent._owner.GetRightWeaponPrefab() == null)
                                {
                                    rightHandGrabCondition = false;
                                }
                            }
                            break;
                        case WeaponGrabFocus.LeftHandFocused:
                            {
                                if (_ownerStateControllingComponent._owner.GetLeftWeaponPrefab() == null)
                                {
                                    rightHandGrabCondition = false;
                                }
                            }
                            break;
                        case WeaponGrabFocus.DualGrab:
                            {
                                if (_ownerStateControllingComponent._owner.GetRightWeaponPrefab() == null ||
                                    _ownerStateControllingComponent._owner.GetLeftWeaponPrefab() == null)
                                {
                                    rightHandGrabCondition = false;
                                }
                            }
                            break;

                        default:
                            break;
                    }

                    if (rightHandGrabCondition == false)
                    {
                        return false;
                    }

                    if (CustomKeyManager.Instance.GetKeyInputDesc(_rightHandAttackKey)._pressType == KeyPressType.Pressed)
                    {
                        return true;
                    }
                }
                break;

            case ConditionType.LeftAttackTry:
                {
                    if (CustomKeyManager.Instance.GetKeyInputDesc(_leftHandAttackKey)._pressType == KeyPressType.Pressed)
                    {
                        //공격 키 둘중에 하나라도 눌렸다.
                        return true;
                    }
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

                                ComboCommandKeyType targetType = KeyConvert2(weaponComboType, ownerGrabType, isRightHandWeapon);

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
            
            default:
                Debug.Assert(false, "데이터가 추가됐습니까?");
                break;
        }

        return false;
    }





    private ComboCommandKeyType KeyConvert2(WeaponUseType target, WeaponGrabFocus ownerGrabType, bool isRightHandWeapon)
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



    private bool KeyConvert(ref WeaponUseType ret, WeaponGrabFocus ownerGrabType, ComboCommandKeyType recordedType, bool isRightHandWeapon)
    {
        switch (recordedType)
        {
            case ComboCommandKeyType.LeftClick: //유저가 일반 왼클릭을 했따
                {
                    switch (ownerGrabType)
                    {
                        case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
                            {
                                if (isRightHandWeapon == false)
                                {
                                    ret = WeaponUseType.MainUse;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            break;

                        case WeaponGrabFocus.RightHandFocused: //일반 왼클릭 했는데 오른손을 주로 잡고있었다
                            {
                                ret = WeaponUseType.SubUse;
                            }
                            break;

                        case WeaponGrabFocus.LeftHandFocused: //일반 왼클릭 했는데 왼손을 주로 잡고있었다
                            {
                                ret = WeaponUseType.MainUse;
                            }
                            break;

                        case WeaponGrabFocus.DualGrab:
                            {
                                ret = WeaponUseType.SubUse;
                            }
                            break;

                        default:
                            break;
                    }
                }
                break;

            case ComboCommandKeyType.RightClick: //유저가 일반 우클릭을 했다
                {
                    switch (ownerGrabType)
                    {
                        case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
                            {
                                if (isRightHandWeapon == false)
                                {
                                    return false;
                                }
                                else
                                {
                                    ret = WeaponUseType.MainUse;
                                }
                            }
                            break;

                        case WeaponGrabFocus.RightHandFocused: //일반 우클릭 했는데 오른손을 주로 잡고있었다
                            {
                                ret = WeaponUseType.MainUse;
                            }
                            break;

                        case WeaponGrabFocus.LeftHandFocused: //일반 왼클릭 했는데 왼손을 주로 잡고있었다
                            {
                                ret = WeaponUseType.SubUse;
                            }
                            break;

                        case WeaponGrabFocus.DualGrab:
                            {
                                ret = WeaponUseType.MainUse;
                            }
                            break;

                        default:
                            break;
                    }
                }
                break;

            case ComboCommandKeyType.CtrlLeftClick: //유저가 스페셜 왼클릭을 했다
                {
                    switch (ownerGrabType)
                    {
                        case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
                            {
                                if (isRightHandWeapon == false)
                                {
                                    ret = WeaponUseType.SpecialUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            break;

                        case WeaponGrabFocus.RightHandFocused: //스페셜 왼클릭 했는데 오른손을 주로 잡고있었다
                            {
                                return false;
                            }

                        case WeaponGrabFocus.LeftHandFocused: //스페셜 왼클릭 했는데 왼손을 주로 잡고있었다
                            {
                                ret = WeaponUseType.SpecialUse;
                            }
                            break;

                        case WeaponGrabFocus.DualGrab:
                            {
                                return false;
                            }

                        default:
                            break;
                    }
                }
                break;

            case ComboCommandKeyType.CtrlRightClick: //유저가 스페셜 우클릭을 했다.
                {
                    switch (ownerGrabType)
                    {
                        case WeaponGrabFocus.Normal:
                            {
                                if (isRightHandWeapon == false)
                                {
                                    return false;
                                }
                                else
                                {
                                    ret = WeaponUseType.SpecialUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
                                }
                            }
                            break;

                        case WeaponGrabFocus.RightHandFocused:
                            {
                                ret = WeaponUseType.SpecialUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
                            }
                            break;

                        case WeaponGrabFocus.LeftHandFocused:
                            {
                                return false;
                            }

                        case WeaponGrabFocus.DualGrab:
                            {
                                ret = WeaponUseType.SpecialUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
                            }
                            break;

                        default:
                            break;
                    }
                }
                break;

            case ComboCommandKeyType.SubLeftClick: //유저가 보조 왼클릭을 했다.
                {
                    switch (ownerGrabType)
                    {
                        case WeaponGrabFocus.Normal:
                            {
                                if (isRightHandWeapon == false)
                                {
                                    ret = WeaponUseType.SubUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            break;

                        case WeaponGrabFocus.RightHandFocused:
                            {
                                return false;
                            }

                        case WeaponGrabFocus.LeftHandFocused:
                            {
                                return false;
                            }

                        case WeaponGrabFocus.DualGrab:
                            {
                                return false;
                            }

                        default:
                            break;
                    }
                }
                break;

            case ComboCommandKeyType.SubRightClick: //유저가 보조 우클릭을 했다.
                {
                    switch (ownerGrabType)
                    {
                        case WeaponGrabFocus.Normal:
                            {
                                if (isRightHandWeapon == false)
                                {
                                    return false;
                                }
                                else
                                {
                                    ret = WeaponUseType.SubUse; //----왼손 무기였다면 왼손을 보조로 사용하려는 거였다
                                }
                            }
                            break;

                        case WeaponGrabFocus.RightHandFocused:
                            {
                                return false;
                            }

                        case WeaponGrabFocus.LeftHandFocused:
                            {
                                return false;
                            }

                        case WeaponGrabFocus.DualGrab:
                            {
                                return false;
                            }

                        default:
                            break;
                    }
                }
                break;

            default:
                return false;
        }

        return true;
    }




    //isCheckingWeaponEntry == T -> Weapon Motion 이 아닌데 Weapon Motion을 쓰려고하는경우
    //isCheckingWeaponEntry == F -> Weapon Motion 에서  Weapon Motion을 쓰려고하는경우
    private void CalculateNextWeaponState(bool isCheckingWeaponEntry) 
    {
        WeaponScript weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(true);

        if (weaponScript != null)
        {
            if (isCheckingWeaponEntry == true) 
            {
                SortedDictionary<int, List<EntryState>> target = weaponScript.GetEntryStates();
                foreach (KeyValuePair<int, List<EntryState>> stateList in target)
                {
                    foreach (EntryState entryState in stateList.Value)
                    {//이미 어려운거부터 정렬돼있다고 가정한다. 그렇지 않다면 정렬로직의 문제다. 여기서 신경쓰지 않는다

                        bool stateCheckPassed = true;

                        //왜 장착하고 e 누르자마자 streak야 ㅅㅂ

                        foreach (ConditionDesc condition in entryState._entryCondition)
                        {
                            if (CheckCondition(condition, true) == false)
                            {
                                stateCheckPassed = false;
                                break;
                            }
                        }

                        if (stateCheckPassed == true)
                        {
                            _reservedNextWeaponState = entryState._state;
                            return;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            else
            {
                StateNodeDesc linkedStaateNodeDesc = weaponScript.FindLinkedStateNodeDesc(_currState);
                SortedDictionary<int, List<LinkedState>> target2 = linkedStaateNodeDesc.GetLinkecStates();
                foreach (KeyValuePair<int, List<LinkedState>> stateList in target2)
                {
                    foreach (LinkedState entryState in stateList.Value)
                    {//이미 어려운거부터 정렬돼있다고 가정한다. 그렇지 않다면 정렬로직의 문제다. 여기서 신경쓰지 않는다

                        bool stateCheckPassed = true;

                        foreach (ConditionDesc condition in entryState._multiConditions)
                        {
                            if (CheckCondition(condition, true) == false)
                            {
                                stateCheckPassed = false;
                                break;
                            }
                        }

                        if (stateCheckPassed == true)
                        {
                            _reservedNextWeaponState = entryState._state;
                            return;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }

        weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(false);

        if (weaponScript != null)
        {
            if (isCheckingWeaponEntry == true) 
            {
                SortedDictionary<int, List<EntryState>> target = weaponScript.GetEntryStates();
                foreach (KeyValuePair<int, List<EntryState>> stateList in target)
                {
                    foreach (EntryState entryState in stateList.Value)
                    {//이미 어려운거부터 정렬돼있다고 가정한다. 그렇지 않다면 정렬로직의 문제다. 여기서 신경쓰지 않는다

                        bool stateCheckPassed = true;

                        foreach (ConditionDesc condition in entryState._entryCondition)
                        {
                            if (CheckCondition(condition, false) == false)
                            {
                                stateCheckPassed = false;
                                break;
                            }
                        }

                        if (stateCheckPassed == true)
                        {
                            _reservedNextWeaponState = entryState._state;
                            return;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                _reservedNextWeaponState = null;
            }
            else
            {
                StateNodeDesc linkedStaateNodeDesc = weaponScript.FindLinkedStateNodeDesc(_currState);
                SortedDictionary<int, List<LinkedState>> target2 = linkedStaateNodeDesc.GetLinkecStates();
                foreach (KeyValuePair<int, List<LinkedState>> stateList in target2)
                {
                    foreach (LinkedState entryState in stateList.Value)
                    {//이미 어려운거부터 정렬돼있다고 가정한다. 그렇지 않다면 정렬로직의 문제다. 여기서 신경쓰지 않는다

                        bool stateCheckPassed = true;

                        foreach (ConditionDesc condition in entryState._multiConditions)
                        {
                            if (CheckCondition(condition, true) == false)
                            {
                                stateCheckPassed = false;
                                break;
                            }
                        }

                        if (stateCheckPassed == true)
                        {
                            _reservedNextWeaponState = entryState._state;
                            return;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }
    }
















    private State CalculateWeaponStateFromEntry(Dictionary<int, List<EntryState>> weponEntryStates, bool isRightWeapon)
    {
        //조건을 만족하면 _nextAttackStates에 집어넣습니다.

        foreach (KeyValuePair<int, List<EntryState>> stateList in weponEntryStates)
        {
            foreach (EntryState entryState in stateList.Value)
            {
                //CheckChangeState()
                foreach (ConditionDesc condition in entryState._entryCondition)
                {
                    CheckCondition(condition, isRightWeapon);
                }
            }
        }

        return null;
    }
}
