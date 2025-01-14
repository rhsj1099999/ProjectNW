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
    public bool _isRightSide = true; //--------------현재 타겟이 오른쪽입니까
    public bool _isMainHandle = false; //------------현재 타겟은 주 손으로 잡는겁니까
    public bool _isHandIK = true; //-----------------현재 타겟은 손으로 잡는겁니까

    public bool _isRotationIK = true; //-------------현재 타겟은 회전 IK를 실행합니까
    public bool _isPositionIK = true; //-------------현재 타겟은 위치 IK를 실행합니까

    public float _positionIKWeight = 1.0f; //--------현재 타겟의 회전 목표 가중치
    public float _rotationIKWeight = 1.0f; //--------현재 타겟의 위치 목표 가중치
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



    private Dictionary<MonoBehaviour/*등록자*/, Dictionary<AvatarIKGoal, IKDesc>/*종류들*/> _ikDic = new Dictionary<MonoBehaviour, Dictionary<AvatarIKGoal, IKDesc>>();
    private Animator _ikAnimator = null;
    private bool _ikRunning = true;

    private void Awake()
    {
        _ikAnimator = GetComponent<Animator>();
        Debug.Assert( _ikAnimator != null, "IK를 사용하려는데 Animator가 없습니까?");
    }

    public void SwitchOnOffIK(MonoBehaviour caller, bool target, bool all = true, AvatarIKGoal goal = AvatarIKGoal.LeftHand) 
    {
        Dictionary<AvatarIKGoal, IKDesc> addedIKs = null;
        _ikDic.TryGetValue(caller, out addedIKs);
        if (addedIKs == null)
        {
            Debug.Assert(false, "등록한 IK가 없는데 켜려고 합니다");
            Debug.Break();
            return;
        }

        if (addedIKs.Count <= 0)
        {
            Debug.Assert(false, "등록한 IK가 없는데 켜려고 합니다. 개수 = 0");
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
                Debug.Assert(false, "해당 goal IK가 없는데 켜려고 합니다");
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
            Debug.Assert(false, "이미 해당 IK를 등록했습니다");
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
            Debug.Assert(false, "caller는 IK를 등록한적이 없습니다");
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
            Debug.Assert(false, "caller는 IK를 등록한적이 없습니다");
            Debug.Break();
        }

        if (target.ContainsKey(goal) == false)
        {
            Debug.Assert(false, "caller는 해당 IK를 등록한적이 없습니다");
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
                    //손에 쥘려고 합니다. i
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
