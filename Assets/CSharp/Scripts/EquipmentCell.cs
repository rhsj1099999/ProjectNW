using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private ItemStoreDesc _itemStoreDesc = null;
    

    private void Awake()
    {
        Debug.Assert(_equipType != EquipType.None, "�������� None�� ������������ �ȵȴ�");
    }


    public void SetItemStoreDesc(ItemStoreDesc storeDesc)
    {
        if (_itemStoreDesc != null)
        {
            Debug.Assert(false, "�̹� �Ҵ�� �������Դϴ�");
            Debug.Break();
        }

        _itemStoreDesc = storeDesc;
    }


    public ItemStoreDesc GetItemStoreDesc()
    {
        return _itemStoreDesc;
    }


    public void ClearItemStoreDesc()
    {
        if (_itemStoreDesc == null)
        {
            Debug.Assert(false, "�̹� ����� �������Դϴ�");
            Debug.Break();
        }
        
        _itemStoreDesc = null;
    }


    public void Initialize(EquipmentCellDesc cellDesc)
    {
        Debug.Assert(cellDesc._owner != null, "EquipCell�ʱ�ȭ�� owner�� null�Դϴ�");
        _ownerEquipmentBoard = cellDesc._owner;
    }

    public bool TryEquipItem(ItemStoreDesc storedDesc)
    {
        Debug.Assert(_ownerEquipmentBoard != null, "Equipcell Owner�� ����� �������� �ʾҽ��ϴ�");

        //���� ������ �ٸ����̴�
        //�������� board ���� cell ������ ��
        if ((storedDesc._info._equipType & _equipType) == (int)EquipType.None)
        {
            return false;
        }

        return _ownerEquipmentBoard.EquipItem(storedDesc, this.gameObject);
    }

    public EquipType GetCellType()
    {
        return _equipType;
    }
}
