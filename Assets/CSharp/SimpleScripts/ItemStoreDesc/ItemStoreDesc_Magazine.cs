using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemStoreDesc_Magazine : ItemStoreDescBase
{
    public ItemStoreDesc_Magazine
    (
        ItemAsset itemAsset,
        int count,
        int stroredIndex,
        bool isRotated,
        BoardUIBaseScript fromInstance,
        List<ItemStoreDescBase> bullets
    ) : base(itemAsset, count, stroredIndex, isRotated, fromInstance)
    {
        if (bullets != null)
        {
            _bullets = bullets;
        }
    }

    public List<ItemStoreDescBase> _bullets = new List<ItemStoreDescBase>();

    public override void OverlapItem(ItemStoreDescBase storeDesc, ref bool delete)
    {
        delete = false;

        ItemAsset_Bullet overlappedItemAsset = storeDesc._itemAsset as ItemAsset_Bullet;

        if (overlappedItemAsset == null)
        {
            Debug.Log("총알이 아니다");
            return;
        }

        ItemAsset_Magazine myItemAsset = _itemAsset as ItemAsset_Magazine;

        if (overlappedItemAsset._BulletType != myItemAsset._MagazineType)
        {
            Debug.Log("장전할 수 없는 총알이다");
            return;
        }

        Debug.Log("총알이 탄창에 겹쳐졌다");
        Debug.Log("현재 개수 = " + storeDesc._count);
        Debug.Log("삽탄하려는 개수 = " + storeDesc._count);

        int loadableCount = Math.Clamp(myItemAsset._MaxBulletCount - _bullets.Count, 0, myItemAsset._MaxBulletCount);
        loadableCount = Math.Clamp(loadableCount, 0, storeDesc._count);

        Debug.Log("삽탄가능 = " + loadableCount);

        for (int i = 0; i < loadableCount; i++) 
        {
            _bullets.Add(storeDesc);
        }

        Debug.Log("삽탄했다");

        int remain = storeDesc._count - loadableCount;

        if (remain < 0)
        {
            Debug.Assert(false, "음수가 나와선 안된다");
            Debug.Break();
        }

        if (remain == 0)
        {
            Debug.Log("전부다 삽탄해서 지워야한다.");
            delete = true;
            return;
        }

        storeDesc._count = remain;
    }
}
