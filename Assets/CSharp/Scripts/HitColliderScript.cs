using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitColliderScript : MonoBehaviour
{
    [SerializeField] private CharacterModelDataInitializer _firstInitializeSubScript = null;

    /*------------------------------------------------
    |NOTI| �� �������� ġ��Ÿ�� ���� �ݶ��̴���!
    ------------------------------------------------*/
    [SerializeField] private bool _isWeakPoint = false;
    
    private Action<Collider, bool> _enterAction = null;
    
    private void Start()
    {
        if (_firstInitializeSubScript == null)
        {
            Debug.Assert(false, "�𵨿� �پ��ִ� ��ũ��Ʈ�Դϴ�. �� ������ �̴ϼȶ������� �����ϼ���");
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
    |TODO| IHitable�� ���� �ʿ� �������� �ֽ��ϴ�.
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
