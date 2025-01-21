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

    public ItemAsset _itemAsset;    //����
    public int _storedIndex;        //����� ĭ(���â�̸� ���� 0)
    public int _count;              //����
    public bool _isRotated;         //ȸ���� ä�� ������־���?

    public BoardUIBaseScript _owner;   //���⼭�κ��� �Դ�.
    public virtual void OverlapItem(ItemStoreDescBase storeDesc, ref bool delete) { delete = false; }
}
