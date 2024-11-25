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
        Debug.Assert(_targetTransform != null, "�� ������Ʈ�� ���󰡷��� Transform �� null�̿����� �ȵȴ�.");

        /*---------------------------------------------------------
        LookAt�� ����� ī�޶��� ������� DampingFollower�� ����̿��� �ȵȴ�. ���߿� ����
        ---------------------------------------------------------*/
        Debug.Assert(_lookAtObject != null, "�� �ٶ󺸴� Transform �� null�̿����� �ȵȴ�.");
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
        //    ////�ʹ� ũ�� �������ٸ� �ϵ������� ����´�
        //    ///

        //    //float dampedPosX = Mathf.SmoothDamp(transform.position.x, _targetTransform.position.x, ref _dampingRefX, _dampingTime.x / (targetRadius / _hardLimitRadius));
        //    //float dampedPosY = Mathf.SmoothDamp(transform.position.y, _targetTransform.position.y, ref _dampingRefY, _dampingTime.y / (targetRadius / _hardLimitRadius));
        //    //transform.position = new Vector3(dampedPosX, dampedPosY, transform.position.z);
        //    transform.position = _targetTransform.position + (dirToTarget * _hardLimitRadius2 * -1.0f);
        //    _hardLimitUpdated = true;
        //    Debug.Log("�ϵ帮��");


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
            Debug.Log("�ϵ帮��");

        }
    }

    private void LateUpdate()
    {
        WhereThisFuncToLocateFuck();

    }

    public void WhereThisFuncToLocateFuck2()
    {
        Debug.Assert(_targetTransform != null, "���� ���߿� ������ٸ� �� ��ü�� ������� �մϴ�");


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
        //    Debug.Log("�ʹ� ���� �׳� �ٿ���");
        //    return;
        //}

        transform.position = Vector3.SmoothDamp(transform.position, _targetTransform.position, ref _dampingRef, _dampingTime2);
        transform.rotation = _targetTransform.rotation;
    }


    public void WhereThisFuncToLocateFuck()
    {
        Debug.Assert(_targetTransform != null, "���� ���߿� ������ٸ� �� ��ü�� ������� �մϴ�");


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
            Debug.Log("�ʹ� ���� �׳� �ٿ���");
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
        //    return; //�������� �ʴ� ������ �ִ�.
        //}

        //if (targetRadius > _hardLimitRadius)
        //{
        //    ////�ʹ� ũ�� �������ٸ� �ϵ������� ����´�
        //    ///

        //    float dampedPosX = Mathf.SmoothDamp(transform.position.x, _targetTransform.position.x, ref _dampingRefX, _dampingTime.x / (targetRadius / _hardLimitRadius));
        //    float dampedPosY = Mathf.SmoothDamp(transform.position.y, _targetTransform.position.y, ref _dampingRefY, _dampingTime.y / (targetRadius / _hardLimitRadius));
        //    transform.position = new Vector3(dampedPosX, dampedPosY, transform.position.z);
        //    //transform.position = _targetTransform.position + (dirToTarget * _hardLimitRadius * -1.0f);
        //    return;

        //    //float additionalSpeed = targetRadius / _hardLimitRadius;

        //    //transform.position = Vector3.SmoothDamp(transform.position, _targetTransform.position, ref _dampingRef, _dampingTime / additionalSpeed);

        //    //Debug.Log("�ϵ帮��");
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
        ////relativePosition �� �� ��ġ�� Ÿ���� �������� �����ϰ� �ȴ�
        ////���������� ȭ �ȴٴ� ��

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
