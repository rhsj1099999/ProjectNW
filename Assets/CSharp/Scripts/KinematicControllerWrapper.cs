using KinematicCharacterController;
using MagicaCloth2;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Xsl;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(KinematicCharacterMotor))]
public class KinematicControllerWrapper : CharacterContollerable, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor _motor = null;
    private List<Vector3> _debuggingList = new List<Vector3>();
    private Vector3 _rootStartPosition = Vector3.zero;
    private bool _fiset = false;

    private Vector3 _currentSpeed = Vector3.zero;
    private Quaternion _currentRotation = Quaternion.identity;
    private bool _rootMotionRequested = false;

    private float _destTarget = 0.0f;
    private Vector3 _anchoredPosition = Vector3.zero;
    private Vector3 _destinyPosition = Vector3.zero;
    private Vector3 _rootDelta = Vector3.zero;
    private Vector3 _rootMovedACC = Vector3.zero;
    private float _rootMoveMaxDistance = 0.0f;

    [SerializeField] private float _rootMoveScale = 1.0f;

    public override void LookAt_Plane(Vector3 dir)
    {
        dir.y = 0.0f;
        dir = dir.normalized;
        _currentRotation = Quaternion.LookRotation(dir);
    }

    private void Awake()
    {
        _motor = GetComponent<KinematicCharacterMotor>();
        _motor.CharacterController = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) == true) 
        {
            Vector3 totalRoot = Vector3.zero;
            foreach (var item in _debuggingList)
            {
                totalRoot += item;
            }

            Vector3 delta = transform.position - _rootStartPosition;
        }
    }

    public override void SubScriptStart() {}

    public override bool GetIsInAir()
    {
        return !_motor.GroundingStatus.IsStableOnGround;
    }

    public override void StateChanged()
    {
        _rootMotionRequested = false;
        _anchoredPosition = Vector3.zero;
        _rootDelta = Vector3.zero;
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
        }

        _rootMotionRequested = false;
        _anchoredPosition = Vector3.zero;
        _rootDelta = Vector3.zero;

        //if (_destTarget >= 0.0f)
        //{
        //    float moved = (_currentSpeed * deltaTime).magnitude;
        //    _destTarget -= moved;
        //    Debug.Log("Remain = " + _destTarget);

        //    if (_destTarget <= 0.0f)
        //    {
        //        Debug.Log("End!");
        //        _destTarget = -1.0f;
        //        _currentSpeed = Vector3.zero;
        //    }
        //}

        //_destinyPosition = Vector3.zero;
        //_rootDelta = Vector3.zero;
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



    public override void CharacterMove(Vector3 inputDirection, float similarities, float ratio)
    {
        _moveTriggerd = true;

        _currentSpeed = inputDirection * _speed * similarities * ratio;
    }

    public override void CharacterRootMove(Vector3 delta, float similarities, float ratio)
    {
        _moveTriggerd = true;

        _anchoredPosition = transform.position;
        _rootDelta = delta;
        _rootMotionRequested = true;

        if (_fiset == false)
        {
            _fiset = true;
            _rootStartPosition = transform.position;
        }

        _debuggingList.Add(delta);

        //_currentSpeed = (delta / Time.fixedDeltaTime) / _motor.MaxMovementIterations;



        //밑에 FixedUpdate를 통해서 이동하지만 이거 이상 이동할수는 없다-------------------
        //_rootMoveMaxDistance = delta.magnitude;
        //----------------------------------------------------------------------------


        //_destTarget = (delta.magnitude);

        //Debug.Log("RootMoveCall" + _destTarget);



        //_rootMovedACC = Vector3.zero;

        //_rootDelta = delta;


        //if (_rootMotionRequested == false)
        //{

        //}

    }


    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //FixedUpdate에서 몇번 불릴지 모르는 함수

        //if (_rootMotionRequested == true)
        //{
        //    float rootMoveSimulatedDistance = (_currentSpeed * deltaTime).magnitude;

        //    //시뮬레이션 거리가 최대거리를 넘어설 예정이다
        //    if (rootMoveSimulatedDistance > _rootMoveMaxDistance)
        //    {

        //    }
        //}



        currentVelocity = _currentSpeed + _gravitySpeed;

        //Ver 0
        {
            //if (_rootMotionRequested == true)
            //{
            //    Vector3 dst = _anchoredPosition + _rootDelta; //여기에 도달해야만함
            //    _currentSpeed = (dst - transform.position) / deltaTime;
            //}

            //currentVelocity = _currentSpeed + _gravitySpeed;
        }




    }
}
