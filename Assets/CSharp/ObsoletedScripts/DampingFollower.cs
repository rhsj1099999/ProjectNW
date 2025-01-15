using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DampingFollower : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform = null;
    [SerializeField] private Vector3 _hardLimitRadius = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private Vector3 _deadzoneRadius = Vector3.zero;

    private Vector3 _prevPosition = Vector3.zero;

    [SerializeField] GameObject _lookAtObject = null;

    //private bool _hardLimitUpdated = false;

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
    }


    private void LateUpdate()
    {
       // WhereThisFuncToLocateFuck();

    }


    public void WhereThisFuncToLocateFuck()
    {
        Debug.Assert(_targetTransform != null, "���� ���߿� ������ٸ� �� ��ü�� ������� �մϴ�");

        Vector3 dirToTarget = (_targetTransform.position - transform.position);

        Vector3 relativePosition = _targetTransform.transform.InverseTransformPoint(transform.position);

        float targetRadiusX = Mathf.Abs(relativePosition.x);
        float targetRadiusY = Mathf.Abs(relativePosition.y);
        float targetRadiusZ = Mathf.Abs(relativePosition.z);

        Vector3 addPosition = new Vector3();

        if (targetRadiusX > _hardLimitRadius.x)
        {
            addPosition.x = (targetRadiusX - _hardLimitRadius.x);
            if (relativePosition.x > 0)
            {
                addPosition.x *= -1.0f;
            }
        }

        if (targetRadiusY > _hardLimitRadius.y)
        {
            addPosition.y = (targetRadiusY - _hardLimitRadius.y);
            if (relativePosition.y > 0)
            {
                addPosition.y *= -1.0f;
            }
        }

        if (targetRadiusZ > _hardLimitRadius.z)
        {
            addPosition.z = (targetRadiusZ - _hardLimitRadius.z);
            if (relativePosition.z > 0)
            {
                addPosition.z *= -1.0f;
            }
        }

        addPosition = transform.InverseTransformPoint(addPosition);

        transform.position += addPosition;
        transform.rotation = _targetTransform.rotation;
    }
}
