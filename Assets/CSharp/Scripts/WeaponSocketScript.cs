using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSocketScript : MonoBehaviour
{
    public List<ItemInfo.WeaponType> _equippableWeaponTypes = new List<ItemInfo.WeaponType>();

    private void Awake()
    {
        Debug.Assert(_equippableWeaponTypes.Count > 0, "������ �� �ִ� ���Ⱑ ���µ� �����Դϱ�?");
    }
}
