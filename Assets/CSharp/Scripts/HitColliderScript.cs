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
    [SerializeField] private Collider _myCollider = null;
    
    private Action<Collider, bool, Vector3, Vector3> _enterAction = null;

    private void Awake()
    {
        if (_myCollider == null) 
        {
            Debug.Assert(false, "�ݶ��̴��� �����ϼ���");
            Debug.Break();
        }
    }

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
        Vector3 closetPoint = _myCollider.ClosestPoint(other.transform.position);
        Vector3 hitNormal = other.GetComponent<ColliderScript>().GetMoveDir().Value;

        if (_enterAction != null)
        {
            _enterAction(other, _isWeakPoint, closetPoint, hitNormal);
        }
    }

    /*----------------------------------------------------------
    |TODO| IHitable�� ���� �ʿ� �������� �ֽ��ϴ�.
    ----------------------------------------------------------*/

    public void CollisionDirectly(DamageDesc damage, CharacterScript attacker, ref Vector3 closetPoint, ref Vector3 hitNormal)
    {
        if (_firstInitializeSubScript.GetOwner().GetDead() == true) 
        {
            return;
        }

        IHitable hitable = _firstInitializeSubScript.GetOwner();

        hitable.DealMe_Final(damage, _isWeakPoint, attacker, _firstInitializeSubScript.GetOwner(), ref closetPoint, ref hitNormal);
    }
}
