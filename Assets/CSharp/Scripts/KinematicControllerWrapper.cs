using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class KinematicControllerWrapper : GameCharacterSubScript, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor _motor = null;
    [SerializeField] private float _speed = 5.0f;
    [SerializeField] private float _rotatingSpeed_DEG = 90.0f;
    [SerializeField] private float _jumpForce = 3.0f;
    [SerializeField] private float _mass = 2.0f;

    private Vector3 _currentGravity = Vector3.zero;
    private Vector3 _currentSpeed = Vector3.zero;
    private Quaternion _currentRotation = Quaternion.identity;



    public override void Init(CharacterScript owner)
    {
        if (_motor == null)
        {
            Debug.Assert(false, "모터를 할당하세여");
            Debug.Break();
        }
        _motor.CharacterController = this;


        _owner = owner;
        _myType = typeof(KinematicControllerWrapper);
    }




    public override void SubScriptStart(){}

    private void Update()
    {
        Vector3 characterInputDir = _owner.GCST<InputController>()._pr_directionByInput;
        Vector3 cameraLook = Camera.main.transform.forward;
        cameraLook.y = 0.0f;
        cameraLook = cameraLook.normalized;
        Vector3 converted = (Quaternion.LookRotation(cameraLook) * characterInputDir);

        Rotate(converted);
        Move(converted);
        GravityUpdate();
    }


    private void GravityUpdate()
    {
        if (_motor.GroundingStatus.IsStableOnGround == true)
        {
            _currentGravity = Vector3.zero;
            return;
        }

        _currentGravity += (_mass * 9.81f * Vector3.down) * Time.deltaTime;
    }


    private void Rotate(Vector3 inputDirection, float ratio = 1.0f)
    {
        if (inputDirection.magnitude <= 0.0f)
        {
            return;
        }

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


    public void Move(Vector3 inputDirection, float ratio = 1.0f)
    {
        if (inputDirection.magnitude <= 0.0f)
        {
            _currentSpeed = Vector3.zero;
            return;
        }

        float similarities = Mathf.Clamp(Vector3.Dot(transform.forward, inputDirection), 0.0f, 1.0f);
        Vector3 desiredSpeed = inputDirection * _speed * similarities * ratio;
        _currentSpeed = desiredSpeed;
    }



    public void AfterCharacterUpdate(float deltaTime) {}

    public void BeforeCharacterUpdate(float deltaTime) {}

    public bool IsColliderValidForCollisions(Collider coll) {return true;}

    public void OnDiscreteCollisionDetected(Collider hitCollider) {}

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {}

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {}

    public void PostGroundingUpdate(float deltaTime) {}

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) {}

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) 
    {
        currentRotation = _currentRotation;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) 
    {
        currentVelocity = _currentSpeed + _currentGravity;
    }
}
