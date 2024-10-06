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
        Debug.Assert(_inputController != null, "��ǲ��Ʈ�ѷ��� ����");
    }

    void Update()
    {
        //CŰ�� ������ ��� ������ ���� ������Ʈ �����ڵ�
        {
            /*---------------------------------------------------------------------
             |TODO| ������� �ؾ��� �ܰ� : �������� �������� Skinned Mesh �ϋ� �������� ������ �Ǻ�
            ---------------------------------------------------------------------
                1. ���� �� �����ΰ�? -> ����ȭ �� �ܰ踦 ����
                2. �ٸ� �� �����ΰ�? -> �Ʒ� �ܰ踦 ����
            ---------------------------------------------------------------------*/
            if (Input.GetKeyUp(KeyCode.C) == true && _equipmentItemModelPrefab_MustDel != null)
            {
                ////1. �� �����ܰ�
                //GameObject emptyGameObject = new GameObject("EquipmentModelDummy"); // "MyChildObject"�� ������ ������Ʈ�� �̸�
                //emptyGameObject.transform.SetParent(this.transform);
                //emptyGameObject.transform.localPosition = Vector3.zero;

                ////2. �ִϸ����� ���� �ܰ�
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

                ////3. ��ε�ĳ���� ����ܰ�
                //AnimPropertyBroadCaster animpropertyBroadCaster = GetComponent<AnimPropertyBroadCaster>();
                //animpropertyBroadCaster.AddAnimator(modelObject);

                ////4. ������ ������Ʈ�� ���� �ܰ�
                //RiggingPublisher ownerRigPublisher = gameObject.GetComponent<RiggingPublisher>();
                //ownerRigPublisher.PublishRigging(modelObject, modelAnimator);

                ////5. Skinned Mesh Renderer ��Ȱ��ȭ �ܰ� (���� ��� �����ֱ� �����̴�)
                ////GameObject itemMeshObject = null;
            }
        }

        //�κ��丮 �����ڵ�
        {
            if (_inputController.GetInventoryOpen() == true)
            {
                UIManager.Instance.TurnOnUI(_inventoryUIPrefab, this.gameObject);
            }
        }

        //�޸��� �ڵ�
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
        
        //���ۿ� ���� �������� Check
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
        
        CharacterMove(inputDirection); //������ �̵��ϱ�
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
        {//���� �õ�
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

        //���� ��Ҵ��� �ӽ� �����
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
