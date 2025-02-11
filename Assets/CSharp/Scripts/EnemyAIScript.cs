using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StateContoller;
using static StateGraphAsset;

public class EnemyAIScript : GameCharacterSubScript
{
    public class StateCoolTimeCoroutineDesc
    {
        public StateAsset _stateAsset = null;
        public float _coolTime = 0.0f;
        public Coroutine _runningCoroutine = null;
    }



    //StateSection
    [SerializeField] private float _detectRange = 3.0f;
    [SerializeField] private float _detectHorizontalDeg = 60.0f;
    //[SerializeField] private float _detectRayCastTick = 1.0f;
    //[SerializeField] private float _stateAcc = 0.0f;

    //--------------IdleState Vars
    //[SerializeField] private float _idleStateTarget = 3.0f;




    //--------------PatrolState Vars
    //[SerializeField] private float _patrolSpeedRatio = 0.5f;
    //[SerializeField] private float _patrolRotateSpeedRatio = 0.5f;



    //--------------NormalAttack Vars
    [SerializeField] private float _basicMeleeAttackRange = 1.5f;
    public float GetBaseMeleeAttackRange() { return _basicMeleeAttackRange; }
    [SerializeField] private float _basicRangeAttackRange = 5.0f;
    public float GetRangeAttackRange() { return _basicRangeAttackRange; }


    //--------------Enemy Vars
    private Dictionary<StateAsset, StateCoolTimeCoroutineDesc> _aiAttackCoolTimes = new Dictionary<StateAsset, StateCoolTimeCoroutineDesc>();





    //--------------Enemy Vars
    [SerializeField] private List<string> _enemyTags = new List<string>();
    private List<CharacterScript> _enemiesInScene = new List<CharacterScript>();
    public List<CharacterScript> GetEnemiesInScene() { return _enemiesInScene; }
    [SerializeField] private CharacterScript _targetEnemy = null; //�ν����Ϳ��� ������ �ӽ� SerializeField
    public CharacterScript GetCurrentEnemy() { return _targetEnemy; }
    public void SetEnemy(CharacterScript enemy) { _targetEnemy = enemy; }





    //--------------Aggressive Vars
    [SerializeField] private int _aggressiveMaxStep = -1;
    private float _aggressivePercentage = 1.0f;
    private int _currAggressiveStep = -1;
    private int _prevAggressiveStep = -1;





    //--------------Runtime Vars







    //---Chase
    private float _chasingDistance = 1.5f;
    public float GetChsingDistance()
    {
        return _chasingDistance;
    }


    virtual public bool IsInAttackRange(float characterHeight)
    {
        if (_targetEnemy == null)
        {
            return false;
        }

        if (_chasingDistance < 0.0f)
        {
            return false;
        }

        Vector3 enemyPosition = _targetEnemy.gameObject.transform.position;
        Vector3 myPosition = gameObject.transform.position;
        Vector3 distanceVector = enemyPosition - myPosition;

        if (Mathf.Abs(distanceVector.y) >= characterHeight / 2.0f)
        {
            return false;
        }

        Vector2 planeDistanceVector = new Vector2(distanceVector.x, distanceVector.z);

        float planeDistance = planeDistanceVector.magnitude;

        if (planeDistance >= _chasingDistance)
        {
            return false;
        }

        return true;
    }

    public bool IsAnyAttackReadied(ref StateGraphAsset aiAggressiveStateGraph)
    {
        foreach (KeyValuePair<StateAsset, List<LinkedStateAsset>> assetWrapper in aiAggressiveStateGraph.GetGraphStates())
        {
            StateAsset stateAsset = assetWrapper.Key;

            AIAttackStateDesc aIAttackStateDesc = stateAsset._myState._aiAttackStateDesc;

            if (stateAsset._myState._isAttackState == false)
            {
                continue;
            }

            if (aIAttackStateDesc == null)
            {
                continue;
            }

            StateCoolTimeCoroutineDesc target = null;
            _aiAttackCoolTimes.TryGetValue(stateAsset, out target);

            if (target == null)
            {
                return true;
            }
        }

        return false;
    }



    public override void Init(CharacterScript owner)
    {
        _myType = typeof(EnemyAIScript);
        _owner = owner;

        if (_aggressiveMaxStep <= 0)
        {
            Debug.Assert(false, "AggressiveMaxStep�� ��ȿ�Ѱ��� �־���մϴ�");
            Debug.Break();
            return;
        }

        _currAggressiveStep = _aggressiveMaxStep;
        _prevAggressiveStep = _aggressiveMaxStep;
    }

    public override void SubScriptStart()
    {
    }


    virtual public void UpdateChasingDistance(ref StateGraphAsset aiAggressiveStateGraph)
    {
        float maxDistance = -1.0f;

        foreach (KeyValuePair<StateAsset, List<LinkedStateAsset>> assetWrapper in aiAggressiveStateGraph.GetGraphStates())
        {
            StateAsset stateAsset = assetWrapper.Key;

            AIAttackStateDesc aiAttackStateDesc = stateAsset._myState._aiAttackStateDesc;

            if (aiAttackStateDesc == null)
            {
                continue;
            }

            if (FindCoolTimeCoroutine(stateAsset) == true)
            {
                continue;
            }

            maxDistance = Mathf.Max(maxDistance, aiAttackStateDesc._range);
        }

        if (maxDistance < 0.0f)
        {
            Debug.Assert(false, "��Ÿ��� ������ ���ݵ� �ֽ��ϱ�?");
            Debug.Break();
            return;
        }

        _chasingDistance = maxDistance;
    }


    public bool FindCoolTimeCoroutine(StateAsset currState)
    {
        StateCoolTimeCoroutineDesc currCoroutine = null;
        _aiAttackCoolTimes.TryGetValue(currState, out currCoroutine);
        if (currCoroutine == null)
        {
            return false;
        }

        return true;
    }

    public void AddCoolTimeCoroutine(StateAsset currState)
    {
        StateCoolTimeCoroutineDesc currCoroutinedesc = null;
        _aiAttackCoolTimes.TryGetValue(currState, out currCoroutinedesc);

        if (currCoroutinedesc != null)
        {
            return;
        }

        currCoroutinedesc = new StateCoolTimeCoroutineDesc();
        currCoroutinedesc._stateAsset = currState;
        currCoroutinedesc._coolTime = currState._myState._aiAttackStateDesc._coolTime;
        currCoroutinedesc._runningCoroutine = StartCoroutine(CoolTimeCoroutine(currCoroutinedesc));
        _aiAttackCoolTimes.Add(currState, currCoroutinedesc);
    }


    private IEnumerator CoolTimeCoroutine(StateCoolTimeCoroutineDesc myTarget)
    {
        while (true)
        {
            myTarget._coolTime -= Time.deltaTime;

            if (myTarget._coolTime <= 0.0f)
            {
                StateCoolTimeCoroutineDesc currDesc = null;
                _aiAttackCoolTimes.TryGetValue(myTarget._stateAsset, out currDesc);
                if (currDesc == null) 
                {
                    Debug.Assert(false, "�ڷ�ƾ�� �߰��� ��������ϴ�?");
                    Debug.Break();
                }
                _aiAttackCoolTimes.Remove(myTarget._stateAsset);
                break;
            }

            yield return null;
        }
    }








    virtual public void ReArrangeStateGraph(List<LinkedStateAssetWrapper> currLinkedState, StateContoller stateController, StateAsset currState)
    {
        float eachStepOffset = 1.0f / (float)_aggressiveMaxStep;

        _currAggressiveStep = (int)(_aggressivePercentage / eachStepOffset);

        if (_currAggressiveStep != _prevAggressiveStep)
        {
            currLinkedState.Clear();

            List<StateGraphType> reArrangeOrder = new List<StateGraphType>();

            if (_aggressivePercentage >= 0.66f)
            {
                reArrangeOrder.Add(StateGraphType.AI_AggresiveGraph);
                reArrangeOrder.Add(StateGraphType.AI_SharpnessGraph);
                reArrangeOrder.Add(StateGraphType.AI_WeaknessGraph);
            }

            else if (_aggressivePercentage >= 0.33f)
            {
                reArrangeOrder.Add(StateGraphType.AI_SharpnessGraph);
                reArrangeOrder.Add(StateGraphType.AI_AggresiveGraph);
                reArrangeOrder.Add(StateGraphType.AI_WeaknessGraph);
            }

            else
            {
                reArrangeOrder.Add(StateGraphType.AI_WeaknessGraph);
                reArrangeOrder.Add(StateGraphType.AI_SharpnessGraph);
                reArrangeOrder.Add(StateGraphType.AI_AggresiveGraph);
            }

            ReArrangeStateGraphPartial(currLinkedState, stateController, reArrangeOrder, currState);
        }

        _prevAggressiveStep = _currAggressiveStep;
    }

    private void ReArrangeStateGraphPartial(List<LinkedStateAssetWrapper> currLinkedState, StateContoller stateController, List<StateGraphType> targetGraphTypes, StateAsset currState)
    {
        //Interaction Point�� ���� �˻��ؾ��ϱ� ������ ���� ��´�.
        List<StateGraphAsset> stateGraphes = stateController.GetStateGraphes();

        StateGraphAsset currentGraphAsset = stateGraphes[(int)StateGraphType.LocoStateGraph];

        Dictionary<StateGraphType, Dictionary<StateAsset, List<ConditionAssetWrapper>>> currentInteractionPoints = currentGraphAsset.GetInteractionPoints();

        foreach (StateGraphType type in targetGraphTypes)
        {
            int keyIndex = (int)type;

            if (stateGraphes[keyIndex] == null)
            {
                continue;
            }

            Dictionary<StateAsset, List<ConditionAssetWrapper>> interactionState = null;
            currentInteractionPoints.TryGetValue(type, out interactionState);
            if (interactionState == null)
            {
                continue;
            }

            foreach (LinkedStateAsset entryStates in stateGraphes[keyIndex].GetEntryStates())
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

                currLinkedState.Add(new LinkedStateAssetWrapper(StateGraphType.LocoStateGraph, type, entryStates, additionalCondition));
            }
        }

        //InGraphState�� ��´�
        List<LinkedStateAsset> linkedStates = stateGraphes[(int)StateGraphType.LocoStateGraph].GetGraphStates()[currState];
        foreach (var linkedState in linkedStates)
        {
            currLinkedState.Add(new LinkedStateAssetWrapper(StateGraphType.LocoStateGraph, StateGraphType.LocoStateGraph, linkedState, null));
        }
    }


    private void Start()
    {
        ReadySceneEnemy();
    }


    private void ReadySceneEnemy()
    {
        foreach (string enemyTag in _enemyTags)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            foreach (GameObject enemy in enemies) 
            {
                CharacterScript enemysCharacterScript = enemy.GetComponent<CharacterScript>();

                if (enemysCharacterScript == null)
                {
                    continue;
                }

                _enemiesInScene.Add(enemysCharacterScript);
            }
        }
    }


    private void StartCoroutineWrapper()
    {

    }





    public bool InBattleCheck()
    {
        for (int i = 0; i < _enemiesInScene.Count; i++)
        {
            if (_enemiesInScene[i].GetDead() == true) //��Ÿ���� Destroy �� ��
            {
                _enemiesInScene.RemoveAt(i); i--;
                continue;
            }

            GameObject enemyObject = _enemiesInScene[i].gameObject;
            Vector3 enemyPosition = enemyObject.transform.position;

            //1��. �Ÿ��� �����Ÿ� ���� �ֳ�
            {
                if (Vector3.Distance(enemyPosition, transform.position) > _detectRange)
                {
                    continue;
                }
            }

            //2��. ���� ���� ���� ���� �����ϴ°�
            {
                Vector3 dirToEnemy = (enemyPosition - transform.position);
                dirToEnemy.y = 0.0f;

                float deltaDeg = Vector3.Angle(transform.forward, dirToEnemy.normalized);
                if (deltaDeg > 180)
                {
                    deltaDeg -= 180.0f;
                }

                if (Mathf.Abs(deltaDeg) > _detectHorizontalDeg)
                {
                    continue;
                }
            }

            //3��. ���� ���� ���� ���� �����ϴ°�
            //if (false)
            //{
            //    continue;
            //}

            _targetEnemy = _enemiesInScene[i];

            return true;
        }

        return false;
    }




    /*----------------------------------------------------
    |NOTI| �� ������ ���ؼ� ���� ������ �ֽ��ϱ�
    �� ������ ���� ���ʹ� ������ ���� ������ �ִ�.
    ----------------------------------------------------*/
    public bool InAttackRangeCheck(StateAsset aiAttackAsset)
    {
        /*----------------------------------------------------
        |TOOD| ���߿� ���͵� ���⸦ ��� �����ϴ� �������� �ҷ���
        StateAsset�� �ƴ϶�, currLinkedState�� �Ѱ��༭ ���� �˻��ؾ��մϴ�
        ----------------------------------------------------*/

        StateDesc aiAttackAssetDesc = aiAttackAsset._myState;

        if (aiAttackAssetDesc._isAttackState == false)
        {
            return false;
        }

        AIAttackStateDesc aiAttackStateDest = aiAttackAssetDesc._aiAttackStateDesc;

        //����� ���ݻ��¿� ���Ͽ� üũ
        {
            
        }


        if (aiAttackAsset == null)
        {
            return false;
        }

        return false;
    }


}
