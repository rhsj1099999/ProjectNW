using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemInfo;


public struct EquipmentCellDesc
{
    public EquipmentBoard _owner;
}

public class EquipmentCell : MonoBehaviour
{
    [SerializeField] private EquipType _equipType = EquipType.None;

    private EquipmentBoard _ownerEquipmentBoard = null;

    public void Initialize(EquipmentCellDesc cellDesc)
    {
        Debug.Assert(cellDesc._owner != null, "EquipCell�ʱ�ȭ�� owner�� null�Դϴ�");
        _ownerEquipmentBoard = cellDesc._owner;
    }


    private void Awake()
    {
        Debug.Assert(_equipType != EquipType.None, "�������� None�� ������������ �ȵȴ�");
    }


    public void TryEquipItem(ItemStoreDesc storedDesc)
    {
        Debug.Assert(_ownerEquipmentBoard != null, "Equipcell Owner�� ����� �������� �ʾҽ��ϴ�");


        if ((storedDesc._info._equipType & _equipType) == (int)EquipType.None)
        {
            //���� ������ �ٸ����̴�
            //�������� board ���� cell ������ ��
            return;
        }

        _ownerEquipmentBoard.EquipItem(storedDesc, this.gameObject);
    }

    public EquipType GetCellType()
    {
        return _equipType;
    }
}
