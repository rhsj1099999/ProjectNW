using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitColliderScript : MonoBehaviour
{
    [SerializeField] private CharacterModelDataInitializer _firstInitializeSubScript = null;
    
    private Action<Collider> _enterAction = null;
    
    private void Start()
    {
        CharacterScript owner = _firstInitializeSubScript.GetOwner();

        if (owner == null)
        {
            Debug.Assert(false, "しいけしけい");
            Debug.Break();
        }

        gameObject.layer = owner.gameObject.layer;
        _enterAction = owner.WhenTriggerEnterWithWeaponCollider;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_enterAction != null)
        {
            _enterAction(other);
        }
    }
}
