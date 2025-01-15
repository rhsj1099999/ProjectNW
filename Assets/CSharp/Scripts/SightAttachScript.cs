using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*-------------------------------------------
 * |Obsoleted|-------------------------------
-------------------------------------------*/

public class SightAttachScript : MonoBehaviour
{
}

//public class SightAttachScript : MonoBehaviour
//{
//    [SerializeField] private GameObject _followTarget = null;
//    [SerializeField] private GameObject _LookAt = null;
//    [SerializeField] private float _dampSpeed = 0.1f;
//    [SerializeField] private bool _isLateUpdate = false;
//    [SerializeField] private bool _isSmoothDeltaTime = false;
//    [SerializeField] private bool _isCameraControl = false;
//    [SerializeField] private bool _isFunc2 = false;
//    [SerializeField] private float _velocity = 1.0f;
//    [SerializeField] private float _distance = 1.0f;
//    [SerializeField] private float _minDistance = 0.001f;


//    private Vector3 _posRef = Vector3.zero;

//    private void FixedUpdate()
//    {
//        if (_isLateUpdate == false)
//        {
//            if (_isFunc2 == false)
//            {
//                Func();
//            }
//            else
//            {
//                Func2();
//            }
//        }

//    }

//    private void Func2()
//    {
//        if (_followTarget == null || _LookAt == null) return;

//        Vector3 dirVector = (_followTarget.transform.position - transform.position);

//        float distance = dirVector.magnitude;

//        if (_distance <= _minDistance) 
//        {
//            transform.position = _followTarget.transform.position;
//            return;
//        }

//        float calVel = _velocity * (distance / _distance);

//        float deltaTime = (_isSmoothDeltaTime == true)
//            ? Time.smoothDeltaTime
//            : Time.deltaTime;

//        transform.position += dirVector * (calVel * deltaTime);

//        if (_isCameraControl == false)
//        {
//            Camera.main.transform.position = transform.position;
//        }
//    }


//    private void Func()
//    {
//        if (_followTarget == null || _LookAt == null) return;

//        float deltaTime = (_isSmoothDeltaTime == true)
//            ? Time.smoothDeltaTime
//            : Time.deltaTime;
//        transform.position = Vector3.SmoothDamp(transform.position, _followTarget.transform.position, ref _posRef, deltaTime);

//        if (_isCameraControl == false)
//        {
//            Camera.main.transform.position = transform.position;
//        }
//    }

//    public void LateUpdate()
//    {
//        if (_isLateUpdate == true)
//        {
//            if (_isFunc2 == false)
//            {
//                Func();
//            }
//            else
//            {
//                Func2();
//            }
//        }
//    }
//}
