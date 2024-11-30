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
    DummyState_EnterLocoStateGraph, //DummyState를 안쓴다면, AnimationEndCoroutine_ReturnToIdle로 구현할 수 있다.
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

public enum StateActionCoroutineType
{
    ChangeToIdle,
    StateChangeReady,
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
    public float _comboStrainedTime = -1.0f; //n초 내에 완성시켜야 하는 콤보
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
    public bool _stateLocked = false; //외부에서 상태변경이 들어와도 씹겠다.
    /*------------------------------------------------------------------------------
    |NOTI| !_isAttackState = _isLocoMotionToAttackAction의 개념일거같지만 지금은 아니다
    ------------------------------------------------------------------------------*/


    public RepresentStateType _stateType = RepresentStateType.End;

    public List<StateActionType> _EnterStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _inStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _ExitStateActionTypes = new List<StateActionType>();

    public List<AdditionalBehaveType> _checkingBehaves = new List<AdditionalBehaveType>();

    public List<ConditionDesc> _breakLoopStateCondition = null;
    //public List<AnimationClip> _bsAnimations = null; //선딜 애니메이션
    //public List<AnimationClip> _asAnimations = null; //후딜 애니메이션
    //public AnimationClip _endStateIdleException = null; //상태의 애니메이션이 끝날때 예외 애니메이션
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

    public class StateActionCoroutineWrapper
    {
        public Coroutine _runningCoroutine = null;
        public float _timeACC = 0.0f;
        public float _timeTarget = 0.0f;
    }

    public class LinkedStateAssetWrapper
    {
        public LinkedStateAssetWrapper(StateGraphType fromType, StateGraphType goalType, LinkedStateAsset linkedStateAsset, List<ConditionAssetWrapper> additionalCondition)
        {
            _fromType = fromType;
            _goalType = goalType;
            _linkedState = linkedStateAsset;
            _additionalCondition = additionalCondition;
        }

        public StateGraphType _fromType = StateGraphType.End;
        public StateGraphType _goalType = StateGraphType.End;
        public LinkedStateAsset _linkedState = null;
        public List<ConditionAssetWrapper> _additionalCondition = null;
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
    private StateContollerComponentDesc _ownerStateControllingComponent = new StateContollerComponentDesc();
    List<LinkedStateAssetWrapper> _currLinkedStates = new List<LinkedStateAssetWrapper>();





    private float _attackStateAutoChangeTime = 0.0f;
    private float _attackStateAutoChangeTimeAcc = 0.0f;
    private bool _attackStateAutoChangeTimeCoroutineStarted = false;

    private KeyCode _rightHandAttackKey = KeyCode.Mouse0;
    private KeyCode _leftHandAttackKey = KeyCode.Mouse1;
    private float _stateChangeTimeAcc = 0.0f;
    private Dictionary<RepresentStateType, State> _states = new Dictionary<RepresentStateType, State>();
    //private StateAsset _reservedNextWeaponState = null;


    public StateAsset GetMyIdleStateAsset()
    {
        return _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState;
    }






    private bool _readyDebugging = false;









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
            Debug.Assert(false, "최소한 LocoState (Idle이 있는) 그래프 하나는 준비돼야 합니다");
            Debug.Break();
        }

        foreach (StateGraphAsset stateGraphAsset in _initialStateGraphes)
        {
            StateGraphType type = stateGraphAsset._graphType;

            if (type == StateGraphType.End)
            {
                Debug.Assert(false, "StateGraphType End는 쓰면 안됩니다");
                Debug.Break();
            }

            if (_stateGraphes[(int)type] != null)
            {
                Debug.Assert(false, "해당 상태가 이미 있습니다");
                Debug.Break();
            }

            _stateGraphes[(int)type] = stateGraphAsset;
            _stateGraphes[(int)type].SettingOwnerComponent(_ownerStateControllingComponent, _ownerStateControllingComponent._owner);
        }

        for (int i = 0; i < (int)StateActionCoroutineType.End; i++)
        {
            _stateActionCoroutines.Add(null);
        }
        ReadyLinkedStates(StateGraphType.LocoStateGraph, GetMyIdleStateAsset(), true);
        ChangeState(StateGraphType.LocoStateGraph, _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState);
    }


    //public void TryChangeState(RepresentStateType representType)
    //{
    //    if (_states.ContainsKey(representType) == false)
    //    {
    //        Debug.Log("해당 상태를 사용하지 않습니다");
    //        return;
    //    }

    //    ChangeState(_states[representType]);
    //}


    private void StatedWillBeChanged()
    {
        //_reservedNextWeaponState = null;
        _currStateTime = 0.0f;
        _prevStateTime = 0.0f;
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

        _ownerStateControllingComponent._owner.ChangeAnimation(_currState);

        AllStopCoroutine();

        DoActions(_currState._myState._EnterStateActionTypes);
    }



    private void ReadyLinkedStates(StateGraphType currentGraphType, StateAsset currState, bool isClear)
    {
        if (isClear == true)
        {
            _currLinkedStates.Clear();
        }

        //Interaction Point를 먼저 검사해야하기 때문에 먼저 담는다.
        StateGraphAsset currentGraphAsset = _stateGraphes[(int)currentGraphType];

        Dictionary<StateGraphType, Dictionary<StateAsset, List<ConditionAssetWrapper>>> currentInteractionPoints = currentGraphAsset.GetInteractionPoints();

        foreach (KeyValuePair<StateGraphType, Dictionary<StateAsset, List<ConditionAssetWrapper>>> pair in currentInteractionPoints)
        {
            int keyIndex = (int)pair.Key;

            if (_stateGraphes[keyIndex] == null)
            {
                continue;
            }

            Dictionary<StateAsset, List<ConditionAssetWrapper>> interactionState = pair.Value;

            foreach (LinkedStateAsset entryStates in _stateGraphes[keyIndex].GetEntryStates())
            {
                List<ConditionAssetWrapper> additionalCondition = null;

                if (interactionState.ContainsKey(currState) == true)
                {
                    additionalCondition = interactionState[currState];
                }

                _currLinkedStates.Add(new LinkedStateAssetWrapper(currentGraphType, pair.Key, entryStates, additionalCondition));
            }
        }

        //InGraphState를 담는다
        List<LinkedStateAsset> linkedStates = _stateGraphes[(int)currentGraphType].GetGraphStates()[currState];
        foreach (var linkedState in linkedStates)
        {
            _currLinkedStates.Add(new LinkedStateAssetWrapper(currentGraphType, currentGraphType, linkedState, null));
        }
    }





    public StateAsset CheckChangeState_Recursion2(out StateGraphType nextGraphType) //최종 상태를 결정할때까지 재귀적으로 실행할 함수
    {
        StateAsset targetState = _currState;
        nextGraphType = _currentGraphType;

        int successCount = 0;
        bool isStateChangeGuaranted = false;
        bool isSuccess = false;

        while (true)
        {
            if (successCount > 100)
            {
                Debug.Assert(false, "상태가 계속 바뀌려합니다");
                Debug.Break();
                return null;
            }

            foreach (LinkedStateAssetWrapper linkedStateAssetWrapper in _currLinkedStates)
            {
                isSuccess = true;

                StateGraphType nextStateGraphtype = linkedStateAssetWrapper._fromType;
                LinkedStateAsset linkedState = linkedStateAssetWrapper._linkedState;
                List<ConditionAssetWrapper> conditionAssetWrappers = linkedState._conditionAsset;

                bool isRightSided = (linkedStateAssetWrapper._goalType != StateGraphType.WeaponState_LeftGraph && linkedStateAssetWrapper._fromType != StateGraphType.WeaponState_LeftGraph);

                foreach (ConditionAssetWrapper conditionAssetWrapper in conditionAssetWrappers)
                {
                    if (CheckCondition(targetState, conditionAssetWrapper, isRightSided) == false)
                    {
                        isSuccess = false;
                        break;
                    }
                }

                if (linkedStateAssetWrapper._additionalCondition != null &&
                    linkedStateAssetWrapper._additionalCondition.Count > 0)
                {
                    foreach (ConditionAssetWrapper conditionAssetWrapper in linkedStateAssetWrapper._additionalCondition)
                    {
                        if (CheckCondition(targetState, conditionAssetWrapper, isRightSided) == false)
                        {
                            isSuccess = false;
                            break;
                        }
                    }
                }


                if (isSuccess == true)
                {
                    if (isStateChangeGuaranted == false)
                    {
                        StatedWillBeChanged();
                        isStateChangeGuaranted = true;
                    }

                    targetState = linkedStateAssetWrapper._linkedState._linkedState;
                    nextGraphType = linkedStateAssetWrapper._goalType;
                    ReadyLinkedStates(nextGraphType, targetState, true);

                    if (targetState._myState._EnterStateActionTypes.Count > 0 && targetState._myState._EnterStateActionTypes[0] == StateActionType.Jump) { return targetState; }
                    break;
                }
            }

            if (isSuccess == true)
            {
                successCount++;
                continue;
            }

            break;
        }

        if (targetState == _currState)
        {
            return null;
        }

        return targetState;
    }



    public void DoWork()
    {
        Debug.Assert(_currState != null, "스테이트 null입니다");

        StateGraphType nextGraphType = StateGraphType.End;

        StateAsset nextState = CheckChangeState_Recursion2(out nextGraphType);

        if (nextState != null)
        {
            ChangeState(nextGraphType, nextState);
        }

        DoActions(_currState._myState._inStateActionTypes);

        _prevStateTime = _currStateTime;
        _currStateTime += Time.deltaTime;
    }




    public void EquipStateGraph(StateGraphAsset graphAsset, StateGraphType graphType)
    {
        _stateGraphes[(int)graphType] = graphAsset;

        if (graphAsset != null)
        {
            graphAsset.SettingOwnerComponent(_ownerStateControllingComponent, _ownerStateControllingComponent._owner);
        }

        ReadyLinkedStates(_currentGraphType, _currState, true);
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
                        float currentSecond = MyUtil.FloatMod(_currStateTime, _currState._myState._stateAnimationClip.length);
                        float prevSecond = MyUtil.FloatMod(_prevStateTime, _currState._myState._stateAnimationClip.length);

                        AnimationHipCurve animationHipCurve = ResourceDataManager.Instance.GetHipCurve(_currState._myState._stateAnimationClip);

                        Vector3 currentUnityLocalHip = new Vector3
                        (
                            animationHipCurve._animationHipCurveX.Evaluate(currentSecond),
                            animationHipCurve._animationHipCurveY.Evaluate(currentSecond),
                            animationHipCurve._animationHipCurveZ.Evaluate(currentSecond)
                        );

                        if (prevSecond > currentSecond)//애니메이션이 바뀌였나? 과거가 더 크다
                        {
                            prevSecond = 0.0f;
                        }

                        Vector3 prevUnityLocalHip = new Vector3
                        (
                            animationHipCurve._animationHipCurveX.Evaluate(prevSecond),
                            animationHipCurve._animationHipCurveY.Evaluate(prevSecond),
                            animationHipCurve._animationHipCurveZ.Evaluate(prevSecond)
                        );

                        Vector3 deltaLocalHip = (currentUnityLocalHip - prevUnityLocalHip);

                        Vector3 worldDelta = _ownerStateControllingComponent._ownerCharacterComponent.transform.localToWorldMatrix * deltaLocalHip;

                        //Root 모션의 y값은 모델에 적용...AnimationClip의 BakeIntoPose가 있다
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
                    Debug.Assert(false, "데이터가 추가됐습니까?");
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
                        Debug.Assert(false, "FrameData가 설정돼있지 않습니다");
                        Debug.Break();
                        return;
                    }

                    newCoroutineWrapper._timeTarget = (float)animationFrameData._frameUp / _currState._myState._stateAnimationClip.frameRate;
                    newCoroutineWrapper._runningCoroutine = StartCoroutine(StateChangeReadyCoroutine(newCoroutineWrapper));
                }
                break;

            default:
                {
                    Debug.Assert(false, "type이 제대로 지정되지 않았습니다");
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
                {
                    ////Idle의 InteractionPoint를 담는다
                    //Dictionary<StateGraphType, Dictionary<StateAsset, List<ConditionAssetWrapper>>> currentInteractionPoints = _stateGraphes[(int)StateGraphType.LocoStateGraph].GetInteractionPoints();
                    //foreach (KeyValuePair<StateGraphType, Dictionary<StateAsset, List<ConditionAssetWrapper>>> pair in currentInteractionPoints)
                    //{
                    //    int graphTypeIndex = (int)pair.Key;

                    //    if (_stateGraphes[graphTypeIndex] == null)
                    //    {
                    //        continue;
                    //    }

                    //    List<LinkedStateAsset> linkedStateAssets = _stateGraphes[graphTypeIndex].GetEntryStates();

                    //    Dictionary<StateAsset, List<ConditionAssetWrapper>> interactionPoint = pair.Value;

                    //    foreach (LinkedStateAsset linkedStateAsset in linkedStateAssets)
                    //    {
                    //        List<ConditionAssetWrapper> additionalCondition = null;
                    //        if (interactionPoint.ContainsKey(_currState) == true)
                    //        {
                    //            additionalCondition = interactionPoint[_currState];
                    //        }
                    //        _currLinkedStates.Add(new LinkedStateAssetWrapper(_currentGraphType, pair.Key, linkedStateAsset, additionalCondition));
                    //    }
                    //}


                    ////Idle의 Linked State를 담는다.
                    //StateAsset idleStateInsuranced = _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState;
                    //List<LinkedStateAsset> idleStateLinked = _stateGraphes[(int)StateGraphType.LocoStateGraph].GetGraphStates()[idleStateInsuranced];
                    //foreach (LinkedStateAsset linkedState in idleStateLinked)
                    //{
                    //    _currLinkedStates.Add(new LinkedStateAssetWrapper(_currentGraphType, StateGraphType.LocoStateGraph, linkedState, null));
                    //}
                }

                ReadyLinkedStates(StateGraphType.LocoStateGraph, GetMyIdleStateAsset(), false);
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






    public bool CheckCondition(StateAsset stateAsset, ConditionAssetWrapper conditionAssetWrapper, bool isRightSided)
    {
        bool ret = false;
        bool forcedValue = false;

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
                }
                break;

            case ConditionType.AnimationEnd:
                {
                    float animationLength = stateAsset._myState._stateAnimationClip.length;
                    if (animationLength - Time.deltaTime < _currStateTime)
                    {
                        ret = true;
                    }
                }
                break;

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
                }
                break;

            case ConditionType.KeyInput:
                {
                    ret = true;
                    forcedValue = true;

                    for (int i = 0; i < conditionDesc._keyInputConditionTarget.Count; ++i)
                    {
                        KeyPressType type = conditionDesc._keyInputConditionTarget[i]._targetState;
                        InputKeyAsset keyAsset = conditionDesc._keyInputConditionTarget[i]._targetKey;
                        bool goal = stateGoal;

                        ret = (keyAsset.GetKeyState(type) == goal);

                        if (ret == false)
                        {
                            break;
                        }
                    }

                }
                break;

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
                    StateDesc currStateDesc = stateAsset._myState;

                    FrameData stateAnimFrameData = ResourceDataManager.Instance.GetAnimationFrameData(currStateDesc._stateAnimationClip, conditionDesc._animationFrameDataType);

                    int currOnwerAnimationFrame = (int)(_currStateTime * currStateDesc._stateAnimationClip.frameRate);

                    ret = stateAnimFrameData.FrameCheck(currOnwerAnimationFrame);
                }
                break;

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
                {
                    forcedValue = true;
                    ret = PartailFunc_ComboCommandCheck(conditionDesc, isRightSided);
                }
                break;

            case ConditionType.FocusedWeapon:
                break;

            case ConditionType.AnimatorLayerNotBusy:
                {
                    //if (conditionDesc._mustNotBusyLayers.Count <= 0)
                    //{
                    //    return true;
                    //}

                    //if (conditionDesc._mustNotBusyLayers_BitShift == -1)
                    //{
                    //    foreach (AnimatorLayerTypes item in conditionDesc._mustNotBusyLayers)
                    //    {
                    //        conditionDesc._mustNotBusyLayers_BitShift = (conditionDesc._mustNotBusyLayers_BitShift | (1 << (int)item));
                    //    }
                    //}

                    //int ownerBusyLayer_Bitshift = _ownerStateControllingComponent._owner.GetCurrentBusyAnimatorLayer_BitShift();

                    //if ((ownerBusyLayer_Bitshift & conditionDesc._mustNotBusyLayers_BitShift) == 0)
                    //{
                    //    return true;
                    //}
                }
                break;

            default:
                Debug.Assert(false, "데이터가 추가됐습니까?");
                break;
        }

        if (forcedValue == true)
        {
            return ret;
        }

        return (ret == stateGoal);
    }




    private bool PartailFunc_ComboCommandCheck(ConditionDesc conditionDesc, bool isRightSided)
    {
        if (false/*뭔가의 현재 공격을 시도하지 않았습니다 로직...구현하자*/)
        {
            return false;
        }

        bool ret = false;

        if (_ownerStateControllingComponent._owner.GetWeaponScript(isRightSided) == null)
        {
            return false;
        }

        ret = CommandCheck(conditionDesc, isRightSided);

        if (ret == true)
        {
            _ownerStateControllingComponent._owner.SetLatestWeaponUse(isRightSided);
        }

        return ret;
    }

    private bool CommandCheck(ConditionDesc conditionDesc, bool isRightSided)
    {
        List<ComboKeyCommandDesc> stateComboKeyCommand = conditionDesc._commandInputConditionTarget;
        if (stateComboKeyCommand.Count <= 0)
        {
            Debug.Assert(false, "CommandCondition인데 키가 하나도 없습니다");
            return false;
        }

        LinkedList<ComboCommandKeyDesc> currCommand = CustomKeyManager.Instance.GetComboCommandKeyDescs();

        int KeyRecoredeCount = currCommand.Count - 1;
        int CommandCount = stateComboKeyCommand.Count - 1;

        if (CommandCount > KeyRecoredeCount)
        {
            return false; //콤보를 확인할만큼 키가 없다.
        }

        int index = 0;

        WeaponGrabFocus ownerGrabType = _ownerStateControllingComponent._owner.GetGrabFocusType();
        WeaponUseType weaponComboType = WeaponUseType.MainUse;
        ComboCommandKeyType recordedType = ComboCommandKeyType.TargetingBack;

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

                if (weaponComboType >= WeaponUseType.MainUse && weaponComboType <= WeaponUseType.SpecialUseUp)
                {
                    if (recordedType >= ComboCommandKeyType.TargetingBack && recordedType <= ComboCommandKeyType.TargetingRight)
                    { return false; }

                    ComboCommandKeyType targetType = KeyConvert(weaponComboType, ownerGrabType, isRightSided);

                    if (targetType <= ComboCommandKeyType.TargetingRight)
                    {
                        return false; //치환에 실패했다
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

        CustomKeyManager.Instance.ClearKeyRecord();
        return true;
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

            case WeaponUseType.MainUseUp: //무기의 콤보가 주사용 클릭이다
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.RightUp;
                            }
                            else
                            {
                                convertedRet = ComboCommandKeyType.LeftUp;
                            }
                        }
                        break;

                    case WeaponGrabFocus.RightHandFocused: //무기를 오른편에 양손으로 잡고있다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.RightUp;
                            }
                        }
                        break;

                    case WeaponGrabFocus.LeftHandFocused:
                        {
                            if (isRightHandWeapon == false)
                            {
                                convertedRet = ComboCommandKeyType.LeftUp;
                            }
                        }
                        break;
                }
                break;

            case WeaponUseType.SubUseUp:  //무기의 콤보가 보조 사용 클릭이다
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.SubRightUp;
                            }
                            else
                            {
                                convertedRet = ComboCommandKeyType.SubLeftUp;
                            }
                        }
                        break;

                    case WeaponGrabFocus.RightHandFocused: //무기를 오른편에 양손으로 잡고있다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.LeftUp;
                            }
                        }
                        break;

                    case WeaponGrabFocus.LeftHandFocused:
                        {
                            if (isRightHandWeapon == false)
                            {
                                convertedRet = ComboCommandKeyType.RightUp;
                            }
                        }
                        break;
                }
                break;

            case WeaponUseType.SpecialUseUp:  //무기의 콤보가 특수 사용 클릭이다
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.CtrlRightUp;
                            }
                            else
                            {
                                convertedRet = ComboCommandKeyType.CtrlLeftUp;
                            }
                        }
                        break;

                    case WeaponGrabFocus.RightHandFocused: //무기를 오른편에 양손으로 잡고있다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.CtrlRightUp;
                            }
                        }
                        break;

                    case WeaponGrabFocus.LeftHandFocused:
                        {
                            if (isRightHandWeapon == false)
                            {
                                convertedRet = ComboCommandKeyType.CtrlLeftUp;
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
















    //public StateAsset CheckChangeState_Recursion(out StateGraphType nextGraphType) //최종 상태를 결정할때까지 재귀적으로 실행할 함수
    //{
    //    StateAsset targetState = _currState;
    //    nextGraphType = _currentGraphType;

    //    int debugChangeCount = 0;
    //    bool isStateChangeGuaranted = false;

    //    //1. 지금 상태가 속한 그래프 내에서 다른 그래프 타입으로 교환 가능한 상태인지 검사
    //    {
    //        //foreach (KeyValuePair<StateGraphType, HashSet<StateAsset>> pair in _currInteractionPoints)
    //        foreach (KeyValuePair<StateGraphType, HashSet<StateAsset>> pair in _currInteractionPoints_DeepCopy)
    //        {
    //            StateGraphAsset anotherStateGraph = _stateGraphes[(int)pair.Key];

    //            {
    //                if (pair.Value.Contains(_currState) == false)
    //                {
    //                    continue; //지금 내 상태는 stateGraphType(Key)로 넘어갈 수 없습니다
    //                }

    //                if (anotherStateGraph == null)
    //                {
    //                    continue; //넘어갈수는 있는데 해당 상태가 없습니다.(공격했는데 무기가 없는경우 같은거)
    //                }
    //            }

    //            SortedDictionary<int, LinkedStateAsset> entryStates = anotherStateGraph.GetEntryStates_Ordered();

    //            bool tempIsRightWeapon = (pair.Key != StateGraphType.WeaponState_LeftGraph);

    //            bool isSuccess = true;

    //            foreach (KeyValuePair<int, LinkedStateAsset> linkedStatePair in entryStates)
    //            {
    //                List<ConditionAssetWrapper> conditionAssetWrappers = linkedStatePair.Value._conditionAsset;

    //                foreach (ConditionAssetWrapper condition in conditionAssetWrappers)
    //                {
    //                    if (CheckCondition(targetState, condition, tempIsRightWeapon) == false)
    //                    {
    //                        isSuccess = false;
    //                        break;
    //                    }
    //                }

    //                if (isSuccess == true)
    //                {
    //                    debugChangeCount++;
    //                    nextGraphType = pair.Key;
    //                    targetState = linkedStatePair.Value._linkedState;

    //                    {
    //                        _currLinkedStates_DeepCopy.Clear();
    //                        Dictionary<StateAsset, List<LinkedStateAsset>> currentGraph = _stateGraphes[(int)nextGraphType].GetGraphStates();
    //                        foreach (LinkedStateAsset item in currentGraph[targetState])
    //                        {
    //                            _currLinkedStates_DeepCopy.Add(item);
    //                        }
    //                    }
    //                    break;
    //                }
    //            }
    //        }
    //    }

    //    //2. 해당하지 않는다면 자신의 그래프내에서 다른 상태로 갈 수 있는지 검사...는 아래 이미 구현됐다.
    //    while (true)
    //    {
    //        if (debugChangeCount > 100)
    //        {
    //            Debug.Assert(false, "상태가 계속 바뀌려합니다. 로직에 문제가 있습니까?");
    //            Debug.Break();
    //            return null;
    //        }

    //        bool isSuccess = true;

    //        bool isRightSided = (nextGraphType != StateGraphType.WeaponState_LeftGraph);

    //        if (_currLinkedStates_DeepCopy.Count <= 0)
    //        {
    //            isSuccess = false;
    //        }

    //        foreach (var linkedStateAsset in _currLinkedStates_DeepCopy)
    //        {
    //            isSuccess = true;

    //            foreach (ConditionAssetWrapper conditionAssetWrapper in linkedStateAsset._conditionAsset)
    //            {
    //                if (CheckCondition(targetState, conditionAssetWrapper, isRightSided) == false)
    //                {
    //                    isSuccess = false;
    //                    break; //멀티컨디션에서 하나라도 삑났다.
    //                }
    //            }

    //            if (isSuccess == true)
    //            {
    //                debugChangeCount++;
    //                targetState = linkedStateAsset._linkedState;
    //                {
    //                    /*--------------------------------------------------------
    //                    |TODO| 점프상태라면 연쇄 검사를 하지않도록 임시방편 처리. 이거 지우는 구조 생각해볼것
    //                    이유는 점프로 바뀌어야 y변화가 있는데 그전에 착지를 했다고 판정해버려서임
    //                    --------------------------------------------------------*/
    //                    if (targetState._myState._EnterStateActionTypes.Count > 0 && targetState._myState._EnterStateActionTypes[0] == StateActionType.Jump) { return targetState; }
    //                }

    //                //_currLinkedStates = _stateGraphes[(int)_currentGraphType].GetGraphStates()[targetState];
    //                /*-------------------------------------------------------------------------
    //                |TODO| 아래 코드 반드시 고쳐야한다.
    //                목적은 ReadyIdle 코루틴 실행시키면 링크스테이트가 섞이는데
    //                임시로 나누는 코드임
    //                -------------------------------------------------------------------------*/
    //                {
    //                    _currLinkedStates_DeepCopy.Clear();
    //                    foreach (StateGraphAsset graphAsset in _stateGraphes)
    //                    {
    //                        if (graphAsset == null)
    //                        {
    //                            continue;
    //                        }

    //                        Dictionary<StateAsset, List<LinkedStateAsset>> currentGraph = graphAsset.GetGraphStates();
    //                        if (currentGraph.ContainsKey(targetState) == false)
    //                        {
    //                            continue;
    //                        }

    //                        if (graphAsset._graphType == StateGraphType.WeaponState_RightGraph ||
    //                            graphAsset._graphType == StateGraphType.WeaponState_LeftGraph)
    //                        {
    //                            nextGraphType = (isRightSided == true)
    //                                ? StateGraphType.WeaponState_RightGraph
    //                                : StateGraphType.WeaponState_LeftGraph;
    //                        }
    //                        else
    //                        {
    //                            nextGraphType = graphAsset._graphType;
    //                        }

    //                        foreach (LinkedStateAsset item in currentGraph[targetState])
    //                        {
    //                            _currLinkedStates_DeepCopy.Add(item);
    //                        }
    //                        break;
    //                    }
    //                }

    //                if (isStateChangeGuaranted == false)
    //                {
    //                    StatedWillBeChanged();
    //                    isStateChangeGuaranted = true;
    //                }

    //                break; //다시 검사하러 간다
    //            }
    //        }

    //        if (targetState == _currState &&
    //            isSuccess == false)
    //        {
    //            if (debugChangeCount > 0)
    //            {
    //                return targetState;
    //            }

    //            return null;
    //        }

    //        if (isSuccess == true)
    //        {
    //            continue;
    //        }

    //        return targetState;
    //    }
    //}










    //private bool CheckChangeStatePartial_InteractionPointCheck(StateAsset targetState, out StateAsset resultState, out StateGraphType resultGraphType)
    //{
    //    bool isSuccess = true;

    //    resultState = targetState;
    //    resultGraphType = _currentGraphType;

    //    foreach (KeyValuePair<StateGraphType, HashSet<StateAsset>> pair in _currInteractionPoints_DeepCopy)
    //    {
    //        isSuccess = true;

    //        if (pair.Value.Contains(targetState) == false)
    //        {
    //            continue; //지금 내 상태는 stateGraphType(Key)로 넘어갈 수 없습니다
    //        }

    //        StateGraphAsset anotherStateGraph = _stateGraphes[(int)pair.Key];

    //        if (anotherStateGraph == null)
    //        {
    //            continue; //넘어갈수는 있는데 해당 상태가 없습니다.(공격했는데 무기가 없는경우 같은거)
    //        }

    //        bool isRightSided = (pair.Key != StateGraphType.WeaponState_LeftGraph);

    //        SortedDictionary<int, LinkedStateAsset> entryStates = anotherStateGraph.GetEntryStates_Ordered();

    //        foreach (KeyValuePair<int, LinkedStateAsset> linkedStatePair in entryStates)
    //        {
    //            List<ConditionAssetWrapper> conditionAssetWrappers = linkedStatePair.Value._conditionAsset;

    //            foreach (ConditionAssetWrapper condition in conditionAssetWrappers)
    //            {
    //                if (CheckCondition(targetState, condition, isRightSided) == false)
    //                {
    //                    isSuccess = false;
    //                    break;
    //                }
    //            }

    //            if (isSuccess == true)
    //            {
    //                resultState = linkedStatePair.Value._linkedState;
    //                resultGraphType = pair.Key;
    //                return true;
    //            }
    //        }
    //    }

    //    return false;
    //}

    //private void CheckChangeStatePartial_ReadyNextState(StateGraphType nextGraphType, StateAsset targetState)
    //{
    //    //연쇄 검사를 위해 LinkedState들을 준비한다
    //    {
    //        _currLinkedStates_DeepCopy.Clear();

    //        foreach (StateGraphAsset graphAsset in _stateGraphes)
    //        {
    //            if (graphAsset == null)
    //            {
    //                continue;
    //            }

    //            Dictionary<StateAsset, List<LinkedStateAsset>> currentGraph = graphAsset.GetGraphStates();

    //            if (currentGraph.ContainsKey(targetState) == false)
    //            {
    //                continue;
    //            }

    //            foreach (LinkedStateAsset item in currentGraph[targetState])
    //            {
    //                _currLinkedStates_DeepCopy.Add(item);
    //            }
    //            break;
    //        }
    //    }

    //    //연쇄 검사를 위해 InteractionPoint들을 준비한다
    //    {
    //        _currInteractionPoints_DeepCopy.Clear();
    //        _currInteractionPoints = _stateGraphes[(int)nextGraphType].GetInteractionPoints();
    //        foreach (KeyValuePair<StateGraphType, HashSet<StateAsset>> interactionPoints in _currInteractionPoints)
    //        {
    //            HashSet<StateAsset> newHashSet = new HashSet<StateAsset>();
    //            foreach (var stateAsset in interactionPoints.Value)
    //            {
    //                newHashSet.Add(stateAsset);
    //            }
    //            _currInteractionPoints_DeepCopy.Add(interactionPoints.Key, newHashSet);
    //        }
    //    }
    //}

    //private bool CheckChangeStatePartial_InGraphCheck(StateAsset targetState, out StateAsset resultState, bool isRightSided)
    //{
    //    bool isSuccess = true;

    //    resultState = targetState;

    //    foreach (var linkedStateAsset in _currLinkedStates_DeepCopy)
    //    {
    //        isSuccess = true;

    //        foreach (ConditionAssetWrapper conditionAssetWrapper in linkedStateAsset._conditionAsset)
    //        {
    //            if (CheckCondition(targetState, conditionAssetWrapper, isRightSided) == false)
    //            {
    //                isSuccess = false;
    //                break; //멀티컨디션에서 하나라도 삑났다.
    //            }
    //        }

    //        if (isSuccess == true) 
    //        {
    //            resultState = linkedStateAsset._linkedState;
    //            return true;
    //        }
    //    }

    //    return false;
    //}








    //isCheckingWeaponEntry == T -> Weapon Motion 이 아닌데 Weapon Motion을 쓰려고하는경우
    //isCheckingWeaponEntry == F -> Weapon Motion 에서  Weapon Motion을 쓰려고하는경우
    //private void CalculateNextWeaponState(bool isCheckingWeaponEntry) 
    //{
    //if (isCheckingWeaponEntry == true)
    //{
    //    WeaponScript weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(true);

    //    SortedDictionary<int, List<LinkedState>> targetDict = null;

    //    //오른손 먼저 검사
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
    //    //공격 -> 공격을 하려는 경우다

    //    bool isLatestRightHandUse = _ownerStateControllingComponent._owner.GetLatestWeaponUse();
    //    WeaponScript weaponScript = _ownerStateControllingComponent._owner.GetWeaponScript(isLatestRightHandUse);
    //    SortedDictionary<int, List<LinkedState>> targetDict = null;

    //    //최근공격을 했던 손의 연결 상태 먼저 검사
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


    //    //반대손의 Entry를 검사
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
    //    |NOTI| 무기 공격을 끝내면 기본적으로 Idle로 간다고 처리하는 구조임
    //    이것은 불완전하다. 나중에 문제가 생길 수 있다
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
    //    //        //이미 어려운거부터 정렬돼있다고 가정한다. 그렇지 않다면 정렬로직의 문제다. 여기서 신경쓰지 않는다
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
