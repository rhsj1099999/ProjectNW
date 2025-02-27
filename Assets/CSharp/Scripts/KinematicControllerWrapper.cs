using KinematicCharacterController;
using MagicaCloth2;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Xml.Xsl;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(KinematicCharacterMotor))]
public class KinematicControllerWrapper : CharacterContollerable, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor _motor = null;

    private bool _inAir = false;

    private bool _jumpRequested = false;
    private bool _knuckBackRequested = false;

    /*-------------------------------------------------------
    |NOTI| 경사각 이하에서 속도가 얼마든 바닥에 붙음을 보장합니다
    오토바이같은거 타면 끄세요
    -------------------------------------------------------*/
    private bool _isAttachedMove = true; 
    
    private Vector3 _capsuleCheckLocal_High = Vector3.zero;
    private Vector3 _capsuleCheckLocal_Low = Vector3.zero;

    private Vector3 _currentSpeed = Vector3.zero;
    private Quaternion _currentRotation = Quaternion.identity;
    private RaycastHit _hit;

    //[SerializeField] private float _maxDownHillDeg = 60.0f;


    public override void CharacterTeleport(Vector3 position)
    {
        _motor.SetPosition(position);
        //CharacterRootMove(position - transform.position, 1.0f, 1.0f);
    }

    public override void LookAt_Plane(Vector3 dir)
    {
        dir.y = 0.0f;
        dir = dir.normalized;
        _currentRotation = Quaternion.LookRotation(dir);
    }

    public override void MoverUpdate()
    {
        _owner.transform.rotation = transform.rotation;
        _owner.transform.position = transform.position;

        GravityUpdate();
        ClearLatestVelocity();

        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
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
        _capsuleCheckLocal_High = _motor.Capsule.center + Vector3.up * (_motor.Capsule.height / 2 - _motor.Capsule.radius);
        _capsuleCheckLocal_Low = _motor.Capsule.center - Vector3.up * (_motor.Capsule.height / 2 - _motor.Capsule.radius);

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
        /*------------------------------------------------------------------
        |NOTI| 난간에서 _motor.GroundingStatus.IsStableOnGround 가 불안정해서
        CapsuleCast를 씁니다
        ------------------------------------------------------------------*/

        if (_gravitySpeed.y > 0.0f)
        {
            return true;
        }

        return _inAir;
    }


    private void InAirCheck_Capsule(CapsuleCollider collider, ref Vector3 currentCharacterPosition)
    {

    }

    private void InAirCheck_Sphere(SphereCollider collider, ref Vector3 currentCharacterPosition)
    {

    }

    private void InAirCheck_Box(BoxCollider collider, ref Vector3 currentCharacterPosition)
    {
        _inAir = !Physics.BoxCast
            (
            currentCharacterPosition + collider.center,
            collider.size / 2.0f * 0.98f,
            Vector3.down,
            transform.rotation,
            0.2f,
            LayerMask.GetMask("StaticNavMeshLayer")
            );
    }

    private void InAircheck()
    {
        Vector3 currentPosition = transform.position;

        Collider finalCollider = _motor._PenetrationCollider;

        if (finalCollider == null)
        {
            //뭔가를 탑승하고있지 않아요
            float checkDistance = 0.2f;
            float checkRadius = _motor.Capsule.radius - 0.005f;
            _inAir = !Physics.CapsuleCast(currentPosition + _motor.CharacterTransformToCapsuleTopHemi, currentPosition + _motor.CharacterTransformToCapsuleBottomHemi, checkRadius, Vector3.down, out _hit, checkDistance, LayerMask.GetMask("StaticNavMeshLayer"));
            return;
        }

        System.Type colliderType = finalCollider.GetType();

        if (colliderType == typeof(CapsuleCollider))
        {
            InAirCheck_Capsule(finalCollider as CapsuleCollider, ref currentPosition);
        }
        else if (colliderType == typeof(BoxCollider))
        {
            InAirCheck_Box(finalCollider as BoxCollider, ref currentPosition);
        }
        else if (colliderType == typeof(SphereCollider))
        {
            InAirCheck_Sphere(finalCollider as SphereCollider, ref currentPosition);
        }
    }


    public override void StateChanged()
    {
        SafeReArrange();
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
            _currentSpeed = Vector3.zero;
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
        _gravitySpeed += (_mass * 9.81f * Vector3.down) * Time.deltaTime;
        InAircheck();
    }

    private void JumpRequestedExecute()
    {
        _inAir = true;
        _jumpRequested = false;
        _motor.ForceUnground(0.1f);
        _gravitySpeed = new Vector3(0.0f, _jumpForce, 0.0f);
    }

    private void KnuckBackRequestedExcute()
    {
        _inAir = true;
        _knuckBackRequested = false;
        _motor.ForceUnground(0.1f);

        Vector3 myForward = transform.forward;
        Vector3 myBackward = Quaternion.AngleAxis(180, transform.right) * myForward;
        _gravitySpeed = new Vector3(0.0f, _jumpForce / 2.0f, 0.0f) + (myBackward * 3.0f);
    }


    public override void DoJump()
    {
        if (_motor.GroundingStatus.IsStableOnGround == false)
        {
            return; //더블 점프 컨텐츠, 스킬 생기면 어떻게할꺼야
        }

        _jumpRequested = true;
    }



    public override void DoKnuckBack()
    {
        if (_motor.GroundingStatus.IsStableOnGround == false)
        {
            return; //더블 점프 컨텐츠, 스킬 생기면 어떻게할꺼야
        }

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

        if (_motor.GroundingStatus.IsStableOnGround == true)
        {
            _gravitySpeed = Vector3.zero;
        }
    }



    public bool IsColliderValidForCollisions(Collider coll) { return true; }

    public void OnDiscreteCollisionDetected(Collider hitCollider) { }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {}

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void PostGroundingUpdate(float deltaTime) { }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        currentRotation = _currentRotation;
    }

    public override void CharacterMove(Vector3 inputDirection, float similarities, float ratio)
    {
        _moveTriggerd = true;

        _currentSpeed = inputDirection * _owner.GCST<StatScript>().GetPassiveStat(LevelStatAsset.PassiveStat.MoveSpeed) * similarities * ratio;
    }

    public override void CharacterRootMove(Vector3 delta, float similarities, float ratio)
    {
        _moveTriggerd = true;

        _currentSpeed = delta / Time.deltaTime;
    }


    public override void CharacterRootMove_Speed(Vector3 delta, float similarities, float ratio)
    {
        _moveTriggerd = true;

        _currentSpeed = (delta / Time.deltaTime) * ratio;
    }


    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        /*-------------------------------------------------------------
        |NOTI| 쓸데없는 y축 속도가 있으면 안된다(이미 바닥인데 중력같은거)
        -------------------------------------------------------------*/

        Vector3 verticalSpeed = Vector3.zero;
        Vector3 planeSpeed = _currentSpeed;

        if (_motor.GroundingStatus.IsStableOnGround == false) 
        {
            verticalSpeed = _gravitySpeed;
        }

        ReArrangePlaneSpeedVector(ref planeSpeed);

        currentVelocity = planeSpeed + verticalSpeed;
    }

    private void ReArrangePlaneSpeedVector(ref Vector3 planeSpeedVector)
    {
        //슬라이딩 모드가 아니다
        if (_isAttachedMove == false)
        {
            return; 
        }

        bool isGrounded = _motor.GroundingStatus.IsStableOnGround;

        //바닥에 안붙어있다
        if (isGrounded == false)
        {
            
            return;
        }

        planeSpeedVector = _motor.GetDirectionTangentToSurface(planeSpeedVector, _motor.GroundingStatus.GroundNormal) * planeSpeedVector.magnitude;
    }
}



//private void PrivateGravityUpdate()
//{
//    bool prevInAir = _inAir;
//    bool currInAir = false;

//    Vector3 currentPosition = transform.position;
//    Vector3 point1 = currentPosition + _motor.Capsule.center + Vector3.up * (_motor.Capsule.height / 2 - _motor.Capsule.radius);
//    Vector3 point2 = currentPosition + _motor.Capsule.center - Vector3.up * (_motor.Capsule.height / 2 - _motor.Capsule.radius);

//    float checkDistance = 0.2f;
//    float checkRadius = _motor.Capsule.radius - 0.01f;

//    if (prevInAir == true && _gravitySpeed.y > 0.0f)
//    {
//        _gravitySpeed += (_mass * 9.81f * Vector3.down) * Time.deltaTime;
//        return;
//    }

//    currInAir = !Physics.CapsuleCast(point1, point2, checkRadius, Vector3.down, out _hit, checkDistance, LayerMask.GetMask("StaticNavMeshLayer"));

//    _inAir = currInAir;

//    if (currInAir == true)
//    {
//        _gravitySpeed += (_mass * 9.81f * Vector3.down) * Time.deltaTime;
//        return;
//    }

//    _gravitySpeed = Vector3.zero;

//    float moveDistance = (currInAir != prevInAir)
//        ? _hit.distance
//        : _hit.distance - checkRadius;

//    //float moveDistance = _hit.distance;

//    if (moveDistance <= 0)
//    {
//        return;
//    }

//    _motor.SetPosition(transform.position + Vector3.down * moveDistance);
//}
