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
        Debug.Assert(_inputController != null, "인풋컨트롤러가 없다");
    }

    void Update()
    {
        //C키를 누르면 장비 장착을 위한 오브젝트 생성코드
        {
            /*---------------------------------------------------------------------
             |TODO| 가장먼저 해야할 단계 : 입으려는 아이템이 Skinned Mesh 일떄 뼈구조가 같은지 판별
            ---------------------------------------------------------------------
                1. 같은 뼈 구조인가? -> 간소화 된 단계를 실행
                2. 다른 뼈 구조인가? -> 아래 단계를 실행
            ---------------------------------------------------------------------*/
            if (Input.GetKeyUp(KeyCode.C) == true && _equipmentItemModelPrefab_MustDel != null)
            {
                ////1. 모델 생성단계
                //GameObject emptyGameObject = new GameObject("EquipmentModelDummy"); // "MyChildObject"는 생성된 오브젝트의 이름
                //emptyGameObject.transform.SetParent(this.transform);
                //emptyGameObject.transform.localPosition = Vector3.zero;

                ////2. 애니메이터 세팅 단계
                //GameObject modelObject = Instantiate(_equipmentItemModelPrefab_MustDel, emptyGameObject.transform);
                //Animator modelAnimator = modelObject.GetComponent<Animator>();
                //if (modelAnimator == null)
                //{
                //    modelAnimator = modelObject.AddComponent<Animator>();
                //}
                //RuntimeAnimatorController ownerController = _AnimController.GetAnimator().runtimeAnimatorController;
                //RuntimeAnimatorController newController = Instantiate<RuntimeAnimatorController>(ownerController);
                //modelAnimator.runtimeAnimatorController = newController;
                //modelAnimator.avatar = _equipmentItemModelAvatar_MustDel;

                ////3. 브로드캐스터 연결단계
                //AnimPropertyBroadCaster animpropertyBroadCaster = GetComponent<AnimPropertyBroadCaster>();
                //animpropertyBroadCaster.AddAnimator(modelObject);

                ////4. 생성된 오브젝트의 리깅 단계
                //RiggingPublisher ownerRigPublisher = gameObject.GetComponent<RiggingPublisher>();
                //ownerRigPublisher.PublishRigging(modelObject, modelAnimator);

                ////5. Skinned Mesh Renderer 비활성화 단계 (입은 장비만 보여주기 위함이다)
                ////GameObject itemMeshObject = null;
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
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) == true)
            {
                _speed = 0.2f;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift) == true)
            {
                _speed = 0.1f;
            }
        }
        
        //조작에 의한 조준인지 Check
        {
            bool isAimed = Input.GetButton(_aimKey);
            _isAim = isAimed;
        }

        UpdateAnimation(_inputController._pr_directionByInput);
    }

    void FixedUpdate()
    {
        Vector3 inputDirection = _inputController._pr_directionByInput;

        if (_isAim == true)
        {
            CharacterAimRotation();
        }
        else
        {
            CharacterRotate(inputDirection);
        }
        
        CharacterMove(inputDirection); //누르면 이동하기
    }



    private void CharacterMove(Vector3 inputDirection)
    {
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

        Vector3 cameraLook = Camera.main.transform.forward;
        cameraLook.y = 0.0f;
        cameraLook = cameraLook.normalized;
        inputDirection = (Quaternion.LookRotation(cameraLook) * inputDirection);

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

        //땅에 닿았는지 임시 디버깅
        {
            if (_physics.isGrounded == true)
            {
                Debug.Log("Grounded");
            }
            else
            {
                Debug.Log("Not Grounded");
            }
        }
    }

    private void UpdateAnimation(Vector3 inputDirection)
    {
        _AnimController.UpdateParameter("IsAim", _isAim);
        _AnimController.UpdateParameter("IsInAir", _isInAir);
        _AnimController.UpdateParameter("IsJump", _isJumping);
        _AnimController.UpdateParameter("MovingSpeed", _physics.velocity.magnitude);
        Vector3 animDir = new Vector3(0.0f, 0.0f, 1.0f);
        if (_isAim == true)
        {
            animDir = inputDirection;
        }
        _AnimController.UpdateParameter("RelativeMovingZ", animDir.z);
        _AnimController.UpdateParameter("RelativeMovingX", animDir.x);
    }

    private void CharacterAimRotation()
    {

    }

    private void CharacterRotate(Vector3 inputDirection)
    {
        if (Camera.main != null)
        {
            Vector3 cameraLook = Camera.main.transform.forward;
            cameraLook.y = 0.0f;
            cameraLook = cameraLook.normalized;
            inputDirection = (Quaternion.LookRotation(cameraLook) * inputDirection);
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
    }


    [SerializeField] private InputController _inputController = null;
    [SerializeField] private AnimContoller _AnimController = null;
    [SerializeField] private CharacterController _physics = null;

    [SerializeField] private string _aimKey = "Fire2";
    [SerializeField] private float _mass = 30.0f;
    [SerializeField] private float _speed = 5.0f;
    [SerializeField] private float _rotatingSpeed_DEG = 90.0f;
    [SerializeField] private float _jumpForce = 3.0f;

    [SerializeField] private GameObject _inventoryUIPrefab = null;

    [SerializeField] private GameObject _equipmentItemModelPrefab_MustDel = null;
    [SerializeField] private Avatar _equipmentItemModelAvatar_MustDel = null;


    private bool _isJumping = false;
    private bool _isInAir = false;
    private bool _isAim = false;
    private float _verticalSpeedAcc = 0.0f;
};
