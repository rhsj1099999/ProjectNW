using MagicaCloth2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Animations.Rigging;




[Serializable]
public class IKTargetDesc
{
    public bool _isRightSide = true; //--------------���� Ÿ���� �������Դϱ�
    public bool _isMainHandle = false; //------------���� Ÿ���� �� ������ ��°̴ϱ�
    public bool _isHandIK = true; //-----------------���� Ÿ���� ������ ��°̴ϱ�

    public bool _isRotationIK = true; //-------------���� Ÿ���� ȸ�� IK�� �����մϱ�
    public bool _isPositionIK = true; //-------------���� Ÿ���� ��ġ IK�� �����մϱ�

    public float _positionIKWeight = 1.0f; //--------���� Ÿ���� ȸ�� ��ǥ ����ġ
    public float _rotationIKWeight = 1.0f; //--------���� Ÿ���� ��ġ ��ǥ ����ġ
}

public class IKDesc
{
    public Transform _targetTransform = null;
    public IKTargetDesc _targetDesc = null;

    public bool _activated = true;
}


public class IKScript : MonoBehaviour
{
    /*------------------------------------------
    IK Section.
    ------------------------------------------*/



    private Dictionary<MonoBehaviour/*�����*/, Dictionary<AvatarIKGoal, IKDesc>/*������*/> _ikDic = new Dictionary<MonoBehaviour, Dictionary<AvatarIKGoal, IKDesc>>();
    private Animator _ikAnimator = null;
    private bool _ikRunning = true;

    private void Awake()
    {
        _ikAnimator = GetComponent<Animator>();
        Debug.Assert( _ikAnimator != null, "IK�� ����Ϸ��µ� Animator�� �����ϱ�?");
    }

    public void SwitchOnOffIK(MonoBehaviour caller, bool target, bool all = true, AvatarIKGoal goal = AvatarIKGoal.LeftHand) 
    {
        Dictionary<AvatarIKGoal, IKDesc> addedIKs = null;
        _ikDic.TryGetValue(caller, out addedIKs);
        if (addedIKs == null)
        {
            Debug.Assert(false, "����� IK�� ���µ� �ѷ��� �մϴ�");
            Debug.Break();
            return;
        }

        if (addedIKs.Count <= 0)
        {
            Debug.Assert(false, "����� IK�� ���µ� �ѷ��� �մϴ�. ���� = 0");
            Debug.Break();
        }

        if (all == true)
        {
            foreach (var ikDesc in addedIKs)
            {
                ikDesc.Value._activated = target;
            }
        }
        else
        {
            IKDesc targetIK = null;
            addedIKs.TryGetValue(goal, out targetIK);
            if (targetIK == null) 
            {
                Debug.Assert(false, "�ش� goal IK�� ���µ� �ѷ��� �մϴ�");
                Debug.Break();
            }
            targetIK._activated = target;
        }
    }

    public Animator GetIKAnimator() { return _ikAnimator; }

    public void ClearIK()
    {
        _ikDic.Clear();
    }

    public void RegistIK(MonoBehaviour caller, AvatarIKGoal goal, IKDesc ikRunDesc)
    {
        if (_ikDic.ContainsKey(caller) == false)
        {
            _ikDic.Add(caller, new Dictionary<AvatarIKGoal, IKDesc>());
        }
        Dictionary<AvatarIKGoal, IKDesc> target = _ikDic[caller];

        if (target.ContainsKey(goal) == true)
        {
            Debug.Assert(false, "�̹� �ش� IK�� ����߽��ϴ�");
            Debug.Break();
        }

        target.Add(goal, ikRunDesc);

        Debug.Log("IKDic Count = " + _ikDic.Count);
    }



    public void DestroyIK(MonoBehaviour caller)
    {
        Dictionary<AvatarIKGoal, IKDesc> target = null;
        _ikDic.TryGetValue(caller, out target);
        if (target == null)
        {
            Debug.Assert(false, "caller�� IK�� ��������� �����ϴ�");
            Debug.Break();
        }

        _ikDic.Remove(caller);
    }

    public void DestroyIK(MonoBehaviour caller, AvatarIKGoal goal, IKDesc ikRunDesc)
    {
        Dictionary<AvatarIKGoal, IKDesc> target = null;
        _ikDic.TryGetValue(caller, out target);
        if (target == null) 
        {
            Debug.Assert(false, "caller�� IK�� ��������� �����ϴ�");
            Debug.Break();
        }

        if (target.ContainsKey(goal) == false)
        {
            Debug.Assert(false, "caller�� �ش� IK�� ��������� �����ϴ�");
            Debug.Break();
        }

        target.Remove(goal);

        Debug.Log("IKDic Count = " + _ikDic.Count);
    }


    public void OnAnimatorIK(int layerIndex)
    {
        /*-------------------------------------------------------
        -------------------------------------------------------*/
        foreach (var callersIKs in _ikDic)
        {
            foreach (var ikRunDesc in callersIKs.Value)
            {
                if (ikRunDesc.Value._activated == false)
                {
                    continue;
                }

                if (ikRunDesc.Value._targetDesc._isRotationIK == true)
                {
                    _ikAnimator.SetIKPositionWeight(ikRunDesc.Key, ikRunDesc.Value._targetDesc._rotationIKWeight);
                    _ikAnimator.SetIKRotation(ikRunDesc.Key, ikRunDesc.Value._targetTransform.rotation);
                }



                Vector3 ikGoalPosition = ikRunDesc.Value._targetTransform.position;

                if (ikRunDesc.Key == AvatarIKGoal.LeftHand || ikRunDesc.Key == AvatarIKGoal.RightHand)
                {
                    //�տ� ����� �մϴ�. i
                    WeaponScript weaponScript = (WeaponScript)callersIKs.Key;

                }
                

                if (ikRunDesc.Value._targetDesc._isPositionIK == true)
                {
                    _ikAnimator.SetIKPositionWeight(ikRunDesc.Key, ikRunDesc.Value._targetDesc._positionIKWeight);
                    _ikAnimator.SetIKPosition(ikRunDesc.Key, ikRunDesc.Value._targetTransform.position);
                }
            }
        }
    }
}
