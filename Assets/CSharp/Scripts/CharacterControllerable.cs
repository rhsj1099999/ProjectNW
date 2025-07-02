using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterControllerable : GameCharacterSubScript
{
    [SerializeField] protected float _mass = 2.0f;
    [SerializeField] protected bool _logMe = false;
    [SerializeField] protected float _risingSlope = 60.0f;

    [SerializeField] protected float _rotatingSpeed_DEG = 720.0f;
    [SerializeField] protected float _jumpForce = 3.0f;
    protected Vector3 _latestPlaneVelocityDontUseY = Vector3.zero;
    protected List<Vector3> _roots = new List<Vector3>();

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
        _myType = typeof(CharacterControllerable);
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

    public abstract void TurnOnGhost();
    public abstract void TurnOffGhost();
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
    public abstract void DoKnuckBack();
    public abstract void CharacterMove(Vector3 inputDirection, float similarities, float ratio);
    public abstract void CharacterRootMove(Vector3 delta, float similarities, float ratio);
    public abstract void CharacterRootMove_Speed(Vector3 delta, float similarities, float ratio);
    public abstract void CharacterTeleport(Vector3 position);
    public abstract void CharacterRotate(Vector3 inputDirection, float ratio);
    public abstract void CharacterRotate(Quaternion rotation);
    public abstract void CharacterRotateDirectly(Quaternion rotation);

    #region IfNeedCapsuleDebug
    //[SerializeField] private float _inAirCheckHeightModify = 0.02f;
    //[SerializeField] private GameObject _spherePrefab = null;
    //[SerializeField] private GameObject _cylinderPrefab = null;
    //private List<GameObject> _debuggCapsules = new List<GameObject>();
    //protected void DebugCheck()
    //{
    //    if (Input.GetKeyDown(KeyCode.T) == true &&
    //       _spherePrefab != null &&
    //       _cylinderPrefab != null)
    //    {
    //        float radius = _motor.Capsule.radius * 2;

    //        GameObject topSphere = Instantiate(_spherePrefab);
    //        GameObject bottomSphere = Instantiate(_spherePrefab);
    //        GameObject middleCylinder = Instantiate(_cylinderPrefab);

    //        topSphere.transform.position = transform.position + _motor.CharacterTransformToCapsuleTopHemi;
    //        bottomSphere.transform.position = transform.position + _motor.CharacterTransformToCapsuleBottomHemi + (Vector3.down * _inAirCheckHeightModify);

    //        topSphere.transform.localScale = new(radius, radius, radius);
    //        bottomSphere.transform.localScale = new(radius, radius, radius);

    //        middleCylinder.transform.position = (topSphere.transform.position + bottomSphere.transform.position) / 2.0f;

    //        float length = (topSphere.transform.position - bottomSphere.transform.position).magnitude;
    //        middleCylinder.transform.localScale = new(radius, length / 2.0f, radius);

    //        _debuggCapsules.Add(topSphere);
    //        _debuggCapsules.Add(bottomSphere);
    //        _debuggCapsules.Add(middleCylinder);
    //    }

    //    if (Input.GetKeyDown(KeyCode.G) == true)
    //    {
    //        foreach (var item in _debuggCapsules)
    //        {
    //            Destroy(item);
    //        }

    //        _debuggCapsules.Clear();
    //    }
    //}
    #endregion
}
