using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStoreDesc_Weapon_Gun : ItemStoreDesc_Weapon
{
    public ItemStoreDesc_Weapon_Gun
    (
        ItemAsset itemAsset,
        int count,
        int stroredIndex,
        bool isRotated,
        BoardUIBaseScript fromInstance
    ) : base(itemAsset, count, stroredIndex, isRotated, fromInstance) { }

    public ItemStoreDesc_Magazine _myMagazine = null;
}
