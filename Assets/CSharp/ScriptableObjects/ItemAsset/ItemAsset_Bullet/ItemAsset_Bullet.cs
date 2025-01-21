using System;
using UnityEngine;
using System.Collections.Generic;

//아이템을 사용할때 필요한 정보들

[CreateAssetMenu(fileName = "ItemAsset_Bullet", menuName = "Scriptable Object/Create_ItemAsset_Bullet", order = (int)MyUtil.CustomToolOrder.CreateItemAsset)]
public class ItemAsset_Bullet : ItemAsset
{
    public enum BulletType
    {
        None = 0,
        bullet_5u5d5,
        bullet_7u5d5,
        bullet_0u5d5,
    }

    [SerializeField] private BulletType _bulletType = BulletType.bullet_5u5d5;
    public BulletType _BulletType => _bulletType;



}