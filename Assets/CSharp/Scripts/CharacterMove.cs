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



public class CharacterMoveScript2 : MonoBehaviour
{
    private CharacterController _characterController = null;
    private UIComponent _inventory = null;

    [SerializeField] private float _mass = 30.0f;
    [SerializeField] private float _speed = 5.0f;
    [SerializeField] private float _rotatingSpeed_DEG = 90.0f;
    [SerializeField] private float _jumpForce = 3.0f;

    private Vector3 _animDir = Vector3.zero;
    private bool _isJumping = false;
    private bool _isInAir = false;
    private bool _isAim = false;
    private bool _mouseLocked = false;
    private float _verticalSpeedAcc = 0.0f;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        Debug.Assert(_characterController != null, "_characterController 없다");
    }

    public void CharacterMove(Vector3 inputDirection, float ratio = 1.0f)
    {
        if (_characterController.isGrounded == true)
        {
            _characterController.stepOffset = 0.3f;
            _verticalSpeedAcc = 0.0f;

            _isJumping = false;
            _isInAir = false;
        }

        _verticalSpeedAcc += Physics.gravity.y * _mass * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) == true)
        {//점프 시도
            if (_characterController.isGrounded == true)
            {
                _characterController.stepOffset = 0.0f;
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

        _characterController.Move(desiredMove);
    }


    public void CharacterRotate(Vector3 inputDirection, float ratio = 1.0f)
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
