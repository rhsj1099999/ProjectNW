using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Cinemachine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;
using Unity.VisualScripting;



public enum AimState
{
    eTPSAim,
    eSightAim,
    ENEND,
};


public class CharacterMoveScript : MonoBehaviour
{
    [SerializeField] private GameObject _debuggingCornerSpherePrefab = null;
    [SerializeField] private GameObject _debuggingCornerCapsulePrefab = null;
    private List<GameObject> _createdDebuggingCornerSphere = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerCapsule = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerSphereReverse = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerCapsuleReverse = new List<GameObject>();
    private float _navModifier = 0.14f;
    private NavMeshHit _navMeshHit;
    private NavMeshPath _navMeshPath = null;
    private List<Vector3> _passedNavPositions = new List<Vector3>();
    private int _targetPathPositionIndex = 0;
    private NavMeshAgent _navAgent = null;

    private Vector3 _animDir = Vector3.zero;


    [SerializeField] private InputController _inputController = null;
    [SerializeField] private AnimContoller _AnimController = null;
    [SerializeField] private CharacterController _physics = null;

    [SerializeField] private string _aimKey = "Fire2";
    [SerializeField] private float _mass = 30.0f;
    [SerializeField] private float _speed = 5.0f;
    [SerializeField] private float _rotatingSpeed_DEG = 90.0f;
    [SerializeField] private float _jumpForce = 3.0f;
    private bool _isJumping = false;
    private bool _isInAir = false;
    private bool _isAim = false;
    private bool _mouseLocked = false;
    private float _verticalSpeedAcc = 0.0f;

    [SerializeField] private GameObject _inventoryUIPrefab = null;
    private void Awake()
    {
        Debug.Assert(_inputController != null, "인풋컨트롤러가 없다");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5) == true)
        {
            FindClosetEdge();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0) == true)
        {
            DeletePath();
        }

        //마우스 디버깅
        {
            if (Input.GetKeyDown(KeyCode.Alpha8) == true)
            {
                _mouseLocked = !_mouseLocked;

                if (_mouseLocked == true)
                {
                    Cursor.lockState = CursorLockMode.Locked;  // 커서 고정을 해제
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;  // 커서 고정을 해제
                }

                Cursor.visible = _mouseLocked;
            }
        }


        //타임디버깅
        {
            if (Input.GetKeyDown(KeyCode.L))  // S키를 누르면 게임 속도를 느리게 함
            {
                Time.timeScale = 0.1f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // 물리적 시간 업데이트
            }

            if (Input.GetKeyDown(KeyCode.O))  // R키를 누르면 게임 속도를 정상으로 복원
            {
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
        }

        //인벤토리 오픈코드
        {
            if (_inputController.GetInventoryOpen() == true)
            {
                UIManager.Instance.TurnOnUI(_inventoryUIPrefab, this.gameObject);
            }
        }

        //달리기 코드
        float speedRatio = 1.0f;
        {
            if (Input.GetKey(KeyCode.LeftShift) == true)
            {
                speedRatio = 2.0f;
            }
            if (Input.GetKey(KeyCode.LeftShift) == false)
            {
                speedRatio = 1.0f;
            }
        }
        
        //조작에 의한 조준인지 Check
        {
            bool isAimed = Input.GetButton(_aimKey);
            _isAim = isAimed;
        }


        _animDir = _inputController._pr_directionByInput;

        if (_isAim == true)
        {
            CharacterAimRotation();
        }
        else
        {
            CharacterRotate(_animDir);
        }

        CharacterMove(_animDir, speedRatio); //누르면 이동하기

    }

    void FixedUpdate()
    {

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

    private void FindClosetEdge()
    {
        NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

        NavigationManager.Instance.DeActiveAllNavMesh();

        NavigationManager.Instance.ActiveNavMesh(shortestNav);
        NavMesh.FindClosestEdge(transform.position, out _navMeshHit, NavMesh.AllAreas);


        GameObject createdSphere = Instantiate(_debuggingCornerSpherePrefab);
        createdSphere.transform.position = _navMeshHit.position;
        _createdDebuggingCornerSphere.Add(createdSphere);
    }

    private void CharacterMove(Vector3 inputDirection, float ratio = 1.0f)
    {
        if (_physics.isGrounded == true)
        {
            _physics.stepOffset = 0.3f;
            _verticalSpeedAcc = 0.0f;

            _isJumping = false;
            _isInAir = false;
        }

        _verticalSpeedAcc += Physics.gravity.y * _mass * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) == true)
        {//점프 시도
            if (_physics.isGrounded == true)
            {
                _physics.stepOffset = 0.0f;
                _verticalSpeedAcc = _jumpForce;

                _isJumping = true;
                _isInAir = true;
            }
        }

        Vector3 cameraLook = Camera.main.transform.forward;
        cameraLook.y = 0.0f;
        cameraLook = cameraLook.normalized;
        inputDirection = (Quaternion.LookRotation(cameraLook) * inputDirection);

        float similarities = (_isAim == true) 
            ? 1.0f
            : Mathf.Clamp(Vector3.Dot(transform.forward, inputDirection), 0.0f, 1.0f);
        
        Vector3 desiredMove = inputDirection * _speed * Time.deltaTime * similarities * ratio;

        desiredMove += Vector3.up * _verticalSpeedAcc * Time.deltaTime;

        Debug.Assert(Mathf.Abs(desiredMove.y) >= float.Epsilon, "부유하는 움직임입니다.");

        if (_physics != null)
        {
            _physics.Move(desiredMove);
        }
        else
        {
            transform.position += desiredMove;
        }

        ////땅에 닿았는지 임시 디버깅
        //{
        //    if (_physics.isGrounded == true)
        //    {
        //        Debug.Log("Grounded");
        //    }
        //    else
        //    {
        //        Debug.Log("Not Grounded");
        //    }
        //}
    }





    private void LateUpdate()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        _AnimController.UpdateParameter("IsAim", _isAim);
        _AnimController.UpdateParameter("IsInAir", _isInAir);
        _AnimController.UpdateParameter("IsJump", _isJumping);
        Vector2 planeSpeed = new Vector2(_physics.velocity.x, _physics.velocity.z);
        _AnimController.UpdateParameter("MovingSpeed", planeSpeed.magnitude / _speed);
        Vector3 animDir = new Vector3(0.0f, 0.0f, 1.0f);
        if (_isAim == true)
        {
            animDir = _animDir;
        }
        _AnimController.UpdateParameter("RelativeMovingZ", animDir.z);
        _AnimController.UpdateParameter("RelativeMovingX", animDir.x);
    }

    private void CharacterAimRotation()
    {

    }

    private void CharacterRotate(Vector3 inputDirection, float ratio = 1.0f)
    {
        if (Camera.main != null)
        {
            Vector3 cameraLook = Camera.main.transform.forward;
            cameraLook.y = 0.0f;
            cameraLook = cameraLook.normalized;
            inputDirection = (Quaternion.LookRotation(cameraLook) * inputDirection);
        }

        float deltaDEG = Vector3.Angle(transform.forward.normalized, inputDirection);

        if (deltaDEG > 180.0f)
        {
            deltaDEG -= 180.0f;
        }

        float nextDeltaDEG = _rotatingSpeed_DEG * Time.deltaTime * ratio;

        if (nextDeltaDEG >= deltaDEG)
        {
            transform.LookAt(transform.position + inputDirection);
            return;
        }
        else
        {
            float isLeftRotate = Vector3.Cross(transform.forward.normalized, inputDirection).y;
            if (isLeftRotate <= 0.0f)
            {
                nextDeltaDEG *= -1.0f;
            }
            transform.Rotate(transform.up, nextDeltaDEG);
            return;
        }
    }






};
