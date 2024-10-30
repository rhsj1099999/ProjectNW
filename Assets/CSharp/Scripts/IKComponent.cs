using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKComponent : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform = null;
    [SerializeField] private Animator _ikAnimator = null;
    [SerializeField] private AvatarIKGoal _ikGoal = AvatarIKGoal.RightHand;

    public bool _isRotationIK = false;
    public bool _isPositionIK = false;
    public float _positionIKWeight = 1.0f;
    public float _rotationIKWeight = 1.0f;

    public void OnAnimatorIK(int layerIndex)
    {
        if (_targetTransform == null || _ikAnimator == null)
        {
            return;
        }

        if (_isRotationIK == true)
        {
            _ikAnimator.SetIKPositionWeight(_ikGoal, _rotationIKWeight);
            _ikAnimator.SetIKRotation(_ikGoal, _targetTransform.rotation);
        }

        if (_isPositionIK == true)
        {
            _ikAnimator.SetIKPositionWeight(_ikGoal, _positionIKWeight);
            _ikAnimator.SetIKPosition(_ikGoal, _targetTransform.position);
        }
    }


}
