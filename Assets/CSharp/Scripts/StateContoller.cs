using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static State;
//using static StateNodeDesc;
using static UnityEngine.Rendering.DebugUI;
using static StateGraphAsset;
using static AnimationClipEditor;

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
    DummyState_EnterLocoStateGraph, //DummyState�� �Ⱦ��ٸ�, AnimationEndCoroutine_ReturnToIdle�� ������ �� �ִ�.
    AddCoroutine_ChangeToIdleState,
    AddCoroutine_StateChangeReady,
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

public enum StateActionCoroutineType
{
    ChangeToIdle,
    StateChangeReady,
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
    public InputKeyAsset _targetKey = null;
    public KeyPressType _targetState = KeyPressType.None;
    public float _keyHoldGoal = 0.0f;
}

[Serializable]
public class ConditionDesc
{
    public ConditionType _singleConditionType;
    public ItemInfo.WeaponType _weaponTypeGoal;
    public List<KeyInputConditionDesc> _keyInputConditionTarget;
    public List<ComboKeyCommandDesc> _commandInputConditionTarget;
    public FrameDataType _animationFrameDataType = FrameDataType.End;
    public float _comboStrainedTime = -1.0f; //n�� ���� �ϼ����Ѿ� �ϴ� �޺�
    public List<AnimatorLayerTypes> _mustNotBusyLayers;
    public int _mustNotBusyLayers_BitShift = 1;
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
    public bool _canUseItem = false;
    public bool _stateLocked = false; //�ܺο��� ���º����� ���͵� �ðڴ�.
    /*------------------------------------------------------------------------------
    |NOTI| !_isAttackState = _isLocoMotionToAttackAction�� �����ϰŰ����� ������ �ƴϴ�
    ------------------------------------------------------------------------------*/


    public RepresentStateType _stateType = RepresentStateType.End;

    public List<StateActionType> _EnterStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _inStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _ExitStateActionTypes = new List<StateActionType>();

    public List<AdditionalBehaveType> _checkingBehaves = new List<AdditionalBehaveType>();

    public List<ConditionDesc> _breakLoopStateCondition = null;
    //public AnimationClip _endStateIdleException = null; //������ �ִϸ��̼��� ������ ���� �ִϸ��̼�
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


    public class StateActionCoroutineWrapper
    {
        public Coroutine _runningCoroutine = null;
        public float _timeACC = 0.0f;
        public float _timeTarget = 0.0f;
    }


    private StateAsset _currState;
    public StateAsset GetCurrState() { return _currState; }

    [SerializeField] private float _stateChangeTime = 0.085f;
    private bool _stateChangeCoroutineStarted = false;
    


    [SerializeField] private List<StateGraphAsset> _initialStateGraphes = new List<StateGraphAsset>();
    private List<StateGraphAsset> _stateGraphes = new List<StateGraphAsset>();
    private StateGraphType _currentGraphType = StateGraphType.LocoStateGraph;

    private List<StateActionCoroutineWrapper> _stateActionCoroutines = new List<StateActionCoroutineWrapper>();


    private float _currStateTime = 0.0f;
    private float _prevStateTime = 0.0f;



    Dictionary<StateAsset, List<LinkedStateAsset>> _currGraphStates = null;
    Dictionary<StateGraphType, HashSet<StateAsset>> _currInteractionPoints = null;


    private float _attackStateAutoChangeTime = 0.0f;
    private float _attackStateAutoChangeTimeAcc = 0.0f;
    private bool _attackStateAutoChangeTimeCoroutineStarted = false;

    private KeyCode _rightHandAttackKey = KeyCode.Mouse0;
    private KeyCode _leftHandAttackKey = KeyCode.Mouse1;
    private float _stateChangeTimeAcc = 0.0f;
    private Dictionary<RepresentStateType, State> _states = new Dictionary<RepresentStateType, State>();
    //private StateAsset _reservedNextWeaponState = null;













    private void Awake()
    {
        PlayerScript playerScript = GetComponent<PlayerScript>();

        _ownerStateControllingComponent._owner = playerScript;

        for (int i = 0; i < (int)StateGraphType.End; i++)
        {
            _stateGraphes.Add(null);
        }

        if (_initialStateGraphes.Count <= 0)
        {
            Debug.Assert(false, "�ּ��� LocoState (Idle�� �ִ�) �׷��� �ϳ��� �غ�ž� �մϴ�");
            Debug.Break();
        }

        foreach (StateGraphAsset stateGraphAsset in _initialStateGraphes)
        {
            StateGraphType type = stateGraphAsset._graphType;

            if (type == StateGraphType.End)
            {
                Debug.Assert(false, "StateGraphType End�� ���� �ȵ˴ϴ�");
                Debug.Break();
            }

            if (_stateGraphes[(int)type] != null)
            {
                Debug.Assert(false, "�ش� ���°� �̹� �ֽ��ϴ�");
                Debug.Break();
            }

            _stateGraphes[(int)type] = stateGraphAsset;
            _stateGraphes[(int)type].SettingOwnerComponent(_ownerStateControllingComponent, _ownerStateControllingComponent._owner);
        }

        for (int i = 0; i < (int)StateActionCoroutineType.End; i++)
        {
            _stateActionCoroutines.Add(null);
        }

        ChangeState(StateGraphType.LocoStateGraph, _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState);
    }


    //public void TryChangeState(RepresentStateType representType)
    //{
    //    if (_states.ContainsKey(representType) == false)
    //    {
    //        Debug.Log("�ش� ���¸� ������� �ʽ��ϴ�");
    //        return;
    //    }

    //    ChangeState(_states[representType]);
    //}


    private void StatedWillBeChanged()
    {
        //_reservedNextWeaponState = null;
        _currStateTime = 0.0f;
        StopAllCoroutines();
    }

    private void ChangeState(StateGraphType nextGraphType, StateAsset nextState)
    {
        StatedWillBeChanged();

        Debug.Log("State Changed : " + nextState._myState._stataName);

        if (_currState != null)
        {
            DoActions(_currState._myState._ExitStateActionTypes);
        }

        _currentGraphType = nextGraphType;
        _currState = nextState;
        _currGraphStates = _stateGraphes[(int)_currentGraphType].GetGraphStates();
        _currInteractionPoints = _stateGraphes[(int)_currentGraphType].GetInteractionPoints();

        AllStopCoroutine();

        DoActions(_currState._myState._EnterStateActionTypes);
    }



    public void DoWork()
    {
        Debug.Assert(_currState != null, "������Ʈ null�Դϴ�");

        StateGraphType nextGraphType = StateGraphType.End;

        StateAsset nextState = CheckChangeState_Recursion(_currState, _currentGraphType, out nextGraphType);

        if (nextState != null)
        {
            ChangeState(nextGraphType, nextState);
        }


        /*---------------------------------------------------------------
        |TODO| �ٸ�������� �����ؾ߸� �մϴ�
        ---------------------------------------------------------------*/
        {
            ////������ �� �� �ִ� ���¿��� ����Ű�� �ƹ��ų� ���Ƚ��ϴ�. 0.1�� �� ���� �ִϸ��̼����� ��ȯ�� �õ��Ұ̴ϴ�.
            //if ((Input.GetKeyDown(_rightHandAttackKey) == true || Input.GetKeyDown(_leftHandAttackKey) == true) &&
            //    _stateChangeCoroutineStarted == false &&
            //    true/*�Ѿ �� �ִ� ���ݻ��°� �ϳ��� �����Ѵ�*/)
            //{
            //    StartCoroutine("AttackComboChangeCoroutine");
            //}
        }


        DoActions(_currState._myState._inStateActionTypes);

        _prevStateTime = _currStateTime;
        _currStateTime += Time.deltaTime;
    }




    public void EquipStateGraph(StateGraphAsset graphAsset, StateGraphType graphType)
    {
        _stateGraphes[(int)graphType] = graphAsset;

        graphAsset.SettingOwnerComponent(_ownerStateControllingComponent, _ownerStateControllingComponent._owner);
    }







    public StateAsset CheckChangeState_Recursion(StateAsset currentState, StateGraphType currType, out StateGraphType resultType) //���� ���¸� �����Ҷ����� ��������� ������ �Լ�
    {
        resultType = currType;
        if (_stateChangeCoroutineStarted == true)
        {
            return null; //���� �޺� üũ�� �������̶� �ƹ��͵� ���Ҳ���
        }

        StateAsset targetState = currentState;

        int debugChangeCount = 0;
        bool isStateChangeGuaranted = false;


        //1. ���� ���°� ���� �׷��� ������ �ٸ� �׷��� Ÿ������ ��ȯ ������ �������� �˻�
        {
            StateGraphAsset currGraphAsset = _stateGraphes[(int)currType];

            foreach (KeyValuePair<StateGraphType, HashSet<StateAsset>> pair in _currInteractionPoints)
            {
                if (pair.Value.Contains(_currState) == false)
                {
                    continue; //���� �� ���´� stateGraphType(Key)�� �Ѿ �� �����ϴ�
                }

                StateGraphAsset anotherStateGraph = _stateGraphes[(int)pair.Key];

                if (anotherStateGraph == null)
                {
                    continue; //�Ѿ���� �ִµ� �ش� ���°� �����ϴ�.(�����ߴµ� ���Ⱑ ���°�� ������)
                }

                SortedDictionary<int, LinkedStateAsset> entryStates = anotherStateGraph.GetEntryStates_Ordered();

                bool tempIsRightWeapon = (pair.Key != StateGraphType.WeaponState_LeftGraph);

                foreach (KeyValuePair<int, LinkedStateAsset> linkedStatePair in entryStates)
                {
                    List<ConditionAssetWrapper> conditionAssetWrappers = linkedStatePair.Value._conditionAsset;

                    bool isSuccess = true;

                    foreach (ConditionAssetWrapper condition in conditionAssetWrappers)
                    {
                        if (CheckCondition(condition, tempIsRightWeapon) == false)
                        {
                            isSuccess = false;
                            break;
                        }
                    }

                    if (isSuccess == true)
                    {
                        resultType = pair.Key;
                        return linkedStatePair.Value._linkedState;
                    }
                }
            }
        }

        //2. �ش����� �ʴ´ٸ� �ڽ��� �׷��������� �ٸ� ���·� �� �� �ִ��� �˻�...�� �Ʒ� �̹� �����ƴ�.
        while (true)
        {
            if (debugChangeCount > 100)
            {
                Debug.Assert(false, "���°� ��� �ٲ���մϴ�. ������ ������ �ֽ��ϱ�?");
                Debug.Break();
                return null;
            }

            bool isSuccess = true;

            if (_currGraphStates.Count <= 0)
            {
                return null;
            }

            List<LinkedStateAsset> linkedStates = _currGraphStates[targetState];

            if (linkedStates.Count <= 0)
            {
                isSuccess = false;
            }

            bool isRightSided = (currType != StateGraphType.WeaponState_LeftGraph);

            foreach (var linkedStateAsset in linkedStates)
            {
                isSuccess = true;

                foreach (ConditionAssetWrapper conditionAssetWrapper in linkedStateAsset._conditionAsset)
                {
                    if (CheckCondition(conditionAssetWrapper, isRightSided) == false)
                    {
                        isSuccess = false;
                        break; //��Ƽ����ǿ��� �ϳ��� �೵��.
                    }
                }

                if (isSuccess == true)
                {
                    targetState = linkedStateAsset._linkedState;

                    {
                        /*--------------------------------------------------------
                        |TODO| �������¶�� ���� �˻縦 �����ʵ��� �ӽù��� ó��. �̰� ����� ���� �����غ���
                        ������ ������ �ٲ��� y��ȭ�� �ִµ� ������ ������ �ߴٰ� �����ع�������
                        --------------------------------------------------------*/
                        if (targetState._myState._EnterStateActionTypes.Count > 0 && targetState._myState._EnterStateActionTypes[0] == StateActionType.Jump) {return targetState;}
                    }

                    if (isStateChangeGuaranted == false)
                    {
                        StatedWillBeChanged();
                        isStateChangeGuaranted = true;
                    }

                    break; //�ٽ� �˻��Ϸ� ����
                }
            }

            if (targetState == currentState &&
                isSuccess == false)
            {
                return null;
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

                        AnimationHipCurve animationHipCurve = ResourceDataManager.Instance.GetHipCurve(_currState._myState._stateAnimationClip);
                        Vector3 currentUnityLocalHip = new Vector3
                            (
                            animationHipCurve._animationHipCurveX.Evaluate(currentSecond),
                            animationHipCurve._animationHipCurveY.Evaluate(currentSecond),
                            animationHipCurve._animationHipCurveZ.Evaluate(currentSecond)
                            );

                        float prevSecond = _prevStateTime;

                        if (prevSecond > currentSecond)//�ִϸ��̼��� �ٲ��? ���Ű� �� ũ��
                        { prevSecond = 0.0f; }

                        Vector3 prevUnityLocalHip = new Vector3
                            (
                            animationHipCurve._animationHipCurveX.Evaluate(prevSecond),
                            animationHipCurve._animationHipCurveY.Evaluate(prevSecond),
                            animationHipCurve._animationHipCurveZ.Evaluate(prevSecond)
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
                        foreach (var type in _currState._myState._checkingBehaves)
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

                case StateActionType.DummyState_EnterLocoStateGraph:
                    {
                        ChangeState(StateGraphType.LocoStateGraph, _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState);
                    }
                    break;

                case StateActionType.AddCoroutine_ChangeToIdleState:
                    AddStateActionCoroutine(StateActionCoroutineType.ChangeToIdle);
                    break;

                case StateActionType.AddCoroutine_StateChangeReady:
                    AddStateActionCoroutine(StateActionCoroutineType.StateChangeReady);
                    break;

                default:
                    Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                    break;
            }
        }
    }

    private void AllStopCoroutine()
    {
        for (int i = 0; i < (int)StateActionCoroutineType.End; i++)
        {
            if (_stateActionCoroutines[i] != null)
            {
                StopCoroutine(_stateActionCoroutines[i]._runningCoroutine);
                _stateActionCoroutines[i] = null;
            }
        }
    }


    private void AddStateActionCoroutine(StateActionCoroutineType type)
    {
        if (_stateActionCoroutines[(int)type] != null)
        {
            StopCoroutine(_stateActionCoroutines[(int)type]._runningCoroutine);
            _stateActionCoroutines[(int)type] = null;
        }

        StateActionCoroutineWrapper newCoroutineWrapper = new StateActionCoroutineWrapper();

        switch (type)
        {
            case StateActionCoroutineType.ChangeToIdle:
                {
                    newCoroutineWrapper._timeTarget = _currState._myState._stateAnimationClip.length;
                    newCoroutineWrapper._runningCoroutine = StartCoroutine(ChangeToIdleCoroutine(newCoroutineWrapper));
                }
                break;

            case StateActionCoroutineType.StateChangeReady:
                {
                    FrameData animationFrameData = ResourceDataManager.Instance.GetAnimationFrameData(_currState._myState._stateAnimationClip, FrameDataType.StateChangeReadyLikeIdle);

                    if (animationFrameData == null)
                    {
                        Debug.Assert(false, "FrameData�� ���������� �ʽ��ϴ�");
                        Debug.Break();
                        return;
                    }

                    newCoroutineWrapper._timeTarget = (float)animationFrameData._frameUp / _currState._myState._stateAnimationClip.frameRate;
                    newCoroutineWrapper._runningCoroutine = StartCoroutine(StateChangeReadyCoroutine(newCoroutineWrapper));
                }
                break;

            default:
                {
                    Debug.Assert(false, "type�� ����� �������� �ʾҽ��ϴ�");
                    Debug.Break();
                }
                break;
        }
        
        _stateActionCoroutines[(int)type] = newCoroutineWrapper;
    }

    private IEnumerator StateChangeReadyCoroutine(StateActionCoroutineWrapper target)
    {
        while (true)
        {
            target._timeACC += Time.deltaTime;

            if (target._timeACC >= target._timeTarget)
            {
                //CopyIdlesState : LinkedState
                Dictionary<StateAsset, List<LinkedStateAsset>> idleStateGraph = _stateGraphes[(int)StateGraphType.LocoStateGraph].GetGraphStates();
                foreach (KeyValuePair<StateAsset, List<LinkedStateAsset>> pair in idleStateGraph)
                {
                    if (_currGraphStates.ContainsKey(pair.Key) == true)
                    {
                        continue;
                    }

                    _currGraphStates.Add(pair.Key, pair.Value);
                }

                //CopyIdlesState : InteractionPoints
                Dictionary<StateGraphType, HashSet<StateAsset>> idleInteractionPoinst = _stateGraphes[(int)_currentGraphType].GetInteractionPoints();
                foreach (KeyValuePair<StateGraphType, HashSet<StateAsset>> pair in idleInteractionPoinst)
                {
                    if (_currInteractionPoints.ContainsKey(pair.Key) == true)
                    {
                        continue;
                    }

                    _currInteractionPoints.Add(pair.Key, pair.Value);
                }


                break;
            }
            yield return null;
        }
    }


    private IEnumerator ChangeToIdleCoroutine(StateActionCoroutineWrapper target)
    {
        while (true)
        {
            target._timeACC += Time.deltaTime;

            if (target._timeACC >= target._timeTarget)
            {
                ChangeState(StateGraphType.LocoStateGraph, _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState);
                break;
            }
            yield return null;
        }
    }








    public bool CheckCondition(ConditionAssetWrapper conditionAssetWrapper, bool isRightSided)
    {
        bool ret = false;

        ConditionDesc conditionDesc = conditionAssetWrapper._conditionAsset._conditionDesc;
        bool stateGoal = conditionAssetWrapper._goal;

        switch (conditionDesc._singleConditionType)
        {
            case ConditionType.MoveDesired:
                {
                    if (_ownerStateControllingComponent._ownerInputController == null)
                    {
                        _ownerStateControllingComponent._ownerInputController = _ownerStateControllingComponent._owner.GetComponent<InputController>();
                    }

                    Vector3 desiredMoved = _ownerStateControllingComponent._ownerInputController._pr_directionByInput;
                    if (desiredMoved != Vector3.zero)
                    {
                        ret = true;
                    }

                    return (ret == stateGoal);
                }

            case ConditionType.AnimationEnd:
                {
                    float animationLength = _currState._myState._stateAnimationClip.length;
                    if (animationLength - Time.deltaTime < _currStateTime)
                    {
                        ret = true;
                    }

                    return (ret == stateGoal);
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

                    return (ret == stateGoal);
                }

            case ConditionType.KeyInput:
                {
                    bool isSuccess = true;

                    for (int i = 0; i < conditionDesc._keyInputConditionTarget.Count; ++i)
                    {
                        KeyPressType type = conditionDesc._keyInputConditionTarget[i]._targetState;
                        InputKeyAsset keyAsset = conditionDesc._keyInputConditionTarget[i]._targetKey;
                        bool goal = stateGoal;

                        isSuccess = (keyAsset.GetKeyState(type) == goal);

                        if (isSuccess == false)
                        {
                            return false; //�Ѱ��� Ʋ�ȴ�
                        }
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
                    StateDesc currStateDesc = _currState._myState;

                    FrameData stateAnimFrameData = ResourceDataManager.Instance.GetAnimationFrameData(currStateDesc._stateAnimationClip, conditionDesc._animationFrameDataType);

                    int currOnwerAnimationFrame = (int)(_currStateTime * currStateDesc._stateAnimationClip.frameRate);

                    return stateAnimFrameData.FrameCheck(currOnwerAnimationFrame);
                }

            case ConditionType.RightHandWeaponSignaled:
                break;

            case ConditionType.LeftHandWeaponSignaled:
                break;

            case ConditionType.RightAttackTry:
                break;

            case ConditionType.LeftAttackTry:
                break;

            case ConditionType.AttackTry:
                break;

            case ConditionType.isTargeting:
                break;

            case ConditionType.ComboKeyCommand:
                return PartailFunc_ComboCommandCheck(conditionDesc, isRightSided);

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




    private bool PartailFunc_ComboCommandCheck(ConditionDesc conditionDesc, bool isRightSided)
    {
        if (false/*������ ���� ������ �õ����� �ʾҽ��ϴ� ����...��������*/)
        {
            return false;
        }

        bool ret = false;

        //1. �ش� ���� ���� �˻�
        {
            if (_ownerStateControllingComponent._owner.GetWeaponScript(isRightSided) == null)
            {
                return false;
            }

            ret = CommandCheck(conditionDesc, isRightSided);
        }

        //2. �ݴ� ���� �˻�
        if (ret == false)
        {
            isRightSided = !isRightSided;

            bool latestSide = _ownerStateControllingComponent._owner.GetLatestWeaponUse();

            //���� �׷����� �������µ�, �������⸦ �޼�, �����տ� �������,
            //���� ������ ������ ���� �״�� �����ϴ� ������ �����ϱ� ������.
            if (latestSide != isRightSided) 
            {
                return false;
            }
            
            if (_ownerStateControllingComponent._owner.GetWeaponScript(isRightSided) == null)
            {
                return false;
            }

            ret = CommandCheck(conditionDesc, isRightSided);
        }

        if (ret == true)
        {
            _ownerStateControllingComponent._owner.SetLatestWeaponUse(isRightSided);
            CustomKeyManager.Instance.ClearKeyRecord();
        }

        return ret;
    }

    private bool CommandCheck(ConditionDesc conditionDesc, bool isRightSided)
    {
        List<ComboKeyCommandDesc> stateComboKeyCommand = conditionDesc._commandInputConditionTarget;
        if (stateComboKeyCommand.Count <= 0)
        {
            Debug.Assert(false, "CommandCondition�ε� Ű�� �ϳ��� �����ϴ�");
            return false;
        }

        LinkedList<ComboCommandKeyDesc> currCommand = CustomKeyManager.Instance.GetComboCommandKeyDescs();

        int KeyRecoredeCount = currCommand.Count - 1;
        int CommandCount = stateComboKeyCommand.Count - 1;

        if (CommandCount > KeyRecoredeCount)
        {
            return false; //�޺��� Ȯ���Ҹ�ŭ Ű�� ����.
        }

        int index = 0;

        WeaponGrabFocus ownerGrabType = _ownerStateControllingComponent._owner.GetGrabFocusType();
        WeaponUseType weaponComboType = WeaponUseType.MainUse;
        ComboCommandKeyType recordedType = ComboCommandKeyType.TargetingBack;

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

                    ComboCommandKeyType targetType = KeyConvert(weaponComboType, ownerGrabType, isRightSided);

                    if (targetType <= ComboCommandKeyType.TargetingRight)
                    {
                        return false; //ġȯ�� �����ߴ�
                    }

                    //if (CustomKeyManager.Instance.AttackKeyRestrainedExist(targetType) == false)
                    //{
                    //    return false;
                    //}

                    if (targetType != recordedType)
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

        return true;
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
    //private void CalculateNextWeaponState(bool isCheckingWeaponEntry) 
    //{
        //if (isCheckingWeaponEntry == true)
        //{
        //    WeaponScript weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(true);

        //    SortedDictionary<int, List<LinkedState>> targetDict = null;

        //    //������ ���� �˻�
        //    if (weaponScript != null)
        //    {
        //        targetDict = weaponScript.GetEntryStates();

        //        _reservedNextWeaponState = CheckNextWeaponState(targetDict, true);
        //    }

        //    if (_reservedNextWeaponState != null)
        //    {
        //        _ownerStateControllingComponent._owner.SetLatestWeaponUse(true);
        //        return;
        //    }

        //    weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(false);

        //    if (weaponScript == null)
        //    {
        //        return;
        //    }

        //    targetDict = weaponScript.GetEntryStates();

        //    _reservedNextWeaponState = CheckNextWeaponState(targetDict, false);
        //    _ownerStateControllingComponent._owner.SetLatestWeaponUse(false);
        //}
        //else 
        //{
        //    //���� -> ������ �Ϸ��� ����

        //    bool isLatestRightHandUse = _ownerStateControllingComponent._owner.GetLatestWeaponUse();
        //    WeaponScript weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(isLatestRightHandUse);
        //    SortedDictionary<int, List<LinkedState>> targetDict = null;

        //    //�ֱٰ����� �ߴ� ���� ���� ���� ���� �˻�
        //    {
        //        if (weaponScript != null)
        //        {
        //            //targetDict = weaponScript.FindLinkedStateNodeDesc(_currState).GetLinkecStates();
        //            targetDict = weaponScript.FindLinkedStateNodeDesc(_currState).GetLinkecStates();

        //            _reservedNextWeaponState = CheckNextWeaponState(targetDict, isLatestRightHandUse);
        //        }

        //        if (_reservedNextWeaponState != null)
        //        {
        //            _ownerStateControllingComponent._owner.SetLatestWeaponUse(isLatestRightHandUse);
        //            return;
        //        }
        //    }


        //    //�ݴ���� Entry�� �˻�
        //    {
        //        weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(!isLatestRightHandUse);

        //        if (weaponScript != null)
        //        {
        //            targetDict = weaponScript.GetEntryStates();

        //            _reservedNextWeaponState = CheckNextWeaponState(targetDict, !isLatestRightHandUse);
        //        }

        //        if (_reservedNextWeaponState != null)
        //        {
        //            _ownerStateControllingComponent._owner.SetLatestWeaponUse(!isLatestRightHandUse);
        //            return;
        //        }
        //    }
        //}
    //}



    //private IEnumerator AttackComboChangeCoroutine()
    //{
    //    _stateChangeTimeAcc = 0.0f;
    //    _stateChangeCoroutineStarted = true;
    //    Debug.Log("Attack Try Coroutine Started");
    //    CustomKeyManager.Instance.SetAttackKeyRestrained(true);

    //    while (true) 
    //    {
    //        _stateChangeTimeAcc += Time.deltaTime;
    //        if (_stateChangeTimeAcc >= _stateChangeTime)
    //        {
    //            Debug.Log("Attack Try Coroutine End Well");
    //            //CalculateNextWeaponState(_currState.GetStateDesc()._isLocoMotionToAttackAction);
    //            break;
    //        }

    //        yield return null;
    //    }

    //    CustomKeyManager.Instance.SetAttackKeyRestrained(false);
    //    _stateChangeCoroutineStarted = false;
    //}





    //private IEnumerator AttackStateAutoChangeCoroutine()
    //{
    //    _attackStateAutoChangeTimeAcc = 0.0f;
    //    _attackStateAutoChangeTime = _currState._myState._stateAnimationClip.length;
    //    _attackStateAutoChangeTimeCoroutineStarted = true;

    //    while (true)
    //    {
    //        _attackStateAutoChangeTimeAcc += Time.deltaTime;

    //        if (_attackStateAutoChangeTimeAcc >= _attackStateAutoChangeTime)
    //        {
    //            CalculateAfterAttackState();
    //            break;
    //        }

    //        yield return null;
    //    }

    //    _attackStateAutoChangeTimeCoroutineStarted = false;
    //}







    //private void CalculateAfterAttackState()
    //{
    //    /*----------------------------------------------------
    //    |NOTI| ���� ������ ������ �⺻������ Idle�� ���ٰ� ó���ϴ� ������
    //    �̰��� �ҿ����ϴ�. ���߿� ������ ���� �� �ִ�
    //    -----------------------------------------------------*/

    //    StateAsset locoStateFirstEntry = _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState;
    //    StateGraphType nextGraphType = StateGraphType.End;

    //    StateAsset nextState = (_reservedNextWeaponState != null)
    //        ? _reservedNextWeaponState
    //        : CheckChangeState_Recursion(locoStateFirstEntry, StateGraphType.LocoStateGraph, out nextGraphType);

    //    if (nextState == null) 
    //    {
    //        ChangeState(nextGraphType, locoStateFirstEntry);
    //    }

    //    else if (nextState != _currState)
    //    {
    //        ChangeState(nextGraphType, nextState);
    //    }
    //}




    //private State CheckNextWeaponState(SortedDictionary<int, List<LinkedState>> targetDict, bool isRightHandWeapon)
    //{
    //    //foreach (KeyValuePair<int, List<LinkedState>> stateList in targetDict)
    //    //{
    //    //    foreach (LinkedState entryState in stateList.Value)
    //    //    {
    //    //        //�̹� �����ź��� ���ĵ��ִٰ� �����Ѵ�. �׷��� �ʴٸ� ���ķ����� ������. ���⼭ �Ű澲�� �ʴ´�
    //    //        bool stateCheckPassed = true;

    //    //        foreach (ConditionDesc condition in entryState._multiConditions)
    //    //        {
    //    //            if (CheckCondition(condition, isRightHandWeapon) == false)
    //    //            {
    //    //                stateCheckPassed = false;
    //    //                break;
    //    //            }
    //    //        }

    //    //        if (stateCheckPassed == true)
    //    //        {
    //    //            return entryState._state;
    //    //        }
    //    //    }
    //    //}

    //    return null;
    //}
















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
