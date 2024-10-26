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
    

    private Vector3 _latestPlaneVelocityDontUseY = Vector3.zero;

    private bool _isJumping = false;
    private bool _isInAir = false;
    private float _verticalSpeedAcc = 0.0f;
    private bool _moveTriggerd = false;

    private void Update()
    {
        if(_characterController.isGrounded == true)
        {
            Debug.Log("Grounded");
        }
        else
        {
            Debug.Log("Not Grounded");
        }
        
    }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        Debug.Assert(_characterController != null, "_characterController 없다");
    }

    public bool GetIsJumping() { return _isJumping; }
    public bool GetIsInAir() { return _isInAir; }
    public Vector3 GetLatestVelocity() { return _latestPlaneVelocityDontUseY; }
    public void ResetLatestVelocity() { _latestPlaneVelocityDontUseY = Vector3.zero; }

    public void ClearLatestVelocity()
    {
        if (_moveTriggerd == false)
        {
            _latestPlaneVelocityDontUseY = Vector3.zero;
        }
        _moveTriggerd = false;
    }

    public void GravityUpdate() //매 프레임마다 호출될 함수니까
    {
        _verticalSpeedAcc += Time.deltaTime * Physics.gravity.y;

        Vector3 gravityMove = Vector3.up * _verticalSpeedAcc * Time.deltaTime;

        Debug.Assert(Mathf.Abs(gravityMove.y) >= float.Epsilon, "부유하는 움직임입니다.");

        _characterController.Move(gravityMove);

        if (_characterController.isGrounded == true)
        {
            _characterController.stepOffset = 0.3f;
            _verticalSpeedAcc = 0.0f;

            _isJumping = false;
            _isInAir = false;
        }
    }

    public void DoJump()
    {
        //if (_characterController.isGrounded == true)
        //{
        //    _characterController.stepOffset = 0.0f;
        //    _verticalSpeedAcc = _jumpForce;

        //    _isJumping = true;
        //    _isInAir = true;
        //}
        //else
        //{
        //    Debug.Log("점프가 씹혔다");
        //}

        _characterController.stepOffset = 0.0f;
        _verticalSpeedAcc = _jumpForce;

        _isJumping = true;
        _isInAir = true;
    }

    public void CharacterMove(Vector3 inputDirection, float ratio = 1.0f)
    {
        Vector3 cameraLook = Camera.main.transform.forward;
        cameraLook.y = 0.0f;
        cameraLook = cameraLook.normalized;
        inputDirection = (Quaternion.LookRotation(cameraLook) * inputDirection);

        float similarities = Mathf.Clamp(Vector3.Dot(transform.forward, inputDirection), 0.0f, 1.0f);

        Vector3 desiredMove = inputDirection * _speed * Time.deltaTime * similarities * ratio;

        _characterController.Move(desiredMove);
        _moveTriggerd = true;

        _latestPlaneVelocityDontUseY = _characterController.velocity;
    }

    public void CharacterForcedMove(Vector3 latestDelta, float ratio = 1.0f)
    {
        _characterController.Move(latestDelta * ratio * Time.deltaTime);
        _moveTriggerd = true;
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
