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
    public AnimationClip _stateAnimationClip;
    public bool _rightWeaponOverride = true;
    public bool _leftWeaponOverride = true;
    public bool _isAttackState = false;
    public bool _isLocoMotionToAttackAction = false;
    /*------------------------------------------------------------------------------
    |NOTI| !_isAttackState = _isLocoMotionToAttackAction�� �����ϰŰ����� ������ �ƴϴ�
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

    [SerializeField] private List<StateAsset> _stateInitial = new List<StateAsset>(); //�׳� ���⿡ ����ִ°Ÿ�ŭ ��𼱰� �����ؿ��� ���ڴ�
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
        Debug.Assert(nextState != _currState, "���� ���·� �����Ϸ��� �������� �ֽ��ϱ�?");

        if (_stateChangeCoroutineStarted == true)
        {
            /*--------------------------------------------------------------
            |NOTI| �� �ڷ�ƾ�� �������̿��ٸ� �÷��̾�� ����üũ�� �Ϸ��°ſ���.
            �ǰݰ��� �ܺο��� ������ ������ ���¿��� �մϴ�.
            --------------------------------------------------------------*/

            StopCoroutine("AttackComboChangeCoroutine");
            _stateChangeCoroutineStarted = false;
            CustomKeyManager.Instance.SetAttackKeyRestrained(false);
            Debug.Log("||--State Intercepted!!--||" + nextState.GetStateDesc()._stataName);
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

        //Debug.Log(nextState.GetStateDesc()._stataName);

        _ownerStateControllingComponent._owner.StateChanged();

        if (_currState != null)
        {
            DoActions(_currState.GetStateDesc()._ExitStateActionTypes);
        }

        _currState = nextState;

        DoActions(_currState.GetStateDesc()._EnterStateActionTypes);

        if (_currState.GetStateDesc()._isAttackState == true) //�������� �Ѿ���� ���°� ���ݻ����Դϴ�.
        {
            StartCoroutine("AttackStateAutoChangeCoroutine");
        }

        _reservedNextWeaponState = null;
        _currStateTime = 0.0f;
    }






    public void DoWork()
    {
        Debug.Assert(_currState != null, "������Ʈ null�Դϴ�");

        State nextState = (_reservedNextWeaponState != null)
            ? _reservedNextWeaponState
            : CheckChangeState();

        if (nextState != null)
        {
            ChangeState(nextState);
        }


        //���� ������ �Ϸ�ư�. ���� ���µ��� Action�� �����Ϸ� �մϴ�.
        {
            //������ �� �� �ִ� ���¿��� ����Ű�� �ƹ��ų� ���Ƚ��ϴ�. 0.1�� �� ���� �ִϸ��̼����� ��ȯ�� �õ��Ұ̴ϴ�.
            if ((Input.GetKeyDown(_rightHandAttackKey) == true || Input.GetKeyDown(_leftHandAttackKey) == true) &&
                _stateChangeCoroutineStarted == false &&
                true/*�Ѿ �� �ִ� ���ݻ��°� �ϳ��� �����Ѵ�*/)
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
        //    //������ ���� ��, ���°��� ����
        //}
        //Debug.Assert(nextState != null, "������ ���� �� �������¸� ������ �� �����ϱ�?");
        //ChangeState(nextState);
        
        /*-----------------------------------------------------
        |NOTI| ������ ����ó�� �ҷ��ߴµ�.
        Idle�� �ٲ۵� Idle���� �ѹ� �� üũ�� �ϴ°� �ϴ� ������
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
            return null; //���� ������ �õ��ؼ� 0.085�� �ڿ� ����üũ�� �Ұ̴ϴ�. �ƹ��͵� ����������...?
        }

        foreach (KeyValuePair<State, List<ConditionDesc>> pair in _currState.GetLinkedState())
        {
            bool isSuccess = true;

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
                return pair.Key; //���δ� �����ߴ�.
            }
        }

        return null; //�����Ѱ� �ϳ��� ����.
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
                        //���� Ű ���߿� �ϳ��� ���ȴ�.
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

                                ComboCommandKeyType targetType = KeyConvert2(weaponComboType, ownerGrabType, isRightHandWeapon);

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
            
            default:
                Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                break;
        }

        return false;
    }





    private ComboCommandKeyType KeyConvert2(WeaponUseType target, WeaponGrabFocus ownerGrabType, bool isRightHandWeapon)
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



    private bool KeyConvert(ref WeaponUseType ret, WeaponGrabFocus ownerGrabType, ComboCommandKeyType recordedType, bool isRightHandWeapon)
    {
        switch (recordedType)
        {
            case ComboCommandKeyType.LeftClick: //������ �Ϲ� ��Ŭ���� �ߵ�
                {
                    switch (ownerGrabType)
                    {
                        case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
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

                        case WeaponGrabFocus.RightHandFocused: //�Ϲ� ��Ŭ�� �ߴµ� �������� �ַ� ����־���
                            {
                                ret = WeaponUseType.SubUse;
                            }
                            break;

                        case WeaponGrabFocus.LeftHandFocused: //�Ϲ� ��Ŭ�� �ߴµ� �޼��� �ַ� ����־���
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

            case ComboCommandKeyType.RightClick: //������ �Ϲ� ��Ŭ���� �ߴ�
                {
                    switch (ownerGrabType)
                    {
                        case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
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

                        case WeaponGrabFocus.RightHandFocused: //�Ϲ� ��Ŭ�� �ߴµ� �������� �ַ� ����־���
                            {
                                ret = WeaponUseType.MainUse;
                            }
                            break;

                        case WeaponGrabFocus.LeftHandFocused: //�Ϲ� ��Ŭ�� �ߴµ� �޼��� �ַ� ����־���
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

            case ComboCommandKeyType.CtrlLeftClick: //������ ����� ��Ŭ���� �ߴ�
                {
                    switch (ownerGrabType)
                    {
                        case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
                            {
                                if (isRightHandWeapon == false)
                                {
                                    ret = WeaponUseType.SpecialUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            break;

                        case WeaponGrabFocus.RightHandFocused: //����� ��Ŭ�� �ߴµ� �������� �ַ� ����־���
                            {
                                return false;
                            }

                        case WeaponGrabFocus.LeftHandFocused: //����� ��Ŭ�� �ߴµ� �޼��� �ַ� ����־���
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

            case ComboCommandKeyType.CtrlRightClick: //������ ����� ��Ŭ���� �ߴ�.
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
                                    ret = WeaponUseType.SpecialUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
                                }
                            }
                            break;

                        case WeaponGrabFocus.RightHandFocused:
                            {
                                ret = WeaponUseType.SpecialUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
                            }
                            break;

                        case WeaponGrabFocus.LeftHandFocused:
                            {
                                return false;
                            }

                        case WeaponGrabFocus.DualGrab:
                            {
                                ret = WeaponUseType.SpecialUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
                            }
                            break;

                        default:
                            break;
                    }
                }
                break;

            case ComboCommandKeyType.SubLeftClick: //������ ���� ��Ŭ���� �ߴ�.
                {
                    switch (ownerGrabType)
                    {
                        case WeaponGrabFocus.Normal:
                            {
                                if (isRightHandWeapon == false)
                                {
                                    ret = WeaponUseType.SubUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
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

            case ComboCommandKeyType.SubRightClick: //������ ���� ��Ŭ���� �ߴ�.
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
                                    ret = WeaponUseType.SubUse; //----�޼� ���⿴�ٸ� �޼��� ������ ����Ϸ��� �ſ���
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




    //isCheckingWeaponEntry == T -> Weapon Motion �� �ƴѵ� Weapon Motion�� �������ϴ°��
    //isCheckingWeaponEntry == F -> Weapon Motion ����  Weapon Motion�� �������ϴ°��
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
                    {//�̹� �����ź��� ���ĵ��ִٰ� �����Ѵ�. �׷��� �ʴٸ� ���ķ����� ������. ���⼭ �Ű澲�� �ʴ´�

                        bool stateCheckPassed = true;

                        //�� �����ϰ� e �����ڸ��� streak�� ����

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
                    {//�̹� �����ź��� ���ĵ��ִٰ� �����Ѵ�. �׷��� �ʴٸ� ���ķ����� ������. ���⼭ �Ű澲�� �ʴ´�

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
                    {//�̹� �����ź��� ���ĵ��ִٰ� �����Ѵ�. �׷��� �ʴٸ� ���ķ����� ������. ���⼭ �Ű澲�� �ʴ´�

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
                    {//�̹� �����ź��� ���ĵ��ִٰ� �����Ѵ�. �׷��� �ʴٸ� ���ķ����� ������. ���⼭ �Ű澲�� �ʴ´�

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
        //������ �����ϸ� _nextAttackStates�� ����ֽ��ϴ�.

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
