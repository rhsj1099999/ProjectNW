using UnityEngine;

public class CharacterMoveScript2 : GameCharacterSubScript
{
    [SerializeField] private bool _logMe = false;

    //[SerializeField] private float _mass = 30.0f;
    //[SerializeField] private float _inAirThreshould = 0.05f;
    [SerializeField] private float _speed = 5.0f;
    [SerializeField] private float _rotatingSpeed_DEG = 90.0f;
    [SerializeField] private float _jumpForce = 3.0f;
    

    private Vector3 _latestPlaneVelocityDontUseY = Vector3.zero;

    private bool _isJumping = false;
    private bool _isInAir = false;
    private float _verticalSpeedAcc = 0.0f;
    private bool _moveTriggerd = false;

    private float _inAirSlopeLimit = 0.0f;
    private float _inGroundSlopeLimit = 45.0f;
    private float _inAirStepOffset = 0.0f;
    private float _inGroundStepOffset = 0.3f;




    public bool GetIsJumping() { return _isJumping; }
    public bool GetIsInAir() { return _isInAir; }
    public Vector3 GetLatestVelocity() { return _latestPlaneVelocityDontUseY; }
    public void ResetLatestVelocity() { _latestPlaneVelocityDontUseY = Vector3.zero; }

    public override void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(CharacterMoveScript2);
    }

    public override void SubScriptStart()
    {

    }

    public void ClearLatestVelocity()
    {
        if (_moveTriggerd == false)
        {
            _latestPlaneVelocityDontUseY = Vector3.zero;
        }
        _moveTriggerd = false;
    }

    public void GravityUpdate() //�� �����Ӹ��� ȣ��� �Լ��ϱ�
    {
        _verticalSpeedAcc += Time.deltaTime * Physics.gravity.y;

        Vector3 gravityMove = Vector3.up * _verticalSpeedAcc * Time.deltaTime;

        Debug.Assert(Mathf.Abs(gravityMove.y) >= float.Epsilon, "�����ϴ� �������Դϴ�.");

        _owner.GCST<CharacterController>().Move(gravityMove);

        
        if (_owner.GCST<CharacterController>().isGrounded == true)
        {
            _owner.GCST<CharacterController>().stepOffset = _inGroundStepOffset;
            _owner.GCST<CharacterController>().slopeLimit = _inGroundSlopeLimit;

            _verticalSpeedAcc = 0.0f;

            _isJumping = false;
            _isInAir = false;
        }
        else
        {
            _owner.GCST<CharacterController>().stepOffset = _inAirStepOffset;
            _owner.GCST<CharacterController>().slopeLimit = _inAirSlopeLimit;

            _isInAir = true;
        }

        if (_logMe == true)
        {
            if (_isInAir == true)
            {
                Debug.Log("InAir");
            }
            else
            {
                Debug.Log("InGround");
            }
        }
    }

    public void DoJump()
    {
        if (_owner.GCST<CharacterController>().isGrounded == false)
        {
            return; //���� ���� ������, ��ų ����� ����Ҳ���
        }

        _owner.GCST<CharacterController>().stepOffset = _inAirStepOffset;
        _owner.GCST<CharacterController>().slopeLimit = _inAirSlopeLimit;

        _verticalSpeedAcc = _jumpForce;

        //_notGroundedCount = 0;
        _isJumping = true;
        _isInAir = true;
    }

    public void CharacterMove_NoSimilarity(Vector3 inputDirection, float ratio = 1.0f)
    {
        float similarities = Mathf.Clamp(Vector3.Dot(transform.forward, inputDirection), 0.0f, 1.0f);

        Vector3 desiredMove = inputDirection * _speed * Time.deltaTime * ratio;

        _owner.GCST<CharacterController>().Move(desiredMove);
        _moveTriggerd = true;

        _latestPlaneVelocityDontUseY = _owner.GCST<CharacterController>().velocity;
    }

    public void CharacterMove(Vector3 inputDirection, float ratio = 1.0f)
    {
        float similarities = Mathf.Clamp(Vector3.Dot(transform.forward, inputDirection), 0.0f, 1.0f);

        Vector3 desiredMove = inputDirection * _speed * Time.deltaTime * similarities * ratio;

        _owner.GCST<CharacterController>().Move(desiredMove);
        _moveTriggerd = true;

        _latestPlaneVelocityDontUseY = _owner.GCST<CharacterController>().velocity;
    }

    public Vector3 GetDirectionConvertedByCamera(Vector3 inputDirection)
    {
        Vector3 cameraLook = Camera.main.transform.forward;
        cameraLook.y = 0.0f;
        cameraLook = cameraLook.normalized;
        return (Quaternion.LookRotation(cameraLook) * inputDirection);
    }

    public void CharacterForcedMove(Vector3 latestDelta, float ratio = 1.0f)
    {
        _owner.GCST<CharacterController>().Move(latestDelta * ratio * Time.deltaTime);
        _moveTriggerd = true;
    }


    public void CharacterRotate(Vector3 inputDirection, float ratio = 1.0f)
    {
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
}