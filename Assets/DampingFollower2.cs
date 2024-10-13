using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DampingFollower2 : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform = null;
    [SerializeField] private Vector3 _hardLimitRadius = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private float _hardLimitRadius2 = 0.1f;
    [SerializeField] private Vector3 _deadzoneRadius = Vector3.zero;
    [SerializeField] private float _deadzoneRadius2 = 0.001f;
    [SerializeField] private Vector3 _dampingTime = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private float _dampingTime2 = 0.01f;

    private Vector3 _prevPosition = Vector3.zero;

    [SerializeField] GameObject _lookAtObject = null;

    private float _dampingRefX = 0.0f;
    private float _dampingRefY = 0.0f;
    private float _dampingRefZ = 0.0f;
    private Vector3 _dampingRef = new Vector3();

    private bool _hardLimitUpdated = false;

    private void Awake()
    {
        Debug.Assert(_targetTransform != null, "이 컴포넌트는 따라가려는 Transform 이 null이여서는 안된다.");

        /*---------------------------------------------------------
        LookAt의 기능은 카메라의 기능이지 DampingFollower의 기능이여선 안된다. 나중에 뺄것
        ---------------------------------------------------------*/
        Debug.Assert(_lookAtObject != null, "이 바라보는 Transform 이 null이여서는 안된다.");
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        //_hardLimitUpdated = false;

        //Vector3 dirToTarget = (_targetTransform.position - transform.position);

        //float targetRadius = dirToTarget.magnitude;

        //if (targetRadius > _hardLimitRadius2)
        //{
        //    ////너무 크게 움직였다면 하드존까지 끌어온다
        //    ///

        //    //float dampedPosX = Mathf.SmoothDamp(transform.position.x, _targetTransform.position.x, ref _dampingRefX, _dampingTime.x / (targetRadius / _hardLimitRadius));
        //    //float dampedPosY = Mathf.SmoothDamp(transform.position.y, _targetTransform.position.y, ref _dampingRefY, _dampingTime.y / (targetRadius / _hardLimitRadius));
        //    //transform.position = new Vector3(dampedPosX, dampedPosY, transform.position.z);
        //    transform.position = _targetTransform.position + (dirToTarget * _hardLimitRadius2 * -1.0f);
        //    _hardLimitUpdated = true;
        //    Debug.Log("하드리밋");


        //    //float additionalSpeed = targetRadius / _hardLimitRadius;

        //    //transform.position = Vector3.SmoothDamp(transform.position, _targetTransform.position, ref _dampingRef, _dampingTime / additionalSpeed);

        //}
    }

    public void HardLimitDrag(Vector3 deltaPosition)
    {
        _hardLimitUpdated = false;

        Vector3 dirToTarget = (_targetTransform.position - transform.position);

        float targetRadius = dirToTarget.magnitude;

        if (targetRadius > _hardLimitRadius2)
        {
            //transform.position = _targetTransform.position + (dirToTarget * _hardLimitRadius2 * 1.0f);
            //_hardLimitUpdated = true;

            transform.position += deltaPosition;
            Debug.Log("하드리밋");

        }
    }

    private void LateUpdate()
    {
        WhereThisFuncToLocateFuck();

    }

    public void WhereThisFuncToLocateFuck2()
    {
        Debug.Assert(_targetTransform != null, "로직 도중에 사라졌다면 이 객체도 사라져야 합니다");


        //if (_hardLimitUpdated == true)
        //{
        //    return;
        //}

        //Vector3 dirToTarget = (_targetTransform.position - transform.position);
        //float targetRadius = dirToTarget.magnitude;

        //float totalDampingTime = _dampingTime2 / (targetRadius / _hardLimitRadius2);

        //if (totalDampingTime <= float.Epsilon)
        //{
        //    transform.position = _targetTransform.position;
        //    Debug.Log("너무 빨라서 그냥 붙였다");
        //    return;
        //}

        transform.position = Vector3.SmoothDamp(transform.position, _targetTransform.position, ref _dampingRef, _dampingTime2);
        transform.rotation = _targetTransform.rotation;
    }


    public void WhereThisFuncToLocateFuck()
    {
        Debug.Assert(_targetTransform != null, "로직 도중에 사라졌다면 이 객체도 사라져야 합니다");


        //if (_hardLimitUpdated == true)
        //{
        //    return;
        //}

        Vector3 dirToTarget = (_targetTransform.position - transform.position);
        float targetRadius = dirToTarget.magnitude;

        float totalDampingTime = _dampingTime2 / (targetRadius / _hardLimitRadius2);

        if (totalDampingTime <= float.Epsilon)
        {
            transform.position = _targetTransform.position;
            Debug.Log("너무 빨라서 그냥 붙였다");
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, _targetTransform.position, ref _dampingRef, _dampingTime2);
       // transform.rotation = _targetTransform.rotation;
        //transform.position = _targetTransform.position;

        transform.LookAt(_lookAtObject.transform.position);






        //Vector3 dirToTarget = (_targetTransform.position - transform.position);

        ////float targetRadius = dirToTarget.magnitude;
        //float targetRadiusX = Mathf.Abs(dirToTarget.x);
        //float targetRadiusY = Mathf.Abs(dirToTarget.y);
        //float targetRadiusZ = Mathf.Abs(dirToTarget.z);

        //if (targetRadius <= _deadzoneRadius)
        //{
        //    return; //움직이지 않는 영역에 있다.
        //}

        //if (targetRadius > _hardLimitRadius)
        //{
        //    ////너무 크게 움직였다면 하드존까지 끌어온다
        //    ///

        //    float dampedPosX = Mathf.SmoothDamp(transform.position.x, _targetTransform.position.x, ref _dampingRefX, _dampingTime.x / (targetRadius / _hardLimitRadius));
        //    float dampedPosY = Mathf.SmoothDamp(transform.position.y, _targetTransform.position.y, ref _dampingRefY, _dampingTime.y / (targetRadius / _hardLimitRadius));
        //    transform.position = new Vector3(dampedPosX, dampedPosY, transform.position.z);
        //    //transform.position = _targetTransform.position + (dirToTarget * _hardLimitRadius * -1.0f);
        //    return;

        //    //float additionalSpeed = targetRadius / _hardLimitRadius;

        //    //transform.position = Vector3.SmoothDamp(transform.position, _targetTransform.position, ref _dampingRef, _dampingTime / additionalSpeed);

        //    //Debug.Log("하드리밋");
        //}
        //else
        //{
        //    float dampedPosX = Mathf.SmoothDamp(transform.position.x, _targetTransform.position.x, ref _dampingRefX, _dampingTime.x);
        //    float dampedPosY = Mathf.SmoothDamp(transform.position.y, _targetTransform.position.y, ref _dampingRefY, _dampingTime.y);
        //    transform.position = new Vector3(dampedPosX, dampedPosY, transform.position.z);
        //}

        //if (targetRadiusX > _hardLimitRadius.x)
        //{
        //    dampedPosX = Mathf.SmoothDamp(transform.position.x, _targetTransform.position.x, ref _dampingRefX, _dampingTime.x / (targetRadiusX / _hardLimitRadius.x));
        //}
        //else
        //{
        //    dampedPosX = Mathf.SmoothDamp(transform.position.x, _targetTransform.position.x, ref _dampingRefX, _dampingTime.x);
        //}


        //if (targetRadiusY > _hardLimitRadius.y)
        //{
        //    dampedPosY = Mathf.SmoothDamp(transform.position.y, _targetTransform.position.y, ref _dampingRefY, _dampingTime.y / (targetRadiusY / _hardLimitRadius.y));
        //}
        //else
        //{
        //    dampedPosY = Mathf.SmoothDamp(transform.position.y, _targetTransform.position.y, ref _dampingRefY, _dampingTime.y);
        //}

        //if (targetRadiusZ > _hardLimitRadius.z)
        //{
        //    dampedPosZ = Mathf.SmoothDamp(transform.position.z, _targetTransform.position.z, ref _dampingRefZ, _dampingTime.z / (targetRadiusZ / _hardLimitRadius.z));
        //}
        //else
        //{
        //    dampedPosZ = Mathf.SmoothDamp(transform.position.z, _targetTransform.position.z, ref _dampingRefZ, _dampingTime.z);
        //}
        //transform.position = new Vector3(dampedPosX, dampedPosY, dampedPosZ);



        //dampedPosX = Mathf.SmoothDamp(transform.position.x, _targetTransform.position.x, ref _dampingRefX, _dampingTime.x / (targetRadiusX / _hardLimitRadius.x));

        //dampedPosY = Mathf.SmoothDamp(transform.position.y, _targetTransform.position.y, ref _dampingRefY, _dampingTime.y / (targetRadiusY / _hardLimitRadius.y));

        //dampedPosZ = Mathf.SmoothDamp(transform.position.z, _targetTransform.position.z, ref _dampingRefZ, _dampingTime.z / (targetRadiusZ / _hardLimitRadius.z));

        //transform.position = new Vector3(dampedPosX, dampedPosY, dampedPosZ);

        //Vector3 relativePosition = _targetTransform.transform.InverseTransformPoint(transform.position);
        ////relativePosition 은 내 위치를 타겟의 기준으로 생각하게 된다
        ////로컬포지션 화 된다는 뜻

        //float dampedPosX = 0.0f;
        //float dampedPosY = 0.0f;
        //float dampedPosZ = 0.0f;

        //if (Mathf.Abs(relativePosition.x) > float.Epsilon)
        //{
        //    dampedPosX = Mathf.SmoothDamp(relativePosition.x, 0.0f, ref _dampingRefX, _dampingTime.x);

        //}

        //if (Mathf.Abs(relativePosition.y) > float.Epsilon)
        //{
        //    dampedPosY = Mathf.SmoothDamp(relativePosition.y, 0.0f, ref _dampingRefY, _dampingTime.y);

        //}

        //if (Mathf.Abs(relativePosition.z) > float.Epsilon)
        //{
        //    dampedPosZ = Mathf.SmoothDamp(relativePosition.z, 0.0f, ref _dampingRefZ, _dampingTime.z);

        //}

        //transform.position -= new Vector3(dampedPosX, dampedPosY, dampedPosZ);


    }
}
