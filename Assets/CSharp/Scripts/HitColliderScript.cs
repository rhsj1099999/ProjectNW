using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitColliderScript : MonoBehaviour
{
    [SerializeField] private CharacterModelDataInitializer _firstInitializeSubScript = null;

    /*------------------------------------------------
    |NOTI| 날 때렸을때 치명타로 들어가는 콜라이더다!
    ------------------------------------------------*/
    [SerializeField] private bool _isWeakPoint = false;
    
    private Action<Collider, bool> _enterAction = null;
    
    private void Start()
    {
        if (_firstInitializeSubScript == null)
        {
            Debug.Assert(false, "모델에 붙어있는 스크립트입니다. 모델 데이터 이니셜라이저를 지정하세요");
            Debug.Break();
        }

        CharacterScript owner = _firstInitializeSubScript.GetOwner();
        gameObject.layer = owner.gameObject.layer;
        _enterAction = owner.WhenTriggerEnterWithWeaponCollider;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_enterAction != null)
        {
            _enterAction(other, _isWeakPoint);
        }
    }

    /*----------------------------------------------------------
    |TODO| IHitable는 이제 필요 없을수도 있습니다.
    ----------------------------------------------------------*/

    public void CollisionDirectly(DamageDesc damage, CharacterScript attacker)
    {
        if (_firstInitializeSubScript.GetOwner().GetDead() == true) 
        {
            return;
        }

        IHitable hitable = _firstInitializeSubScript.GetOwner() as IHitable;

        hitable.DealMe_Final(damage, _isWeakPoint, attacker, _firstInitializeSubScript.GetOwner());
    }
}
