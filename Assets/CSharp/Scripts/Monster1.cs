using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using TMPro;
using UnityEngine.EventSystems;
public class Monster1 : MonoBehaviour
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
    private NavMeshPath _navMeshPath = null;
    private List<Vector3> _passedNavPositions = new List<Vector3>();
    private int _targetPathPositionIndex = 0;

    private void Awake()
    {
        _navMeshPath = new NavMeshPath(); //모노 비해이비어보다 먼저하면 안된단다.
    }

    void Update()
    {
        GravityUpdate();
        MonsterFSM();
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
    }

    private void Move(Vector3 direction, float ratio = 1.0f)
    {
        float similarity = Mathf.Clamp(Vector3.Dot(transform.forward.normalized, direction.normalized), 0.0f, 1.0f);

        //  float similarities = (_isAim == true)
        //? 1.0f
        //: Mathf.Clamp(Vector3.Dot(transform.forward, inputDirection), 0.0f, 1.0f);

        Vector3 desiredMove = direction * _movingSpeed * Time.deltaTime * similarity * ratio;

        desiredMove += Vector3.up * _verticalSpeedAcc * Time.deltaTime;
        
        Debug.Assert(Mathf.Abs(desiredMove.y) >= float.Epsilon, "부유하는 움직임입니다.");

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
        _AnimController.UpdateParameter("MovingSpeed", _characterController.velocity.magnitude / _movingSpeed);
        Vector3 animDir = new Vector3(0.0f, 0.0f, 1.0f);
        if (_isAim == true)
        {
            //animDir = _animDir;
        }
        _AnimController.UpdateParameter("RelativeMovingZ", animDir.z);
        _AnimController.UpdateParameter("RelativeMovingX", animDir.x);
    }

    private void LateUpdate()
    {
        UpdateAnimation();
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
                _stateEnd = FSM_InIdle();
                break;
            case MonstarState.PatrolMove:
                _stateEnd = FSM_InPatrolMove();
                break;
            case MonstarState.Detected:
                break;
            case MonstarState.Chase:
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
                return true;
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
        //Debug.Log("My State is : " + _currState);

    }

    private bool FSM_InIdle()
    {
        _stateAcc += Time.deltaTime;

        if (_stateAcc > _idleStateTarget)
        {
            return true;
        }

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

            return true;
        }

        Move(pathDirection, _patrolSpeedRatio);

        if (_characterController.velocity.magnitude <= float.Epsilon && similarity > float.Epsilon)
        {
            //Debug.Log("벡터의 내적이 존재함에도 불구하고 안움직이고 있다");
        }

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
            //Debug.Assert(_navMeshPath.status == NavMeshPathStatus.PathComplete, "경로찾기가 실패했습니다, 상태가 실패했습니다");
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
            //Debug.Assert(_navMeshPath.status == NavMeshPathStatus.PathComplete, "경로찾기가 실패했습니다. 정점이 0개 입니다");
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
