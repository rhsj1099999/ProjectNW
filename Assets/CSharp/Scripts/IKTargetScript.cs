using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class IKTargetDesc
{
    public bool _isRotationIK = true;
    public bool _isPositionIK = true;
    public float _positionIKWeight = 1.0f;
    public float _rotationIKWeight = 1.0f;
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
        //isRight = �����տ� ������ ���� �Դϱ�
        AvatarIKGoal ikGoal = AvatarIKGoal.LeftFoot;

        if (isRight == true)
        {
            //�����տ� ������ �����̴�
            if (_isMainHandle == true)
            {
                //return;//���Ͽ� �������� ������ �ִ� ���� IK�� �����ϸ� ������ �߻��Ѵ�.
                ikGoal = AvatarIKGoal.RightHand;

            }
            else
            {
                ikGoal = AvatarIKGoal.LeftHand;
            }
        }
        else
        {
            //�޼տ� ������ �����̴�
            if (_isMainHandle == true)
            {
                //return; //���Ͽ� �������� ������ �ִ� ���� IK�� �����ϸ� ������ �߻��Ѵ�.
                ikGoal = AvatarIKGoal.LeftHand;
            }
            else
            {
                ikGoal = AvatarIKGoal.RightHand;
            }
        }

        ikRunner.RegistIK(ikGoal, transform, _desc);
    }
}
