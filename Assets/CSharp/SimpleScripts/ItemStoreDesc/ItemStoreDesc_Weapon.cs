using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStoreDesc_Weapon : ItemStoreDescBase
{

    public ItemStoreDesc_Weapon
    (
        ItemAsset itemAsset,
        int count,
        int stroredIndex,
        bool isRotated,
        BoardUIBaseScript fromInstance
    ) : base(itemAsset, count, stroredIndex, isRotated, fromInstance) { }



    /*---------------------------------------------------
    |NOTI| 내구도 등등의 변수가 추가 될 예정!
    ---------------------------------------------------*/

}
