using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(KinematicCharacterMotor))]
public class KinematicControllerWrapper : CharacterContollerable, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor _motor = null;

    private bool _jumpRequested = false;
    private bool _inAir = false;

    private Vector3 _currentSpeed = Vector3.zero;
    private Quaternion _currentRotation = Quaternion.identity;


    public override void LookAt(Vector3 dir)
    {
        _currentRotation = Quaternion.LookRotation(dir);
    }

    private void Awake()
    {
        _motor = GetComponent<KinematicCharacterMotor>();
        _motor.CharacterController = this;
    }

    public override void SubScriptStart() {}

    public override bool GetIsInAir()
    {
        return !_motor.GroundingStatus.IsStableOnGround;
    }

    public override void CharacterInertiaMove(float ratio)
    {
        Vector3 planeVelocity = _latestPlaneVelocityDontUseY;
        planeVelocity.y = 0.0f;
        _currentSpeed = (planeVelocity) * ratio;
        _moveTriggerd = true;
    }

    public override void ClearLatestVelocity()
    {
        if (_moveTriggerd == false)
        {
            //이동명령이 온적이 없다.
            _currentSpeed = Vector3.zero;
        }

        _moveTriggerd = false;
    }

    public override void GravityUpdate()
    {
        _gravitySpeed += (_mass * 9.81f * Vector3.down) * Time.deltaTime;
    }

    public override void DoJump()
    {
        if (_motor.GroundingStatus.IsStableOnGround == false)
        {
            return; //더블 점프 컨텐츠, 스킬 생기면 어떻게할꺼야
        }

        _gravitySpeed = new Vector3(0.0f, _jumpForce, 0.0f);
        _motor.ForceUnground(0.1f);
    }

    public override void CharacterMove(Vector3 inputDirection, float similarities, float ratio)
    {
        _moveTriggerd = true;

        _currentSpeed = inputDirection * _speed * similarities * ratio;
    }

    public override void CharacterRootMove(Vector3 delta, float similarities, float ratio)
    {
        _moveTriggerd = true;

        //_currentSpeed = (delta / Time.deltaTime) * similarities * ratio;

        //_motor.MoveCharacter();
        _motor.SetPosition(transform.position + (delta * similarities * ratio));
    }

    public override void CharacterRotate(Vector3 inputDirection, float ratio)
    {
        Vector3 crossRet = Vector3.Cross((_currentRotation * Vector3.forward), (Quaternion.LookRotation(inputDirection) * Vector3.forward));

        float isRightRotate = (crossRet.y > 0.0f)
            ? 1.0f
            : -1.0f;

        float deltaDEG = Quaternion.Angle(_currentRotation, Quaternion.LookRotation(inputDirection));

        float nextDeltaDEG = _rotatingSpeed_DEG * Time.deltaTime * ratio * isRightRotate;

        if (Mathf.Abs(nextDeltaDEG) >= deltaDEG)
        {
            _currentRotation.SetLookRotation(inputDirection);
        }
        else
        {
            Quaternion rotateMatrix = Quaternion.AngleAxis(nextDeltaDEG, Vector3.up);
            _currentRotation *= rotateMatrix;
        }
    }




    



    public void AfterCharacterUpdate(float deltaTime) 
    {
        _latestPlaneVelocityDontUseY = _motor.Velocity;

        if (_motor.GroundingStatus.IsStableOnGround == true)
        {
            _gravitySpeed = Vector3.zero;
            return;
        }
    }

    public void BeforeCharacterUpdate(float deltaTime) { }

    public bool IsColliderValidForCollisions(Collider coll) { return true; }

    public void OnDiscreteCollisionDetected(Collider hitCollider) { }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void PostGroundingUpdate(float deltaTime) { }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        currentRotation = _currentRotation;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        currentVelocity = _currentSpeed + _gravitySpeed;
        _jumpRequested = false;
    }
}
