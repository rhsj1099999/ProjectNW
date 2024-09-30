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


    public Transform _targetTransform;  // 오른손 뼈
    public string _holdBreathKey = "LeftShift";  // 숨참기 키
    private bool _isHoldBreath = false;

    private Quaternion _originalRotation;  // 초기 오른손 회전 값 저장용
    private Vector3 _originalPosition;  // 초기 오른손 회전 값 저장용
}
