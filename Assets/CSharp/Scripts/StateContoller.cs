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
    Attack, //���� ���⿡ ���� �޶��� �� �ִ�.
    SaveLatestVelocity,
    Jump,
    ForcedMove,
    ResetLatestVelocity,
    RootMove,
    RotateWithoutInterpolate,
    RightHandWeaponSignal,
    LeftHandWeaponSignal,
    AttackCommandCheck, //Action�� �̰��� ������ �ִٸ�, ���⿡�� ������ �Ѱ��־� ������ �õ��� �� �ִ�.
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
    AnimationFrame, //~~�� �̻����� ��� �ƽ��ϴ�. -> Animator�� �����ϴ� normalizedTime�� ���� �ʽ��ϴ�.
    RightHandWeaponSignaled, //����� �����Ϸ�������, �Ѿ �� �ֽ��ϴ�.
    LeftHandWeaponSignaled, //����� �����Ϸ�������, �Ѿ �� �ֽ��ϴ�.
    RightAttackTry, //�Ѽչ��⸦ ������� ��ų�, �ּ�ȭ�� �����ϸ� Right �������� Ű������ ����ϴ�.
    LeftAttackTry,
    AttackTry,
    isTargeting,
    ComboKeyCommand,
    FocusedWeapon, //������� ��ƾ߸� �Ѵ�.
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
    public List<WeaponUseType> _targetCommandKeys = new List<WeaponUseType>(); //���� ������ �ϴ� Ű

    //�޺� Ŀ�ǵ忡 Ȧ�� ���� �����������...������
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
    public float _comboStrainedTime = -1.0f; //n�� ���� �ϼ����Ѿ� �ϴ� �޺�
    public List<AnimatorLayerTypes> _mustNotBusyLayers;
    public int _mustNotBusyLayers_BitShift = 1;
}

[Serializable]
public class StateLinkDesc
{
    public List<ConditionDesc> _multiConditionAsset; //MultiCondition
    public StateAsset _stateAsset;
    private int _autoLinkWeight = 0; //�� ���ǵ��� �ڵ����� ����ϴ� ����ġ ����
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

    public bool _stateLocked = false; //�ܺο��� ���º����� ���͵� �ðڴ�.
    /*------------------------------------------------------------------------------
    |NOTI| !_isAttackState = _isLocoMotionToAttackAction�� �����ϰŰ����� ������ �ƴϴ�
    ------------------------------------------------------------------------------*/

    public bool _canUseItem = false;

    public List<RepresentStateType> _stateType = new List<RepresentStateType>();
    public List<StateActionType> _EnterStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _inStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _ExitStateActionTypes = new List<StateActionType>();
    public List<AdditionalBehaveType> _checkingBehaves = new List<AdditionalBehaveType>();

    public List<ConditionDesc> _breakLoopStateCondition = null;
    public List<StateLinkDesc> _linkedStates = new List<StateLinkDesc>();
    public AnimationClip _endStateIdleException = null; //������ �ִϸ��̼��� ������ ���� �ִϸ��̼�
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
            Debug.Assert(_states.ContainsKey(_stateInitial[i]._stateRepresentType) == false, "StateRepresent�� ��Ĩ�ϴ�");

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
            Debug.Log("�ش� ���¸� ������� �ʽ��ϴ�");
            return;
        }

        ChangeState(_states[representType]);
    }


    private void StatedWillBeChanged()
    {
        if (_stateChangeCoroutineStarted == true)
        {
            /*--------------------------------------------------------------
            |NOTI| �� �ڷ�ƾ�� �������̿��ٸ� �÷��̾�� ����üũ�� �Ϸ��°ſ���.
            �ǰݰ��� �ܺο��� ������ ������ ���¿��� �մϴ�.
            --------------------------------------------------------------*/
            StopCoroutine("AttackComboChangeCoroutine");
            _stateChangeCoroutineStarted = false;
            CustomKeyManager.Instance.SetAttackKeyRestrained(false);
        }

        if (_attackStateAutoChangeTimeCoroutineStarted == true)
        {
            /*--------------------------------------------------------------
            |NOTI| ������ ������, �ǰݵż� ������, �÷��̾� ������ ������ ���°�
            ����Ǹ� �� �ڷ�ƾ�� �������ϴ°� �½��ϴ�.
            --------------------------------------------------------------*/
            StopCoroutine("AttackStateAutoChangeCoroutine");
            _attackStateAutoChangeTimeCoroutineStarted = false;
        }

        _reservedNextWeaponState = null;
        _currStateTime = 0.0f;
    }

    private void ChangeState(State nextState)
    {
        //Debug.Assert(nextState != _currState, "���� ���·� �����Ϸ��� �������� �ֽ��ϱ�?"); ��

        StatedWillBeChanged();

        Debug.Log("State Changed : " + nextState.GetStateDesc()._stataName);

        if (_currState != null)
        {
            DoActions(_currState.GetStateDesc()._ExitStateActionTypes);
        }

        _currState = nextState;

        DoActions(_currState.GetStateDesc()._EnterStateActionTypes);

        if (_currState.GetStateDesc()._isAttackState == true && //�������� �Ѿ���� ���°� ���ݻ����Դϴ�.
            _currState.GetStateDesc()._isLoopState == false)  //�ݺ����µ� �ƴմϴ�.
        {
            StartCoroutine("AttackStateAutoChangeCoroutine");
        }

        _ownerStateControllingComponent._owner.StateChanged();
    }



    public void DoWork()
    {
        Debug.Assert(_currState != null, "������Ʈ null�Դϴ�");

        State nextState = (_reservedNextWeaponState != null)
            ? _reservedNextWeaponState
            : CheckChangeState_Recursion(_currState);

        if (nextState != null)
        {
            ChangeState(nextState);
        }

        //���� ������ �Ϸ�ư�. ���� ���µ��� Action�� �����Ϸ� �մϴ�.

        //������ �� �� �ִ� ���¿��� ����Ű�� �ƹ��ų� ���Ƚ��ϴ�. 0.1�� �� ���� �ִϸ��̼����� ��ȯ�� �õ��Ұ̴ϴ�.
        if ((Input.GetKeyDown(_rightHandAttackKey) == true || Input.GetKeyDown(_leftHandAttackKey) == true) &&
            _stateChangeCoroutineStarted == false &&
            true/*�Ѿ �� �ִ� ���ݻ��°� �ϳ��� �����Ѵ�*/)
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
        |NOTI| ���� ������ ������ �⺻������ Idle�� ���ٰ� ó���ϴ� ������
        �̰��� �ҿ����ϴ�. ���߿� ������ ���� �� �ִ�
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







    public State CheckChangeState_Recursion(State currentState) //���� ���¸� �����Ҷ����� ��������� ������ �Լ�
    {
        if (_stateChangeCoroutineStarted == true)
        {
            return null; //���� �޺� üũ�� �������̶� �ƹ��͵� ���Ҳ���
        }

        State targetState = currentState;

        int debugChangeCount = 0;
        bool isStateChangeGuaranted = false;

        while (true)
        {
            if (debugChangeCount > 100)
            {
                Debug.Assert(false, "���°� ��� �ٲ���մϴ�. ������ ������ �ֽ��ϱ�?");
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
                        break; //��Ƽ����ǿ��� �ϳ��� �೵��.
                    }
                }

                if (isSuccess == true)
                {
                    targetState = pair.Key;
                    
                    {
                        /*--------------------------------------------------------
                        |TODO| �������¶�� ���� �˻縦 �����ʵ��� �ӽù��� ó��. �̰� ����� ���� �����غ���
                        ������ ������ �ٲ��� y��ȭ�� �ִµ� ������ ������ �ߴٰ� �����ع�������
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

                    break; //�ٽ� �˻��Ϸ� ����
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

                        if (prevSecond > currentSecond)//�ִϸ��̼��� �ٲ��? ���Ű� �� ũ��
                        {prevSecond = 0.0f;}

                        Vector3 prevUnityLocalHip = new Vector3
                            (
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveX.Evaluate(prevSecond),
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveY.Evaluate(prevSecond),
                            _currState.GetStateAnimActionInfo()._myAnimationCurve._animationHipCurveZ.Evaluate(prevSecond)
                            );

                        Vector3 deltaLocalHip = (currentUnityLocalHip - prevUnityLocalHip);
                        Vector3 worldDelta = _ownerStateControllingComponent._ownerCharacterComponent.transform.localToWorldMatrix * deltaLocalHip;

                        //Root ����� y���� �𵨿� ����
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
                    Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                    break;
            }
        }
    }











    public bool CheckCondition(ConditionDesc conditionDesc, bool isRightHandWeapon = false/*TODO : �� ������ CommandCheck �ϳ������� �ֽ��ϴ�. �����մϴ�*/)
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
                                Debug.Log("KeyState��ǥ���� �����ϴ�.");
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
                    //{ return false; } //���⸦ �������� �ʽ��ϴ�.

                    //if (ownerCurrWeapon._weaponType == conditionDesc._weaponTypeGoal)
                    //{ return false; } //�����ִ� ���Ⱑ ��ǥ���� �ٸ��ϴ�.

                    return true;
                }

            case ConditionType.AnimationFrame:
                {
                    StateDesc currStateDesc = _currState.GetStateDesc();
                    
                    int currOnwerAnimationFrame = (int)(_currStateTime * currStateDesc._stateAnimationClip.frameRate);

                    StateAnimActionInfo currStateAnimInfo = _currState.GetStateAnimActionInfo();

                    if (currStateAnimInfo._myFrameData == null)
                    {
                        //�ѹ��� ã�ƺ���
                        currStateAnimInfo._myFrameData = ResourceDataManager.Instance.GetAnimationFrameData(currStateDesc._stateAnimationClip, conditionDesc._animationFrameDataType);

                        Debug.Assert(currStateAnimInfo._myFrameData != null, "Condition�� AnimationFrame�ε�, FrameData�� null�Դϴ�.");
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
                    //    //���� Ű ���߿� �ϳ��� ���ȴ�.
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
                        Debug.Assert(false, "CommandCondition�ε� Ű�� �ϳ��� �����ϴ�");
                    }

                    int KeyRecoredeCount = currCommand.Count - 1;
                    int CommandCount = stateComboKeyCommand.Count - 1;

                    WeaponGrabFocus ownerGrabType = _ownerStateControllingComponent._owner.GetGrabFocusType();
                    WeaponUseType weaponComboType = WeaponUseType.MainUse;
                    ComboCommandKeyType recordedType = ComboCommandKeyType.TargetingBack;

                    int index = 0;

                    if (CommandCount > KeyRecoredeCount)
                    {
                        return false; //�޺��� Ȯ���Ҹ�ŭ Ű�� ����.
                    }

                    for (LinkedListNode<ComboCommandKeyDesc> node = currCommand.Last; node != null; node = node.Previous)
                    {
                        if ((CommandCount - index) < 0)
                        {
                            break; //�޺��� �� �˻��ߴ�. �̻��� �����ٸ� return�� ������
                        }

                        recordedType = node.Value._type; //�Էµ� Ű

                        //���� Ű üũ
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
                                    return false; //ġȯ�� �����ߴ�
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
                Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                break;
        }

        return false;
    }





    private ComboCommandKeyType KeyConvert(WeaponUseType target, WeaponGrabFocus ownerGrabType, bool isRightHandWeapon)
    {
        ComboCommandKeyType convertedRet = ComboCommandKeyType.TargetingFront;

        switch (target)
        {
            case WeaponUseType.MainUse: //������ �޺��� �ֻ�� Ŭ���̴�
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
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

                    case WeaponGrabFocus.RightHandFocused: //���⸦ ������ ������� ����ִ�.
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

            case WeaponUseType.SubUse:  //������ �޺��� ���� ��� Ŭ���̴�
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
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

                    case WeaponGrabFocus.RightHandFocused: //���⸦ ������ ������� ����ִ�.
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

            case WeaponUseType.SpecialUse:  //������ �޺��� Ư�� ��� Ŭ���̴�
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
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

                    case WeaponGrabFocus.RightHandFocused: //���⸦ ������ ������� ����ִ�.
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



    




    //isCheckingWeaponEntry == T -> Weapon Motion �� �ƴѵ� Weapon Motion�� �������ϴ°��
    //isCheckingWeaponEntry == F -> Weapon Motion ����  Weapon Motion�� �������ϴ°��
    private void CalculateNextWeaponState(bool isCheckingWeaponEntry) 
    {
        if (isCheckingWeaponEntry == true)
        {
            WeaponScript weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(true);

            SortedDictionary<int, List<LinkedState>> targetDict = null;

            //������ ���� �˻�
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
            //���� -> ������ �Ϸ��� ����

            bool isLatestRightHandUse = _ownerStateControllingComponent._owner.GetLatestWeaponUse();
            WeaponScript weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(isLatestRightHandUse);
            SortedDictionary<int, List<LinkedState>> targetDict = null;

            //�ֱٰ����� �ߴ� ���� ���� ���� ���� �˻�
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


            //�ݴ���� Entry�� �˻�
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
                //�̹� �����ź��� ���ĵ��ִٰ� �����Ѵ�. �׷��� �ʴٸ� ���ķ����� ������. ���⼭ �Ű澲�� �ʴ´�
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
    //    //������ �����ϸ� _nextAttackStates�� ����ֽ��ϴ�.

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
    //        case ComboCommandKeyType.LeftClick: //������ �Ϲ� ��Ŭ���� �ߵ�
    //            {
    //                switch (ownerGrabType)
    //                {
    //                    case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
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

    //                    case WeaponGrabFocus.RightHandFocused: //�Ϲ� ��Ŭ�� �ߴµ� �������� �ַ� ����־���
    //                        {
    //                            ret = WeaponUseType.SubUse;
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.LeftHandFocused: //�Ϲ� ��Ŭ�� �ߴµ� �޼��� �ַ� ����־���
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

    //        case ComboCommandKeyType.RightClick: //������ �Ϲ� ��Ŭ���� �ߴ�
    //            {
    //                switch (ownerGrabType)
    //                {
    //                    case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
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

    //                    case WeaponGrabFocus.RightHandFocused: //�Ϲ� ��Ŭ�� �ߴµ� �������� �ַ� ����־���
    //                        {
    //                            ret = WeaponUseType.MainUse;
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.LeftHandFocused: //�Ϲ� ��Ŭ�� �ߴµ� �޼��� �ַ� ����־���
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

    //        case ComboCommandKeyType.CtrlLeftClick: //������ ����� ��Ŭ���� �ߴ�
    //            {
    //                switch (ownerGrabType)
    //                {
    //                    case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
    //                        {
    //                            if (isRightHandWeapon == false)
    //                            {
    //                                ret = WeaponUseType.SpecialUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
    //                            }
    //                            else
    //                            {
    //                                return false;
    //                            }
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.RightHandFocused: //����� ��Ŭ�� �ߴµ� �������� �ַ� ����־���
    //                        {
    //                            return false;
    //                        }

    //                    case WeaponGrabFocus.LeftHandFocused: //����� ��Ŭ�� �ߴµ� �޼��� �ַ� ����־���
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

    //        case ComboCommandKeyType.CtrlRightClick: //������ ����� ��Ŭ���� �ߴ�.
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
    //                                ret = WeaponUseType.SpecialUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
    //                            }
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.RightHandFocused:
    //                        {
    //                            ret = WeaponUseType.SpecialUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
    //                        }
    //                        break;

    //                    case WeaponGrabFocus.LeftHandFocused:
    //                        {
    //                            return false;
    //                        }

    //                    case WeaponGrabFocus.DualGrab:
    //                        {
    //                            ret = WeaponUseType.SpecialUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
    //                        }
    //                        break;

    //                    default:
    //                        break;
    //                }
    //            }
    //            break;

    //        case ComboCommandKeyType.SubLeftClick: //������ ���� ��Ŭ���� �ߴ�.
    //            {
    //                switch (ownerGrabType)
    //                {
    //                    case WeaponGrabFocus.Normal:
    //                        {
    //                            if (isRightHandWeapon == false)
    //                            {
    //                                ret = WeaponUseType.SubUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
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

    //        case ComboCommandKeyType.SubRightClick: //������ ���� ��Ŭ���� �ߴ�.
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
    //                                ret = WeaponUseType.SubUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
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
