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

    private Vector3 _animDir = Vector3.zero;

    private void Awake()
    {
        Debug.Assert(_inputController != null, "��ǲ��Ʈ�ѷ��� ����");
    }

    void Update()
    {
        //Ÿ�ӵ����
        {
            if (Input.GetKeyDown(KeyCode.L))  // SŰ�� ������ ���� �ӵ��� ������ ��
            {
                Time.timeScale = 0.1f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // ������ �ð� ������Ʈ
            }

            if (Input.GetKeyDown(KeyCode.O))  // RŰ�� ������ ���� �ӵ��� �������� ����
            {
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
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
        
        //���ۿ� ���� �������� Check
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

        CharacterMove(_animDir, speedRatio); //������ �̵��ϱ�

    }

    void FixedUpdate()
    {

    }



    private void CharacterMove(Vector3 inputDirection, float ratio = 1.0f)
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
        
        Vector3 desiredMove = inputDirection * _speed * Time.deltaTime * similarities * ratio;

        desiredMove += Vector3.up * _verticalSpeedAcc * Time.deltaTime;

        Debug.Assert(Mathf.Abs(desiredMove.y) >= float.Epsilon, "�����ϴ� �������Դϴ�.");

        if (_physics != null)
        {
            _physics.Move(desiredMove);
        }
        else
        {
            transform.position += desiredMove;
        }

        ////���� ��Ҵ��� �ӽ� �����
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
        _AnimController.UpdateParameter("MovingSpeed", _physics.velocity.magnitude / _speed);
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
    private float _verticalSpeedAcc = 0.0f;

    [SerializeField] private GameObject _inventoryUIPrefab = null;

    [SerializeField] private GameObject _equipmentItemModelPrefab_MustDel = null;
    [SerializeField] private Avatar _equipmentItemModelAvatar_MustDel = null;



};
