using System;
using UnityEngine;
using System.Collections.Generic;




[CreateAssetMenu(fileName = "ItemAsset_Weapon", menuName = "Scriptable Object/Create_ItemAsset_Weapon", order = (int)MyUtil.CustomToolOrder.CreateItemAsset)]
public class ItemAsset_Weapon : ItemAsset
{
    public enum WeaponType //ĳ���͸��� ���� �������� �޶��� �� ������ ��üȭ �س����� �ʴ´�
    {
        NotWeapon = 0,

        SmallSword, //�ܰ˰�����
        MediumSword, //�Ѽհ˷�
        LargeSword, //��˰�����

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
