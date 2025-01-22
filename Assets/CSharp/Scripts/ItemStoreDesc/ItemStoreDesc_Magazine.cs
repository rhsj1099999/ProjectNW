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
            Debug.Log("�Ѿ��� �ƴϴ�");
            return;
        }

        ItemAsset_Magazine myItemAsset = _itemAsset as ItemAsset_Magazine;

        if (overlappedItemAsset._BulletType != myItemAsset._MagazineType)
        {
            Debug.Log("������ �� ���� �Ѿ��̴�");
            return;
        }

        Debug.Log("�Ѿ��� źâ�� ��������");
        Debug.Log("���� ���� = " + storeDesc._count);
        Debug.Log("��ź�Ϸ��� ���� = " + storeDesc._count);

        int loadableCount = Math.Clamp(myItemAsset._MaxBulletCount - _bullets.Count, 0, myItemAsset._MaxBulletCount);
        loadableCount = Math.Clamp(loadableCount, 0, storeDesc._count);

        Debug.Log("��ź���� = " + loadableCount);

        for (int i = 0; i < loadableCount; i++) 
        {
            _bullets.Add(storeDesc);
        }

        Debug.Log("��ź�ߴ�");

        int remain = storeDesc._count - loadableCount;

        if (remain < 0)
        {
            Debug.Assert(false, "������ ���ͼ� �ȵȴ�");
            Debug.Break();
        }

        if (remain == 0)
        {
            Debug.Log("���δ� ��ź�ؼ� �������Ѵ�.");
            delete = true;
            return;
        }

        storeDesc._count = remain;
    }
}
