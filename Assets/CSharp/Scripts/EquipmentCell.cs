using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static ItemAsset;



public class EquipmentCell : BoardUICellBase
{
    [SerializeField] private EquipType _equipType = EquipType.None;

    //private ItemStoreDesc _itemStoreDesc = null;
    //public ItemStoreDesc _ItemStoreDesc { get ; set; }

    private void Awake()
    {
        Debug.Assert(_equipType != EquipType.None, "장착셀에 None이 설정돼있으면 안된다");
    }

    //private ItemStoreDesc _itemStoreDesc = null;

    //public void SetItemStoreDesc(ItemStoreDesc storeDesc)
    //{
    //    if (_itemStoreDesc != null)
    //    {
    //        Debug.Assert(false, "이미 할당된 장착셀입니다");
    //        Debug.Break();
    //    }

    //    _itemStoreDesc = storeDesc;
    //}


    //public ItemStoreDesc GetItemStoreDesc()
    //{
    //    return _itemStoreDesc;
    //}


    //public void ClearItemStoreDesc()
    //{
    //    if (_itemStoreDesc == null)
    //    {
    //        Debug.Assert(false, "이미 비워진 장착셀입니다");
    //        Debug.Break();
    //    }

    //    _itemStoreDesc = null;
    //}




    public override bool TryMoveItemDropOnCell(ItemStoreDesc storedDesc, ref int startX, ref int startY, bool grabRotation)
    {
        Debug.Assert(_owner != null, "Cell의 오너는 널일 수 없다.");

        //장착하려는 셀과 다른 타입입니다.
        if ((storedDesc._itemAsset._EquipType & _equipType) == (int)EquipType.None)
        {
            return false;
        }

        //해당 마우스 포지션으로는 아이템을 넣을 수 없다.
        if (_owner.CheckItemDragDrop(storedDesc, ref startX, ref startY, grabRotation, this) == false)
        {
            return false;
        }

        return true;
    }


    public EquipType GetCellType()
    {
        return _equipType;
    }
}
