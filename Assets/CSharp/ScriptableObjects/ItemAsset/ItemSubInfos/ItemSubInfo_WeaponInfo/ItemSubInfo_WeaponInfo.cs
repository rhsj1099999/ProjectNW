using System;
using UnityEngine;
using System.Collections.Generic;




[CreateAssetMenu(fileName = "ItemSubInfo_WeaponInfo", menuName = "Scriptable Object/Create_ItemSubInfo_WeaponInfo", order = int.MinValue)]
public class ItemSubInfo_WeaponInfo : ItemSubInfo
{
    [SerializeField] private GameObject _weaponPrefab = null;
    public GameObject _WeaponPrefab => _weaponPrefab;
}
