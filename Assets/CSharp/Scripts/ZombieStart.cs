using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using TMPro;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using UnityEngine.XR;
using System.Runtime.InteropServices;

public class ZombieStart : MonoBehaviour, IHitable
{
    public enum MonstarState
    {
        Idle,
        PatrolMove,
        Detected,
        Chase,
        Attack,
        Shoot,
        Dead,
        End,
    }

    //StateSection
    [SerializeField] private MonstarState _currState = MonstarState.Idle;
    [SerializeField] private MonstarState _prevState = MonstarState.Idle;
    [SerializeField] private bool _stateEndWithoutInterCept = false; //상태를 바꾸는 방해 없이 잘 끝마쳤습니다.
    [SerializeField] private float _detectRange = 3.0f;
    [SerializeField] private float _detectRayCastTick = 1.0f;
    [SerializeField] private float _detectHorizontalDeg = 60.0f;
    [SerializeField] private float _stateAcc = 0.0f;
    private Animator _animator = null;
    private AnimatorOverrideController _overrideController = null;
    private bool _currAnimNode = true; //true = State1;
    //--------------IdleState Vars
    [SerializeField] private float _idleStateTarget = 3.0f;
    //--------------PatrolState Vars
    [SerializeField] private float _patrolSpeedRatio = 0.5f;
    [SerializeField] private float _patrolRotateSpeedRatio = 0.5f;
    //--------------ChaseState Vars
    [SerializeField] private GameObject _debuggingPlayer = null;
    [SerializeField] private GameObject _debuggingCornerSpherePrefab = null;
    [SerializeField] private GameObject _debuggingCornerCapsulePrefab = null;
    [SerializeField] private int _filterTarget = 0;
    [SerializeField] private float _maxChasingDistance = 20.0f;
    [SerializeField] private float _chaseSpeedRatio = 2.0f;
    [SerializeField] private float _chaseRotateSpeedRatio = 2.0f;
    //--------------NormalAttack Vars
    [SerializeField] private float _normalAttackRange = 1.0f;

    //--------------Battle Vars
    private List<GameObject> _enemies = new List<GameObject>();
    [SerializeField] private GameObject _battleTarget = null;


    int _hp = 10;







    //Move Function Section
    [SerializeField] private CharacterController _characterController = null;
    [SerializeField] private float _movingSpeed = 2.0f;
    [SerializeField] private float _rotatingSpeedDeg = 360.0f;
    private bool _isJumping = false;
    private bool _isInAir = false;
    private bool _isAim = false;
    private float _verticalSpeedAcc = 0.0f;
    [SerializeField] private float _jumpForce = 3.0f;
    [SerializeField] private float _mass = 30.0f;


    //--------------Anim Vars |TODO|이거 다른 클래스로 빼라
    [SerializeField] private AnimContoller _AnimController = null;
    [SerializeField] private List<string> _enemyTags =  new List<string>();
    private float _crossFadeTime = 0.1f;
    private string _targetName1 = "Zombie Idle";
    private string _targetName2 = "UseThisToChange";

    [SerializeField] private AnimationClip Clip_Idle = null;
    [SerializeField] private AnimationClip Clip_Walk = null;
    [SerializeField] private AnimationClip Clip_Chase = null;
    [SerializeField] private AnimationClip Clip_Scream = null;
    [SerializeField] private AnimationClip Clip_turn = null;
    [SerializeField] private AnimationClip Clip_Run = null;
    [SerializeField] private AnimationClip Clip_Punch = null;
    [SerializeField] private AnimationClip Clip_jump = null;
    [SerializeField] private AnimationClip Clip_InAir = null;
    [SerializeField] private AnimationClip Clip_Dead = null;
    private AnimationClip _currAnimClip = null;
    private AnimationClip _prevAnimClip = null;
    //그냥 착지하면 서로 뒤바꾸는걸로

    //--------------Navigation Section
    private float _minOffMeshSampleRadius = 0.05f;
    private NavMeshHit _navMeshHit;
    private NavMeshHit _navMeshHitFindOffMesh;
    private NavMeshPath _navMeshPath = null;
    private int _targetPathPositionIndex = 0;
    private List<bool> _isOffMeshLinks = new List<bool>();

    private List<GameObject> _createdDebuggingCornerSphere = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerCapsule = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerSphereReverse = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerCapsuleReverse = new List<GameObject>();

    private void Start()
    {
        if (_enemyTags.Count > 0)
        {
            InitEnemy();
        }

        _animator = GetComponentInChildren<Animator>();
        Debug.Assert(_animator != null, "애니메이터가 없습니다");
        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;
    }

    private void Awake()
    {
        _navMeshPath = new NavMeshPath(); //모노 비해이비어보다 먼저하면 안된단다.
    }

    void Update()
    {
        GravityUpdate();

        MonsterFSM();

        {
            if (Input.GetKeyDown(KeyCode.Alpha1) == true)
            {
                Debug_ChangeState(MonstarState.Idle);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) == true)
            {
                Debug_ChangeState(MonstarState.Chase);
            }

            if (Input.GetKeyDown(KeyCode.Alpha7) == true)
            {
                Debug_ChangeState(MonstarState.Detected);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) == true)
            {
                FindAndDrawPath();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4) == true)
            {
                FindClosetEdge();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5) == true)
            {
                FindAndDrawPathOnlyJump();
            }

            if (Input.GetKeyDown(KeyCode.Alpha6) == true)
            {
                FindAndDrawPathReverse();
            }

            if (Input.GetKeyDown(KeyCode.Alpha0) == true)
            {
                DeletePath();
            }
        }
    }

    private void InitEnemy()
    {
        //매 프레임마다 오브젝트를 하이라키에서 찾고 거리를 비교하는게 아니라.
        //미리 자기가 적대적으로 생각하는 오브젝트를 캐싱합니다. //Destory(Object) 호출시 자동으로 null참조입니다 (댕글링 포인터가 아니라네?)
        //|TODO| 이와 관련하여 Alias 매니저를 만들자. 런타임중 추가로 몬스터나 동료가 생성되면 enemy설정을 해주는 매니저
        foreach (var tag in _enemyTags)
        {
            foreach (var enemy in GameObject.FindGameObjectsWithTag(tag))
            {
                _enemies.Add(enemy);
            }
        }
    }

    private bool InAttackRangeCheck(float range, float height)
    {
        if (_battleTarget == null)
        {
            return false;
        }

        if (Vector3.Distance(_battleTarget.transform.position, transform.position) > range)
        {
            return false;
        }

        return true;
    }

    private bool InBattleCheck()
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (_enemies[i] == null) //런타임중 Destroy 된 놈
            {
                _enemies.RemoveAt(i); i--;
                continue;
            }

            //1차. 거리가 감지거리 내에 있나
            if (Vector3.Distance(_enemies[i].transform.position, transform.position) > _detectRange)
            {
                continue;
            }

            //2차. 수평 감지 각도 내에 존재하는가
            Vector3 dirToEnemy = (_enemies[i].transform.position - transform.position);
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

            //3차. 수직 감지 각도 내에 존재하는가
            //if (false)
            //{
            //    continue;
            //}
            
            //이때, null일 수도 있는 적이 List에서 사라지지 않는다. 다음 순회시에 삭제하기로 한다.
            _battleTarget = _enemies[i];
            return true;
        }

        return false;
    }







    public void DealMe(int damage, GameObject caller)
    {
        if (_hp <= 0)
        {
            return; //이미 죽었다.
        }

        _hp -= damage;

        if (_hp <= 0)
        {
            _hp = 0; //죽었다

            Dead();
        }
    }






    private void Dead()
    {
        _currState = MonstarState.Dead;
        //애니메이션 연출

        //디졸브하면서 사라지거나

        //랙돌로 변신, 모든 연산을 off
    }




    private bool DiffState_Dead()
    {
        ChangeAnimation(Clip_Dead);
        return true;
    }



    private void CheckJumpPosition()
    {
        if (_isJumping == true)
        {
            return; //이미 점프 중이다
        }

        if (_isOffMeshLinks[_targetPathPositionIndex] == false || _isOffMeshLinks[_targetPathPositionIndex + 1] == false)
        {
            return; //점프를 시도할 포지션이 아니다
        }

        _verticalSpeedAcc = _jumpForce;
        _isJumping = true;
        _isInAir = true;
        ChangeAnimation(Clip_jump);

        Vector3 desiredMove = Vector3.up * _verticalSpeedAcc * Time.deltaTime;

        _characterController.Move(desiredMove);
    }


    private void Rotate(Vector3 direction, float ratio = 1.0f)
    {
        float deltaDEG = Vector3.Angle(transform.forward.normalized, direction);

        if (deltaDEG > 180.0f)
        {
            deltaDEG -= 180.0f;
        }

        float nextDeltaDEG = _rotatingSpeedDeg * Time.deltaTime * ratio;

        if (nextDeltaDEG >= deltaDEG)
        {
            transform.LookAt(transform.position + direction);
        }
        else
        {
            float isLeftRotate = Vector3.Cross(transform.forward.normalized, direction).y;
            if (isLeftRotate <= 0.0f)
            {
                nextDeltaDEG *= -1.0f;
            }
            transform.Rotate(transform.up, nextDeltaDEG);
        }
    }

    



    private bool DiffState_Detected()
    {
        ChangeAnimation(Clip_Scream);

        float delay = Clip_Scream.length;

        StartCoroutine(ChangeStateCoroutine(delay, MonstarState.Chase));

        return true;
    }

    IEnumerator ChangeStateCoroutine(float delay, MonstarState state)
    {
        yield return new WaitForSeconds(delay);
        _currState = state;
    }

    private void GravityUpdate()
    {
        if (_characterController.isGrounded == true)
        {
            if (_isInAir == true)
            {
                //직전에 점프를 하고 있었습니다.
                ChangeAnimation(_prevAnimClip);
            }
            _verticalSpeedAcc = 0.0f;

            _isJumping = false;
            _isInAir = false;
        }

        _verticalSpeedAcc += Physics.gravity.y * _mass * Time.deltaTime;

        Vector3 gravityMove = Time.deltaTime *_verticalSpeedAcc * Vector3.up;

        Debug.Assert(Mathf.Abs(gravityMove.y) >= float.Epsilon, "부유하는 움직임입니다.");

        _characterController.Move(gravityMove);
    }

    private void Move(Vector3 direction, float ratio = 1.0f)
    {
        float similarity = Mathf.Clamp(Vector3.Dot(transform.forward.normalized, direction.normalized), 0.0f, 1.0f);

        Vector3 desiredMove = direction * _movingSpeed * Time.deltaTime * similarity * ratio;

        if (_characterController != null)
        {
            _characterController.Move(desiredMove);
        }
        else
        {
            transform.position += desiredMove;
        }
    }

    private void MonsterFSM()
    {
        int changeCount = 0;

        if (_prevState != _currState)
        {
            if (_stateEndWithoutInterCept == false)//인터셉트(외부 요인에 의해서 강제로 바뀐경우
            {
                EndStateIntercepted();
            }

            while (changeCount < 3)
            {
                bool changeSuccess = DiffState();

                if (changeSuccess == false)
                {
                    changeCount++;
                    continue;
                }

                break;
            }
            _stateEndWithoutInterCept = false;
        }

        Debug.Assert(changeCount < 3, "상태 변환 횟수가 계속 늘어나려 합니다. 로직에 문제가 있습니까?");

        _prevState = _currState;

        switch (_currState)
        {
            case MonstarState.Idle:
                FSM_InIdle();
                break;
            case MonstarState.PatrolMove:
                FSM_InPatrol();
                break;
            case MonstarState.Detected:
                FSM_InDetected();
                break;
            case MonstarState.Chase:
                FSM_InChase();
                break;
            case MonstarState.Attack:
                FSM_InNormalAttack();
                break;
            case MonstarState.Shoot:
                break;
            case MonstarState.Dead:
                break;
            case MonstarState.End:
                break;
            default:
                break;
        }

        if (_stateEndWithoutInterCept == true) //계획된 동작을 제대로 끝마친 경우
        {
            EndStateWell();
        }
    }

    private bool DiffState()
    {
        switch (_currState)
        {
            case MonstarState.Idle:
                return DiffState_Idle();
            case MonstarState.PatrolMove:
                return DiffState_Patrol();
            case MonstarState.Detected:
                return DiffState_Detected();
            case MonstarState.Chase:
                return DiffState_Chase();
            case MonstarState.Attack:
                return DiffState_NormalAttack();
            case MonstarState.Shoot:
                return true;
            case MonstarState.Dead:
                return DiffState_Dead();
            case MonstarState.End:
                return true;
            default:
                return true;
        }
    }


    /*---------------------------------------------------------------------------------------------------------------------
    |TODO| EndStateIntercepted 와 EndStateWell 두개를 신경써야 합니까? 너무 번거롭다 뭔가 다른 구조는 없을까
    ---------------------------------------------------------------------------------------------------------------------*/
    private void EndStateIntercepted()
    {
        switch (_prevState)
        {
            case MonstarState.Idle:
                break;
            case MonstarState.PatrolMove:
                _characterController.Move(new Vector3(0.0f, 0.0f, 0.0f));
                break;
            case MonstarState.Detected:
                break;
            case MonstarState.Chase:
                _characterController.Move(new Vector3(0.0f, 0.0f, 0.0f));
                break;
            case MonstarState.Shoot:
                break;
            case MonstarState.Dead:
                break;
            case MonstarState.End:
                break;
            default:
                break;
        }
    }

    private void EndStateWell()
    {
        switch (_currState)
        {
            case MonstarState.Idle:
                ChooseRandomState_NoBattle();
                break;
            case MonstarState.PatrolMove:
                ChooseRandomState_NoBattle();
                _characterController.Move(new Vector3(0.0f, 0.0f, 0.0f));
                break;
            case MonstarState.Detected:
                break;
            case MonstarState.Chase:
                _characterController.Move(new Vector3(0.0f, 0.0f, 0.0f));
                break;
            case MonstarState.Shoot:
                break;
            case MonstarState.Dead:
                break;
            case MonstarState.End:
                break;
            default:
                break;
        }
    }

    private void ChooseRandomState_NoBattle()
    {
        _stateAcc = 0.0f;
        int random = Random.Range(0, (int)MonstarState.Detected);
        _currState = (MonstarState)random;
    }

    private void ChangeAnimation(AnimationClip targetClip)
    {
        AnimatorClipInfo[] currentClipInfos = _animator.GetCurrentAnimatorClipInfo(0);

        AnimatorClipInfo currentClipInfo = currentClipInfos[0];

        AnimationClip currentClip = currentClipInfo.clip;

        if (currentClip == targetClip)
        {
            return;
        }

        string nextNode = (_currAnimNode == true) ? "State2" : "State1";
        _currAnimNode = !_currAnimNode;
        if (_currAnimNode == true)
        {
            _overrideController[_targetName1] = targetClip;
        }
        else
        {
            _overrideController[_targetName2] = targetClip;
        }
        
        _animator.CrossFade(nextNode, _crossFadeTime);

        _prevAnimClip = currentClip;
        _currAnimClip = targetClip;
    }

    private bool DiffState_Idle()
    {
        ChangeAnimation(Clip_Idle);
        return true;
    }

    private bool DiffState_Chase()
    {
        ChangeAnimation(Clip_Run);

        if (_battleTarget == null)
        {
            _battleTarget = _debuggingPlayer;
            _currState = MonstarState.Idle;
            return true;
        }

        Vector3 startPosition = transform.position;
        Vector3 endPosition = _battleTarget.transform.position;

        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        filter.areaMask = (_filterTarget == -1) ? NavMesh.AllAreas : 1 << _filterTarget;
        filter.agentTypeID = 0;

        FindPath(startPosition, endPosition, filter);

        switch (_navMeshPath.status)
        {
            case NavMeshPathStatus.PathComplete:
                break;

            case NavMeshPathStatus.PathPartial: //거꾸로 뒤집어서 한번 더 찾아본다
                {
                    NavMeshPath temp = _navMeshPath;
                    FindPath(endPosition, startPosition, filter);

                    switch (_navMeshPath.status)
                    {
                        case NavMeshPathStatus.PathComplete:
                            System.Array.Reverse(_navMeshPath.corners);
                            break;
                        case NavMeshPathStatus.PathPartial:
                            _navMeshPath = temp;
                            break;
                        case NavMeshPathStatus.PathInvalid:
                            _currState = MonstarState.Idle;
                            return true;
                        default:
                            break;
                    }
                }
                break;
            case NavMeshPathStatus.PathInvalid:
                _currState = MonstarState.Idle;
                return true;

            default:
                break;
        }

        CalculateOffMeshLinks();

        CreateDebugRoute();

        return true;
    }

    private void FindPath(Vector3 startPosition, Vector3 endPosition, NavMeshQueryFilter filter)
    {
        _targetPathPositionIndex = 0;

        NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

        NavigationManager.Instance.DeActiveAllNavMesh();

        NavigationManager.Instance.ActiveNavMesh(shortestNav);

        if (shortestNav == null)
        {
            Debug.Assert(shortestNav != null, "맵에 네비게이션이 없을수도 있습니다. 아직 해당로직을 처리하지 않았습니다");
            return;
        }

        NavMesh.SamplePosition(endPosition, out _navMeshHit, 1.0f, filter);

        NavMesh.CalculatePath(startPosition, endPosition, filter, _navMeshPath);
    }


    private void CalculateOffMeshLinks()
    {
        _isOffMeshLinks.Clear();
        
        _isOffMeshLinks.Add(false); //길찾기를 시작한 순간, 시작 지점은 무조건 땅에 닿아있다고 가정합니다.

        for (int i = 1; i < _navMeshPath.corners.Length - 1; i++)
        {
            Vector3 startPosition = _navMeshPath.corners[i];
            Vector3 endPosition = _navMeshPath.corners[i + 1];
            Vector3 betweenPosition = (startPosition + endPosition) / 2.0f;

            bool samplingSuccess = NavMesh.SamplePosition(betweenPosition, out _navMeshHitFindOffMesh, _minOffMeshSampleRadius, 1);

            if (samplingSuccess == true) //이어져있는 메쉬입니다
            {
                _isOffMeshLinks.Add(false);
                continue;
            }

            /*--------------------------------------------------------------
            |TODO| 자유낙하로 도달할 수 있는 지점인지 판별하는 로직 생각해볼것
            --------------------------------------------------------------*/
            //if (false /*자유낙하로 도달할 수 있습니까?*/) //점프하지 않아도 도달할 수 있습니다.
            //{
            //    _isOffMeshLinks.Add(false);
            //    continue;
            //}


            //양 끝이 OffMeshLink이기에 2개를 추가합니다 //true true true 3연속 true는 있을수 없다고 가정합니다. 그냥 코너가 그래
            _isOffMeshLinks.Add(true); _isOffMeshLinks.Add(true);
            i++;
        }

        _isOffMeshLinks.Add(false); //길찾기를 끝낸 지점은 무조건 땅에 닿아있다고 가정합니다.
    }





    private void FSM_InDetected()
    {
        Vector3 targetPosition = _battleTarget.transform.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition);
    }



    private void FSM_InIdle()
    {
        _stateAcc += Time.deltaTime;

        if (_stateAcc > _idleStateTarget)
        {
            _stateEndWithoutInterCept = true;
        }

        if (InBattleCheck() == true)
        {
            _currState = MonstarState.Detected;
            _stateEndWithoutInterCept = true;
        }
    }




    private void FSM_InNormalAttack()
    {
        Vector3 targetPosition = _battleTarget.transform.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition);
    }




    private bool DiffState_NormalAttack()
    {
        ChangeAnimation(Clip_Punch);

        float delay = Clip_Punch.length;

        StartCoroutine(ChangeStateCoroutine(delay, MonstarState.Chase));

        return true;
    }




    private void FSM_InChase()
    {
        if (InAttackRangeCheck(_normalAttackRange, 1.0f) == true)
        {
            _currState = MonstarState.Attack;
            _stateEndWithoutInterCept = true;
            return;
        }

        NavPathRotate(_chaseRotateSpeedRatio);

        if (NavPathMove(_chaseSpeedRatio) == true)
        {
            DiffState_Chase();
        }
    }




    private void FSM_InPatrol()
    {
        if (InBattleCheck() == true)
        {
            _currState = MonstarState.Detected;
            _stateEndWithoutInterCept = true;
            return;
        }

        NavPathRotate(_patrolRotateSpeedRatio);

        if (NavPathMove(_patrolSpeedRatio) == true)
        {
            _stateEndWithoutInterCept = true;
        }
    }




    private bool DiffState_Patrol()
    {
        ChangeAnimation(Clip_Walk);

        _targetPathPositionIndex = 0;

        Vector3 startPosition = transform.position;

        NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

        NavigationManager.Instance.DeActiveAllNavMesh();

        NavigationManager.Instance.ActiveNavMesh(shortestNav);

        if (shortestNav == null)
        {
            Debug.Assert(shortestNav != null, "맵에 네비게이션이 없을수도 있습니다. 아직 해당로직을 처리하지 않았습니다");
            _currState = MonstarState.Idle;
            return true;
        }

        Vector3 navPosition = shortestNav.transform.position;

        float RandomDegree = Random.Range(0.0f, Mathf.PI * 2.0f);

        Vector3 randomedDirection = new Vector3(Mathf.Cos(RandomDegree), 0.0f, Mathf.Sin(RandomDegree));

        Vector3 randomedPosition = 24.0f * randomedDirection + navPosition;

        NavMesh.SamplePosition(randomedPosition, out _navMeshHit, 1.0f, NavMesh.AllAreas);

        Vector3 endPosition = _navMeshHit.position;

        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        filter.areaMask = (_filterTarget == -1) ? NavMesh.AllAreas : 1 << _filterTarget;
        filter.agentTypeID = 0;

        FindPath(startPosition, endPosition, filter);

        switch (_navMeshPath.status)
        {
            case NavMeshPathStatus.PathComplete:
                break;

            case NavMeshPathStatus.PathPartial: //거꾸로 뒤집어서 한번 더 찾아본다
                {
                    NavMeshPath temp = _navMeshPath;
                    FindPath(endPosition, startPosition, filter);

                    switch (_navMeshPath.status)
                    {
                        case NavMeshPathStatus.PathComplete:
                            System.Array.Reverse(_navMeshPath.corners);
                            break;
                        case NavMeshPathStatus.PathPartial:
                            _navMeshPath = temp;
                            break;
                        case NavMeshPathStatus.PathInvalid:
                            _currState = MonstarState.Idle;
                            return true;
                        default:
                            break;
                    }
                }
                break;
            case NavMeshPathStatus.PathInvalid:
                _currState = MonstarState.Idle;
                return true;

            default:
                break;
        }

        CalculateOffMeshLinks();

        CreateDebugRoute();

        return true;
    }



    private void NavPathRotate(float speedRatio = 1.0f)
    {
        if ((_navMeshPath.corners.Length - 1) <= _targetPathPositionIndex)
        {
            Debug.Log("Nav End");//다 갔습니다
            return;
        }

        Vector3 targetPosition = _navMeshPath.corners[_targetPathPositionIndex + 1];
        targetPosition.y = transform.position.y;

        Vector3 rotateDirection = (targetPosition - transform.position);
        rotateDirection.y = 0.0f;
        rotateDirection = rotateDirection.normalized;

        Rotate(rotateDirection, speedRatio);
    }

    private bool NavPathMove(float speedRatio = 1.0f)
    {
        for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
        {
            Debug.DrawLine(_navMeshPath.corners[i], _navMeshPath.corners[i + 1], Color.green);
        }

        if ((_navMeshPath.corners.Length - 1) <= _targetPathPositionIndex)
        {
            Debug.Log("Nav End");//다 갔습니다
            return true;
        }

        Vector3 targetPosition = _navMeshPath.corners[_targetPathPositionIndex + 1];
        targetPosition.y = transform.position.y;

        Vector3 pathDirection = (targetPosition - transform.position).normalized; // = 내가 진행해야 할 방향

        float similarity = Mathf.Clamp(Vector3.Dot(pathDirection, transform.forward.normalized), 0.0f, 1.0f);

        float deltaDistance = _movingSpeed * similarity * Time.deltaTime * _patrolRotateSpeedRatio; //다음 프레임엔 이만큼 움직일 것이다

        float targetDistance = Vector3.Distance(targetPosition, transform.position);

        if (targetDistance <= deltaDistance)
        {
            _targetPathPositionIndex++;
        }

        CheckJumpPosition();

        if ((_navMeshPath.corners.Length - 1) <= _targetPathPositionIndex)
        {
            Debug.Log("Nav End");//다 갔습니다
            Vector3 lastPosition = _navMeshPath.corners[_navMeshPath.corners.Length - 1];
            lastPosition.y = transform.position.y;
            float distance = Vector3.Distance(lastPosition, transform.position);
            if (distance > 0.5f)
            {
                Debug.Log("패트롤을 종료했는데 거리가 너무 멀다. 거리 : " + distance);
            }

            return true;
        }

        Move(pathDirection, speedRatio);

        if (_characterController.velocity.magnitude <= float.Epsilon && similarity > float.Epsilon)
        {
            //Debug.Log("벡터의 내적이 존재함에도 불구하고 안움직이고 있다");
        }

        return false;
    }





















    /*-----------------------------------
     * Debugging Section
    -----------------------------------*/

    private void Debug_ChangeState(MonstarState state)
    {
        _currState = state;
    }

    private void FindAndDrawPathOnlyJump()
    {
        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        if (_filterTarget == -1)
        {
            filter.areaMask = NavMesh.AllAreas;
        }
        else
        {
            filter.areaMask = 1 << _filterTarget;
        }

        NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

        NavigationManager.Instance.DeActiveAllNavMesh();

        NavigationManager.Instance.ActiveNavMesh(shortestNav);

        Vector3 randomedPosition = _debuggingPlayer.transform.position;


        NavMesh.SamplePosition(randomedPosition, out _navMeshHit, 30.0f, filter);

        Vector3 patrolPosition = _navMeshHit.position;
        NavMesh.CalculatePath(transform.position, patrolPosition, filter, _navMeshPath);

        DebugLog_PathFindingResult(true);
    }

    private void FindClosetEdge()
    {
        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        if (_filterTarget == -1)
        {
            filter.areaMask = NavMesh.AllAreas;
        }
        else
        {
            filter.areaMask = 1 << _filterTarget;
        }

        NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

        NavigationManager.Instance.DeActiveAllNavMesh();

        NavigationManager.Instance.ActiveNavMesh(shortestNav);

        NavMesh.FindClosestEdge(transform.position, out _navMeshHit, filter);


        GameObject createdSphere = Instantiate(_debuggingCornerSpherePrefab);
        createdSphere.transform.position = _navMeshHit.position;
        _createdDebuggingCornerSphere.Add(createdSphere);
    }

    private void CalculateTriangulation()
    {
        NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

        NavigationManager.Instance.DeActiveAllNavMesh();

        NavigationManager.Instance.ActiveNavMesh(shortestNav);

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
    }

    private void FindAndDrawPathReverse()
    {
        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        if (_filterTarget == -1)
        {
            filter.areaMask = NavMesh.AllAreas;
        }
        else
        {
            filter.areaMask = 1 << _filterTarget;
        }

        NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

        NavigationManager.Instance.DeActiveAllNavMesh();

        NavigationManager.Instance.ActiveNavMesh(shortestNav);

        Vector3 randomedPosition = transform.position;

        NavMesh.SamplePosition(randomedPosition, out _navMeshHit, 1.0f, filter);

        Vector3 patrolPosition = _navMeshHit.position;

        NavMesh.CalculatePath(_debuggingPlayer.transform.position, patrolPosition, filter, _navMeshPath);

        DebugLog_PathFindingResult(true, true);
    }

    private void DebugLog_PathFindingResult(bool drawRoute = false, bool color = false)
    {
        if (_navMeshPath.status == NavMeshPathStatus.PathInvalid)
        {
            Debug.Log("경로찾기 실패");
            return;
        }

        if (_navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            Debug.Log("경로찾기 성공");
        }

        if (_navMeshPath.status == NavMeshPathStatus.PathPartial)
        {
            Debug.Log("경로찾기 쪼개짐");
        }

        if (drawRoute == true)
        {
            CreateDebugRoute(color);
        }
    }

    private void FindAndDrawPath()
    {
        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        if (_filterTarget == -1)
        {
            filter.areaMask = NavMesh.AllAreas;
        }
        else
        {
            filter.areaMask = 1 << _filterTarget;
        }


        //경로계산 -> 꼭지점마다 구 설치
        NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

        NavigationManager.Instance.DeActiveAllNavMesh();

        NavigationManager.Instance.ActiveNavMesh(shortestNav);

        Vector3 randomedPosition = _debuggingPlayer.transform.position;

        NavMesh.SamplePosition(randomedPosition, out _navMeshHit, 2.0f, filter);

        Vector3 patrolPosition = _navMeshHit.position;

        NavMesh.CalculatePath(transform.position, patrolPosition, filter, _navMeshPath);

        DebugLog_PathFindingResult(true);
    }

    private void CreateDebugRoute(bool color = false)
    {
        for (int i = 0; i < _navMeshPath.corners.Length; i++)
        {
            GameObject createdSphere = Instantiate(_debuggingCornerSpherePrefab);
            createdSphere.transform.position = _navMeshPath.corners[i];
            _createdDebuggingCornerSphere.Add(createdSphere);
            MeshRenderer meshRenderer = createdSphere.GetComponent<MeshRenderer>();
            if (color == true)
            {
                meshRenderer.material.color = Color.yellow;
            }
            

            if (i == _navMeshPath.corners.Length - 1)
            {
                break;
            }

            GameObject createdCapsule = Instantiate(_debuggingCornerCapsulePrefab);
            Vector3 firstPosition = _navMeshPath.corners[i];
            Vector3 secondPosition = _navMeshPath.corners[i + 1];
            createdCapsule.transform.position = (firstPosition + secondPosition) / 2.0f;

            Vector3 targetUp = (secondPosition - firstPosition).normalized;
            createdCapsule.transform.up = targetUp;
            Vector3 localScale = createdCapsule.transform.localScale;
            localScale.y = Vector3.Distance(firstPosition, secondPosition) / 2.0f;
            createdCapsule.transform.localScale = localScale;
            _createdDebuggingCornerCapsule.Add(createdCapsule);
            meshRenderer = createdCapsule.GetComponent<MeshRenderer>();
            if (color == true)
            {
                meshRenderer.material.color = Color.yellow;
            }
        }
    }

    private void DeletePath()
    {
        foreach (var item in _createdDebuggingCornerSphere)
        {
            Destroy(item);
        }
        foreach (var item in _createdDebuggingCornerCapsule)
        {
            Destroy(item);
        }
        foreach (var item in _createdDebuggingCornerSphereReverse)
        {
            Destroy(item);
        }
        foreach (var item in _createdDebuggingCornerCapsuleReverse)
        {
            Destroy(item);
        }

        _createdDebuggingCornerSphere.Clear();
        _createdDebuggingCornerCapsule.Clear();
        _createdDebuggingCornerSphereReverse.Clear();
        _createdDebuggingCornerCapsuleReverse.Clear();
    }



















    /*-----------------------------------
     * Trash
    -----------------------------------*/

    //private void UpdateAnimation()
    //{
    //    _AnimController.UpdateParameter("IsAim", _isAim);
    //    _AnimController.UpdateParameter("IsInAir", _isInAir);
    //    _AnimController.UpdateParameter("IsJump", _isJumping);
    //    Vector2 characterControllerVelocity = new Vector2(_characterController.velocity.x, _characterController.velocity.z);
    //    _AnimController.UpdateParameter("MovingSpeed", characterControllerVelocity.magnitude);
    //    Vector3 animDir = new Vector3(0.0f, 0.0f, 1.0f);
    //    if (_isAim == true)
    //    {
    //        //animDir = _animDir;
    //    }
    //    _AnimController.UpdateParameter("RelativeMovingZ", animDir.z);
    //    _AnimController.UpdateParameter("RelativeMovingX", animDir.x);
    //}
}
