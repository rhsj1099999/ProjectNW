using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript
{
    public PlayerScript _owner = null;
    public ItemInfo _itemInfo = null;
    public ItemInfo.WeaponType _weaponType = ItemInfo.WeaponType.NotWeapon;
    public AnimationClip _handlingIdleAnimation = null;

    public GameObject _itemPrefab = null;
    public string _itemPrefabRoute = null;

}
