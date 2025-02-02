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
            Debug.Assert(false, "���� �ݶ��̴����� �������ּ���");
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
