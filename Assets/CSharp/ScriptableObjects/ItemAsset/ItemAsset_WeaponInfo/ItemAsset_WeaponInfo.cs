using System;
using UnityEngine;
using System.Collections.Generic;




[CreateAssetMenu(fileName = "ItemAsset_Weapon", menuName = "Scriptable Object/Create_ItemAsset_Weapon", order = (int)MyUtil.CustomToolOrder.CreateItemAsset)]
public class ItemAsset_Weapon : ItemAsset
{
    public enum WeaponType //캐릭터마다 무기 파지법이 달라질 수 있으니 구체화 해놓지는 않는다
    {
        NotWeapon = 0,

        SmallSword, //단검같은거
        MediumSword, //한손검류
        LargeSword, //대검같은거

        SmallGun,
        MediumGun,
        LargeGun,
    }

    /*------------------------------------------
    Item Spec Section.
    ------------------------------------------*/

    [SerializeField] private WeaponType _weaponType;
    public WeaponType _WeaponType => _weaponType;

    [SerializeField] private DamageDesc _weaponDamageDesc = new DamageDesc();
    public DamageDesc _WeaponDamageDesc => _weaponDamageDesc;

    /*------------------------------------------
    State Section.
    ------------------------------------------*/
    [SerializeField] private StateGraphAsset _weaponStateGraph = null;
    public StateGraphAsset _WeaponStateGraph => _weaponStateGraph;




   
}
