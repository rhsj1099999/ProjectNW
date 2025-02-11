using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static StateContoller;

[CreateAssetMenu(fileName = "StateGraphAsset", menuName = "Scriptable Object/CreateStateGraphAsset", order = int.MinValue)]
public class StateGraphAsset : ScriptableObject
{
    /*---------------------------------------------------
    |TODO| ������� ���� �Ϸ�Ǹ� �׷��� �� ���� �ϳ���
    �׷����� ���ĳ��� �۾��ϴ°� ������
    ---------------------------------------------------*/
    public enum StateGraphType
    {
        LocoStateGraph,
        WeaponState_RightGraph,
        WeaponState_LeftGraph,
        HitStateGraph,
        FlyStateGraph,
        AI_AggresiveGraph,
        AI_SharpnessGraph,
        AI_WeaknessGraph,
        AI_GuardGraph,
        DieGraph,
        End,
    };



    [Serializable]
    public class ConditionAssetWrapper
    {
        public ConditionAsset _conditionAsset = null;
        public bool _goal = false;
    }

    [Serializable] public class LinkedStateAsset
    {
        public LinkedStateAsset(StateAsset stateAsset, List<ConditionAssetWrapper> conditionAssetsWrapper)
        {
            _linkedState = stateAsset;
            _conditionAsset = conditionAssetsWrapper;
        }

        public StateAsset _linkedState = null;
        public List<ConditionAssetWrapper> _conditionAsset = new List<ConditionAssetWrapper>();
    }

    [Serializable] public class StateAssetWrapper
    {
        public bool _isEntryState = false;
        public StateAsset _stateAsset = null;
        public List<ConditionAssetWrapper> _entryCondition = new List<ConditionAssetWrapper>();
        public List<LinkedStateAsset> _linkedStates = new List<LinkedStateAsset>();
    }

    [Serializable] public class InteractionPoint
    {
        public StateGraphType _anotherType = StateGraphType.End;
        public List<LinkedStateAsset> _interactionStates = new List<LinkedStateAsset>();
    }


    [SerializeField] public StateGraphType _graphType = StateGraphType.End;
    [SerializeField] public List<StateAssetWrapper> _usingStates = new List<StateAssetWrapper>();
    [SerializeField] public List<InteractionPoint> _interactionPointsInitial = new List<InteractionPoint>();

    private List<LinkedStateAsset> _entryStates = new List<LinkedStateAsset>();
    private SortedDictionary<int, LinkedStateAsset> _entryStates_Ordered = new SortedDictionary<int, LinkedStateAsset>(new DescendingComparer<int>());
    private Dictionary<StateAsset, List<LinkedStateAsset>> _states = new Dictionary<StateAsset, List<LinkedStateAsset>>();
    private Dictionary<StateAsset, SortedDictionary<int,LinkedStateAsset>> _states_Ordered = new Dictionary<StateAsset, SortedDictionary<int, LinkedStateAsset>>();
    private Dictionary<StateGraphType, Dictionary<StateAsset, List<ConditionAssetWrapper>>> _interactionPoints = new Dictionary<StateGraphType, Dictionary<StateAsset, List<ConditionAssetWrapper>>>();

    private Dictionary<RepresentStateType, StateAsset> _usingRepresentStateTypes = new Dictionary<RepresentStateType, StateAsset>();
    

    public List<LinkedStateAsset> GetEntryStates() { return _entryStates; }
    public SortedDictionary<int, LinkedStateAsset> GetEntryStates_Ordered() { return _entryStates_Ordered; }
    public Dictionary<StateAsset, List<LinkedStateAsset>> GetGraphStates() { return _states; }
    public Dictionary<StateAsset, SortedDictionary<int, LinkedStateAsset>> GetGraphStates_Ordered() { return _states_Ordered; }
    public Dictionary<StateGraphType, Dictionary<StateAsset, List<ConditionAssetWrapper>>> GetInteractionPoints() { return _interactionPoints; }
    public StateAsset GetRepresentStateAsset(RepresentStateType stateType) 
    {
        if (_usingRepresentStateTypes.ContainsKey(stateType) == false)
        {
            return null;
        }
        return _usingRepresentStateTypes[stateType];
    }




    public void InitlaizeGraphAsset()
    {
        ClearData();
        ReadyStateAsset();
        ReadyInteractionPoints();
        ReadyHipCurve();
    }



    /*-----------------------------------------
    |TODO| Scriptable Object�� Awake�� �Ҹ��� ������ ã��
    ClearData �Լ��� ���ֶ�
    -----------------------------------------*/
    private void ClearData()
    {
        _entryStates = new List<LinkedStateAsset>();
        _entryStates_Ordered = new SortedDictionary<int, LinkedStateAsset>();
        _states = new Dictionary<StateAsset, List<LinkedStateAsset>>();
        _states_Ordered = new Dictionary<StateAsset, SortedDictionary<int, LinkedStateAsset>>();
        _interactionPoints = new Dictionary<StateGraphType, Dictionary<StateAsset, List<ConditionAssetWrapper>>>();
    }


    private void ReadyStateAsset()
    {
        foreach (StateAssetWrapper state in _usingStates)
        {
            StateAsset stateAsset = state._stateAsset;

            //��ǥ�����غ�
            {
                RepresentStateType stateType = stateAsset._myState._stateType;
                if (_usingRepresentStateTypes.ContainsKey(stateType) == true)
                {
                    //Debug.Log("���°� �ߺ��˴ϴ�.");
                    //Debug.Assert(false, "���°� �ߺ��˴ϴ�.");
                    //Debug.Break();
                    //return;
                }
                else
                {
                    _usingRepresentStateTypes.Add(stateType, stateAsset);
                }
            }

            //������ �غ�
            {
                if (state._isEntryState == true)
                {
                    LinkedStateAsset newLinkedState = new LinkedStateAsset(stateAsset, state._entryCondition);

                    _entryStates.Add(newLinkedState);

                    int priority = MyUtil.CalculateConditionWeight(state._entryCondition);

                    while (true)
                    {
                        if (_entryStates_Ordered.ContainsKey(priority) == true)
                        {
                            priority++;
                            continue;
                        }
                        break;
                    }

                    _entryStates_Ordered.Add(priority, newLinkedState);
                }
            }

            //������ ������Ʈ �غ�
            {
                if (_states.ContainsKey(state._stateAsset) == true)
                {
                    Debug.Assert(false, "�̹� ������Ʈ�� �ֽ��ϴ�");
                    Debug.Break();
                }
                _states.Add(state._stateAsset, state._linkedStates);
            }

            //���� ������Ʈ �غ�
            {
                if (_states_Ordered.ContainsKey(state._stateAsset) == true)
                {
                    Debug.Assert(false, "�̹� ������Ʈ�� �ֽ��ϴ�");
                    Debug.Break();
                }

                _states_Ordered.Add(state._stateAsset, new SortedDictionary<int, LinkedStateAsset>(new DescendingComparer<int>()));
                SortedDictionary<int, LinkedStateAsset> linkedStates_Ordered = _states_Ordered[state._stateAsset];

                foreach (LinkedStateAsset linkedState in state._linkedStates)
                {
                    int priority = MyUtil.CalculateConditionWeight(linkedState._conditionAsset);

                    while (true)
                    {
                        if (linkedStates_Ordered.ContainsKey(priority) == true)
                        {
                            priority++;
                            continue;
                        }
                        break;
                    }

                    linkedStates_Ordered.Add(priority, linkedState);
                }
            }
        }
    }



    private void ReadyInteractionPoints()
    {
        //�� Graph�� Ÿ�Կ��� �ٸ� �׷����� Ÿ������ �Ѿ �� �ִ� ��ȯ�� �غ� �Լ�
        foreach (InteractionPoint interactionPoint in _interactionPointsInitial)
        {
            StateGraphType anotherGraphType = interactionPoint._anotherType;

            if (_interactionPoints.ContainsKey(anotherGraphType) == false)
            {
                _interactionPoints.Add(anotherGraphType, new Dictionary<StateAsset, List<ConditionAssetWrapper>>());
            }

            Dictionary<StateAsset, List<ConditionAssetWrapper>> interactionPointDic = _interactionPoints[anotherGraphType];

            foreach (LinkedStateAsset stateAsset in interactionPoint._interactionStates)
            {
                StateAsset state = stateAsset._linkedState;
                List<ConditionAssetWrapper> pushCondition = stateAsset._conditionAsset;

                if (interactionPointDic.ContainsKey(state) == true)
                {
                    Debug.Assert(false, "Interaction Point�� ��Ĩ�ϴ�" + state.name);
                    Debug.Break();
                    return;
                }

                interactionPointDic.Add(state, pushCondition);
            }
        }
    }



    private void ReadyHipCurve()
    {
        foreach (StateAssetWrapper stateAssetWrapper in _usingStates)
        {
            ResourceDataManager.Instance.AddHipCurve(stateAssetWrapper._stateAsset._myState._stateAnimationClip);

            SubAnimationStateMachine subAnimationStateMachine = stateAssetWrapper._stateAsset._myState._subAnimationStateMachine;

            if (subAnimationStateMachine != null)
            {
                foreach (AnimationClip animationClip in subAnimationStateMachine._animations)
                {
                    ResourceDataManager.Instance.AddHipCurve(animationClip);
                }
            }
        }
    }

    



    public void SettingOwnerComponent(StateContollerComponentDesc ownerComponent, CharacterScript caller)
    {
        foreach (StateAssetWrapper stateAssetWrapper in _usingStates)
        {
            InitPartial(ownerComponent, stateAssetWrapper._stateAsset._myState._EnterStateActionTypes, caller);
            InitPartial(ownerComponent, stateAssetWrapper._stateAsset._myState._inStateActionTypes, caller);
            InitPartial(ownerComponent, stateAssetWrapper._stateAsset._myState._ExitStateActionTypes, caller);
        }
    }



    private void InitPartial(StateContollerComponentDesc _ownerActionComponent, List<StateActionType> list, CharacterScript owner)
    {
        foreach (var actions in list)
        {
            switch (actions)
            {
                case StateActionType.Move:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "Move�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponent<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "Input�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.Attack:
                    {
                        //���� �����ϰ��ִ� ���⿡ ������ �޽��ϴ�.
                    }
                    break;

                case StateActionType.SaveLatestVelocity:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "SaveLatestVelocity�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.Jump:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "Jump�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.ForcedMove:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "ForcedMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        //if (_ownerActionComponent._ownerCharacterComponent == null)
                        //{
                        //    _ownerActionComponent._ownerCharacterComponent = owner.GetComponent<CharacterController>();
                        //    Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "ForcedMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        //}
                    }
                    break;

                case StateActionType.ResetLatestVelocity:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "ResetLatestVelocity�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.RootMove:
                    {
                        if (_ownerActionComponent._ownerCharacterAnimatorScript == null)
                        {
                            _ownerActionComponent._ownerCharacterAnimatorScript = owner.GetComponentInChildren<CharacterAnimatorScript>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterAnimatorScript != null, "RootMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }


                        //if (_ownerActionComponent._ownerCharacterComponent == null)
                        //{
                        //    _ownerActionComponent._ownerCharacterComponent = owner.GetComponentInChildren<CharacterController>();
                        //    Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "RootMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        //}
                    }
                    break;

                case StateActionType.RotateWithoutInterpolate:
                    {
                        //if (_ownerActionComponent._ownerCharacterComponent == null)
                        //{
                        //    _ownerActionComponent._ownerCharacterComponent = owner.GetComponentInChildren<CharacterController>();
                        //    Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "RotateWithoutInterpolate�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        //}

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponentInChildren<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "RotateWithoutInterpolate�ൿ�� �ִµ� �� ������Ʈ�� ����");
                        }

                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponentInChildren<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "RotateWithoutInterpolate�ൿ�� �ִµ� �� ������Ʈ�� ����");
                        }
                    }
                    break;

                case StateActionType.RightHandWeaponSignal:
                    {
                        //_ownerActionComponent._owner.RightHandWeaponSignal();
                    }
                    break;

                case StateActionType.LeftHandWeaponSignal:
                    {
                        //_ownerActionComponent._owner.LeftHandWeaponSignal();
                    }
                    break;

                case StateActionType.AttackCommandCheck:
                    break;

                case StateActionType.StateEndDesierdCheck:
                    break;

                case StateActionType.CheckBehaves:
                    break;

                case StateActionType.CalculateWeaponLayer_EnterAttack:
                case StateActionType.CalculateWeaponLayer_ExitAttack:
                    {
                        _ownerActionComponent._ownerCharacterAnimatorScript = owner.GetComponentInChildren<CharacterAnimatorScript>();
                        if (_ownerActionComponent._ownerCharacterAnimatorScript == null)
                        {
                            Debug.Assert(false, "CalculateWeaponLayer_EnterAttack Action�ε�, ������Ʈ�� ����");
                            Debug.Break();
                        }
                    }
                    break;

                case StateActionType.DummyState_EnterLocoStateGraph:
                    break;

                case StateActionType.AddCoroutine_ChangeToIdleState:
                    break;

                case StateActionType.AddCoroutine_StateChangeReady:
                    break;

                case StateActionType.CharacterRotate:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "Move�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponent<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "Input�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.AI_CharacterRotateToEnemy:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "AI_CharacterRotateToEnemy�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerEnemyAIScript == null)
                        {
                            _ownerActionComponent._ownerEnemyAIScript = owner.GetComponent<EnemyAIScript>();
                            Debug.Assert(_ownerActionComponent._ownerEnemyAIScript != null, "AI_CharacterRotateToEnemy�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.AI_ChaseToEnemy:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "AI_CharacterRotateToEnemy�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerEnemyAIScript == null)
                        {
                            _ownerActionComponent._ownerEnemyAIScript = owner.GetComponent<EnemyAIScript>();
                            Debug.Assert(_ownerActionComponent._ownerEnemyAIScript != null, "AI_CharacterRotateToEnemy�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.AI_ForcedLookAtEnemy:
                    break;

                case StateActionType.AI_ReArrangeStateGraph:
                    {
                        if (_ownerActionComponent._ownerEnemyAIScript == null)
                        {
                            _ownerActionComponent._ownerEnemyAIScript = owner.GetComponent<EnemyAIScript>();
                            if (_ownerActionComponent._ownerEnemyAIScript == null)
                            {
                                Debug.Assert(false, "AI_ReArrangeStateGraph �ൿ�� ������Ʈ�� �����ϴ�");
                                Debug.Break();
                                return;
                            }
                        }
                        
                    }
                    break;

                case StateActionType.AI_UpdateAttackRange:
                    {
                        if (_ownerActionComponent._ownerEnemyAIScript == null)
                        {
                            _ownerActionComponent._ownerEnemyAIScript = owner.GetComponent<EnemyAIScript>();
                            if (_ownerActionComponent._ownerEnemyAIScript == null)
                            {
                                Debug.Assert(false, "AI_UpdateAttackRange �ൿ�� ������Ʈ�� �����ϴ�");
                                Debug.Break();
                                return;
                            }
                        }
                    }
                    break;

                case StateActionType.Move_WithOutRotate:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "Move_WithOutRotate�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponent<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "Input�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.LookAtLockOnTarget:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "Move_WithOutRotate�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerAimScript == null)
                        {
                            _ownerActionComponent._ownerAimScript = owner.GetComponent<AimScript2>();
                            Debug.Assert(_ownerActionComponent._ownerAimScript != null, "LookAtLockOnTarget �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.RotateToLockOnTarget:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "Move_WithOutRotate�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerAimScript == null)
                        {
                            _ownerActionComponent._ownerAimScript = owner.GetComponent<AimScript2>();
                            Debug.Assert(_ownerActionComponent._ownerAimScript != null, "RotateToLockOnTarget �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.RootMove_WithOutRotate:
                    {
                        if (_ownerActionComponent._ownerCharacterAnimatorScript == null)
                        {
                            _ownerActionComponent._ownerCharacterAnimatorScript = owner.GetComponentInChildren<CharacterAnimatorScript>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterAnimatorScript != null, "RootMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        //if (_ownerActionComponent._ownerCharacterComponent == null)
                        //{
                        //    _ownerActionComponent._ownerCharacterComponent = owner.GetComponentInChildren<CharacterController>();
                        //    Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "RootMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        //}
                    }
                    break;

                case StateActionType.CharacterRotateToCameraLook:
                    {
                        if (_ownerActionComponent._ownerCharacterAnimatorScript == null)
                        {
                            _ownerActionComponent._ownerCharacterAnimatorScript = owner.GetComponentInChildren<CharacterAnimatorScript>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterAnimatorScript != null, "RootMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        //if (_ownerActionComponent._ownerCharacterComponent == null)
                        //{
                        //    _ownerActionComponent._ownerCharacterComponent = owner.GetComponentInChildren<CharacterController>();
                        //    Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "RootMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        //}
                    }
                    break;

                case StateActionType.EnterGunAiming:
                    break;

                case StateActionType.ExitGunAiming:
                    break;

                case StateActionType.Move_WithOutRotate_Gun:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "Move_WithOutRotate�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponent<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "Move_WithOutRotate_Gun�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.LookAtLockOnTarget_Gun:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "LookAtLockOnTarget_Gun�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerAimScript == null)
                        {
                            _ownerActionComponent._ownerAimScript = owner.GetComponent<AimScript2>();
                            Debug.Assert(_ownerActionComponent._ownerAimScript != null, "LookAtLockOnTarget_Gun �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.AnimationAttack:
                    {
                        if (_ownerActionComponent._ownerCharacterColliderScript == null)
                        {
                            _ownerActionComponent._ownerCharacterColliderScript = owner.GetComponent<CharacterColliderScript>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterColliderScript != null, "AnimationAttack �ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.AttackLookAtLockOnTarget:
                    {
                        if (_ownerActionComponent._ownerCharacterControllable == null)
                        {
                            _ownerActionComponent._ownerCharacterControllable = owner.GetComponent<CharacterContollerable>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterControllable != null, "LookAtLockOnTarget_Gun�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerAimScript == null)
                        {
                            _ownerActionComponent._ownerAimScript = owner.GetComponent<AimScript2>();
                            Debug.Assert(_ownerActionComponent._ownerAimScript != null, "LookAtLockOnTarget_Gun �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.AddCoroutine_DeadCall:
                    break;

                case StateActionType.AddBuff:
                    break;

                case StateActionType.RemoveBuff:
                    break;

                case StateActionType.CharacterRevive:
                    break;

                default:
                    Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                    break;
            }
        }
    }
}
