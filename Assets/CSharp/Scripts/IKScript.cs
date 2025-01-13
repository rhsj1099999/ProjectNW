using MagicaCloth2;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class IKScript : MonoBehaviour
{
    //모델에 달려있는 컴포넌트. 객체가 IK를 요청한다 그에따라서 끄고 켠다
    /*------------------------------------------
    IK Section.
    ------------------------------------------*/
    private class IKDesc
    {
        public IKDesc(IKTargetDesc targetDesc, Transform targetTransform, AvatarIKGoal goal)
        {
            _targetTransform = targetTransform;
            _targetDesc = targetDesc;
            _goal = goal;
        }

        public Transform _targetTransform = null;
        public IKTargetDesc _targetDesc = null;
        public AvatarIKGoal _goal;
        public bool _activated = true;
    }


    private Dictionary<IKTargetDesc, IKDesc> _ikDic = new Dictionary<IKTargetDesc, IKDesc>();
    private Animator _ikAnimator = null;
    private bool _ikRunning = true;

    private void Awake()
    {
        _ikAnimator = GetComponent<Animator>();
        Debug.Assert( _ikAnimator != null, "IK를 사용하려는데 Animator가 없습니까?");
    }

    public void OnIK(IKTargetDesc ikDesc) 
    {
        if (_ikDic.ContainsKey(ikDesc) == false)
        {
            return;
        }

        _ikDic[ikDesc]._activated = true;
    }

    public void OffIK(IKTargetDesc ikDesc) 
    {
        if (_ikDic.ContainsKey(ikDesc) == false)
        {
            return;
        }

        _ikDic[ikDesc]._activated = false;
    }

    public Animator GetIKAnimator() { return _ikAnimator; }

    public void ClearIK()
    {
        _ikDic.Clear();
    }

    public void RegistIK(AvatarIKGoal goal, Transform targetTransform, IKTargetDesc desc)
    {
        if (_ikDic.ContainsKey(desc) == true)
        {
            _ikDic.Remove(desc);
        }

        _ikDic.Add(desc, new IKDesc(desc, targetTransform, goal));

        Debug.Log("IKDic Count = " + _ikDic.Count);
    }

    public void DestroyIK(IKTargetDesc desc)
    {
        if (_ikDic.ContainsKey(desc) == false)
        {
            Debug.Assert(false, "없는데 지우려고 했습니다");
            Debug.Break();
            return;
        }

        _ikDic.Remove(desc);
    }


    public void OnAnimatorIK(int layerIndex)
    {
        foreach (KeyValuePair<IKTargetDesc, IKDesc> ikdesc in _ikDic)
        {
            if (ikdesc.Value._activated == false)
            {
                continue;
            }
            
            if (ikdesc.Value._targetDesc._isRotationIK == true)
            {
                _ikAnimator.SetIKPositionWeight(ikdesc.Value._goal, ikdesc.Value._targetDesc._rotationIKWeight);
                _ikAnimator.SetIKRotation(ikdesc.Value._goal, ikdesc.Value._targetTransform.rotation);
            }

            if (ikdesc.Value._targetDesc._isPositionIK == true)
            {
                _ikAnimator.SetIKPositionWeight(ikdesc.Value._goal, ikdesc.Value._targetDesc._positionIKWeight);
                _ikAnimator.SetIKPosition(ikdesc.Value._goal, ikdesc.Value._targetTransform.position);
            }
        }
    }
}
