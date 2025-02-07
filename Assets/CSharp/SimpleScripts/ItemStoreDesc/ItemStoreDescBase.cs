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
    public bool _isRotated;         //회전된 채로 저장돼있었나?

    /*--------------------------------------------------------
    |NOTI| 개수 = 함수로만 바꾸세요. 델리게이터 있습니다
    --------------------------------------------------------*/
    private int _count;
    public int _Count => _count;

    public BoardUIBaseScript _owner;   //여기서로부터 왔다.
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
            //0개 -> 없어져야한다!
            int a = 10;
            _owner.DeleteOnMe(this);
        }
    }

    public void AddCountDeleAction(Action<int> action)
    {
        _itemCountDelegate += action;
    }
}
