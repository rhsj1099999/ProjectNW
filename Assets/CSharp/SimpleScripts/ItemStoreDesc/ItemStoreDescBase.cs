using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStoreDescBase
{
    protected ItemStoreDescBase() { }
    public ItemStoreDescBase
    (
        ItemAsset itemAsset,
        int count,
        int stroredIndex,
        bool isRotated,
        BoardUIBaseScript fromInstance
    )
    {
        _itemAsset = itemAsset;
        _count = count;
        _storedIndex = stroredIndex;
        _isRotated = isRotated;
        _owner = fromInstance;
    }


    public UInt64 _absoluteKey = 0;

    public ItemAsset _itemAsset;    //인포
    public int _storedIndex;        //저장된 칸(장비창이면 거의 0)
    public int _count;              //개수
    public bool _isRotated;         //회전된 채로 저장돼있었나?

    public BoardUIBaseScript _owner;   //여기서로부터 왔다.
    public virtual void OverlapItem(ItemStoreDescBase storeDesc, ref bool delete) { delete = false; }
}
