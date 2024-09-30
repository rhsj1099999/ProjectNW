using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldBreth : MonoBehaviour
{


    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) == true)
        {
            _isHoldBreath = (!_isHoldBreath);

            if (_isHoldBreath == true)
            {
                _originalRotation = _targetTransform.localRotation;
                _originalPosition = _targetTransform.localPosition;
            }
        }
    }

    private void LateUpdate()
    {
        if (_isHoldBreath == true)
        {
            _targetTransform.localRotation = _originalRotation;
            _targetTransform.localPosition = _originalPosition;
        }
    }


    public Transform _targetTransform;  // ������ ��
    public string _holdBreathKey = "LeftShift";  // ������ Ű
    private bool _isHoldBreath = false;

    private Quaternion _originalRotation;  // �ʱ� ������ ȸ�� �� �����
    private Vector3 _originalPosition;  // �ʱ� ������ ȸ�� �� �����
}
