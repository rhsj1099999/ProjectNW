using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterContollerable : GameCharacterSubScript
{
    [SerializeField] protected float _mass = 2.0f;
    [SerializeField] protected bool _logMe = false;
    [SerializeField] protected float _speed = 5.0f;
    [SerializeField] protected float _rotatingSpeed_DEG = 720.0f;
    [SerializeField] protected float _jumpForce = 3.0f;
    protected Vector3 _latestPlaneVelocityDontUseY = Vector3.zero;

    //protected bool _isInAir = false;

    protected bool _moveTriggerd = false;

    protected Vector3 _gravitySpeed = Vector3.zero;


    public Vector3 GetLatestVelocity() { return _latestPlaneVelocityDontUseY; }
    public void ResetLatestVelocity() { _latestPlaneVelocityDontUseY = Vector3.zero; }

    public override sealed void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(CharacterContollerable);
    }


    public float CalculateMoveDirSimilarities(Vector3 desiredDir)
    {
        return Mathf.Clamp(Vector3.Dot(transform.forward, desiredDir), 0.0f, 1.0f);
    }

    public Vector3 GetDirectionConvertedByCamera(Vector3 inputDirection)
    {
        Vector3 cameraLook = Camera.main.transform.forward;
        cameraLook.y = 0.0f;
        cameraLook = cameraLook.normalized;
        return (Quaternion.LookRotation(cameraLook) * inputDirection);
    }

    public abstract void LookAt(Vector3 dir);
    public abstract bool GetIsInAir();
    public abstract void CharacterInertiaMove(float ratio);
    public abstract void ClearLatestVelocity();
    public abstract void GravityUpdate();
    public abstract void DoJump();
    public abstract void CharacterMove(Vector3 inputDirection, float similarities, float ratio);
    public abstract void CharacterRootMove(Vector3 delta, float similarities, float ratio);
    public abstract void CharacterRotate(Vector3 inputDirection, float ratio);
}
