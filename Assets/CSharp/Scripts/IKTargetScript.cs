using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;



public class IKTargetScript : MonoBehaviour
{
    [SerializeField] public IKTargetDesc _desc = new IKTargetDesc();

    public AvatarIKGoal CalculateIKGoalType(bool isRight)
    {
        AvatarIKGoal ret = AvatarIKGoal.RightFoot;

        if (isRight == true)
        {
            //�����տ� ������ �����̴�
            if (_desc._isMainHandle == true)
            {
                ret = AvatarIKGoal.RightHand;
            }
            else
            {
                ret = AvatarIKGoal.LeftHand;
            }
        }
        else
        {
            //�޼տ� ������ �����̴�
            if (_desc._isMainHandle == true)
            {
                ret = AvatarIKGoal.LeftHand;
            }
            else
            {
                ret = AvatarIKGoal.RightHand;
            }
        }

        return ret;
    }
}
