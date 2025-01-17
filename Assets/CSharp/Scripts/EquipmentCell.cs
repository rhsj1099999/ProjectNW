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
        Debug.Assert(_equipType != EquipType.None, "�������� None�� ������������ �ȵȴ�");
    }

    //private ItemStoreDesc _itemStoreDesc = null;

    //public void SetItemStoreDesc(ItemStoreDesc storeDesc)
    //{
    //    if (_itemStoreDesc != null)
    //    {
    //        Debug.Assert(false, "�̹� �Ҵ�� �������Դϴ�");
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
    //        Debug.Assert(false, "�̹� ����� �������Դϴ�");
    //        Debug.Break();
    //    }

    //    _itemStoreDesc = null;
    //}




    public override bool TryMoveItemDropOnCell(ItemStoreDesc storedDesc, ref int startX, ref int startY, bool grabRotation)
    {
        Debug.Assert(_owner != null, "Cell�� ���ʴ� ���� �� ����.");

        //�����Ϸ��� ���� �ٸ� Ÿ���Դϴ�.
        if ((storedDesc._itemAsset._EquipType & _equipType) == (int)EquipType.None)
        {
            return false;
        }

        //�ش� ���콺 ���������δ� �������� ���� �� ����.
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
