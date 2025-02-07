using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static ItemAsset;
using static ItemUI;



public class EquipmentCell : BoardUICellBase
{
    [SerializeField] private EquipType _equipType = EquipType.None;

    private void Awake()
    {
        Debug.Assert(_equipType != EquipType.None, "장착셀에 None이 설정돼있으면 안된다");
    }

    public override bool TryMoveItemDropOnCell(ItemStoreDescBase storedDesc, ref int startX, ref int startY, bool grabRotation)
    {
        Debug.Assert(_owner != null, "Cell의 오너는 널일 수 없다.");

        //All type은 Armor 입니다. 따라서 모든 부위에 장착할 수 있는 버그를 수정
        if ((_equipType < EquipType.HumanHead || _equipType > EquipType.HumanBackpack) &&
            storedDesc._itemAsset._EquipType == EquipType.All)
        {
            return false;
        }

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
