using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*-------------------------------------------
 * |Obsoleted|-------------------------------
-------------------------------------------*/

public class TempMover : MMonoBehaviour
{
    //[SerializeField] private GameObject _follower = null;
    //[SerializeField] private float _velocity = 3.0f;
    //[SerializeField] private float _rotatingVelocity_Deg = 30.0f;
    //[SerializeField] private float _maxVelocity = 20.0f;
    //[SerializeField] private float _currVelocity = 0.0f;
    //[SerializeField] private float _accel = 0.5f;
    //[SerializeField] private float _rayDistance = 0.2f;
    //[SerializeField] private float _rayRadiusRatio = 0.95f;
    //private bool _inAir = false;
    //private Rigidbody _rb = null;

    //[SerializeField] private GameObject _point1 = null;
    //[SerializeField] private GameObject _point2 = null;


    //private void Awake()
    //{
    //    _rb = GetComponent<Rigidbody>();
    //}

    //private void GravityUpdate(float deltaTime)
    //{
    //    Vector3 dir = transform.up;
    //    Vector3 dirToCast = transform.up * -1.0f;

    //    int layerMask = LayerMask.GetMask("StaticNavMeshLayer");
    //    float originalRadius = 0.5f;

    //    float capsuleHeight = 2.5f - originalRadius;
    //    if (capsuleHeight <= 0.0f)
    //    {
    //        capsuleHeight = 0.0f;
    //    }

    //    Vector3 point1 = transform.position;
    //    Vector3 point2 = transform.position;
    //    point1 += new Vector3(0.0f, originalRadius, capsuleHeight / 2.0f + 0.2f);
    //    point2 += new Vector3(0.0f, originalRadius, -capsuleHeight / 2.0f + 0.2f);


    //    float checkRadius = originalRadius * _rayRadiusRatio;

    //    _inAir = !Physics.CapsuleCast
    //    (
    //        point1,
    //        point2,
    //        checkRadius,
    //        dirToCast,
    //        (originalRadius - checkRadius) + _rayDistance,
    //        layerMask
    //    );

    //    Debug.Log(_inAir);
    //}

    //private void RotationUpdate(float speedRatio, float deltaTime)
    //{
    //    if (speedRatio <= 0.0f) 
    //    {
    //        return;
    //    }

    //    Vector3 torqueAxis = Vector3.zero;
    //    if (Input.GetKey(KeyCode.A) == true)
    //    {
    //        torqueAxis = Vector3.down;
    //    }
    //    else if (Input.GetKey(KeyCode.D) == true)
    //    {
    //        torqueAxis = Vector3.up;
    //    }

    //    if (torqueAxis.sqrMagnitude <= 0.0f)
    //    {
    //        return;
    //    }

    //    _rb.AddTorque(torqueAxis * _rotatingVelocity_Deg * speedRatio, ForceMode.Force);
    //}

    //private void MoveUpdate(float deltaTime)
    //{
    //    GravityUpdate(deltaTime);

    //    Vector3 moveDesired = Vector3.zero;

    //    if (Input.GetKey(KeyCode.W) == true)
    //    {
    //        moveDesired = transform.forward;
    //    }
    //    else if (Input.GetKey(KeyCode.S) == true)
    //    {
    //        moveDesired = transform.forward * -1.0f;
    //    }

    //    if (moveDesired.sqrMagnitude <= 0.0f ||
    //        _inAir == true)
    //    {
    //        _currVelocity -= (_rb.drag * Time.deltaTime);
    //        if (_currVelocity <= 0.0f) {_currVelocity = 0.0f;}
    //        return;
    //    }

    //    _currVelocity += _accel * deltaTime;
    //    _currVelocity = Mathf.Clamp(_currVelocity, 0.0f, _maxVelocity);

    //    RotationUpdate(_currVelocity/_maxVelocity, deltaTime);


    //    //existVel = (moveDesired * _currVelocity);
    //    //nextVel.x = existVel.x;
    //    //nextVel.z = existVel.z;

    //    //_rb.velocity = nextVel;

    //    //moveDesired *= _currVelocity;
    //    //_rb.AddForce(moveDesired, ForceMode.VelocityChange);
    //    _rb.AddForce(_accel * moveDesired, ForceMode.Acceleration);
    //}

    //private void FixedUpdate()
    //{
    //    MoveUpdate(Time.deltaTime);
    //}

    //private void Update()
    //{
    //    //SynchronizedUpdate(MoveUpdate);
    //    //MoveUpdate(Time.deltaTime);
    //}
}
