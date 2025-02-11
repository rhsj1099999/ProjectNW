using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterContollerable : GameCharacterSubScript
{
    [SerializeField] protected float _mass = 2.0f;
    [SerializeField] protected bool _logMe = false;

    /*----------------------------------------
    |NOTI| speed = 이제 Stat 에서 관리합니다
    ----------------------------------------*/
    //[SerializeField] protected float _speed = 5.0f;

    [SerializeField] protected float _rotatingSpeed_DEG = 720.0f;
    [SerializeField] protected float _jumpForce = 3.0f;
    protected Vector3 _latestPlaneVelocityDontUseY = Vector3.zero;
    protected List<Vector3> _roots = new List<Vector3>();

    //protected bool _isInAir = false;

    protected bool _moveTriggerd = false;

    protected Vector3 _gravitySpeed = Vector3.zero;

    private void Update()
    {
        if (Input.GetKey(KeyCode.P) == true)
        {
            _roots.Clear();
        }
    }


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

    public abstract void CharacterDie();
    public abstract void CharacterRevive();
    public abstract void MoverUpdate();
    public abstract void StateChanged();
    public abstract void LookAt_Plane(Vector3 dir);
    public abstract bool GetIsInAir();
    public abstract void CharacterInertiaMove(float ratio);
    public abstract void ClearLatestVelocity();
    public abstract void GravityUpdate();
    public abstract void DoJump();
    public abstract void CharacterMove(Vector3 inputDirection, float similarities, float ratio);
    public abstract void CharacterRootMove(Vector3 delta, float similarities, float ratio);
    public abstract void CharacterTeleport(Vector3 position);
    public abstract void CharacterRotate(Vector3 inputDirection, float ratio);
    public abstract void CharacterRotate(Quaternion rotation);
    public abstract void CharacterRotateDirectly(Quaternion rotation);
}
