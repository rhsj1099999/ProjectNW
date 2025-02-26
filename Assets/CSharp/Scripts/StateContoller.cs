using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StateGraphAsset;
using static MyUtil;
using static AnimationFrameDataAsset;
using static LevelStatAsset;
using static StatScript;
using UnityEngine.Playables;
using MagicaCloth2;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using JetBrains.Annotations;
using static StateContoller;
using TMPro.EditorUtilities;

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
    AddCoroutine_BuffCheck,

    AddBuff,
    RemoveBuff,

    CharacterRevive,

    PostureReset,

    SpuriousDead, //���� ��ȯ�ÿ� �������� �𸣴� ��Ȳ... -> ������� �����ֽ��ϴ� ... ������� �Ͼ�ߵǴµ� dead�� true�� �ǹ��� ��Ȳ
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

    IsWeaponUseReady, //�ݹ��� �غ� �ƽ��ϴ� -> ���⸶�� ���� Override

    StateDeeper, //���� �����Ϸ��� ������ ������ �����߽��ϴ�.

    NeedStat,

    BuffAssetCount,

    IsDead,
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

    Groggy,

    Riposte_BackStab,
    Riposte_FrontStab,
    Riposte_Smash,


    Die_Riposte_BackStab,
    Die_Riposte_FrontStab,
    Die_Riposte_Smash,


    Hit_Riposte_BackStab,
    Hit_Riposte_FrontStab,
    Hit_Riposte_Smash,

    Stagger_Parried,

    AttackRecoil,

    End = 31,
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
public class ActiveStatNeedTarget
{
    public ActiveStat _statType = ActiveStat.End;
    public int _needAmount = -1;
    public bool _isConsume = false;
}

[Serializable]
public class PassiveStatNeedTarget
{
    public PassiveStat _statType = PassiveStat.End;
    public int _needAmount = -1;
    public bool _isConsume = false;
}

[Serializable]
public class NeedStatDesc
{
    public List<PassiveStatNeedTarget> _needPassiveStatList = new List<PassiveStatNeedTarget>();
    public List<ActiveStatNeedTarget> _needActiveStatList = new List<ActiveStatNeedTarget>();
}

[Serializable]
public class ConditionDesc
{
    public ConditionType _singleConditionType;
    public ItemAsset_Weapon.WeaponType _weaponTypeGoal;
    public List<KeyInputConditionDesc> _keyInputConditionTarget;
    public List<ComboKeyCommandDesc> _commandInputConditionTarget;
    //public FrameDataType _animationFrameDataType = FrameDataType.End;
    public float _comboStrainedTime = -1.0f; //n�� ���� �ϼ����Ѿ� �ϴ� �޺�
    public List<AnimatorLayerTypes> _mustNotBusyLayers;
    public int _mustNotBusyLayers_BitShift = 1;
    public float _enterStatetimeThreshould = 0.0f;
    public int _randomChance = 1;
    public float _degThreshould = 0.0f;
    public int _damageHitThreshould = 1;

    public BuffAssetBase _targetBuffAsset = null;
    public int _targetBuffAssetCount = -1;
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
    public List<AnimationClip> _stateAnimationClipOverride = new List<AnimationClip>();


    public bool _isAttackState = false;
    public bool _isAttackState_effectedBySpeed = false;
    public DamageDesc _attackDamageMultiply = null;


    public bool _isGuardState = false;
    public bool _canUseItem = false;
    public bool _isAIState = false;

    public AIStateDesc _aiStateDesc = null;
    public AIAttackStateDesc _aiAttackStateDesc = null;

    public List<BuffAssetBase> _stateBuffs_Add = new List<BuffAssetBase>();
    public List<BuffAssetBase> _stateBuffs_Remove = new List<BuffAssetBase>();

    public List<StateActionType> _EnterStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _inStateActionTypes = new List<StateActionType>();
    public List<StateActionType> _ExitStateActionTypes = new List<StateActionType>();
    public List<AdditionalBehaveType> _checkingBehaves = new List<AdditionalBehaveType>();

    public bool _isOverrideStateExsist = false;
    public List<StateAsset> _overrideStates = null;

    public bool _isSubBlendTreeExist = false;
    public SubBlendTreeAsset_2D _subBlendTree = null;

    public bool _isSubAnimationStateMachineExist = false;
    public SubAnimationStateMachine _subAnimationStateMachine = null;

    public bool _isNeedStat = false;
    public NeedStatDesc _needStat = new NeedStatDesc();

    public bool _isNeedStatLoopBreak = false;
    public NeedStatDesc _needStat_LoopBreak = new NeedStatDesc();
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
        public StateActionCoroutineWrapper(AEachFrameData frameData, StateActionCoroutineTimerBase timer, FrameDataWorkType workType)
        {
            _frameData = frameData;
            _workType = workType;
            _timer = timer;
        }

        public AEachFrameData _frameData = null;
        public StateActionCoroutineTimerBase _timer = null;
        public Coroutine _coroutine = null;
        public FrameDataWorkType _workType = FrameDataWorkType.End;
    }

    public abstract class StateActionCoroutineTimerBase 
    {
        public float _timeACC = 0.0f;
        public void Update(float deltaTime){_timeACC += deltaTime;}
        public abstract bool Check();
    }
    public class StateActionCoroutineTimer_UpTimer : StateActionCoroutineTimerBase
    {
        public StateActionCoroutineTimer_UpTimer(float upTimeTarget) {_timeTarget_Up = upTimeTarget;}
        float _timeTarget_Up = 0.0f;
        public override bool Check()
        {
            if (_timeACC >= _timeTarget_Up)
            {return true;}

            return false;
        }
    }
    public class StateActionCoroutineTimer_DownTimer : StateActionCoroutineTimerBase
    {
        public StateActionCoroutineTimer_DownTimer(float downTimeTarget) { _timeTarget_Down = downTimeTarget; }
        float _timeTarget_Down = 0.0f;
        public override bool Check()
        {
            if (_timeACC <= _timeTarget_Down)
            { return true; }

            return false;
        }
    }
    public class StateActionCoroutineTimer_BetweenTimer : StateActionCoroutineTimerBase
    {
        public StateActionCoroutineTimer_BetweenTimer(float upTimeTarget, float downTimeTarget) 
        {
            _timeTarget_Up = upTimeTarget;
            _timeTarget_Down = downTimeTarget;
        }

        float _timeTarget_Up = 0.0f;
        float _timeTarget_Down = 0.0f;
        public override bool Check()
        {
            if (_timeACC <= _timeTarget_Down && _timeACC >= _timeTarget_Up)
            { return true; }

            return false;
        }
    }

    public class LinkedStateAssetWrapper
    {
        public LinkedStateAssetWrapper(StateGraphType fromType, StateGraphType goalType, LinkedStateAsset linkedStateAsset, List<ConditionAssetWrapper> additionalCondition)
        {
            _fromType = fromType;
            _goalType = goalType;
            _linkedStateWrapper = linkedStateAsset;
            _additionalCondition = additionalCondition;
            _hardness = CalculateConditionWeight(_linkedStateWrapper._conditionAsset);

            if (_additionalCondition != null)
            {
                _hardness += CalculateConditionWeight(additionalCondition);
            }
        }

        public StateGraphType _fromType = StateGraphType.End;
        public StateGraphType _goalType = StateGraphType.End;
        public LinkedStateAsset _linkedStateWrapper = null;
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
            Debug.Assert(false, "�ּ��� LocoState (Idle�� �ִ�) �׷��� �ϳ��� �غ�ž� �մϴ�");
            Debug.Break();
        }

        foreach (StateGraphInitialWrapper stateGraphAssetWrapper in _initialStateGraphes)
        {
            StateGraphType type = stateGraphAssetWrapper._type;

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
            _initialStateGraphes = null; //���̻� �����ʴ´�!

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
    public void OverrideAnimationClip(int index) 
    {
        List<AnimationClip> overrides = _currState._myState._stateAnimationClipOverride;
        if (overrides.Count <= index)
        {
            return;
        }

        _owner.GCST<CharacterAnimatorScript>().AnimationOverride(overrides[index]);
    }



    private float _currStateTime = 0.0f;
    private float _prevStateTime = 0.0f;
    private bool _stateDeeper = false;
    public void StateDeeperCall() { _stateDeeper = true; }
    

    private List<LinkedStateAssetWrapper> _currLinkedStates = new List<LinkedStateAssetWrapper>();
    public IReadOnlyList<LinkedStateAssetWrapper> _CurrLinkedStated => _currLinkedStates;

    private List<StateActionCoroutineWrapper> _stateActionCoroutines = new List<StateActionCoroutineWrapper>();
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
    |NOTI| ���ݾִϸ��̼��� �ƴҶ� ������ true���� �����մϴ�.
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


    public void TryChangeStateContinue(StateGraphType graphType, RepresentStateType representType)
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

        ChangeStateContinue(graphType, targetAsset);

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

        ReadyLinkedStates(graphType, targetAsset, true);
    }

    private void StatedWillBeChanged()
    {
        _currStateTime = 0.0f;
        _prevStateTime = 0.0f;
        _failedRandomChanceState.Clear();
        _stateDeeper = false;
    }

    private void ChangeState(StateGraphType nextGraphType, StateAsset nextState)
    {
        StatedWillBeChanged();


        /*------------------------------------------------------------------------------------------------
        |TODO| Perfect Guard Buff�� Skill/Buff ������ ó���ϰ� ������...
        ------------------------------------------------------------------------------------------------*/
        if (nextState._myState._isGuardState == true &&
            _currState._myState._isGuardState == false)
        {
            StatScript statScript = _owner.GCST<StatScript>();
            statScript.ApplyBuff(LevelStatInfoManager.Instance.GetBuff("PerfectGuardBuff"), 1);
        }




        if (_currState != null)
        {
            DoActions(_currState._myState._ExitStateActionTypes);
        }

        {
            //�߷� ... ���� ��ȯ ������ ���� ���¿� ���� ���� ����
            {
                //�������� ���� ����
                _nextComboReady = true;
                _previousGraphType = _currentGraphType;
                _currentGraphType = nextGraphType;
                _currState = nextState;

                _randomStateInstructIndex = -1;
                _randomStateTryCount = 0;
                _failedRandomChanceState.Clear();
            }


            //owner���� �ñ׳� -> �ٸ� ������Ʈ���� ���� ��ȭ�� ������
            {
                if (_currentGraphType == StateGraphType.WeaponState_RightGraph ||
                    _currentGraphType == StateGraphType.WeaponState_LeftGraph)
                {
                    _owner.SetLatestWeaponUse(_currentGraphType == StateGraphType.WeaponState_RightGraph);
                }
                _owner.StateChanged(_currState);
            }

        }

        AllStopCoroutine();

        DoActions(_currState._myState._EnterStateActionTypes);

        AddStateActionCoroutine();
    }



    private void ChangeStateContinue(StateGraphType nextGraphType, StateAsset nextState)
    {
        if (_currState != null)
        {
            DoActions(_currState._myState._ExitStateActionTypes);
        }

        {
            //�߷� ... ���� ��ȯ ������ ���� ���¿� ���� ���� ����
            {
                //�������� ���� ����
                _nextComboReady = true;
                _previousGraphType = _currentGraphType;
                _currentGraphType = nextGraphType;
                _currState = nextState;

                _randomStateInstructIndex = -1;
                _randomStateTryCount = 0;
                _failedRandomChanceState.Clear();
            }


            //owner���� �ñ׳� -> �ٸ� ������Ʈ���� ���� ��ȭ�� ������
            {
                if (_currentGraphType == StateGraphType.WeaponState_RightGraph ||
                    _currentGraphType == StateGraphType.WeaponState_LeftGraph)
                {
                    _owner.SetLatestWeaponUse(_currentGraphType == StateGraphType.WeaponState_RightGraph);
                }
                _owner.StateChanged(_currState);
            }

        }

        AllStopCoroutine();

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
            List<LinkedStateAsset> linkedStates = null;
            _stateGraphes[(int)currentGraphType].GetGraphStates().TryGetValue(currState, out linkedStates);

            if (linkedStates == null)
            {
                return;
            }

            foreach (var linkedState in linkedStates)
            {
                _currLinkedStates.Add(new LinkedStateAssetWrapper(currentGraphType, currentGraphType, linkedState, null));
            }
        }
    }


    private bool CheckStateJumpCondotion(List<ConditionAssetWrapper> conditionAssets, LinkedStateAssetWrapper linkedStateAssetWrapper, StateAsset targetState, bool isRightSided)
    {
        foreach (ConditionAssetWrapper conditionAssetWrapper in conditionAssets)
        {
            if (CheckCondition(targetState, linkedStateAssetWrapper, conditionAssetWrapper, isRightSided) == false)
            {
                return false;
            }
        }

        return true;
    }

    private void ReadyCheckNextState(ref StateAsset targetState,StateAsset nextState, ref StateGraphType targetGraphType, StateGraphType nextStateGraph, ref bool stateChangeGraunted)
    {
        if (stateChangeGraunted == false)
        {
            StatedWillBeChanged();
            stateChangeGraunted = true;
        }

        targetState = nextState;
        targetGraphType = nextStateGraph;
        ReadyLinkedStates(targetGraphType, targetState, true);
    }

    private bool CheckStateNeedStat(List<ActiveStatNeedTarget> needActiveStatList, StatScript ownerStatScript)
    {
        foreach (var item in needActiveStatList)
        {
            int currVar = ownerStatScript.GetActiveStat(item._statType);

            if (currVar < item._needAmount || currVar == 0)
            {
                return false;
            }
        }

        return true;
    }


    //���� ���¸� �����Ҷ����� ��������� ������ �Լ�
    public StateAsset CheckChangeState_Recursion(out StateGraphType nextGraphType) 
    {
        nextGraphType = _currentGraphType;

        //�߷�...(�ݺ������� ����� ������ �̸� ����)
        #region PreDeclare
        StatScript ownerStatScript = _owner.GCST<StatScript>();
        StateAsset targetState = _currState;
        LinkedStateAsset linkedState = null;
        List<ConditionAssetWrapper> conditionAssetWrappers = null;

        bool isRightSided = false;
        bool isStateChangeGuaranted = false;
        bool isSuccess = false;

        int successCount = 0;
        #endregion PreDeclare

        while (true)
        {
            //�߷�...(����� : 100ȸ �̻��̸� Break)
            if (successCount > 100)
            {
                Debug.Assert(false, "���°� ��� �ٲ���մϴ� : " + targetState.name);
                Debug.Break();
                return null;
            }

            
            if (targetState._myState._isNeedStatLoopBreak == true)
            {
                bool isLoopContinue = CheckStateNeedStat(targetState._myState._needStat_LoopBreak._needActiveStatList, ownerStatScript);

                if (isLoopContinue == false)
                {
                    //Idle�� ������ ��
                    ReadyCheckNextState(ref targetState, GetMyIdleStateAsset(), ref nextGraphType, StateGraphType.LocoStateGraph, ref isStateChangeGuaranted);
                    continue;
                }
            }

            if (_currLinkedStates.Count <= 0)
            {
                isSuccess = false;
            }

            foreach (LinkedStateAssetWrapper linkedStateAssetWrapper in _currLinkedStates)
            {
                linkedState = linkedStateAssetWrapper._linkedStateWrapper;
                conditionAssetWrappers = linkedState._conditionAsset;
                isRightSided = (linkedStateAssetWrapper._goalType != StateGraphType.WeaponState_LeftGraph && linkedStateAssetWrapper._fromType != StateGraphType.WeaponState_LeftGraph);

                isSuccess = CheckStateJumpCondotion(conditionAssetWrappers, linkedStateAssetWrapper, targetState, isRightSided);

                if (linkedStateAssetWrapper._additionalCondition != null &&
                    linkedStateAssetWrapper._additionalCondition.Count > 0 &&
                    isSuccess == true)
                {
                    isSuccess = CheckStateJumpCondotion(linkedStateAssetWrapper._additionalCondition, linkedStateAssetWrapper, targetState, isRightSided);
                }


                //NeedStat�˻�
                StateAsset nextState = linkedStateAssetWrapper._linkedStateWrapper._linkedState;
                if (isSuccess == true && nextState._myState._isNeedStat == true)
                {
                    isSuccess = CheckStateNeedStat(nextState._myState._needStat._needActiveStatList, ownerStatScript);

                    if (isSuccess == false)
                    {
                        continue;
                    }

                    foreach (var item in nextState._myState._needStat._needActiveStatList)
                    {
                        if (item._isConsume == true)
                        {
                            ownerStatScript.ChangeActiveStat(item._statType, -item._needAmount);
                        }
                    }
                }

                if (isSuccess == true)
                {
                    ReadyCheckNextState(ref targetState, linkedStateAssetWrapper._linkedStateWrapper._linkedState, ref nextGraphType, linkedStateAssetWrapper._goalType, ref isStateChangeGuaranted);

                    //������ ���� ����ó��
                    {
                        if (targetState._myState._EnterStateActionTypes.Count > 0 &&
                            targetState._myState._EnterStateActionTypes[0] == StateActionType.Jump)
                        {
                            return targetState;
                        }
                    }

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

        if (targetState == _currState &&
            successCount <= 0)
        {
            return null;
        }

        return targetState;
    }

    public void DoWork()
    {
        StateGraphType nextGraphType = StateGraphType.End;

        StateAsset nextState = CheckChangeState_Recursion(out nextGraphType);

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
        float animationSpeed = _owner.GCST<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetFloat("Speed");
        float currentSecond = FloatMod(_currStateTime * animationSpeed, _currState._myState._stateAnimationClip.length);
        float prevSecond = FloatMod(_prevStateTime * animationSpeed, _currState._myState._stateAnimationClip.length);

        Vector3 currentUnityLocalHip = CalculateCurrHipCurve(currentSecond);
        //���� �ִϸ��̼ǿ��� �Ѿ���� ó����
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
                        Debug.Assert(false, "���� �Ⱦ��ϴ�" + StateActionType.AddCoroutine_ChangeToIdleState);
                        Debug.Break();
                        /*AddStateActionCoroutine(StateActionCoroutineType.ChangeToIdle);*/
                    }
                    break;
                case StateActionType.AddCoroutine_StateChangeReady: 
                    {
                        Debug.Assert(false, "���� �Ⱦ��ϴ�" + StateActionType.AddCoroutine_StateChangeReady);
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
                            Debug.Assert(false, "AI_UpdateAttackRange�ൿ���� �ݵ�� Aggressive StateGraph�� �־�� �մϴ�");
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
                        Debug.Assert(false, "���� ���� �ʽ��ϴ�");
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
                        Debug.Assert(false, "���� �Ⱦ��ϴ�" + StateActionType.AddCoroutine_DeadCall);
                        Debug.Break();
                        /*AddStateActionCoroutine(StateActionCoroutineType.DeadCall);*/
                    }
                    break;

                case StateActionType.AddCoroutine_BuffCheck: 
                    {
                        Debug.Assert(false, "���� �Ⱦ��ϴ�" + StateActionType.AddCoroutine_BuffCheck);
                        Debug.Break();
                        /*AddStateActionCoroutine(StateActionCoroutineType.Buff);*/
                    }
                    break;

                case StateActionType.AddBuff:
                    {
                        List<BuffAssetBase> addBuffList = _currState._myState._stateBuffs_Add;

                        foreach (BuffAssetBase buff in addBuffList)
                        {
                            _owner.GCST<StatScript>().ApplyBuff(buff, 1);
                        }
                    }
                    break;

                case StateActionType.RemoveBuff:
                    {
                        List<BuffAssetBase> removeBuffList = _currState._myState._stateBuffs_Remove;

                        foreach (BuffAssetBase buff in removeBuffList)
                        {
                            _owner.GCST<StatScript>().RemoveBuff(buff, -1);
                        }
                    }
                    break;

                case StateActionType.CharacterRevive:
                    {
                        _owner.GCST<StatScript>().CharacterRevive();

                        int myLayer = 0;
                        switch (_owner._CharacterType)
                        {
                            case CharacterType.Player:
                                myLayer = LayerMask.NameToLayer("Player");
                                break;
                            case CharacterType.Monster_Zombie:
                                myLayer = LayerMask.NameToLayer("Monster");
                                break;
                            default:
                                break;
                        }
                        gameObject.layer = myLayer;

                        _owner.GCST<CharacterContollerable>().CharacterRevive();
                    }
                    break;

                case StateActionType.PostureReset:
                    {
                        StatScript statScript = _owner.GCST<StatScript>();
                        int currPosture = statScript.GetActiveStat(ActiveStat.PosturePercent);
                        statScript.ChangeActiveStat(ActiveStat.PosturePercent, -currPosture);
                        statScript.RemoveBuff(LevelStatInfoManager.Instance.GetBuff("PostureMaxBuff"), -1);
                    }
                    break;

                case StateActionType.SpuriousDead:
                    {
                    }
                    break;

                default:
                    //Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?" + action);
                    break;
            }
        }
    }


    private void AllStopCoroutine()
    {
        foreach (StateActionCoroutineWrapper coroutineWrapper in _stateActionCoroutines)
        {
            if (coroutineWrapper._coroutine == null)
            {
                continue;
            }

            if (coroutineWrapper._workType == FrameDataWorkType.RemoveBuff)
            {
                StatScript statScript = _owner.GCST<StatScript>();

                foreach (BuffAssetBase buff in coroutineWrapper._frameData._buffs)
                {
                    if (statScript.GetRuntimeBuffAsset(buff) != null)
                    {
                        statScript.RemoveBuff(buff, -1);
                    }
                }
            }

            StopCoroutine(coroutineWrapper._coroutine);
        }

        _stateActionCoroutines.Clear();
    }


    private void AddCoroutine(Func<StateActionCoroutineWrapper, IEnumerator> func, StateActionCoroutineWrapper wrapper)
    {
        wrapper._coroutine = StartCoroutine(func(wrapper));
        if (wrapper._coroutine != null)
        {
            _stateActionCoroutines.Add(wrapper);
            return;
        }

        //Debug.Assert(false, "�ڷ�ƾ ���ۿ� �����߽��ϴ�");
        Debug.Log("�ڷ�ƾ ���ۿ� �����߽��ϴ�, �ڷ�ƾ�� �ʹ� ���� ������ �����ϴ�");
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

            Animator currAnimator = _owner.GCST<CharacterAnimatorScript>().GetCurrActivatedAnimator();
            float speed = currAnimator.GetFloat("Speed");

            foreach (AEachFrameData eachFrameData in pair.Value)
            {
                switch (type)
                {
                    case FrameDataWorkType.ChangeToIdle:
                        {
                            StateActionCoroutineTimer_UpTimer upTimer = new StateActionCoroutineTimer_UpTimer(_currState._myState._stateAnimationClip.length / speed);
                            StateActionCoroutineWrapper newCoroutineWrapper = new StateActionCoroutineWrapper(eachFrameData, upTimer, FrameDataWorkType.ChangeToIdle);
                            AddCoroutine(ChangeToIdleCoroutine, newCoroutineWrapper);
                        }
                        break;

                    case FrameDataWorkType.StateChangeReady:
                        {
                            StateActionCoroutineTimer_UpTimer upTimer = new StateActionCoroutineTimer_UpTimer(eachFrameData._frameUp / _currState._myState._stateAnimationClip.frameRate / speed);
                            StateActionCoroutineWrapper newCoroutineWrapper = new StateActionCoroutineWrapper(eachFrameData, upTimer, FrameDataWorkType.StateChangeReady);
                            AddCoroutine(StateChangeReadyCoroutine, newCoroutineWrapper);
                        }
                        break;

                    case FrameDataWorkType.NextAttackMotion:
                        {
                            float underSec = eachFrameData._frameUnder / _currState._myState._stateAnimationClip.frameRate / speed;
                            float upSec = eachFrameData._frameUp / _currState._myState._stateAnimationClip.frameRate / speed;

                            StateActionCoroutineTimer_BetweenTimer upTimer = new StateActionCoroutineTimer_BetweenTimer(upSec, underSec);
                            StateActionCoroutineWrapper newCoroutineWrapper = new StateActionCoroutineWrapper(eachFrameData, upTimer, FrameDataWorkType.ChangeToIdle);
                            AddCoroutine(NextAttackComboCoroutine, newCoroutineWrapper);
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
                            StateActionCoroutineTimer_UpTimer upTimer = new StateActionCoroutineTimer_UpTimer(_currState._myState._stateAnimationClip.length / speed);
                            StateActionCoroutineWrapper newCoroutineWrapper = new StateActionCoroutineWrapper(eachFrameData, upTimer, FrameDataWorkType.DeadCall);
                            AddCoroutine(DeadCallCoroutine, newCoroutineWrapper);
                        }
                        break;

                    case FrameDataWorkType.AddBuff:
                        {
                            StateActionCoroutineTimer_UpTimer upTimer = new StateActionCoroutineTimer_UpTimer(eachFrameData._frameUp / _currState._myState._stateAnimationClip.frameRate / speed);
                            StateActionCoroutineWrapper newCoroutineWrapper = new StateActionCoroutineWrapper(eachFrameData, upTimer, FrameDataWorkType.AddBuff);
                            AddCoroutine(StateAddBuffCoroutine, newCoroutineWrapper);
                        }
                        break;

                    case FrameDataWorkType.RemoveBuff:
                        {
                            StateActionCoroutineTimer_UpTimer upTimer = new StateActionCoroutineTimer_UpTimer(eachFrameData._frameUp / _currState._myState._stateAnimationClip.frameRate / speed);
                            StateActionCoroutineWrapper newCoroutineWrapper = new StateActionCoroutineWrapper(eachFrameData, upTimer, FrameDataWorkType.RemoveBuff);
                            AddCoroutine(StateRemoveBuffCoroutine, newCoroutineWrapper);
                        }
                        break;

                    case FrameDataWorkType.RipositeAttack:
                        {
                        }
                        break;

                    default:
                        {
                            Debug.Assert(false, "type�� ����� �������� �ʾҽ��ϴ�");
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
            target._timer.Update(Time.deltaTime);

            if (target._timer.Check() == true)
            {
                target._coroutine = null;
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
            target._timer.Update(Time.deltaTime);

            if (target._timer.Check() == true)
            {
                target._coroutine = null;
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
            Debug.Assert(false, "NextAttackComboCoroutine�� frameData�� ��� �ȵȴ�");
            Debug.Break();
        }

        _nextComboReady = false;

        while (true)
        {
            target._timer.Update(Time.deltaTime);

            if (target._timer.Check() == true)
            {
                target._coroutine = null;
                _nextComboReady = true;
                break;
            }

            yield return null;
        }
    }

    private IEnumerator DeadCallCoroutine(StateActionCoroutineWrapper target)
    {
        while (true)
        {
            target._timer.Update(Time.deltaTime);

            if (target._timer.Check() == true)
            {
                target._coroutine = null;
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
            target._timer.Update(Time.deltaTime);

            if (target._timer.Check() == true)
            {
                target._coroutine = null;
                List<BuffAssetBase> buffs = target._frameData._buffs;
                foreach (BuffAssetBase buff in buffs)
                {
                    _owner.GCST<StatScript>().ApplyBuff(buff, 1);
                }
                break;
            }
            yield return null;
        }
    }

    private IEnumerator StateRemoveBuffCoroutine(StateActionCoroutineWrapper target)
    {
        while (true)
        {
            target._timer.Update(Time.deltaTime);

            if (target._timer.Check() == true)
            {
                target._coroutine = null;
                List<BuffAssetBase> buffs = target._frameData._buffs;
                foreach (BuffAssetBase buff in buffs)
                {
                    _owner.GCST<StatScript>().RemoveBuff(buff, -1);
                }
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
                    Debug.Assert(false, "���� �� ������ ������� �ʽ��ϴ�" + ConditionType.EquipWeaponByType);
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

                    ret = _owner.GCST<EnemyAIScript>().InAttackRangeCheck(nextStateAsset._linkedStateWrapper._linkedState);
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
                    ret = ownerEnemyAIScript.FindCoolTimeCoroutine(nextStateAsset._linkedStateWrapper._linkedState);
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
                        Debug.Assert(false, "AI_ReadiedAttackExist ���ǿ��� �ݵ�� Aggressive Graph ������ �ʿ��մϴ�");
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

                    if (nextStateAsset._linkedStateWrapper._linkedState._myState._isAIState == false)
                    {
                        Debug.Assert(false, "AI_TryRandomChance �����ε� �ش� ���� false�Դϴ�");
                        Debug.Break();
                        return false;
                    }

                    if (_failedRandomChanceState.Contains(nextStateAsset._linkedStateWrapper._linkedState) == true)
                    {
                        ret = false;
                        break;
                    }

                    if (nextStateAsset._linkedStateWrapper._linkedState._myState._aiStateDesc._randomChancePercentage < 0.0f)
                    {
                        Debug.Assert(false, "AI_TryRandomChance ���ǿ��� Ȯ���� ��ȿ�Ѱ��� �־���մϴ�" + nextStateAsset._linkedStateWrapper._linkedState.name);
                        Debug.Break();
                        return false;
                    }

                    ret = (randomValue <= nextStateAsset._linkedStateWrapper._linkedState._myState._aiStateDesc._randomChancePercentage);

                    if (ret == false)
                    {
                        _failedRandomChanceState.Add(nextStateAsset._linkedStateWrapper._linkedState);
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

            case ConditionType.StateDeeper:
                {
                    if (_stateDeeper == true)
                    {
                        ret = true;
                    }
                }
                break;

            case ConditionType.NeedStat:
                {
                }
                break;

            case ConditionType.BuffAssetCount:
                {
                    StatScript statScript = _owner.GCST<StatScript>();

                    if (statScript == null)
                    {
                        ret = false;
                        break;
                    }

                    RuntimeBuffAsset currRuntimeBuffAsset = statScript.GetRuntimeBuffAsset(conditionDesc._targetBuffAsset);

                    if (currRuntimeBuffAsset == null)
                    {
                        ret = false;
                        break;
                    }

                    if (currRuntimeBuffAsset._Count == conditionDesc._targetBuffAssetCount)
                    {
                        ret = true;
                        break;
                    }

                    ret = false;
                }
                break;

            case ConditionType.IsDead:
                {
                    ret = _owner.GetDead();
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
        /*-----------------------------------------------------------
        |TODO| ������ ���� ������ �õ����� �ʾҽ��ϴ� ����...��������
        if (false) return false;
        -----------------------------------------------------------*/

        if (CustomKeyManager.Instance._AttackKeyTry == false)
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

        WeaponGrabFocus ownerGrabType = _owner.GetGrabFocusType();
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
    
    private ComboCommandKeyType KeyConvert(WeaponUseType target, WeaponGrabFocus ownerGrabType, bool isRightHandWeapon, bool isSingleControl = false)
    {
        /*----------------------------------------------------
        |NOTI| isSingleControl ���� -> (��Ŭ + ��Ŭ) �޺��϶� �Ѽ� ���� ���̶��
        Click + (x + click)�ε� ���������� �Ұ����� �޺� Ŀ�ǵ�ϱ� Click + OppositeClick���� �ٲٴ� �Լ�
        ----------------------------------------------------*/

        ComboCommandKeyType convertedRet = ComboCommandKeyType.TargetingFront;

        bool isModifyClick = (target != WeaponUseType.MainUse && isSingleControl == false);

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

            case WeaponUseType.OppositeMainUse: //������ �޺��� �ݴ� ��� Ŭ���̴�
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

            /*----------------------------
            ���⼭���� ��ųŰ
            ----------------------------*/

            case WeaponUseType.EleUse:
                convertedRet = ComboCommandKeyType.EleClikc;
                break;

            case WeaponUseType.CtrlEleUse:
                convertedRet = ComboCommandKeyType.CtrlEleClick;
                break;

            case WeaponUseType.SubEleUse:
                convertedRet = ComboCommandKeyType.SubEleClick;
                break;

            case WeaponUseType.UltUse:
                convertedRet = ComboCommandKeyType.UltClikc;
                break;

            case WeaponUseType.CtrlUltUse:
                convertedRet = ComboCommandKeyType.CtrlUltClick;
                break;

            case WeaponUseType.SubUltUse:
                convertedRet = ComboCommandKeyType.SubUltClick;
                break;

            case WeaponUseType.EleUp:
                convertedRet = ComboCommandKeyType.EleUp;
                break;

            case WeaponUseType.CtrlEleUp:
                convertedRet = ComboCommandKeyType.CtrlEleUp;
                break;

            case WeaponUseType.SubEleUp:
                convertedRet = ComboCommandKeyType.SubEleUp;
                break;

            case WeaponUseType.UltUp:
                convertedRet = ComboCommandKeyType.UltUp;
                break;

            case WeaponUseType.CtrlUltUp:
                convertedRet = ComboCommandKeyType.CtrlUltUp;
                break;

            case WeaponUseType.SubUltUp:
                convertedRet = ComboCommandKeyType.SubUltUp;
                break;

            default:
                break;
        }

        return convertedRet;
    }
}
