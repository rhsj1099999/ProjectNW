using System;
using UnityEngine;
using System.Collections.Generic;

//�������� ����Ҷ� �ʿ��� ������

[CreateAssetMenu(fileName = "ItemAsset_Magazine", menuName = "Scriptable Object/Create_ItemAsset_Magazine", order = (int)MyUtil.CustomToolOrder.CreateItemAsset)]
public class ItemAsset_Magazine : ItemAsset
{
    [SerializeField]
    private ItemAsset_Bullet.BulletType _magazineType = ItemAsset_Bullet.BulletType.bullet_5u5d5;
    public ItemAsset_Bullet.BulletType _MagazineType => _magazineType;


    [SerializeField]
    private int _maxBulletCount = -1;
    public int _MaxBulletCount => _maxBulletCount;
}