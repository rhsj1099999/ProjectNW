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
using System.Security.Policy;
using Unity.Mathematics;
using static MyUtil;

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
    CharacterRotate,
    AI_CharacterRotateToEnemy,
    AI_ChaseToEnemy,
    AI_ForcedLookAtEnemy,
    AI_ReArrangeStateGraph,
    AI_UpdateAttackRange, //Chase�� ���, �޷����ٰ� ���Ÿ� ������ �����ϸ� �ָ��� ���缭���Ѵ�.
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
    ReturnFalse, //������ False�� ��ȯ�մϴ�.
    EnterStatetimeThreshould,
    AI_CheckEnemyInMySight,
    AI_Check_I_CAN_ATTACK_MY_ENEMY,
    AI_RandomChance,

    AI_InAttackRange,   //������ ��Ÿ�
    AI_StateCoolTimeCheck, //���� ��Ÿ��
    AI_EnemyDegreeRange, //��������

    AI_EnemyIsExitst,
    AI_ReadiedAttackExist, //�ϳ��� �غ�� ������ �ֳ�? ������ Chase �Ұ���. �غ�� �ƴµ� ��Ÿ��� ª���Ŵϱ�
    AI_ArriveInAttackRange,
    AI_EnemyIsAttackState, //���� �������Դϱ�
    SameConditionDiveringChance,
    AI_TryRandomChance, // n �ۼ�Ʈ�� Ȯ���� ���¸� �õ��Ϸ� �մϴ�. �����ϸ� ���� �ɷ�, ���°� �ٲ�� ������ �õ��� �� �����ϴ�.
    AI_StateTimeThreshould, // n �ۼ�Ʈ�� Ȯ���� ���¸� �õ��Ϸ� �մϴ�. �����ϸ� ���� �ɷ�, ���°� �ٲ�� ������ �õ��� �� �����ϴ�.
    
    IsLockOnTarget, //������ �� �����Դϴ�.

    IsHoldingWeaponKey, //���� ���Ű(��Ŭ, ����Ŭ)�� ������ �ֽ��ϴ�. ���⵵ ��� �ֽ��ϴ�.
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
    Guard_Lvl_0, //�Ϲ� ����
    Guard_Lvl_1, //�ɾƼ� ����
    Guard_Lvl_2, //�ɾƼ� ���� + ���

    Hit_Lvl_0, //�����ϴ�����
    Hit_Lvl_1, //�ڼ��� �������� ��û�Ÿ�
    Hit_Lvl_2, //����

    Blocked_Reaction, //�� ������
    Blocked_Sliding, //�̲�����
    Blocked_Crash, //�ڼ��� ������

    DieNormal,
    DieThrow,

    End,
}

public enum StateActionCoroutineType
{
    ChangeToIdle,
    StateChangeReady,
    DeadCall,
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
    public bool _stateLocked = false; //�ܺο��� ���º����� ���͵� �ðڴ�.
    /*------------------------------------------------------------------------------
    |NOTI| !_isAttackState = _isLocoMotionToAttackAction�� �����ϰŰ����� ������ �ƴϴ�
    ------------------------------------------------------------------------------*/
    public bool _isBlockState = false; //��������Դϴ�.


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

    //public List<AnimationClip> _bsAnimations = null; //���� �ִϸ��̼�
    //public List<AnimationClip> _asAnimations = null; //�ĵ� �ִϸ��̼�
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
        public CharacterScript _owner = null;
        public InputController _ownerInputController = null;
        public CharacterMoveScript2 _ownerMoveScript = null;
        public CharacterController _ownerCharacterComponent = null;
        public CharacterAnimatorScript _ownerCharacterAnimatorScript = null;
        public AINavigationScript _ownerNavigationScript = null;
        public EnemyAIScript _ownerEnemyAIScript = null;
        public AimScript2 _ownerAimScript = null;
        public CharacterColliderScript _ownerCharacterColliderScript = null;
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

    private StateAsset _currState = null;
    private StateAsset _prevState = null;
    public StateAsset GetCurrState() { return _currState; }

    [SerializeField] private float _stateChangeTime = 0.085f;
    private bool _stateChangeCoroutineStarted = false;

    [SerializeField] private List<StateGraphAsset> _initialStateGraphes = new List<StateGraphAsset>();
    private List<StateGraphAsset> _stateGraphes = new List<StateGraphAsset>();
    private StateGraphType _currentGraphType = StateGraphType.LocoStateGraph;
    private StateGraphType _previousGraphType = StateGraphType.LocoStateGraph;
    public StateGraphType GetCurrStateGraphType() { return _currentGraphType; }
    public StateGraphAsset GetCurrStateGraph() { return _stateGraphes[(int)_currentGraphType]; }
    public List<StateGraphAsset> GetStateGraphes() { return _stateGraphes; }

    private List<StateActionCoroutineWrapper> _stateActionCoroutines = new List<StateActionCoroutineWrapper>();

    private float _currStateTime = 0.0f;
    private float _prevStateTime = 0.0f;
    private StateContollerComponentDesc _ownerStateControllingComponent = new StateContollerComponentDesc();
    List<LinkedStateAssetWrapper> _currLinkedStates = new List<LinkedStateAssetWrapper>();


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


    //---CoroutineTimer ...
    /*---------------------------------------
    |TODO| ���ִ� ������ �ѹ� �����غ���
    ---------------------------------------*/



    public StateAsset GetMyIdleStateAsset()
    {
        return _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState;
    }


    private void Awake()
    {
        CharacterScript characterScript = GetComponent<CharacterScript>();

        _ownerStateControllingComponent._owner = characterScript;

        if (_ownerStateControllingComponent._owner == null)
        {
            Debug.Assert(false, "�� ������Ʈ�� CharacterScript�� �ݵ�� �ʿ�� �մϴ�");
            Debug.Break();
        }

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
            return; //�ش� �׷����� ����.
        }

        StateAsset targetAsset = targetGraphAsset.GetRepresentStateAsset(representType);

        if (targetAsset == null) 
        {
            return; //�׷����� �ش� ���°� ����.
        }

        ChangeState(graphType, targetAsset);

        ReadyLinkedStates(graphType, targetAsset, true);
    }

    public void TryChangeState(StateGraphType graphType, StateAsset targetAsset)
    {
        StateGraphAsset targetGraphAsset = _stateGraphes[(int)graphType];

        if (targetGraphAsset == null)
        {
            return; //�ش� �׷����� ����.
        }

        ChangeState(graphType, targetAsset);
    }

    private void StatedWillBeChanged()
    {
        _currStateTime = 0.0f;
        _prevStateTime = 0.0f;
        StopAllCoroutines();
        _failedRandomChanceState.Clear();
    }

    private void ChangeState(StateGraphType nextGraphType, StateAsset nextState)
    {
        StatedWillBeChanged();

        if (gameObject.name != "Zombie")
        {
            Debug.Log("State Changed : " + nextState.name);
        }

        if (_currState != null)
        {
            DoActions(_currState._myState._ExitStateActionTypes);
        }

        _prevState = _currState;
        _previousGraphType = _currentGraphType;

        _currentGraphType = nextGraphType;




        _currState = nextState;

        _randomStateInstructIndex = -1;
        _randomStateTryCount = 0;
        _failedRandomChanceState.Clear();
        

        _ownerStateControllingComponent._owner.StateChanged(_currState);

        AllStopCoroutine();

        if (_currentGraphType == StateGraphType.WeaponState_RightGraph)
        {
            _ownerStateControllingComponent._owner.SetLatestWeaponUse(true);
        }
        else if (_currentGraphType == StateGraphType.WeaponState_LeftGraph)
        {
            _ownerStateControllingComponent._owner.SetLatestWeaponUse(false);
        }

        DoActions(_currState._myState._EnterStateActionTypes);
    }



    public int SubAnimationStateIndex(StateAsset nextState)
    {
        SubAnimationStateMachine.CalculateLogic logic = nextState._myState._subAnimationStateMachine._calculateLogic;

        int ret = -1;

        switch (logic)
        {
            case SubAnimationStateMachine.CalculateLogic.MoveDesiredDirection:
                {
                    Vector3 moveDesiredDir = _ownerStateControllingComponent._ownerInputController._pr_directionByInput;
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

                    if (_ownerStateControllingComponent._ownerCharacterAnimatorScript.GetCurrActivatedAnimator().GetBool("IsMirroring") == true)
                    {
                        ret = 8 - ret;
                    }
                }
                


                break;

            default:
                {
                    Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
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

        //Interaction Point�� ���� �˻��ؾ��ϱ� ������ ���� ��´�.
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

        //����� ���������� ����
        {
            _currLinkedStates.Sort((a, b) => b._hardness.CompareTo(a._hardness));
        }

        //InGraphState�� ��´�
        {
            List<LinkedStateAsset> linkedStates = _stateGraphes[(int)currentGraphType].GetGraphStates()[currState];
            foreach (var linkedState in linkedStates)
            {
                _currLinkedStates.Add(new LinkedStateAssetWrapper(currentGraphType, currentGraphType, linkedState, null));
            }
        }
    }





    public StateAsset CheckChangeState_Recursion2(out StateGraphType nextGraphType) //���� ���¸� �����Ҷ����� ��������� ������ �Լ�
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
                Debug.Assert(false, "���°� ��� �ٲ���մϴ�");
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
        Debug.Assert(_currState != null, "������Ʈ null�Դϴ�");

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
            _ownerStateControllingComponent._ownerEnemyAIScript.AddCoolTimeCoroutine(_currState);
        }

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
                        Vector3 characterInputDir = _ownerStateControllingComponent._ownerInputController._pr_directionByInput;
                        characterInputDir = _ownerStateControllingComponent._ownerMoveScript.GetDirectionConvertedByCamera(characterInputDir);
                        _ownerStateControllingComponent._ownerMoveScript.CharacterRotate(characterInputDir, 1.0f);
                        _ownerStateControllingComponent._ownerMoveScript.CharacterMove(characterInputDir, 1.0f);
                    }
                    break;

                case StateActionType.Attack:
                    break;

                case StateActionType.SaveLatestVelocity:
                    break;

                case StateActionType.Jump:
                    {
                        if (gameObject.name == "Zombie")
                        {
                            int a = 10;

                        }
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


                        AnimationClip currentAnimationClip = (_currState._myState._isSubAnimationStateMachineExist == true) 
                            ?_ownerStateControllingComponent._ownerCharacterAnimatorScript.GetCurrAnimationClip()
                            :_currState._myState._stateAnimationClip;


                        AnimationHipCurve animationHipCurve = ResourceDataManager.Instance.GetHipCurve(currentAnimationClip);

                        Vector3 currentUnityLocalHip = new Vector3
                        (
                            animationHipCurve._animationHipCurveX.Evaluate(currentSecond),
                            animationHipCurve._animationHipCurveY.Evaluate(currentSecond),
                            animationHipCurve._animationHipCurveZ.Evaluate(currentSecond)
                        );

                        if (prevSecond > currentSecond)//�ִϸ��̼��� �ٲ��? ���Ű� �� ũ��
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

                        //Root ����� y���� �𵨿� ����...AnimationClip�� BakeIntoPose�� �ִ�
                        {
                            Vector3 modelLocalPosition = _ownerStateControllingComponent._ownerCharacterAnimatorScript.GetCurrActivatedModelObject().transform.localPosition;
                            modelLocalPosition.y = worldDelta.y;
                            _ownerStateControllingComponent._ownerCharacterAnimatorScript.GetCurrActivatedModelObject().transform.localPosition = modelLocalPosition;
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
                        _ownerStateControllingComponent._ownerCharacterAnimatorScript.WeaponLayerChange_EnterAttack
                            (
                            _ownerStateControllingComponent._owner.GetGrabFocusType(),
                            _currState,
                            _ownerStateControllingComponent._owner.GetLatestWeaponUse()
                            );
                    }
                    break;

                case StateActionType.CalculateWeaponLayer_ExitAttack:
                    {
                        _ownerStateControllingComponent._ownerCharacterAnimatorScript.WeaponLayerChange_ExitAttack
                            (
                            _ownerStateControllingComponent._owner.GetGrabFocusType(),
                            _currState,
                            _ownerStateControllingComponent._owner.GetLatestWeaponUse()
                            );
                    }
                    break;

                case StateActionType.DummyState_EnterLocoStateGraph:
                    {
                        ChangeState(StateGraphType.LocoStateGraph, _stateGraphes[(int)StateGraphType.LocoStateGraph].GetEntryStates()[0]._linkedState);
                    }
                    break;

                case StateActionType.AddCoroutine_ChangeToIdleState:
                    {
                        AddStateActionCoroutine(StateActionCoroutineType.ChangeToIdle);
                    }
                    break;

                case StateActionType.AddCoroutine_StateChangeReady:
                    {
                        AddStateActionCoroutine(StateActionCoroutineType.StateChangeReady);
                    }
                    break;

                case StateActionType.CharacterRotate:
                    {
                        Vector3 characterInputDir = _ownerStateControllingComponent._ownerInputController._pr_directionByInput;
                        characterInputDir = _ownerStateControllingComponent._ownerMoveScript.GetDirectionConvertedByCamera(characterInputDir);
                        _ownerStateControllingComponent._ownerMoveScript.CharacterRotate(characterInputDir, 1.0f);
                    }
                    break;

                case StateActionType.AI_CharacterRotateToEnemy:
                    {
                        Vector3 enemyPosition = _ownerStateControllingComponent._ownerEnemyAIScript.GetCurrentEnemy().gameObject.transform.position;
                        Vector3 myPosition = transform.position;
                        Vector3 toEnemyDir = (enemyPosition - myPosition).normalized;
                        _ownerStateControllingComponent._ownerMoveScript.CharacterRotate(toEnemyDir, 1.0f);
                    }
                    break;

                case StateActionType.AI_ChaseToEnemy:
                    {
                        Vector3 myPosition = _ownerStateControllingComponent._owner.gameObject.transform.position;
                        Vector3 enemyPosition = _ownerStateControllingComponent._ownerEnemyAIScript.GetCurrentEnemy().gameObject.transform.position;
                        Vector3 toEnemyPosition = (enemyPosition - myPosition).normalized;

                        _ownerStateControllingComponent._ownerMoveScript.CharacterRotate(toEnemyPosition, 1.0f);
                        _ownerStateControllingComponent._ownerMoveScript.CharacterMove(toEnemyPosition, 1.0f);
                    }
                    break;

                case StateActionType.AI_ForcedLookAtEnemy:
                    {
                        Vector3 myPosition = _ownerStateControllingComponent._owner.gameObject.transform.position;
                        Vector3 enemyPosition = _ownerStateControllingComponent._ownerEnemyAIScript.GetCurrentEnemy().gameObject.transform.position;
                        Vector3 dirToEnemy = (enemyPosition - myPosition).normalized;

                        _ownerStateControllingComponent._owner.gameObject.transform.LookAt(dirToEnemy + myPosition);
                    }
                    break;

                case StateActionType.AI_ReArrangeStateGraph:
                    {
                        EnemyAIScript aiScript = _ownerStateControllingComponent._ownerEnemyAIScript;
                        aiScript.ReArrangeStateGraph(_currLinkedStates, this, _currState);
                    }
                    break;

                case StateActionType.AI_UpdateAttackRange:
                    {
                        EnemyAIScript ownerEnemyAIScript = _ownerStateControllingComponent._ownerEnemyAIScript;
                        StateGraphAsset aggressiveStateAsset = _stateGraphes[(int)StateGraphType.AI_AggresiveGraph];
                        if (aggressiveStateAsset == null)
                        {
                            Debug.Assert(false, "AI_UpdateAttackRange�ൿ���� �ݵ�� Aggressive StateGraph�� �־�� �մϴ�");
                            Debug.Break();
                            return;
                        }
                        ownerEnemyAIScript.UpdateChasingDistance(ref aggressiveStateAsset);
                    }
                    break;

                case StateActionType.Move_WithOutRotate:
                    {
                        Vector3 characterInputDir = _ownerStateControllingComponent._ownerInputController._pr_directionByInput;
                        characterInputDir = _ownerStateControllingComponent._ownerMoveScript.GetDirectionConvertedByCamera(characterInputDir);
                        _ownerStateControllingComponent._ownerMoveScript.CharacterMove_NoSimilarity(characterInputDir, 1.0f);
                    }
                    break;


                case StateActionType.LookAtLockOnTarget:
                    {
                        AimScript2 ownerAimScript = _ownerStateControllingComponent._ownerAimScript;

                        if (ownerAimScript.GetLockOnObject() == null)
                        {
                            continue;
                        }

                        Vector3 targetPosition = ownerAimScript.GetLockOnObject().transform.position;
                        Vector3 ownerPosition = gameObject.transform.position;
                        Vector3 ownerToTargetPlaneVector = (targetPosition - ownerPosition);
                        ownerToTargetPlaneVector.y = 0.0f;
                        ownerToTargetPlaneVector = ownerToTargetPlaneVector.normalized;
                        gameObject.transform.LookAt(ownerToTargetPlaneVector + ownerPosition);
                    }
                    break;

                case StateActionType.RotateToLockOnTarget:
                    {
                        AimScript2 ownerAimScript = _ownerStateControllingComponent._ownerAimScript;
                        Vector3 targetPosition = ownerAimScript.GetLockOnObject().transform.position;
                        Vector3 ownerPosition = gameObject.transform.position;
                        Vector3 ownerToTargetPlaneVector = (targetPosition - ownerPosition);
                        ownerToTargetPlaneVector.y = 0.0f;
                        ownerToTargetPlaneVector = ownerToTargetPlaneVector.normalized;

                        _ownerStateControllingComponent._ownerMoveScript.CharacterRotate(ownerToTargetPlaneVector);
                    }
                    break;

                case StateActionType.RootMove_WithOutRotate:
                    {
                        float currentSecond = MyUtil.FloatMod(_currStateTime, _currState._myState._stateAnimationClip.length);
                        float prevSecond = MyUtil.FloatMod(_prevStateTime, _currState._myState._stateAnimationClip.length);


                        AnimationClip currentAnimationClip = (_currState._myState._isSubAnimationStateMachineExist == true)
                            ? _ownerStateControllingComponent._ownerCharacterAnimatorScript.GetCurrAnimationClip()
                            : _currState._myState._stateAnimationClip;


                        AnimationHipCurve animationHipCurve = ResourceDataManager.Instance.GetHipCurve(currentAnimationClip);

                        Vector3 currentUnityLocalHip = new Vector3
                        (
                            animationHipCurve._animationHipCurveX.Evaluate(currentSecond),
                            animationHipCurve._animationHipCurveY.Evaluate(currentSecond),
                            animationHipCurve._animationHipCurveZ.Evaluate(currentSecond)
                        );

                        if (prevSecond > currentSecond)//�ִϸ��̼��� �ٲ��? ���Ű� �� ũ��
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

                        //Root ����� y���� �𵨿� ����...AnimationClip�� BakeIntoPose�� �ִ�
                        {
                            Vector3 modelLocalPosition = _ownerStateControllingComponent._ownerCharacterAnimatorScript.GetCurrActivatedModelObject().transform.localPosition;
                            modelLocalPosition.y = worldDelta.y;
                            _ownerStateControllingComponent._ownerCharacterAnimatorScript.GetCurrActivatedModelObject().transform.localPosition = modelLocalPosition;
                        }

                        worldDelta.y = 0.0f;
                        _ownerStateControllingComponent._ownerCharacterComponent.Move(worldDelta);
                    }
                    break;

                case StateActionType.CharacterRotateToCameraLook:
                    {
                        Vector3 cameraLook = Camera.main.transform.forward;
                        cameraLook.y = 0.0f;
                        _ownerStateControllingComponent._ownerMoveScript.CharacterRotate(cameraLook.normalized, 1.0f);
                    }
                    break;

                case StateActionType.EnterGunAiming:
                    {
                        bool isRightWeapon = _ownerStateControllingComponent._owner.GetLatestWeaponUse();

                        if (isRightWeapon == true)
                        {
                            _ownerStateControllingComponent._owner._isRightWeaponAimed = true;
                        }
                        else
                        {
                            _ownerStateControllingComponent._owner._isLeftWeaponAimed = true;
                        }
                    }
                    break;

                case StateActionType.ExitGunAiming:
                    {
                        bool isRightWeapon = (_currentGraphType == StateGraphType.WeaponState_RightGraph);

                        if (isRightWeapon == true)
                        {
                            _ownerStateControllingComponent._owner._isRightWeaponAimed = false;
                        }
                        else
                        {
                            _ownerStateControllingComponent._owner._isLeftWeaponAimed = false;
                        }
                    }
                    break;

                case StateActionType.Move_WithOutRotate_Gun:
                    {
                        AimScript2 ownerAimScript = _ownerStateControllingComponent._ownerAimScript;
                        GameObject lockOnTarget = ownerAimScript.GetLockOnObject();

                        if (lockOnTarget == null)
                        {
                            float currentSecond = MyUtil.FloatMod(_currStateTime, _currState._myState._stateAnimationClip.length);
                            float prevSecond = MyUtil.FloatMod(_prevStateTime, _currState._myState._stateAnimationClip.length);


                            AnimationClip currentAnimationClip = (_currState._myState._isSubAnimationStateMachineExist == true)
                                ? _ownerStateControllingComponent._ownerCharacterAnimatorScript.GetCurrAnimationClip()
                                : _currState._myState._stateAnimationClip;


                            AnimationHipCurve animationHipCurve = ResourceDataManager.Instance.GetHipCurve(currentAnimationClip);

                            Vector3 currentUnityLocalHip = new Vector3
                            (
                                animationHipCurve._animationHipCurveX.Evaluate(currentSecond),
                                animationHipCurve._animationHipCurveY.Evaluate(currentSecond),
                                animationHipCurve._animationHipCurveZ.Evaluate(currentSecond)
                            );

                            if (prevSecond > currentSecond)//�ִϸ��̼��� �ٲ��? ���Ű� �� ũ��
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

                            //Root ����� y���� �𵨿� ����...AnimationClip�� BakeIntoPose�� �ִ�
                            {
                                Vector3 modelLocalPosition = _ownerStateControllingComponent._ownerCharacterAnimatorScript.GetCurrActivatedModelObject().transform.localPosition;
                                modelLocalPosition.y = worldDelta.y;
                                _ownerStateControllingComponent._ownerCharacterAnimatorScript.GetCurrActivatedModelObject().transform.localPosition = modelLocalPosition;
                            }

                            worldDelta.y = 0.0f;
                            _ownerStateControllingComponent._ownerCharacterComponent.Move(worldDelta);
                        }
                        else 
                        {
                            Vector3 characterInputDir = _ownerStateControllingComponent._ownerInputController._pr_directionByInput;
                            characterInputDir = _ownerStateControllingComponent._ownerMoveScript.GetDirectionConvertedByCamera(characterInputDir);
                            _ownerStateControllingComponent._ownerMoveScript.CharacterMove_NoSimilarity(characterInputDir, 1.0f);
                        }
                    }
                    break;

                case StateActionType.LookAtLockOnTarget_Gun:
                    {
                        AimScript2 ownerAimScript = _ownerStateControllingComponent._ownerAimScript;
                        GameObject lockOnTarget = ownerAimScript.GetLockOnObject();

                        if (lockOnTarget == null)
                        {
                            Vector3 cameraLook = Camera.main.transform.forward;
                            cameraLook.y = 0.0f;
                            _ownerStateControllingComponent._ownerMoveScript.CharacterRotate(cameraLook.normalized, 1.0f);
                        }
                        else
                        {
                            Vector3 targetPosition = ownerAimScript.GetLockOnObject().transform.position;
                            Vector3 ownerPosition = gameObject.transform.position;
                            Vector3 ownerToTargetPlaneVector = (targetPosition - ownerPosition);
                            ownerToTargetPlaneVector.y = 0.0f;
                            ownerToTargetPlaneVector = ownerToTargetPlaneVector.normalized;
                            gameObject.transform.LookAt(ownerToTargetPlaneVector + ownerPosition);
                        }
                    }
                    break;

                case StateActionType.AnimationAttack:
                    {
                        List<AnimationAttackFrameAsset.AttackFrameDesc> frameDesc = 
                        AnimationAttackManager.Instance.GetAttackFrameDesc(_currState._myState._stateAnimationClip);

                        if (frameDesc == null)
                        {
                            Debug.Assert(false, "FrameData�� �����ϴ�.");
                            Debug.Break();
                        }   
                        else
                        {
                            _ownerStateControllingComponent._ownerCharacterColliderScript.ColliderWork(frameDesc, _currState);
                        }
                    }
                    break;

                case StateActionType.AttackLookAtLockOnTarget:
                    {
                        AimScript2 ownerAimScript = _ownerStateControllingComponent._ownerAimScript;
                        GameObject lockOnTarget = ownerAimScript.GetLockOnObject();

                        if (lockOnTarget == null)
                        {
                            Vector3 convertedDirection = _ownerStateControllingComponent._ownerMoveScript.GetDirectionConvertedByCamera(_ownerStateControllingComponent._ownerInputController._pr_directionByInput);
                            gameObject.transform.LookAt(gameObject.transform.position + convertedDirection);
                        }
                        else
                        {
                            Vector3 targetPosition = ownerAimScript.GetLockOnObject().transform.position;
                            Vector3 ownerPosition = gameObject.transform.position;
                            Vector3 ownerToTargetPlaneVector = (targetPosition - ownerPosition);
                            ownerToTargetPlaneVector.y = 0.0f;
                            ownerToTargetPlaneVector = ownerToTargetPlaneVector.normalized;
                            gameObject.transform.LookAt(ownerToTargetPlaneVector + ownerPosition);
                        }
                    }
                    break;

                case StateActionType.AddCoroutine_DeadCall:
                    {
                        AddStateActionCoroutine(StateActionCoroutineType.DeadCall);
                    }
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

            case StateActionCoroutineType.DeadCall:
                {
                    newCoroutineWrapper._timeTarget = _currState._myState._stateAnimationClip.length;
                    newCoroutineWrapper._runningCoroutine = StartCoroutine(DeadCallCoroutine(newCoroutineWrapper));
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
                ReadyLinkedStates(StateGraphType.LocoStateGraph, GetMyIdleStateAsset(), false);
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
                _ownerStateControllingComponent._owner.DeadCall();
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
                    float animationLength = currstateAsset._myState._stateAnimationClip.length;
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
                    //{ return false; } //���⸦ �������� �ʽ��ϴ�.

                    //if (ownerCurrWeapon._weaponType == conditionDesc._weaponTypeGoal)
                    //{ return false; } //�����ִ� ���Ⱑ ��ǥ���� �ٸ��ϴ�.

                    return true;
                }

            case ConditionType.AnimationFrame:
                {
                    StateDesc currStateDesc = currstateAsset._myState;

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
                    if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                    {
                        _ownerStateControllingComponent._ownerEnemyAIScript = _ownerStateControllingComponent._owner.GetComponent<EnemyAIScript>();
                        if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                        {
                            Debug.Assert(false, "AICheckEnemyInMySight ���ǿ� �ʿ��� ������Ʈ�� �����ϴ�");
                            Debug.Break();
                            ret = false;
                        }
                    }

                    forcedValue = true;

                    ret = _ownerStateControllingComponent._ownerEnemyAIScript.InBattleCheck();
                }
                break;

            case ConditionType.AI_Check_I_CAN_ATTACK_MY_ENEMY:
                {
                    if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                    {
                        _ownerStateControllingComponent._ownerEnemyAIScript = _ownerStateControllingComponent._owner.GetComponent<EnemyAIScript>();
                        if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                        {
                            Debug.Assert(false, "AICheckEnemyInMySight ���ǿ� �ʿ��� ������Ʈ�� �����ϴ�");
                            Debug.Break();
                            ret = false;
                        }
                    }

                    forcedValue = true;

                    //ret = _ownerStateControllingComponent._ownerEnemyAIScript.InAttackRangeCheck(_currLinkedStates);
                    ret = _ownerStateControllingComponent._ownerEnemyAIScript.InAttackRangeCheck(nextStateAsset._linkedState._linkedState);
                }
                break;

            case ConditionType.AI_RandomChance:
                {
                    forcedValue = true;

                    if (conditionDesc._randomChance <= 0)
                    {
                        Debug.Assert(false, "Random Ȯ���� �и�� 1 �̻��̿����մϴ�");
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
                    CharacterScript enemyScript = _ownerStateControllingComponent._ownerEnemyAIScript.GetCurrentEnemy();

                    if (enemyScript == null) 
                    {
                        ret = false;
                        break;
                    }

                    Vector3 enemyPosition = enemyScript.gameObject.transform.position;
                    Vector3 myPosition = gameObject.transform.position;
                    Vector3 distanceVector = enemyPosition - myPosition;

                    if (_ownerStateControllingComponent._ownerCharacterComponent == null)
                    {
                        //�ѹ��� ã�ƺ���.
                        _ownerStateControllingComponent._ownerCharacterComponent = gameObject.GetComponentInChildren<CharacterController>();

                        if (_ownerStateControllingComponent._ownerCharacterComponent == null)
                        {
                            Debug.Assert(false, "�ش� ���ǿ��� CharacterController�� �־���մϴ�");
                            Debug.Break();
                            ret = false;
                            break;
                        }
                    }

                    float characterHeight = _ownerStateControllingComponent._ownerCharacterComponent.height;

                    if (Mathf.Abs(distanceVector.y) >= characterHeight / 2.0f)
                    {
                        ret = false;
                        break;
                    }

                    Vector2 planeDistanceVector = new Vector2(distanceVector.x, distanceVector.z);

                    float planeDistance = planeDistanceVector.magnitude;

                    float attackRange = _ownerStateControllingComponent._ownerEnemyAIScript.GetChsingDistance();

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
                    EnemyAIScript ownerEnemyAIScript = _ownerStateControllingComponent._ownerEnemyAIScript;
                    ret = ownerEnemyAIScript.FindCoolTimeCoroutine(nextStateAsset._linkedState._linkedState);
                }
                break;

            case ConditionType.AI_EnemyDegreeRange:
                {
                    Vector3 myPosition = gameObject.transform.position;
                    Vector3 enemyPosition = _ownerStateControllingComponent._ownerEnemyAIScript.GetCurrentEnemy().transform.position;
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
                    if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                    {
                        _ownerStateControllingComponent._ownerEnemyAIScript = _ownerStateControllingComponent._owner.gameObject.GetComponentInChildren<EnemyAIScript>();

                        if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                        {
                            Debug.Assert(false, "EnemyAIScript�� �����ϴ�");
                            Debug.Break();
                            ret = false;
                        }
                    }

                    ret = (_ownerStateControllingComponent._ownerEnemyAIScript.GetCurrentEnemy() != null);
                }
                break;

            case ConditionType.AI_ReadiedAttackExist:
                {
                    if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                    {
                        _ownerStateControllingComponent._ownerEnemyAIScript = _ownerStateControllingComponent._owner.gameObject.GetComponentInChildren<EnemyAIScript>();

                        if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                        {
                            Debug.Assert(false, "EnemyAIScript�� �����ϴ�");
                            Debug.Break();
                            ret = false;
                        }
                    }

                    StateGraphAsset aiAggressiveStateGraphAsset = _stateGraphes[(int)StateGraphType.AI_AggresiveGraph];

                    if (aiAggressiveStateGraphAsset == null)
                    {
                        Debug.Assert(false, "AI_ReadiedAttackExist ���ǿ��� �ݵ�� Aggressive Graph ������ �ʿ��մϴ�");
                        Debug.Break();
                        return false;
                    }

                    ret = _ownerStateControllingComponent._ownerEnemyAIScript.IsAnyAttackReadied(ref aiAggressiveStateGraphAsset);
                }
                break;

            case ConditionType.AI_ArriveInAttackRange:
                {
                    forcedValue = true;

                    if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                    {
                        _ownerStateControllingComponent._ownerEnemyAIScript = _ownerStateControllingComponent._owner.gameObject.GetComponentInChildren<EnemyAIScript>();

                        if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                        {
                            Debug.Assert(false, "EnemyAIScript�� �����ϴ�");
                            Debug.Break();
                            ret = false;
                        }
                    }
                    
                    ret = _ownerStateControllingComponent._ownerEnemyAIScript.IsInAttackRange(_ownerStateControllingComponent._ownerCharacterComponent);
                }
                break;

            case ConditionType.AI_EnemyIsAttackState:
                {
                    if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                    {
                        _ownerStateControllingComponent._ownerEnemyAIScript = _ownerStateControllingComponent._owner.gameObject.GetComponentInChildren<EnemyAIScript>();

                        if (_ownerStateControllingComponent._ownerEnemyAIScript == null)
                        {
                            Debug.Assert(false, "AI_EnemyIsAttackState ���ǿ� EnemyAIScript�� �����ϴ�");
                            Debug.Break();
                            ret = false;
                        }
                    }

                    CharacterScript enemyCharacterScript = _ownerStateControllingComponent._ownerEnemyAIScript.GetCurrentEnemy();
                    if (enemyCharacterScript == null)
                    {
                        ret = false;
                        break;
                    }

                    StateContoller enemyStateController = enemyCharacterScript.GetStateContoller();
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
                        //�ֱٿ� ���°� �ٲ����� �ְ�, ������ ��������� ���� = ������
                        
                        int maxCount = 0;
                        //Interaction Point�� ������ ���� ���µ��� ������ �ľ��Ѵ�
                        foreach (LinkedStateAssetWrapper linkedStateAssetWrapper in _currLinkedStates)
                        {
                            if (linkedStateAssetWrapper._goalType == linkedStateAssetWrapper._fromType)
                            {
                                maxCount++;
                            }
                        }

                        //������ �����Ѵ�
                        _randomStateInstructIndex = UnityEngine.Random.Range(0, maxCount);
                    }

                    if (_randomStateTryCount > _randomStateInstructIndex)
                    {
                        //�������� �ϳ��� �ݵ�� �����ؾ� �մϴ�
                        Debug.Assert(false, "������ ���� ������ �̻��մϴ�");
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
                    float randomValue = UnityEngine.Random.Range(0.0f, 100.0f); // 0.0f ~ 100.0f ������ float ����

                    if (nextStateAsset._linkedState._linkedState._myState._isAIState == false)
                    {
                        Debug.Assert(false, "AI_TryRandomChance �����ε� �ش� ���� false�Դϴ�");
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
                        Debug.Assert(false, "AI_TryRandomChance ���ǿ��� Ȯ���� ��ȿ�Ѱ��� �־���մϴ�" + nextStateAsset._linkedState._linkedState.name);
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
                        Debug.Assert(false, "AI_StateTimeThreshould �����ε� �ش� ���� false�Դϴ�");
                        Debug.Break();
                        return false;
                    }

                    if (currstateAsset._myState._aiStateDesc._stateTimeThreshould < 0.0f)
                    {
                        Debug.Assert(false, "AI_StateTimeThreshould ���ǿ��� ���Ѱ��� ��ȿ�Ѱ��� �־���մϴ�");
                        Debug.Break();
                        return false;
                    }

                    ret = (_currStateTime >= currstateAsset._myState._aiStateDesc._stateTimeThreshould);
                }
                break;

            case ConditionType.IsLockOnTarget:
                {
                    if (_ownerStateControllingComponent._ownerAimScript == null)
                    {
                        _ownerStateControllingComponent._ownerAimScript = _ownerStateControllingComponent._owner.GetComponent<AimScript2>();
                        if (_ownerStateControllingComponent._ownerAimScript == null)
                        {
                            Debug.Assert(false, "IsLockOnTarget �����ε� AimScript�� �����ϴ�");
                            Debug.Break();
                            return false;
                        }
                    }

                    AimScript2 ownerAimScript = _ownerStateControllingComponent._ownerAimScript;

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
                        Debug.Assert(false, "goalType, fromeType�� �Ѵ� �����ϼ��� �����ϴ�.");
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

                    WeaponScript currWeapon = _ownerStateControllingComponent._owner.GetCurrentWeaponScript(isRightWeapon);

                    if (currWeapon == null)
                    {
                        ret = false;
                        break;
                    }

                    ret = Input.GetKey(targetKeyCode);
                }
                break;

            default:
                Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
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
        if (false/*������ ���� ������ �õ����� �ʾҽ��ϴ� ����...��������*/)
        {
            return false;
        }

        bool ret = false;

        if (_ownerStateControllingComponent._owner.GetCurrentWeaponScript(isRightSided) == null)
        {
            return false;
        }

        ret = CommandCheck(conditionDesc, isRightSided);

        //if (ret == true)
        //{
        //    _ownerStateControllingComponent._owner.SetLatestWeaponUse(isRightSided);
        //}

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

                if (weaponComboType >= WeaponUseType.WeaponUseStart_DontUseThis && weaponComboType <= WeaponUseType.WeaponUseEnd_DontUseThis)
                {
                    if (recordedType >= ComboCommandKeyType.TargetingBack && recordedType <= ComboCommandKeyType.TargetingRight)
                    { return false; }

                    ComboCommandKeyType targetType = KeyConvert(weaponComboType, ownerGrabType, isRightSided);

                    if (targetType <= ComboCommandKeyType.TargetingRight)
                    {
                        return false; //ġȯ�� �����ߴ�
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

            case WeaponUseType.MainUseUp: //������ �޺��� �ֻ�� Ŭ���̴�
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
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

                    case WeaponGrabFocus.RightHandFocused: //���⸦ ������ ������� ����ִ�.
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

            case WeaponUseType.SubUseUp:  //������ �޺��� ���� ��� Ŭ���̴�
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
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

                    case WeaponGrabFocus.RightHandFocused: //���⸦ ������ ������� ����ִ�.
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

            case WeaponUseType.SpecialUseUp:  //������ �޺��� Ư�� ��� Ŭ���̴�
                switch (ownerGrabType)
                {
                    case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
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

                    case WeaponGrabFocus.RightHandFocused: //���⸦ ������ ������� ����ִ�.
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
                    case WeaponGrabFocus.Normal: //�Ѽ�, �Ѽ� ����־���.
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

            default:
                break;
        }

        return convertedRet;
    }
}
