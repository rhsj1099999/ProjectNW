using System;
using System.Collections.Generic;
using UnityEngine;


public class ZombieScript : CharacterScript, IHitable
{
    public override LayerMask CalculateWeaponColliderIncludeLayerMask()
    {
        int ret = LayerMask.GetMask("Player");
        return ret;
    }

    public override void DeadCall()
    {
        base.DeadCall();

        int dropItemCount = UnityEngine.Random.Range(2, 5);

        if (dropItemCount == 0) 
        {
            return;
        }

        for (int i = 0; i < dropItemCount; i++)
        {
            ItemAsset randomItemAsset = ItemInfoManager.Instance.GetItemInfo(UnityEngine.Random.Range(0, ItemInfoManager.Instance.GetMaxItemCount()));
            ItemStoreDescBase newStoreDescBase = ItemInfoManager.Instance.CreateItemStoreDesc(randomItemAsset, 1, 0, false, null);

            int randomDeg_X = UnityEngine.Random.Range(0, 90);
            int randomDeg_Y = UnityEngine.Random.Range(0, 360);

            Vector3 dir = Vector3.up;
            Quaternion itemThrowRotation = Quaternion.Euler(randomDeg_X, randomDeg_Y, 0);
            dir = itemThrowRotation * dir;

            int randomForce = UnityEngine.Random.Range(2, 4);
            dir *= randomForce;

            ItemInfoManager.Instance.DropItemToField(transform, newStoreDescBase, dir);
        }



    }
}
