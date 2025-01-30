using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StateGraphAsset;
using static MyUtil;
using static AnimationFrameDataAsset;
using UnityEngine.Playables;
using MagicaCloth2;

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
    CharacterRotate,
    AI_CharacterRotateToEnemy,
    AI_ChaseToEnemy,
    AI_ForcedLookAtEnemy,
    AI_ReArrangeStateGraph,
    AI_UpdateAttackRange, //Chase의 경우, 달려가다가 원거리 공격이 가능하면 멀리서 멈춰서야한다.
    Move_WithOutRotate,
    LookAtLockOnTarget,
    RotateToLockOnTarget,
    RootMove_WithOutRotate,
    CharacterRotateToCameraLook,
    EnterGunAiming,
    ExitGunAiming,

    Move_WithOutRotate_Gun,
    LookAtLockOnTarget_Gun,

    AnimationAttack,
    AttackLookAtLockOnTarget,

    AddCoroutine_DeadCall,
    AddCoroutine_BuffCheck,
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
    ReturnFalse, //무조건 False를 반환합니다.
    EnterStatetimeThreshould,
    AI_CheckEnemyInMySight,
    AI_Check_I_CAN_ATTACK_MY_ENEMY,
    AI_RandomChance,

    AI_InAttackRange,   //상태의 사거리
    AI_StateCoolTimeCheck, //상태 쿨타임
    AI_EnemyDegreeRange, //각도편차

    AI_EnemyIsExitst,
    AI_ReadiedAttackExist, //하나라도 준비된 공격이 있나? 있으면 Chase 할거임. 준비는 됐는데 사거리가 짧은거니까
    AI_ArriveInAttackRange,
    AI_EnemyIsAttackState, //적이 공격중입니까
    SameConditionDiveringChance,
    AI_TryRandomChance, // n 퍼센트의 확률로 상태를 시도하려 합니다. 실패하면 락에 걸려, 상태가 바뀌기 전까지 시도할 수 없습니다.
    AI_StateTimeThreshould, // n 퍼센트의 확률로 상태를 시도하려 합니다. 실패하면 락에 걸려, 상태가 바뀌기 전까지 시도할 수 없습니다.
    
    IsLockOnTarget, //락온을 한 상태입니다.

    IsHoldingWeaponKey, //무기 사용키(왼클, 오른클)을 누르고 있습니다. 무기도 들고 있습니다.

    IsWeaponUseReady, //격발할 준비가 됐습니다 -> 무기마다 따로 Override
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
    RollFront,
    Guard_Lvl_0, //일반 가드
    Guard_Lvl_1, //앉아서 가드
    Guard_Lvl_2, //앉아서 가드 + 잠금

    Hit_Lvl_0, //움찔하는정도
    Hit_Lvl_1, //자세가 무너지고 휘청거림
    Hit_Lvl_2, //날라감

    Blocked_Reaction, //잘 막았음
    Blocked_Sliding, //미끄러짐
    Blocked_Crash, //자세가 무너짐

    DieNormal,
    DieThrow,

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
    public ItemAsset_Weapon.WeaponType _weaponTypeGoal;
    public List<KeyInputConditionDesc> _keyInputConditionTarget;
    public List<ComboKeyCommandDesc> _commandInputConditionTarget;
    //public FrameDataType _animationFrameDataType = FrameDataType.End;
    public float _comboStrainedTime = -1.0f; //n초 내에 완성시켜야 하는 콤보
    public List<AnimatorLayerTypes> _mustNotBusyLayers;
    public int _mustNotBusyLayers_BitShift = 1;
    public float _enterStatetimeThreshould = 0.0f;
    public int _randomChance = 1;
    public float _degThreshould = 0.0f;
}



[Serializable]
public class AIStateDesc
{
    public float _coolTime = -1.0f;
    public float _randomChancePercentage = -1.0f;
    public float _stateTimeThreshould = -1.0f;
}



[Serializable]
public class AIAttackStateDesc
{
    public float _coolTime = -1.0f;
    public float _range = -1.0f;
}





[Serializable]
public class StateDesc
{
    public RepresentStateType _stateType = RepresentStateType.End;

    public AnimationClip _stateAnimationClip = null;
    public bool _rightWeaponOverride = true;
    public bool _leftWeaponOverride = true;

    public bool _isAttackState = false;
    public DamageDesc _attackDamageMultiply = null;

    public bool _isLocoMotionToAttackAction = false;
    public bool _isLoopState = false;
    public bool _canUseItem = false;
    public bool _stateLocked = false; //외부에서 상태변경이 들어와도 씹겠다.
    /*------------------------------------------------------------------------------
    |NOTI| !_isAttackState = _isLocoMotionToAttackAction의 개념일거같지만 지금은 아니다
    ------------------------------------------------------------------------------*/
    public bool _isBlockState = false; //가드상태입니다.


    public bool _isAIAttackState = false;
    public bool _isAIState = false;

    public AIStateDesc _aiStateDesc = null;
    public AIAttackStateDesc _aiAttackStateDesc = null;
    

    public List<StateActionType> _EnterStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _inStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _ExitStateActionTypes = new List<StateActionType>();

    public List<AdditionalBehaveType> _checkingBehaves = new List<AdditionalBehaveType>();

    public List<ConditionDesc> _breakLoopStateCondition = null;
    public bool _isSubBlendTreeExist = false;
    public SubBlendTreeAsset_2D _subBlendTree = null;

    public bool _isSubAnimationStateMachineExist = false;
    public SubAnimationStateMachine _subAnimationStateMachine = null;

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

public class StateContoller : GameCharacterSubScript
{
    public class StateActionCoroutineWrapper
    {
        //public Coroutine _runningCoroutine = null;
        public AEachFrameData _frameData = null;
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
            _hardness = CalculateConditionWeight(_linkedState._conditionAsset);

            if (_additionalCondition != null)
            {
                _hardness += CalculateConditionWeight(additionalCondition);
            }
        }

        public StateGraphType _fromType = StateGraphType.End;
        public StateGraphType _goalType = StateGraphType.End;
        public LinkedStateAsset _linkedState = null;
        public List<ConditionAssetWrapper> _additionalCondition = null;
        public int _hardness = -1;
    }


    [Serializable]
    public class StateGraphInitialWrapper
    {
        public StateGraphType _type = StateGraphType.End;
        public StateGraphAsset _asset = null;
    }


    public override void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(StateContoller);

        for (int i = 0; i < (int)StateGraphType.End; i++)
        {
            _stateGraphes.Add(null);
        }

        if (_initialStateGraphes.Count <= 0)
        {
            Debug.Assert(false, "최소한 LocoState (Idle이 있는) 그래프 하나는 준비돼야 합니다");
            Debug.Break();
        }

        foreach (StateGraphInitialWrapper stateGraphAssetWrapper in _initialStateGraphes)
        {
            StateGraphType type = stateGraphAssetWrapper._type;

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
            _initialStateGraphes = null; //더이상 쓰지않는다!

            _initialStateGraphesDict.Add(type, stateGraphAssetWrapper._asset);
            _stateGraphes[(int)type] = stateGraphAssetWrapper._asset;
            //_stateGraphes[(int)type].SettingOwnerComponent(_ownerStateControllingComponent, _owner);
        }

        //for (int i = 0; i < (int)StateActionCoroutineType.End; i++)
        //{
        //    _stateActionCoroutines.Add(null);
        //}
    }
    public override void SubScriptStart() {}


    private StateAsset _currState = null;
    public StateAsset GetCurrState() { return _currState; }


    [SerializeField] private List<StateGraphInitialWrapper> _initialStateGraphes = new List<StateGraphInitialWrapper>();
    private Dictionary<StateGraphType, StateGraphAsset> _initialStateGraphesDict = new Dictionary<StateGraphType, StateGraphAsset>();

    private List<StateGraphAsset> _stateGraphes = new List<StateGraphAsset>();
    private StateGraphType _currentGraphType = StateGraphType.LocoStateGraph;
    private StateGraphType _previousGraphType = StateGraphType.LocoStateGraph;
    public StateGraphType GetCurrStateGraphType() { return _currentGraphType; }
    public StateGraphAsset GetCurrStateGraph() { return _stateGraphes[(int)_currentGraphType]; }
    public List<StateGraphAsset> GetStateGraphes() { return _stateGraphes; }
    public StateGraphAsset GetBasicStateGraphes(StateGraphType type) {return _initialStateGraphesDict[type];}

    private float _currStateTime = 0.0f;
    private float _prevStateTime = 0.0f;
    

    List<LinkedStateAssetWrapper> _currLinkedStates = new List<LinkedStateAssetWrapper>();

    private List<Coroutine> _stateActionCoroutines = new List<Coroutine>();
    private int _randomStateInstructIndex = -1;
    private int _randomStateTryCount = 0;
    private HashSet<StateAsset> _failedRandomChanceState = new HashSet<StateAsset>();
    public void AddFailedRandomChanceState(StateAsset stateAsset)
    {
        _failedRandomChanceState.Add(stateAsset);
    }
    public bool FindFailedRandomChanceState(StateAsset stateAsset)
    {
        return _failedRandomChanceState.Contains(stateAsset);
    }
    public void ClearFailedRandomChanceState()
    {
        _failedRandomChanceState.Clear();
    }




    /*----------------------------------------------------------
    |NOTI| 공격애니메이션이 아닐땐 무조건 true에서 시작합니다.
    ----------------------------------------------------------*/
    private bool _nextComboReady = true;





    public StateAsset GetMyIdleStateAsset()
    {
        return _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState;
    }

    protected virtual void Start()
    {
        ReadyLinkedStates(StateGraphType.LocoStateGraph, GetMyIdleStateAsset(), true);
        ChangeState(StateGraphType.LocoStateGraph, _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState);
    }

    public void TryChangeState(StateGraphType graphType, RepresentStateType representType)
    {
        StateGraphAsset targetGraphAsset = _stateGraphes[(int)graphType];

        if (targetGraphAsset == null)
        {
            return; //해당 그래프가 없다.
        }

        StateAsset targetAsset = targetGraphAsset.GetRepresentStateAsset(representType);

        if (targetAsset == null) 
        {
            return; //그래프에 해당 상태가 없다.
        }

        ChangeState(graphType, targetAsset);

        ReadyLinkedStates(graphType, targetAsset, true);
    }

    public void TryChangeState(StateGraphType graphType, StateAsset targetAsset)
    {
        StateGraphAsset targetGraphAsset = _stateGraphes[(int)graphType];

        if (targetGraphAsset == null)
        {
            return; //해당 그래프가 없다.
        }

        ChangeState(graphType, targetAsset);
    }

    private void StatedWillBeChanged()
    {
        _currStateTime = 0.0f;
        _prevStateTime = 0.0f;
        _failedRandomChanceState.Clear();
    }

    private void ChangeState(StateGraphType nextGraphType, StateAsset nextState)
    {
        StatedWillBeChanged();

        if (_currState != null)
        {
            DoActions(_currState._myState._ExitStateActionTypes);
        }

        _nextComboReady = true;

        _previousGraphType = _currentGraphType;

        _currentGraphType = nextGraphType;
        _currState = nextState;

        _randomStateInstructIndex = -1;
        _randomStateTryCount = 0;
        _failedRandomChanceState.Clear();
        
        _owner.StateChanged(_currState);
        
        AllStopCoroutine();

        if (_currentGraphType == StateGraphType.WeaponState_RightGraph)
        {
            _owner.SetLatestWeaponUse(true);
        }
        else if (_currentGraphType == StateGraphType.WeaponState_LeftGraph)
        {
            _owner.SetLatestWeaponUse(false);
        }

        DoActions(_currState._myState._EnterStateActionTypes);

        AddStateActionCoroutine();
    }



    public int SubAnimationStateIndex(StateAsset nextState)
    {
        SubAnimationStateMachine.CalculateLogic logic = nextState._myState._subAnimationStateMachine._calculateLogic;

        int ret = -1;

        switch (logic)
        {
            case SubAnimationStateMachine.CalculateLogic.MoveDesiredDirection:
                {
                    Vector3 moveDesiredDir = _owner.GCST<InputController>()._pr_directionByInput;
                    float angle = Vector3.Angle(Vector3.forward, moveDesiredDir);

                    if (angle <= 0.0f)
                    {
                        ret = 0;
                        break;
                    }

                    angle /= 45.0f;
                    ret = (int)angle;

                    bool isOverPlane = (Vector3.Cross(Vector3.forward, moveDesiredDir).y < 0.0f);

                    if (isOverPlane == true)
                    {
                        ret = 8 - ret;
                    }

                    if (_owner.GCST<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBool("IsMirroring") == true)
                    {
                        ret = 8 - ret;
                    }
                }
                
                break;

            default:
                {
                    Debug.Assert(false, "데이터가 추가됐습니까?");
                    Debug.Break();
                }
                break;
        }

        return ret;
    }

    private void ReadyLinkedStates(StateGraphType currentGraphType, StateAsset currState, bool isClear)
    {
        if (isClear == true)
        {
            _currLinkedStates.Clear();
        }

        //Interaction Point를 먼저 검사해야하기 때문에 먼저 담는다.
        {
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
                    if (interactionState.ContainsKey(currState) == false)
                    {
                        continue;
                    }

                    List<ConditionAssetWrapper> additionalCondition = null;

                    if (interactionState.ContainsKey(currState) == true)
                    {
                        additionalCondition = interactionState[currState];
                    }

                    _currLinkedStates.Add(new LinkedStateAssetWrapper(currentGraphType, pair.Key, entryStates, additionalCondition));
                }
            }
        }

        //담고나서 어려운순서로 정렬
        {
            _currLinkedStates.Sort((a, b) => b._hardness.CompareTo(a._hardness));
        }

        //InGraphState를 담는다
        {
            List<LinkedStateAsset> linkedStates = _stateGraphes[(int)currentGraphType].GetGraphStates()[currState];
            foreach (var linkedState in linkedStates)
            {
                _currLinkedStates.Add(new LinkedStateAssetWrapper(currentGraphType, currentGraphType, linkedState, null));
            }
        }
    }

    public StateAsset CheckChangeState_Recursion2(out StateGraphType nextGraphType) //최종 상태를 결정할때까지 재귀적으로 실행할 함수
    {
        //List<StateAsset> record = new List<StateAsset>();
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

            if (_currLinkedStates.Count <= 0)
            {
                isSuccess = false;
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
                    if (CheckCondition(targetState, linkedStateAssetWrapper, conditionAssetWrapper, isRightSided) == false)
                    {
                        isSuccess = false;
                        break;
                    }
                }

                if (linkedStateAssetWrapper._additionalCondition != null &&
                    linkedStateAssetWrapper._additionalCondition.Count > 0 &&
                    isSuccess == true)
                {
                    foreach (ConditionAssetWrapper conditionAssetWrapper in linkedStateAssetWrapper._additionalCondition)
                    {
                        if (CheckCondition(targetState, linkedStateAssetWrapper, conditionAssetWrapper, isRightSided) == false)
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
                //record.Add(targetState);
                successCount++;
                continue;
            }

            break;
        }

        if (targetState == _currState &&
            successCount <= 0)
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

        AIAttackStateDesc aiAttackStateDesc = _currState._myState._aiAttackStateDesc;
        if (aiAttackStateDesc != null &&
            aiAttackStateDesc._coolTime >= 0.0f)
        {
            _owner.GCST<EnemyAIScript>().AddCoolTimeCoroutine(_currState);
        }

        _prevStateTime = _currStateTime;
        _currStateTime += Time.deltaTime;
    }

    public void EquipStateGraph(StateGraphAsset graphAsset, StateGraphType graphType)
    {
        _stateGraphes[(int)graphType] = graphAsset;

        //if (graphAsset != null)
        //{
        //    graphAsset.SettingOwnerComponent(_ownerStateControllingComponent, _owner);
        //}

        ReadyLinkedStates(_currentGraphType, _currState, true);
    }


    public Vector3 CalculateCurrHipCurve(float time)
    {
        AnimationClip currentAnimationClip = (_currState._myState._isSubAnimationStateMachineExist == true)
            ? _owner.GCST<CharacterAnimatorScript>().GetCurrAnimationClip()
            : _currState._myState._stateAnimationClip;


        AnimationHipCurveAsset animationHipCurve = ResourceDataManager.Instance.GetHipCurve(currentAnimationClip);

        Vector3 currentUnityLocalHip = new Vector3
        (
            animationHipCurve._animationHipCurveX.Evaluate(time),
            animationHipCurve._animationHipCurveY.Evaluate(time),
            animationHipCurve._animationHipCurveZ.Evaluate(time)
        );

        return currentUnityLocalHip;
    }


    public Vector3 CalculateCurrentRootDelta()
    {
        float currentSecond = FloatMod(_currStateTime, _currState._myState._stateAnimationClip.length);
        float prevSecond = FloatMod(_prevStateTime, _currState._myState._stateAnimationClip.length);

        Vector3 currentUnityLocalHip = CalculateCurrHipCurve(currentSecond);
        //루프 애니메이션에서 넘어갔을때 처리임
        if (prevSecond > currentSecond) {prevSecond = 0.0f;}
        Vector3 prevUnityLocalHip = CalculateCurrHipCurve(prevSecond);

        Vector3 deltaLocalHip = (currentUnityLocalHip - prevUnityLocalHip);


        //return _owner.GCST<CharacterContollerable>().transform.localToWorldMatrix * deltaLocalHip;
        return Matrix4x4.TRS(Vector3.zero, transform.localToWorldMatrix.rotation, transform.localToWorldMatrix.lossyScale) * deltaLocalHip;
    }

    private void SwitchFunc_RootMove()
    {
        Vector3 worldDelta = CalculateCurrentRootDelta();

        {
            Vector3 modelLocalPosition = _owner.GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject().transform.localPosition;
            modelLocalPosition.y = worldDelta.y;
            _owner.GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject().transform.localPosition = modelLocalPosition;
        }

        worldDelta.y = 0.0f;

        _owner.GCST<CharacterContollerable>().CharacterRootMove(worldDelta, 1.0f, 1.0f);

    }

    private void SwitchFunc_SimpleMove()
    {
        Vector3 characterInputDir = _owner.GCST<InputController>()._pr_directionByInput;
        characterInputDir = _owner.GCST<CharacterContollerable>().GetDirectionConvertedByCamera(characterInputDir);
        _owner.GCST<CharacterContollerable>().CharacterMove(characterInputDir, 1.0f, 1.0f);
    }

    private void SwitchFunc_SimpleRotate()
    {
        Vector3 characterInputDir = _owner.GCST<InputController>()._pr_directionByInput;
        CharacterContollerable ownerCharacterControllerable = _owner.GCST<CharacterContollerable>();
        characterInputDir = ownerCharacterControllerable.GetDirectionConvertedByCamera(characterInputDir);
        ownerCharacterControllerable.CharacterRotate(characterInputDir, 1.0f);
    }


    public void DoActions(List<StateActionType> actions)
    {
        foreach (var action in actions)
        {
            switch (action)
            {
                case StateActionType.Move:
                    {
                        SwitchFunc_SimpleMove();
                        SwitchFunc_SimpleRotate();
                    }
                    break;

                case StateActionType.Jump:
                    {
                        _owner.GCST<CharacterContollerable>().DoJump();
                    }
                    break;

                case StateActionType.ForcedMove:
                    {
                        _owner.GCST<CharacterContollerable>().CharacterInertiaMove(1.0f);
                    }
                    break;

                case StateActionType.RootMove:
                    {
                        SwitchFunc_RootMove();
                    }
                    break;

                case StateActionType.RotateWithoutInterpolate:
                    {
                        Vector3 convertedDirection = _owner.GCST<CharacterContollerable>().GetDirectionConvertedByCamera(_owner.GCST<InputController>()._pr_directionByInput);
                        _owner.GCST<CharacterContollerable>().LookAt_Plane(convertedDirection);
                    }
                    break;

                case StateActionType.CheckBehaves:
                    {
                        foreach (var type in _currState._myState._checkingBehaves)
                        {
                            _owner.CheckBehave(type);
                        }
                    }
                    break;

                case StateActionType.CalculateWeaponLayer_EnterAttack:
                    {
                        _owner.GCST<CharacterAnimatorScript>().WeaponLayerChange(_owner.GetGrabFocusType(),_currState,_owner.GetLatestWeaponUse(),true);
                    }
                    break;

                case StateActionType.CalculateWeaponLayer_ExitAttack:
                    {
                        _owner.GCST<CharacterAnimatorScript>().WeaponLayerChange(_owner.GetGrabFocusType(),_currState,_owner.GetLatestWeaponUse(),false);
                    }
                    break;

                case StateActionType.DummyState_EnterLocoStateGraph:
                    {
                        ChangeState(StateGraphType.LocoStateGraph, _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState);
                    }
                    break;

                case StateActionType.AddCoroutine_ChangeToIdleState: 
                    {
                        Debug.Assert(false, "이제 안씁니다" + StateActionType.AddCoroutine_ChangeToIdleState);
                        Debug.Break();
                        /*AddStateActionCoroutine(StateActionCoroutineType.ChangeToIdle);*/
                    }
                    break;
                case StateActionType.AddCoroutine_StateChangeReady: 
                    {
                        Debug.Assert(false, "이제 안씁니다" + StateActionType.AddCoroutine_StateChangeReady);
                        Debug.Break();
                        /*AddStateActionCoroutine(StateActionCoroutineType.StateChangeReady);*/
                    }
                    break;

                case StateActionType.CharacterRotate:
                    {
                        SwitchFunc_SimpleRotate();
                    }
                    break;

                case StateActionType.AI_CharacterRotateToEnemy:
                    {
                        Vector3 enemyPosition = _owner.GCST<EnemyAIScript>().GetCurrentEnemy().gameObject.transform.position;
                        Vector3 myPosition = transform.position;
                        Vector3 toEnemyDir = (enemyPosition - myPosition).normalized;
                        _owner.GCST<CharacterContollerable>().CharacterRotate(toEnemyDir, 1.0f);
                    }
                    break;

                case StateActionType.AI_ChaseToEnemy:
                    {
                        Vector3 myPosition = _owner.gameObject.transform.position;
                        Vector3 enemyPosition = _owner.GCST<EnemyAIScript>().GetCurrentEnemy().gameObject.transform.position;
                        Vector3 toEnemyPosition = (enemyPosition - myPosition).normalized;

                        CharacterContollerable characterContollerable = _owner.GCST<CharacterContollerable>();

                        characterContollerable.CharacterRotate(toEnemyPosition, 1.0f);
                        characterContollerable.CharacterMove(toEnemyPosition, characterContollerable.CalculateMoveDirSimilarities(toEnemyPosition), 1.0f);
                        
                    }
                    break;

                case StateActionType.AI_ForcedLookAtEnemy:
                    {
                        Vector3 myPosition = _owner.gameObject.transform.position;
                        Vector3 enemyPosition = _owner.GCST<EnemyAIScript>().GetCurrentEnemy().gameObject.transform.position;
                        Vector3 dirToEnemy = (enemyPosition - myPosition).normalized;

                        _owner.GCST<CharacterContollerable>().LookAt_Plane(dirToEnemy);
                    }
                    break;

                case StateActionType.AI_ReArrangeStateGraph:
                    {
                        EnemyAIScript aiScript = _owner.GCST<EnemyAIScript>();
                        aiScript.ReArrangeStateGraph(_currLinkedStates, this, _currState);
                    }
                    break;

                case StateActionType.AI_UpdateAttackRange:
                    {
                        EnemyAIScript ownerEnemyAIScript = _owner.GCST<EnemyAIScript>();
                        StateGraphAsset aggressiveStateAsset = _stateGraphes[(int)StateGraphType.AI_AggresiveGraph];
                        if (aggressiveStateAsset == null)
                        {
                            Debug.Assert(false, "AI_UpdateAttackRange행동에는 반드시 Aggressive StateGraph가 있어야 합니다");
                            Debug.Break();
                            return;
                        }
                        ownerEnemyAIScript.UpdateChasingDistance(ref aggressiveStateAsset);
                    }
                    break;

                case StateActionType.Move_WithOutRotate:
                    {
                        Vector3 characterInputDir = _owner.GCST<InputController>()._pr_directionByInput;
                        characterInputDir = _owner.GCST<CharacterContollerable>().GetDirectionConvertedByCamera(characterInputDir);
                        _owner.GCST<CharacterContollerable>().CharacterMove(characterInputDir, 1.0f, 1.0f);
                    }
                    break;


                case StateActionType.LookAtLockOnTarget:
                    {
                        AimScript2 ownerAimScript = _owner.GCST<AimScript2>();

                        if (ownerAimScript.GetLockOnObject() == null)
                        {
                            continue;
                        }

                        Vector3 targetPosition = ownerAimScript.GetLockOnObject().transform.position;
                        Vector3 ownerPosition = gameObject.transform.position;
                        Vector3 ownerToTargetPlaneVector = (targetPosition - ownerPosition);
                        ownerToTargetPlaneVector.y = 0.0f;
                        ownerToTargetPlaneVector = ownerToTargetPlaneVector.normalized;

                        _owner.GCST<CharacterContollerable>().LookAt_Plane(ownerToTargetPlaneVector);
                    }
                    break;

                case StateActionType.RotateToLockOnTarget:
                    {
                        AimScript2 ownerAimScript = _owner.GCST<AimScript2>();
                        Vector3 targetPosition = ownerAimScript.GetLockOnObject().transform.position;
                        Vector3 ownerPosition = gameObject.transform.position;
                        Vector3 ownerToTargetPlaneVector = (targetPosition - ownerPosition);
                        ownerToTargetPlaneVector.y = 0.0f;
                        ownerToTargetPlaneVector = ownerToTargetPlaneVector.normalized;

                        _owner.GCST<CharacterContollerable>().CharacterRotate(ownerToTargetPlaneVector, 1.0f);
                    }
                    break;

                case StateActionType.RootMove_WithOutRotate:
                    {
                        SwitchFunc_RootMove();
                    }
                    break;

                case StateActionType.CharacterRotateToCameraLook:
                    {
                        Vector3 cameraLook = Camera.main.transform.forward;
                        cameraLook.y = 0.0f;
                        _owner.GCST<CharacterContollerable>().CharacterRotate(cameraLook.normalized, 1.0f);
                    }
                    break;

                case StateActionType.EnterGunAiming:
                    {
                        bool isRightWeapon = _owner.GetLatestWeaponUse();

                        if (isRightWeapon == true)
                        {
                            _owner._isRightWeaponAimed = true;
                        }
                        else
                        {
                            _owner._isLeftWeaponAimed = true;
                        }
                    }
                    break;

                case StateActionType.ExitGunAiming:
                    {
                        bool isRightWeapon = (_currentGraphType == StateGraphType.WeaponState_RightGraph);

                        if (isRightWeapon == true)
                        {
                            _owner._isRightWeaponAimed = false;
                        }
                        else
                        {
                            _owner._isLeftWeaponAimed = false;
                        }
                    }
                    break;

                case StateActionType.Move_WithOutRotate_Gun:
                    {
                        SwitchFunc_RootMove();
                    }
                    break;

                case StateActionType.LookAtLockOnTarget_Gun:
                    {
                        Vector3 cameraLook = Camera.main.transform.forward;
                        cameraLook.y = 0.0f;
                        _owner.GCST<CharacterContollerable>().CharacterRotate(cameraLook.normalized, 1.0f);
                    }
                    break;

                case StateActionType.AnimationAttack:
                    {
                        Debug.Assert(false, "이제 쓰지 않습니다");
                        Debug.Break();
                    }
                    break;

                case StateActionType.AttackLookAtLockOnTarget:
                    {
                        AimScript2 ownerAimScript = _owner.GCST<AimScript2>();
                        GameObject lockOnTarget = ownerAimScript.GetLockOnObject();

                        if (lockOnTarget == null)
                        {
                            Vector3 convertedDirection = _owner.GCST<CharacterContollerable>().GetDirectionConvertedByCamera(_owner.GCST<InputController>()._pr_directionByInput);
                            if (convertedDirection == Vector3.zero)
                            {
                                convertedDirection = transform.forward;
                            }
                            _owner.GCST<CharacterContollerable>().LookAt_Plane(convertedDirection);
                        }
                        else
                        {
                            Vector3 targetPosition = ownerAimScript.GetLockOnObject().transform.position;
                            Vector3 ownerPosition = gameObject.transform.position;
                            Vector3 ownerToTargetPlaneVector = (targetPosition - ownerPosition);
                            ownerToTargetPlaneVector.y = 0.0f;
                            ownerToTargetPlaneVector = ownerToTargetPlaneVector.normalized;
                            _owner.GCST<CharacterContollerable>().LookAt_Plane(ownerToTargetPlaneVector);
                        }
                    }
                    break;

                case StateActionType.AddCoroutine_DeadCall: 
                    {
                        Debug.Assert(false, "이제 안씁니다" + StateActionType.AddCoroutine_DeadCall);
                        Debug.Break();
                        /*AddStateActionCoroutine(StateActionCoroutineType.DeadCall);*/
                    }
                    break;

                case StateActionType.AddCoroutine_BuffCheck: 
                    {
                        Debug.Assert(false, "이제 안씁니다" + StateActionType.AddCoroutine_BuffCheck);
                        Debug.Break();
                        /*AddStateActionCoroutine(StateActionCoroutineType.Buff);*/
                    }
                    break;

                default:
                    //Debug.Assert(false, "데이터가 추가됐습니까?" + action);
                    break;
            }
        }
    }


    private void AllStopCoroutine()
    {
        foreach (Coroutine coroutine in _stateActionCoroutines)
        {
            StopCoroutine(coroutine);
        }

        _stateActionCoroutines.Clear();
    }


    private void AddStateActionCoroutine()
    {
        Dictionary<FrameDataWorkType, List<AEachFrameData>> allFrameDatas = ResourceDataManager.Instance.GetAnimationAllFrameData(_currState._myState._stateAnimationClip);

        if (allFrameDatas == null)
        {
            return;
        }

        foreach (KeyValuePair<FrameDataWorkType, List<AEachFrameData>> pair in allFrameDatas)
        {
            FrameDataWorkType type = pair.Key;

            foreach (AEachFrameData eachFrameData in pair.Value)
            {
                StateActionCoroutineWrapper newCoroutineWrapper = new StateActionCoroutineWrapper();
                switch (type)
                {
                    case FrameDataWorkType.ChangeToIdle:
                        {
                            newCoroutineWrapper._timeTarget = _currState._myState._stateAnimationClip.length;
                            _stateActionCoroutines.Add(StartCoroutine(ChangeToIdleCoroutine(newCoroutineWrapper)));
                        }
                        break;

                    case FrameDataWorkType.StateChangeReady:
                        {
                            newCoroutineWrapper._frameData = eachFrameData;
                            newCoroutineWrapper._timeTarget = (float)eachFrameData._frameUp / _currState._myState._stateAnimationClip.frameRate;
                            _stateActionCoroutines.Add(StartCoroutine(StateChangeReadyCoroutine(newCoroutineWrapper)));
                        }
                        break;

                    case FrameDataWorkType.NextAttackMotion:
                        {
                            newCoroutineWrapper._frameData = eachFrameData;
                            _stateActionCoroutines.Add(StartCoroutine(NextAttackComboCoroutine(newCoroutineWrapper)));
                        }
                        break;

                    case FrameDataWorkType.Attack:
                        {
                            CharacterColliderScript colliderScript = _owner.GCST<CharacterColliderScript>();
                            colliderScript.ColliderWork(pair.Value, _currState);
                        }
                        break;

                    case FrameDataWorkType.DeadCall:
                        {
                            newCoroutineWrapper._timeTarget = _currState._myState._stateAnimationClip.length;
                            _stateActionCoroutines.Add(StartCoroutine(DeadCallCoroutine(newCoroutineWrapper)));
                        }
                        break;

                    case FrameDataWorkType.AddBuff:
                        {
                        }
                        break;

                    case FrameDataWorkType.RemoveBuff:
                        {
                        }
                        break;

                    default:
                        {
                            Debug.Assert(false, "type이 제대로 지정되지 않았습니다");
                            Debug.Break();
                        }
                        break;
                }
            }
        }
    }


    private IEnumerator ChangeToIdleCoroutine(StateActionCoroutineWrapper target)
    {
        while (true)
        {
            target._timeACC += Time.deltaTime;

            if (target._timeACC >= target._timeTarget)
            {
                ReadyLinkedStates(StateGraphType.LocoStateGraph, _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState, true);
                ChangeState(StateGraphType.LocoStateGraph, _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState);
                break;
            }
            yield return null;
        }
    }

    private IEnumerator StateChangeReadyCoroutine(StateActionCoroutineWrapper target)
    {
        while (true)
        {
            target._timeACC += Time.deltaTime;

            if (target._timeACC >= target._timeTarget)
            {
                ReadyLinkedStates(StateGraphType.LocoStateGraph, GetMyIdleStateAsset(), false);
                break;
            }
            yield return null;
        }
    }

    private IEnumerator NextAttackComboCoroutine(StateActionCoroutineWrapper target)
    {
        if (target._frameData == null)
        {
            Debug.Assert(false, "NextAttackComboCoroutine은 frameData가 없어선 안된다");
            Debug.Break();
        }

        _nextComboReady = false;

        float underSec = target._frameData._frameUnder / _currState._myState._stateAnimationClip.frameRate;
        float upSec = target._frameData._frameUp / _currState._myState._stateAnimationClip.frameRate;

        while (true)
        {
            target._timeACC += Time.deltaTime;

            if (target._timeACC < underSec && target._timeACC > upSec)
            {
                _nextComboReady = true;
            }

            if (target._timeACC >= underSec)
            {
                _nextComboReady = false;
                break;
            }

            yield return null;
        }
    }

    private IEnumerator DeadCallCoroutine(StateActionCoroutineWrapper target)
    {
        while (true)
        {
            target._timeACC += Time.deltaTime;

            if (target._timeACC >= target._timeTarget)
            {
                _owner.DeadCall();
                break;
            }
            yield return null;
        }
    }

    private IEnumerator StateAddBuffCoroutine(StateActionCoroutineWrapper target)
    {
        while (true)
        {
            target._timeACC += Time.deltaTime;

            if (target._timeACC >= target._timeTarget)
            {
                break;
            }
            yield return null;
        }
    }

    private IEnumerator StateRemoveBuffCoroutine(StateActionCoroutineWrapper target)
    {
        while (true)
        {
            target._timeACC += Time.deltaTime;

            if (target._timeACC >= target._timeTarget)
            {
                break;
            }
            yield return null;
        }
    }


    public bool CheckCondition(StateAsset currstateAsset, LinkedStateAssetWrapper nextStateAsset, ConditionAssetWrapper conditionAssetWrapper, bool isRightSided)
    {
        bool ret = false;
        bool forcedValue = false;

        ConditionDesc conditionDesc = conditionAssetWrapper._conditionAsset._conditionDesc;
        bool stateGoal = conditionAssetWrapper._goal;

        switch (conditionDesc._singleConditionType)
        {
            case ConditionType.MoveDesired:
                {
                    Vector3 desiredMoved = _owner.GCST<InputController>()._pr_directionByInput;
                    if (desiredMoved != Vector3.zero)
                    {
                        ret = true;
                    }
                }
                break;

            case ConditionType.AnimationEnd:
                {
                    float animationLength = currstateAsset._myState._stateAnimationClip.length;
                    if (animationLength - Time.deltaTime < _currStateTime)
                    {
                        ret = true;
                    }
                }
                break;

            case ConditionType.InAir:
                {
                    if (_owner.GCST<CharacterContollerable>().GetIsInAir() == true)
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
                    Debug.Assert(false, "이제 이 조건은 사용하지 않습니다" + ConditionType.EquipWeaponByType);
                    Debug.Break();
                }
                break;

            case ConditionType.AnimationFrame:
                {
                    ret = _nextComboReady;
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
                }
                break;

            case ConditionType.ReturnFalse:
                {
                    forcedValue = true;
                    ret = false;
                }
                break;

            case ConditionType.EnterStatetimeThreshould:
                {
                    ret = (_currStateTime > conditionDesc._enterStatetimeThreshould);
                }
                break;

            case ConditionType.AI_CheckEnemyInMySight:
                {
                    forcedValue = true;

                    ret = _owner.GCST<EnemyAIScript>().InBattleCheck();
                }
                break;

            case ConditionType.AI_Check_I_CAN_ATTACK_MY_ENEMY:
                {
                    forcedValue = true;

                    ret = _owner.GCST<EnemyAIScript>().InAttackRangeCheck(nextStateAsset._linkedState._linkedState);
                }
                break;

            case ConditionType.AI_RandomChance:
                {
                    forcedValue = true;

                    if (conditionDesc._randomChance <= 0)
                    {
                        Debug.Assert(false, "Random 확률의 분모는 1 이상이여야합니다");
                        Debug.Break();
                    }

                    if (conditionDesc._randomChance == 1)
                    {
                        ret = true;
                        break;
                    }

                    System.Random random = new System.Random();
                    int randomNumber = random.Next(1, conditionDesc._randomChance + 1);

                    if (randomNumber == conditionDesc._randomChance)
                    {
                        ret = true;
                    }
                }
                break;

            case ConditionType.AI_InAttackRange:
                {
                    forcedValue = true;
                    CharacterScript enemyScript = _owner.GCST<EnemyAIScript>().GetCurrentEnemy();

                    if (enemyScript == null) 
                    {
                        ret = false;
                        break;
                    }

                    Vector3 enemyPosition = enemyScript.gameObject.transform.position;
                    Vector3 myPosition = gameObject.transform.position;
                    Vector3 distanceVector = enemyPosition - myPosition;

                    float characterHeight = _owner.GCST<CharacterAnimatorScript>().GetCharacterHeight();

                    if (Mathf.Abs(distanceVector.y) >= characterHeight / 2.0f)
                    {
                        ret = false;
                        break;
                    }

                    Vector2 planeDistanceVector = new Vector2(distanceVector.x, distanceVector.z);

                    float planeDistance = planeDistanceVector.magnitude;

                    float attackRange = _owner.GCST<EnemyAIScript>().GetChsingDistance();

                    if (planeDistance >= attackRange)
                    {
                        ret = false;
                        break;
                    }

                    ret = true;
                }
                break;

            case ConditionType.AI_StateCoolTimeCheck:
                {
                    EnemyAIScript ownerEnemyAIScript = _owner.GCST<EnemyAIScript>();
                    ret = ownerEnemyAIScript.FindCoolTimeCoroutine(nextStateAsset._linkedState._linkedState);
                }
                break;

            case ConditionType.AI_EnemyDegreeRange:
                {
                    Vector3 myPosition = gameObject.transform.position;
                    Vector3 enemyPosition = _owner.GCST<EnemyAIScript>().GetCurrentEnemy().transform.position;
                    Vector3 direction = enemyPosition - myPosition;
                    direction.y = 0.0f;
                    float betweenDeg = Vector3.Angle(direction.normalized, gameObject.transform.forward.normalized);

                    if (betweenDeg >= conditionDesc._degThreshould)
                    {
                        ret = false;
                        break;
                    }

                    ret = true;
                }
                break;

            case ConditionType.AI_EnemyIsExitst:
                {
                    ret = (_owner.GCST<EnemyAIScript>().GetCurrentEnemy() != null);
                }
                break;

            case ConditionType.AI_ReadiedAttackExist:
                {
                    StateGraphAsset aiAggressiveStateGraphAsset = _stateGraphes[(int)StateGraphType.AI_AggresiveGraph];

                    if (aiAggressiveStateGraphAsset == null)
                    {
                        Debug.Assert(false, "AI_ReadiedAttackExist 조건에는 반드시 Aggressive Graph 에셋이 필요합니다");
                        Debug.Break();
                        return false;
                    }

                    ret = _owner.GCST<EnemyAIScript>().IsAnyAttackReadied(ref aiAggressiveStateGraphAsset);
                }
                break;

            case ConditionType.AI_ArriveInAttackRange:
                {
                    forcedValue = true;
                    
                    ret = _owner.GCST<EnemyAIScript>().IsInAttackRange(_owner.GCST<CharacterAnimatorScript>().GetCharacterHeight());
                }
                break;

            case ConditionType.AI_EnemyIsAttackState:
                {
                    CharacterScript enemyCharacterScript = _owner.GCST<EnemyAIScript>().GetCurrentEnemy();
                    if (enemyCharacterScript == null)
                    {
                        ret = false;
                        break;
                    }

                    StateContoller enemyStateController = _owner.GCST<StateContoller>();
                    if (enemyStateController == null)
                    {
                        ret = false;
                        break;
                    }

                    if (enemyStateController.GetCurrState()._myState._isAttackState == true)
                    {
                        ret = true;
                        break;
                    }

                    ret = false;
                }
                break;

            case ConditionType.SameConditionDiveringChance:
                {
                    if (_randomStateInstructIndex < 0)
                    {
                        //최근에 상태가 바뀐적이 있고, 랜덤을 계산한적이 없다 = 음수다
                        
                        int maxCount = 0;
                        //Interaction Point를 제외한 연결 상태들의 개수를 파악한다
                        foreach (LinkedStateAssetWrapper linkedStateAssetWrapper in _currLinkedStates)
                        {
                            if (linkedStateAssetWrapper._goalType == linkedStateAssetWrapper._fromType)
                            {
                                maxCount++;
                            }
                        }

                        //난수를 생성한다
                        _randomStateInstructIndex = UnityEngine.Random.Range(0, maxCount);
                    }

                    if (_randomStateTryCount > _randomStateInstructIndex)
                    {
                        //여러개중 하나는 반드시 성공해야 합니다
                        Debug.Assert(false, "무작위 적중 로직이 이상합니다");
                        Debug.Break();
                        return false;
                    }

                    ret = (_randomStateTryCount == _randomStateInstructIndex);

                    if (ret == false)
                    {
                        _randomStateTryCount++;
                    }
                }
                break;

            case ConditionType.AI_TryRandomChance:
                {
                    float randomValue = UnityEngine.Random.Range(0.0f, 100.0f); // 0.0f ~ 100.0f 범위의 float 생성

                    if (nextStateAsset._linkedState._linkedState._myState._isAIState == false)
                    {
                        Debug.Assert(false, "AI_TryRandomChance 조건인데 해당 값이 false입니다");
                        Debug.Break();
                        return false;
                    }

                    if (_failedRandomChanceState.Contains(nextStateAsset._linkedState._linkedState) == true)
                    {
                        ret = false;
                        break;
                    }

                    if (nextStateAsset._linkedState._linkedState._myState._aiStateDesc._randomChancePercentage < 0.0f)
                    {
                        Debug.Assert(false, "AI_TryRandomChance 조건에는 확률에 유효한값이 있어야합니다" + nextStateAsset._linkedState._linkedState.name);
                        Debug.Break();
                        return false;
                    }

                    ret = (randomValue <= nextStateAsset._linkedState._linkedState._myState._aiStateDesc._randomChancePercentage);

                    if (ret == false)
                    {
                        _failedRandomChanceState.Add(nextStateAsset._linkedState._linkedState);
                    }
                }
                break;

            case ConditionType.AI_StateTimeThreshould:
                {
                    if (currstateAsset._myState._isAIState == false)
                    {
                        Debug.Assert(false, "AI_StateTimeThreshould 조건인데 해당 값이 false입니다");
                        Debug.Break();
                        return false;
                    }

                    if (currstateAsset._myState._aiStateDesc._stateTimeThreshould < 0.0f)
                    {
                        Debug.Assert(false, "AI_StateTimeThreshould 조건에는 상한값에 유효한값이 있어야합니다");
                        Debug.Break();
                        return false;
                    }

                    ret = (_currStateTime >= currstateAsset._myState._aiStateDesc._stateTimeThreshould);
                }
                break;

            case ConditionType.IsLockOnTarget:
                {
                    AimScript2 ownerAimScript = _owner.GCST<AimScript2>();

                    GameObject lockOnObject = ownerAimScript.GetLockOnObject();

                    ret = (lockOnObject != null);
                }
                break;

            case ConditionType.IsHoldingWeaponKey:
                {
                    StateGraphType nextStateGraphType = StateGraphType.End;

                    if (UIManager.Instance.IsConsumeInput() == true)
                    {
                        ret = false;
                        break;
                    }

                    if ((nextStateAsset._fromType == StateGraphType.WeaponState_RightGraph || nextStateAsset._fromType == StateGraphType.WeaponState_LeftGraph) &&
                        (nextStateAsset._goalType == StateGraphType.WeaponState_RightGraph || nextStateAsset._goalType == StateGraphType.WeaponState_LeftGraph))
                    {
                        Debug.Assert(false, "goalType, fromeType이 둘다 무기일수는 없습니다.");
                        Debug.Break();
                        forcedValue = true;
                        ret = false;
                        break;
                    }


                    if (nextStateAsset._fromType == StateGraphType.WeaponState_RightGraph || nextStateAsset._fromType == StateGraphType.WeaponState_LeftGraph)
                    {
                        nextStateGraphType = nextStateAsset._fromType;
                    }
                    else
                    {
                        nextStateGraphType = nextStateAsset._goalType;
                    }

                    KeyCode targetKeyCode = (nextStateGraphType == StateGraphType.WeaponState_RightGraph)
                        ? KeyCode.Mouse1
                        : KeyCode.Mouse0;

                    bool isRightWeapon = (nextStateGraphType == StateGraphType.WeaponState_RightGraph);
                    
                    AnimatorLayerTypes targetLayer = (isRightWeapon == true)
                        ? AnimatorLayerTypes.RightHand
                        : AnimatorLayerTypes.LeftHand;

                    if (_owner.GetCurrentWeapon(targetLayer) == null)
                    {
                        ret = false;
                        break;
                    }

                    ret = Input.GetKey(targetKeyCode);
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
        /*-----------------------------------------------------------
        |TODO| 뭔가의 현재 공격을 시도하지 않았습니다 로직...구현하자
        -----------------------------------------------------------*/
        /*-----------------------------------------------------------
        if (false) return false;
        -----------------------------------------------------------*/

        if (_nextComboReady == false)
        {
            return false;
        }

        bool ret = false;

        int stateGraphType = (isRightSided == true)
            ? (int)StateGraphType.WeaponState_RightGraph
            : (int)StateGraphType.WeaponState_LeftGraph;


        if (_stateGraphes[stateGraphType] == null)
        {
            return false;
        }

        ret = CommandCheck(conditionDesc, isRightSided);

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

        WeaponGrabFocus ownerGrabType = _owner.GetGrabFocusType();
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

                if (weaponComboType >= WeaponUseType.WeaponUseStart_DontUseThis && weaponComboType <= WeaponUseType.WeaponUseEnd_DontUseThis)
                {
                    if (recordedType >= ComboCommandKeyType.TargetingBack && recordedType <= ComboCommandKeyType.TargetingRight)
                    { return false; }

                    ComboCommandKeyType targetType = KeyConvert(weaponComboType, ownerGrabType, isRightSided);

                    if (targetType <= ComboCommandKeyType.TargetingRight)
                    {
                        return false; //치환에 실패했다
                    }

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

            case WeaponUseType.OppositeMainUse:
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //한손, 한손 잡고있었다.
                        {
                            if (isRightHandWeapon == true)
                            {
                                convertedRet = ComboCommandKeyType.LeftClick;
                            }
                            else
                            {
                                convertedRet = ComboCommandKeyType.RightClick;
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

            default:
                break;
        }

        return convertedRet;
    }
}
