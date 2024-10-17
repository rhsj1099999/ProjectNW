using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using TMPro;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

public class ZombieStart : MonoBehaviour
{
    public enum MonstarState
    {
        Idle,
        PatrolMove,
        Detected,
        Chase,
        Shoot,
        Dead,
        End,
    }

    //StateSection
    [SerializeField] private MonstarState _currState = MonstarState.Idle;
    [SerializeField] private MonstarState _prevState = MonstarState.Idle;
    [SerializeField] private bool _stateEnd = false;
    [SerializeField] private float _detectRange = 3.0f;
    [SerializeField] private float _detectRayCastTick = 1.0f;
    [SerializeField] private float _detectHorizontalFovDeg = 60.0f;
    [SerializeField] private float _stateAcc = 0.0f;
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


    private List<GameObject> _createdDebuggingCornerSphere = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerCapsule = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerSphereReverse = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerCapsuleReverse = new List<GameObject>();



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
    private float _debuggingDistance = Mathf.Infinity;
    private int _debuggingDistanceCount = 0;
    private int _debuggingDistanceMax = 0;

    //Animator Section
    [SerializeField] private AnimContoller _AnimController = null;



    private float _navModifier = 0.14f;
    private NavMeshHit _navMeshHit;
    private NavMeshHit _navMeshHitFindOffMesh;
    private NavMeshPath _navMeshPath = null;
    private List<Vector3> _passedNavPositions = new List<Vector3>();
    private int _targetPathPositionIndex = 0;
    private NavMeshAgent _navAgent = null;
    private List<bool> _isOffMeshLinks = new List<bool>();

    private void Awake()
    {
        _navMeshPath = new NavMeshPath(); //모노 비해이비어보다 먼저하면 안된단다.
        _navAgent = GetComponentInChildren<NavMeshAgent>();
    }

    void Update()
    {
        MonsterFSM();

        if (Input.GetKeyDown(KeyCode.Alpha2) == true)
        {
            FindAndDrawPath();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) == true)
        {
            FindAndDrawPathReverse();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) == true)
        {
            FindClosetEdge();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5) == true)
        {
            FindAndDrawPathOnlyJump();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0) == true)
        {
            DeletePath();
        }

        //if (_navAgent.isOnOffMeshLink == true)
        //{
        //    _navAgent.isStopped = true;
        //}
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

        foreach (var item in _navMeshPath.corners)
        {
            GameObject createdSphere = Instantiate(_debuggingCornerSpherePrefab);
            createdSphere.transform.position = item;
            _createdDebuggingCornerSphere.Add(createdSphere);
        }

        for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
        {
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
        }
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

        int a = 10;
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

        foreach (var item in _navMeshPath.corners)
        {
            GameObject createdSphere = Instantiate(_debuggingCornerSpherePrefab);
            createdSphere.transform.position = item;
            _createdDebuggingCornerSphere.Add(createdSphere);
        }

        for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
        {
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

        NavMesh.SamplePosition(randomedPosition, out _navMeshHit, 1.0f, filter);

        Vector3 patrolPosition = _navMeshHit.position;

        NavMesh.CalculatePath(transform.position, patrolPosition, filter, _navMeshPath);

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

        foreach (var item in _navMeshPath.corners)
        {
            GameObject createdSphere = Instantiate(_debuggingCornerSpherePrefab);
            createdSphere.transform.position = item;
            _createdDebuggingCornerSphere.Add(createdSphere);
        }

        for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
        {
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

    private void LateUpdate()
    {
        GravityUpdate();

        UpdateAnimation();
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

    private void GravityUpdate()
    {
        if (_characterController.isGrounded == true)
        {
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

        //  float similarities = (_isAim == true)
        //? 1.0f
        //: Mathf.Clamp(Vector3.Dot(transform.forward, inputDirection), 0.0f, 1.0f);

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

    private void UpdateAnimation()
    {
        _AnimController.UpdateParameter("IsAim", _isAim);
        _AnimController.UpdateParameter("IsInAir", _isInAir);
        _AnimController.UpdateParameter("IsJump", _isJumping);
        Vector2 agentPlaneVelocity = new Vector2(_navAgent.velocity.x, _navAgent.velocity.z);
        Vector2 characterControllerVelocity = new Vector2(_characterController.velocity.x, _characterController.velocity.z);
        float moveVelocity = Mathf.Max(agentPlaneVelocity.magnitude, characterControllerVelocity.magnitude);
        _AnimController.UpdateParameter("MovingSpeed", moveVelocity);
        Vector3 animDir = new Vector3(0.0f, 0.0f, 1.0f);
        if (_isAim == true)
        {
            //animDir = _animDir;
        }
        _AnimController.UpdateParameter("RelativeMovingZ", animDir.z);
        _AnimController.UpdateParameter("RelativeMovingX", animDir.x);
    }








    private void MonsterFSM()
    {
        int changeCount = 0;

        if (_prevState != _currState || _stateEnd == true)
        {
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
            _stateEnd = false;
        }

        Debug.Assert(changeCount < 3, "상태 변환 횟수가 계속 늘어나려 합니다. 로직에 문제가 있습니까?");

        _prevState = _currState;

        switch (_currState)
        {
            case MonstarState.Idle:
                FSM_InIdle();
                break;
            case MonstarState.PatrolMove:
                FSM_InPatrolMove();
                break;
            case MonstarState.Detected:
                break;
            case MonstarState.Chase:
                FSM_InChase();
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

        if (_stateEnd == true)
        {
            EndState();
        }
    }

    private bool DiffState()
    {
        switch (_currState)
        {
            case MonstarState.Idle:
                return true;
            case MonstarState.PatrolMove:
                return DiffState_Patrol();
            case MonstarState.Detected:
                return true;
            case MonstarState.Chase:
                return DiffState_Chase();
            case MonstarState.Shoot:
                return true;
            case MonstarState.Dead:
                return true;
            case MonstarState.End:
                return true;
            default:
                return true;
        }
    }

    private void EndState()
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
        int random = Random.Range(0, (int)MonstarState.PatrolMove);
        _currState = (MonstarState)random;
        //Debug.Log("My State is : " + _currState);
    }



    private bool DiffState_Chase()
    {
        _targetPathPositionIndex = 0;

        NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

        NavigationManager.Instance.DeActiveAllNavMesh();

        NavigationManager.Instance.ActiveNavMesh(shortestNav);

        if (shortestNav == null)
        {
            Debug.Assert(shortestNav != null, "맵에 네비게이션이 없을수도 있습니다. 아직 해당로직을 처리하지 않았습니다");
            _currState = MonstarState.Idle;
            return true;
        }

        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        if (_filterTarget == -1)
        {
            filter.areaMask = NavMesh.AllAreas;
        }
        else
        {
            filter.areaMask = 1 << _filterTarget;
        }

        Vector3 randomedPosition = _debuggingPlayer.transform.position;

        NavMesh.SamplePosition(randomedPosition, out _navMeshHit, 1.0f, filter);

        Vector3 patrolPosition = _navMeshHit.position;

        NavMesh.CalculatePath(transform.position, patrolPosition, filter, _navMeshPath);


        //_navAgent.SetDestination(patrolPosition);
        _navAgent.SetPath(_navMeshPath);
        _navAgent.updatePosition = false;
        _navAgent.updateRotation = false;

        Vector3 betweenCorner = (_navMeshPath.corners[0] + _navMeshPath.corners[1]) / 2.0f;
        bool isEmpty = NavMesh.SamplePosition(betweenCorner, out _navMeshHitFindOffMesh, 0.01f, NavMesh.AllAreas);
        if (isEmpty == true) 
        {
            int a = 10;
        }
        betweenCorner = (_navMeshPath.corners[1] + _navMeshPath.corners[2]) / 2.0f;
        isEmpty = NavMesh.SamplePosition(betweenCorner, out _navMeshHitFindOffMesh, 0.01f, NavMesh.AllAreas);
        if (isEmpty == true)
        {
            int a = 10;
        }
        betweenCorner = (_navMeshPath.corners[2] + _navMeshPath.corners[3]) / 2.0f;
        isEmpty = NavMesh.SamplePosition(betweenCorner, out _navMeshHitFindOffMesh, 0.01f, NavMesh.AllAreas);
        if (isEmpty == true)
        {
            int a = 10;
        }

        for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
        {
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
        }

        if (_navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            return true;
        }

        if (_navMeshPath.status == NavMeshPathStatus.PathPartial)
        {
            Debug.Log("경로찾기가 실패했습니다, 경로가 쪼개졌습니다");
            return true;
        }


        if (_navMeshPath.status == NavMeshPathStatus.PathInvalid)
        {
            Debug.Log("경로찾기가 실패했습니다, 상태가 실패했습니다");
            _currState = MonstarState.Idle;
            return true;
        }

        if (_navMeshPath.corners.Length >= 0)
        {
            Debug.Log("경로찾기가 실패했습니다. 정점이 0개 입니다");
            _currState = MonstarState.Idle;
            return true;
        }

        return false;
    }


    private void CalculateOffMeshLinks()
    {

    }



    private bool FSM_InChase()
    {
        //FSM_InPatrolRotate();

        for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
        {
            Debug.DrawLine(_navMeshPath.corners[i], _navMeshPath.corners[i + 1], Color.green);
        }

        Vector3 targetPosition = _navMeshPath.corners[_targetPathPositionIndex + 1];

        float targetDistance = Vector3.Distance(targetPosition, transform.position);

        if (targetDistance <= 0.1f)
        {
            _targetPathPositionIndex++;
        }

        //_navAgent.nextPosition = transform.position;

        if (_navAgent.nextOffMeshLinkData.offMeshLink == null)
        {
            Debug.Log("OffMesh가 없읍이다");
        }

        if (_navAgent.nextOffMeshLinkData.offMeshLink == null)
        {
            Debug.Log("OffMesh가 있습니다");
        }

        if (_navAgent.isOnOffMeshLink == true)
        {
            Debug.Log("Link에 도달했습니다");
        }

        //return FSM_InPatrolMove();
        if ((_navMeshPath.corners.Length - 1) <= _targetPathPositionIndex)
        {
            Debug.Log("State Done_Patrol Move");//다 갔습니다
            Vector3 lastPosition = _navMeshPath.corners[_navMeshPath.corners.Length - 1];
            lastPosition.y = transform.position.y;
            float distance = Vector3.Distance(lastPosition, transform.position);

            if (distance > 0.5f)
            {
                Debug.Log("패트롤을 종료했는데 거리가 너무 멀다. 거리 : " + distance);
            }

            _stateEnd = true;
            return true;
        }
        _stateEnd = false;
        return false;
    }














    private bool FSM_InIdle()
    {
        _stateAcc += Time.deltaTime;

        if (_stateAcc > _idleStateTarget)
        {
            _stateEnd = true;
            return true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) == true)
        {
            _currState = MonstarState.Chase;
            _stateEnd = true;
            return true;
        }

        _stateEnd = false;
        return false;
    }

    private void FSM_InPatrolRotate()
    {
        if ((_navMeshPath.corners.Length - 1) <= _targetPathPositionIndex)
        {
            Debug.Log("State Done_Patrol Move");//다 갔습니다
            return;
        }

        Vector3 targetPosition = _navMeshPath.corners[_targetPathPositionIndex + 1];
        //targetPosition.y -= _navModifier;
        targetPosition.y = transform.position.y;

        Vector3 rotateDirection = (targetPosition - transform.position);
        rotateDirection.y = 0.0f;
        rotateDirection = rotateDirection.normalized;

        Rotate(rotateDirection, _patrolRotateSpeedRatio);
    }

    private bool FSM_InPatrolMove()
    {
        FSM_InPatrolRotate(); //회전을 수행했다.

        for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
        {
            Debug.DrawLine(_navMeshPath.corners[i], _navMeshPath.corners[i + 1], Color.green);
        }

        Vector3 targetPosition = _navMeshPath.corners[_targetPathPositionIndex + 1];
        //targetPosition.y -= _navModifier;
        targetPosition.y = transform.position.y;

        Vector3 pathDirection = (targetPosition - transform.position).normalized; // = 내가 진행해야 할 방향

        float similarity = Mathf.Clamp(Vector3.Dot(pathDirection, transform.forward.normalized), 0.0f, 1.0f);

        float deltaDistance = _movingSpeed * similarity * Time.deltaTime * _patrolRotateSpeedRatio; //다음 프레임엔 이만큼 움직일 것이다

        float targetDistance = Vector3.Distance(targetPosition, transform.position);

        if (targetDistance <= deltaDistance)
        {
            _targetPathPositionIndex++;
        }

        if ((_navMeshPath.corners.Length - 1) <= _targetPathPositionIndex)
        {
            Debug.Log("State Done_Patrol Move");//다 갔습니다
            Vector3 lastPosition = _navMeshPath.corners[_navMeshPath.corners.Length - 1];
            lastPosition.y = transform.position.y;
            float distance = Vector3.Distance(lastPosition, transform.position);
            if (distance > 0.5f)
            {
                Debug.Log("패트롤을 종료했는데 거리가 너무 멀다. 거리 : " + distance);
            }

            _stateEnd = true;
            return true;
            
        }

        Move(pathDirection, _patrolSpeedRatio);

        if (_characterController.velocity.magnitude <= float.Epsilon && similarity > float.Epsilon)
        {
            //Debug.Log("벡터의 내적이 존재함에도 불구하고 안움직이고 있다");
        }

        _stateEnd = false;
        return false;
    }

    private bool DiffState_Patrol()
    {
        _targetPathPositionIndex = 0;

        {
            /*---------------------------------------------------------------------------
           |TODO| return 할때마다 끄고 켜야하는데 이거 수정할것
           ---------------------------------------------------------------------------*/
            //자기 자신에게 NavObstacle Component가 달려있다면 잠시 비활성화 로직
            NavMeshObstacle navMeshObstacleComponent = gameObject.GetComponentInChildren<NavMeshObstacle>();
            if (navMeshObstacleComponent != null)
            {
                navMeshObstacleComponent.enabled = false;
            }
        }

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

        Vector3 patrolPosition = _navMeshHit.position;

        NavMesh.CalculatePath(transform.position, patrolPosition, NavMesh.AllAreas, _navMeshPath);

        if (_navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            {
                /*---------------------------------------------------------------------------
                |TODO| return 할때마다 끄고 켜야하는데 이거 수정할것
                ---------------------------------------------------------------------------*/
                NavMeshObstacle navMeshObstacleComponent = gameObject.GetComponentInChildren<NavMeshObstacle>();
                if (navMeshObstacleComponent != null)
                {
                    navMeshObstacleComponent.enabled = true;
                }
            }
            return true;
        }

        if (_navMeshPath.status != NavMeshPathStatus.PathComplete)
        {
            Debug.Assert(_navMeshPath.status == NavMeshPathStatus.PathComplete, "경로찾기가 실패했습니다, 상태가 실패했습니다");
            _currState = MonstarState.Idle;

            {
                /*---------------------------------------------------------------------------
                |TODO| return 할때마다 끄고 켜야하는데 이거 수정할것
                ---------------------------------------------------------------------------*/
                NavMeshObstacle navMeshObstacleComponent = gameObject.GetComponentInChildren<NavMeshObstacle>();
                if (navMeshObstacleComponent != null)
                {
                    navMeshObstacleComponent.enabled = true;
                }
            }
            return true;
        }


        if (_navMeshPath.corners.Length >= 0)
        {
            Debug.Assert(_navMeshPath.status == NavMeshPathStatus.PathComplete, "경로찾기가 실패했습니다. 정점이 0개 입니다");
            _currState = MonstarState.Idle;

            {
                /*---------------------------------------------------------------------------
                |TODO| return 할때마다 끄고 켜야하는데 이거 수정할것
                ---------------------------------------------------------------------------*/
                NavMeshObstacle navMeshObstacleComponent = gameObject.GetComponentInChildren<NavMeshObstacle>();
                if (navMeshObstacleComponent != null)
                {
                    navMeshObstacleComponent.enabled = true;
                }
            }
            return true;
        }

        {
            /*---------------------------------------------------------------------------
            |TODO| return 할때마다 끄고 켜야하는데 이거 수정할것
            ---------------------------------------------------------------------------*/
            NavMeshObstacle navMeshObstacleComponent = gameObject.GetComponentInChildren<NavMeshObstacle>();
            if (navMeshObstacleComponent != null)
            {
                navMeshObstacleComponent.enabled = true;
            }
        }
        return false;
    }
}
