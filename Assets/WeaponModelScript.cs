using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponModelScript : MonoBehaviour
{
    [SerializeField] private List<Collider> _weaponColliders = new List<Collider>();

    private void Awake()
    {
        if (_weaponColliders.Count <= 0)
        {
            Debug.Assert(false, "웨폰 콜라이더들을 설정해주세요");
            Debug.Break();
        }
    }

    public void OffCollider()
    {
        foreach (var collider in _weaponColliders)
        {
            collider.enabled = false;
        }
    }
}
