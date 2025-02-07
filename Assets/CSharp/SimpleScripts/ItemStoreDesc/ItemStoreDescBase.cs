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
    public bool _isRotated;         //ȸ���� ä�� ������־���?

    /*--------------------------------------------------------
    |NOTI| ���� = �Լ��θ� �ٲټ���. ���������� �ֽ��ϴ�
    --------------------------------------------------------*/
    private int _count;
    public int _Count => _count;

    public BoardUIBaseScript _owner;   //���⼭�κ��� �Դ�.
    public virtual void OverlapItem(ItemStoreDescBase storeDesc, ref bool delete) { delete = false; }

    public Action<int> _itemCountDelegate = null;

    public void PlusItemCount(int delta)
    {
        _count += delta;
        _itemCountDelegate?.Invoke(_count);
    }

    public void SetItemCount(int count)
    {
        _count = count;
        _itemCountDelegate?.Invoke(_count);

        if (_count <= 0)
        {
            //0�� -> ���������Ѵ�!
            int a = 10;
            _owner.DeleteOnMe(this);
        }
    }

    public void AddCountDeleAction(Action<int> action)
    {
        _itemCountDelegate += action;
    }
}
