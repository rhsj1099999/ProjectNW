using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSocketScript : MonoBehaviour
{
    public List<ItemInfo.WeaponType> _equippableWeaponTypes = new List<ItemInfo.WeaponType>();

    private void Awake()
    {
        Debug.Assert(_equippableWeaponTypes.Count > 0, "장착할 수 있는 무기가 없는데 소켓입니까?");
    }
}
