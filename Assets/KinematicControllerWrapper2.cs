using KinematicCharacterController;
using UnityEngine;

[RequireComponent(typeof(KinematicCharacterMotor))]
public class KinematicControllerWrapper2 : CharacterControllerable, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor _motor = null;

    private bool _jumpRequested = false;
    private bool _knuckBackRequested = false;

    private Vector3 _desiredSpeed = Vector3.zero;
    private Quaternion _currentRotation = Quaternion.identity;

    public override void CharacterTeleport(Vector3 position)
    {
        _motor.SetPosition(position);
    }

    public override void LookAt_Plane(Vector3 dir)
    {
        dir.y = 0.0f;
        dir = dir.normalized;
        _currentRotation = Quaternion.LookRotation(dir);
    }

    public override void MoverUpdate() 
    {
        ClearLatestVelocity();
    }

    public override void CharacterRevive()
    {
        gameObject.layer = LayerMask.NameToLayer("CharacterVolume");
        _motor.Capsule.includeLayers = (LayerMask.GetMask("StaticNavMeshLayer") | LayerMask.GetMask("CharacterVolume"));
        _motor.CollidableLayers = _motor.Capsule.includeLayers;
    }

    public override void CharacterDie()
    {
        gameObject.layer = LayerMask.GetMask("Default");
        _motor.Capsule.includeLayers = (LayerMask.GetMask("StaticNavMeshLayer"));
        _motor.CollidableLayers = _motor.Capsule.includeLayers;
    }

    
    public override void CharacterRotate(Quaternion rotation)
    {
        _currentRotation = rotation;
    }


    private void Awake()
    {
        _motor = GetComponent<KinematicCharacterMotor>();
        _motor.CharacterController = this;
    }

    private void Start()
    {
        _motor.Capsule.includeLayers = (LayerMask.GetMask("StaticNavMeshLayer") | LayerMask.GetMask("CharacterVolume"));
        _motor.CollidableLayers = _motor.Capsule.includeLayers;
    }

    public override void TurnOnGhost()
    {
        _motor.Capsule.includeLayers = (LayerMask.GetMask("StaticNavMeshLayer"));
        _motor.CollidableLayers = _motor.Capsule.includeLayers;
    }

    public override void TurnOffGhost()
    {
        _motor.Capsule.includeLayers = (LayerMask.GetMask("StaticNavMeshLayer") | LayerMask.GetMask("CharacterVolume"));
        _motor.CollidableLayers = _motor.Capsule.includeLayers;
    }

    public override void SubScriptStart() {}


    public override void CharacterRotateDirectly(Quaternion rotation)
    {
        _currentRotation = rotation;
        _motor.SetRotation(rotation);
    }


    public override bool GetIsInAir()
    {
        return !_motor.GroundingStatus.FoundAnyGround;
    }



    public override void StateChanged()
    {
        SafeReArrange();
    }


    public override void CharacterInertiaMove(float ratio)
    {
        Vector3 planeVelocity = _latestPlaneVelocityDontUseY;
        planeVelocity.y = 0.0f;
        _desiredSpeed = (planeVelocity) * ratio;
        _moveTriggerd = true;
    }

    public override void ClearLatestVelocity()
    {
        if (_moveTriggerd == false)
        {
            _desiredSpeed = Vector3.zero;
        }

        _moveTriggerd = false;
    }

    private void SafeReArrange()
    {
        _jumpRequested = false;
        _knuckBackRequested = false;
    }

    public override void GravityUpdate() 
    {
        _gravitySpeed += (9.81f * Vector3.down) * Time.deltaTime;
    }

    private void JumpRequestedExecute()
    {
        _jumpRequested = false;
        _motor.ForceUnground(0.1f);
        _gravitySpeed = new Vector3(0.0f, _jumpForce, 0.0f);
    }

    private void KnuckBackRequestedExcute()
    {
        _knuckBackRequested = false;
        _motor.ForceUnground(0.1f);

        Vector3 myForward = transform.forward;
        Vector3 myBackward = Quaternion.AngleAxis(180, transform.right) * myForward;
        _gravitySpeed = new Vector3(0.0f, _jumpForce / 2.0f, 0.0f) + (myBackward * 3.0f);
    }


    public override void DoJump()
    {
        if (_motor.GroundingStatus.FoundAnyGround == false)
        {
            return;
        }

        _jumpRequested = true;
    }



    public override void DoKnuckBack()
    {
        _knuckBackRequested = true;
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

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        currentRotation = _currentRotation;
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        if (_jumpRequested == true)
        {
            JumpRequestedExecute();
        }

        if (_knuckBackRequested == true)
        {
            KnuckBackRequestedExcute();
        }
    }

    public void AfterCharacterUpdate(float deltaTime) 
    {
        _latestPlaneVelocityDontUseY = _motor.Velocity;
        _jumpRequested = false;

        if (_motor.GroundingStatus.FoundAnyGround == true)
        {
            _gravitySpeed = Vector3.zero;
        }

        GravityUpdate();
    }



    public bool IsColliderValidForCollisions(Collider coll) { return true; }

    public void OnDiscreteCollisionDetected(Collider hitCollider) { }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {}

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void PostGroundingUpdate(float deltaTime) { }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }





    public override void CharacterRootMove(Vector3 delta, float similarities, float ratio)
    {
        _moveTriggerd = true;

        _desiredSpeed = delta / Time.deltaTime;
    }


    public override void CharacterRootMove_Speed(Vector3 delta, float similarities, float ratio)
    {
        _moveTriggerd = true;

        _desiredSpeed = (delta / Time.deltaTime) * ratio;
    }

    /*---------------------------------------------------------------------
    |NOTI| 작업영역
    ---------------------------------------------------------------------*/

    public override void CharacterMove(Vector3 inputDirection, float similarities, float ratio)
    {
        _moveTriggerd = true;

        _desiredSpeed = inputDirection * _owner.GCST<StatScript>().GetPassiveStat(LevelStatAsset.PassiveStat.MoveSpeed) * similarities * ratio;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        /*-------------------------------------------------------------
        |NOTI| 쓸데없는 y축 속도가 있으면 안된다(이미 바닥인데 중력같은거)
        -------------------------------------------------------------*/

        //if (Vector3.Angle(_motor.GroundingStatus.GroundNormal, transform.up) > (_risingSlope) &&
        //    Vector3.Dot(_motor.GroundingStatus.GroundNormal, _desiredSpeed) < 0.0f)
        //{
        //    _desiredSpeed = Vector3.zero;
        //}

        Vector3 planeSpeed = (_motor.GroundingStatus.FoundAnyGround == false)
        ? _desiredSpeed
        : _motor.GetDirectionTangentToSurface(_desiredSpeed, _motor.GroundingStatus.GroundNormal) * _desiredSpeed.magnitude;

        Vector3 verticalSpeed = (_motor.GroundingStatus.FoundAnyGround == false)
            ? _gravitySpeed
            : Vector3.zero;

        currentVelocity = planeSpeed + verticalSpeed;
    }

    /*---------------------------------------------------------------------
    |NOTI| 작업영역
    ---------------------------------------------------------------------*/
}