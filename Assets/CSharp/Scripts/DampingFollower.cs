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
    }


    private void LateUpdate()
    {
       // WhereThisFuncToLocateFuck();

    }


    public void WhereThisFuncToLocateFuck()
    {
        Debug.Assert(_targetTransform != null, "로직 도중에 사라졌다면 이 객체도 사라져야 합니다");

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
