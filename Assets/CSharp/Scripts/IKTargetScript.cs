using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class IKTargetDesc
{
    //public AvatarIKGoal _ikType = AvatarIKGoal.RightHand;
    public bool _isRightSide = true;
    public bool _isRotationIK = true;
    public bool _isPositionIK = true;
    public float _positionIKWeight = 1.0f;
    public float _rotationIKWeight = 1.0f;
    public Transform _targetTransform = null;
}

public class IKTargetScript : MonoBehaviour
{
    [SerializeField] public bool _isMainHandle = false;
    [SerializeField] public bool _isHandIK = true;
    [SerializeField] public IKTargetDesc _desc = new IKTargetDesc();

    public IKTargetDesc GetDesc()
    {
        return _desc;
    }

    public void RegistIK(IKScript ikRunner, bool isRight)
    {
        //isRight = 오른손에 장착한 무기 입니까
        AvatarIKGoal ikGoal = AvatarIKGoal.LeftFoot;

        if (isRight == true)
        {
            //오른손에 장착한 무기이다
            if (_isMainHandle == true)
            {
                ikGoal = AvatarIKGoal.RightHand;

            }
            else
            {
                ikGoal = AvatarIKGoal.LeftHand;
            }
        }
        else
        {
            //왼손에 장착한 무기이다
            if (_isMainHandle == true)
            {
                ikGoal = AvatarIKGoal.LeftHand;
            }
            else
            {
                ikGoal = AvatarIKGoal.RightHand;
            }
        }
        _desc._targetTransform = transform;
        ikRunner.RegistIK(ikGoal, transform, _desc);
    }
}
