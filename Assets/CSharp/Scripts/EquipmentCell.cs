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
        Debug.Assert(_equipType != EquipType.None, "�������� None�� ������������ �ȵȴ�");
    }

    public override bool TryMoveItemDropOnCell(ItemStoreDescBase storedDesc, ref int startX, ref int startY, bool grabRotation)
    {
        Debug.Assert(_owner != null, "Cell�� ���ʴ� ���� �� ����.");

        //All type�� Armor �Դϴ�. ���� ��� ������ ������ �� �ִ� ���׸� ����
        if ((_equipType < EquipType.HumanHead || _equipType > EquipType.HumanBackpack) &&
            storedDesc._itemAsset._EquipType == EquipType.All)
        {
            return false;
        }

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
