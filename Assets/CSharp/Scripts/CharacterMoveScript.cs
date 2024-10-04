using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    private void Awake()
    {
        _inputController = GetComponent<InputController>(); //AI에 의한 컨트롤러건, 플레이어의 의지를 방향화 하는 컴포넌트건 뭐 하나라도 있어야한다
        if ( _inputController != null )
        {//없으면 터칠꺼다
            //Functions.ForceCrash();
        }
    }

    void Start()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.C) == true && _equipmentItemModelPrefab_MustDel != null)
        {
            //바지를 입은것과 동일한 효과를 내는 임시함수


            //모델 생성단계
            GameObject emptyGameObject = new GameObject("EquipmentModelDummy"); // "MyChildObject"는 생성된 오브젝트의 이름
            emptyGameObject.transform.SetParent(this.transform);
            emptyGameObject.transform.localPosition = Vector3.zero;

            //애니메이터 세팅 단계
            GameObject modelObject = Instantiate(_equipmentItemModelPrefab_MustDel, emptyGameObject.transform);
            Animator modelAnimator = modelObject.GetComponent<Animator>();
            if (modelAnimator == null)
            {
                modelAnimator = modelAnimator.AddComponent<Animator>();
            }
            RuntimeAnimatorController ownerController = _AnimController.GetAnimator().runtimeAnimatorController;
            RuntimeAnimatorController newController = Instantiate<RuntimeAnimatorController>(ownerController);
            modelAnimator.runtimeAnimatorController = newController;
            modelAnimator.avatar = _equipmentItemModelAvatar_MustDel;

            //브로드캐스터 연결단계
            AnimPropertyBroadCaster animpropertyBroadCaster = GetComponent<AnimPropertyBroadCaster>();
            animpropertyBroadCaster.AddAnimator(modelObject);
        }



        if (_inputController.GetInventoryOpen() == true)
        {
            UIManager.Instance.TurnOnUI(_inventoryUIPrefab, this.gameObject);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) == true)
        {
            _speed = 0.2f;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) == true)
        {
            _speed = 0.1f;
        }


        if (Input.GetKeyDown(KeyCode.K) == true)
        {
            _aimState += 1;
            _aimState = (AimState)((int)_aimState % (int)AimState.ENEND);
        }
        bool isAimed = Input.GetButton(_aimKey);
        if (_isAim != isAimed)
        {
            _aimOrbit.CallControlOn();

            if (_gunscript != null)
            {
                _gunscript._isAim = isAimed;
            }

            if (isAimed == true)
            {
                _sightCamera.enabled = false;
                _freeRunCamera.enabled = false;
                _tpsCamera.enabled = false;

                switch(_aimState)
                {
                    default:
                        break;
                    case AimState.eSightAim:
                        _sightCamera.enabled = true;
                        break;
                    case AimState.eTPSAim:
                        _tpsCamera.enabled = true;
                        break;
                }
            }

            if (isAimed == false)
            {
                _sightCamera.enabled = false;
                _freeRunCamera.enabled = false;
                _tpsCamera.enabled = false;

                _freeRunCamera.enabled = true;
            }
        }
        _isAim = isAimed;

        Vector3 inputDirection = _inputController._pr_directionByInput;
        UpdateAnimation(inputDirection);


        if (_isSteady == true)
        {
            //숨참기 = 정조준시 에임 떨림을 최소화
        }
    }

    void FixedUpdate()
    {
        Vector3 inputDirection = _inputController._pr_directionByInput;

        if (_isAim == true)
        {
            CharacterAimRotation_2();
        }
        else
        {
            CharacterRotate(inputDirection);
        }
        
        CharacterMove(inputDirection); //누르면 이동하기
    }



    private void CharacterMove(Vector3 inputDirection)
    {
        if (_physics != null)
        {//AirCheck
            if (_physics.isGrounded == true)
            {
                _verticalSpeedAcc = 0.0f;

                _isJumping = false;
                _isInAir = false;
            }

            _verticalSpeedAcc += Physics.gravity.y * _mass * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space) == true)
            {//점프 시도
                if (_physics.isGrounded == true)
                {
                    _verticalSpeedAcc = _jumpForce;

                    _isJumping = true;
                    _isInAir = true;
                }
            }
        }


        if (_camera != null)
        {
            Vector3 cameraLook = Camera.main.transform.forward;
            cameraLook.y = 0.0f;
            cameraLook = cameraLook.normalized;
            inputDirection = (Quaternion.LookRotation(cameraLook) * inputDirection);
        }

        float similarities = (_isAim == true) 
            ? 1.0f
            : Mathf.Clamp(Vector3.Dot(transform.forward, inputDirection), 0.0f, 1.0f);
        
        Vector3 desiredMove = inputDirection * _speed * Time.deltaTime * similarities;

        desiredMove += Vector3.up * _verticalSpeedAcc * Time.deltaTime;

        if (Mathf.Abs(desiredMove.y) <= float.Epsilon && _physics.isGrounded == true)
        {
            //Crash
        }

        if (_physics != null)
        {
            _physics.Move(desiredMove);
        }
        else
        {
            transform.position += desiredMove;
        }
        
        _latestDesiredVelocity = _physics.velocity;

        GravityUpdate();
    }

    private void GravityUpdate()
    {
        if (_physics == null)
        {
            //Crash
        }

        if (_physics.isGrounded == true)
        {
            Debug.Log("Grounded");
        }
        else
        {
            Debug.Log("Not Grounded");
        }
    }

    private void UpdateAnimation(Vector3 inputDirection)
    {
        _AnimController.UpdateParameter("IsAim", _isAim);
        _AnimController.UpdateParameter("IsInAir", _isInAir);
        _AnimController.UpdateParameter("IsJump", _isJumping);

        _AnimController.UpdateParameter("MovingSpeed", _physics.velocity.magnitude);
        _AnimController.UpdateParameter("AimDegree", _aimAngle);

        Vector3 animDir = new Vector3(0.0f, 0.0f, 1.0f);
        if (_isAim == true)
        {
            animDir = inputDirection;
        }

        _AnimController.UpdateParameter("RelativeMovingZ", animDir.z);
        _AnimController.UpdateParameter("RelativeMovingX", animDir.x);
    }

    private void CharacterAimRotation_2() //실제 정조준
    {
        Vector2 mouseMove = _inputController._pr_mouseMove;

        {
            //X축 회전
            transform.rotation *= Quaternion.Euler(0.0f, mouseMove.x * _directionVelocity.x, 0f);
        }

        {
            //Camera.main.transform.rotation *= Quaternion.Euler(mouseMove.y * _directionVelocity.y, 0.0f, 0f);
            //_sightCamera.transform.rotation *= Quaternion.Euler(mouseMove.y * _directionVelocity.y, 0.0f, 0f);
            //transform.localRotation *= Quaternion.Euler(0.0f, mouseMove.x * _directionVelocity.x, 0f);
            //_aimAngle += mouseMove.y * _directionVelocity.y;
            //Debug.Log("AimAngle : " + _aimAngle);
        }

        if (_aimTarget != null)
        {
            _aimTarget.transform.localRotation *= Quaternion.Euler(-mouseMove.y * _directionVelocity.y, 0.0f, 0f);
        }
    }

    private void CharacterRotate(Vector3 inputDirection)
    {
        if (Camera.main != null)
        {
            Vector3 cameraLook = Camera.main.transform.forward;
            cameraLook.y = 0.0f;
            cameraLook = cameraLook.normalized;
            inputDirection = (Quaternion.LookRotation(cameraLook) * inputDirection);


            _aimAngle = Camera.main.transform.eulerAngles.x;

            if (_aimAngle > 270f)
            {
                _aimAngle -= 360f;
            }

            _aimAngle = _aimAngle / 180f * -1f + 0.5f;
        }


        float deltaDEG = Vector3.Angle(transform.forward.normalized, inputDirection);

        float nextDeltaDEG = _rotatingSpeed_DEG * Time.deltaTime;

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


        //조준상태가 아니라면 wasd



    }
    [SerializeField] private RiggingWithAimTarget _riggingWithAimTarget;
    [SerializeField] private InputController _inputController = null;
    [SerializeField] private string _aimKey = "Fire2";
    [SerializeField] private string _jumpKey = "Jump";
    [SerializeField] private float _mass = 30.0f;
    [SerializeField] private Vector2 _directionVelocity = new Vector2(1.0f,1.0f);
    [SerializeField] private AimState _aimState = AimState.eTPSAim;
    [SerializeField] private Gunscript _gunscript = null;
    [SerializeField] private GameObject _inventoryUIPrefab = null;

    [SerializeField] private float _speed = 5.0f;
    [SerializeField] private float _rotatingSpeed_DEG = 90.0f;
    [SerializeField] private Camera _camera = null;
    [SerializeField] private AnimContoller _AnimController = null;
    [SerializeField] private CharacterController _physics = null;
    [SerializeField] private float _jumpForce = 3.0f;
    [SerializeField] private GameObject _aimTarget = null;
    [SerializeField] private AimOrbit _aimOrbit = null;


    [SerializeField] private GameObject _equipmentItemModelPrefab_MustDel = null;
    [SerializeField] private Avatar _equipmentItemModelAvatar_MustDel = null;



    private Vector2 _currentVector = Vector2.zero;
    private Vector2 _currentVectorRef = Vector2.zero;

    private float _aimAngle = 0.5f; private float _aimAngleRef = 0.5f;

    public CinemachineVirtualCameraBase _sightCamera = null;
    public CinemachineVirtualCameraBase _freeRunCamera = null;
    public CinemachineVirtualCameraBase _tpsCamera = null;

    private Vector3 _latestDesiredVelocity = Vector3.zero;

    public delegate void CustomGroundedDelegate();

    private bool _isJumping = false;
    private bool _isInAir = false;
    private bool _isSteady = false;
    

    private bool _isAim = false;
    private float _verticalSpeedAcc = 0.0f;
};
