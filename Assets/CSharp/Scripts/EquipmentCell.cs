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
        Debug.Assert(_equipType != EquipType.None, "장착셀에 None이 설정돼있으면 안된다");
    }


    public void SetItemStoreDesc(ItemStoreDesc storeDesc)
    {
        if (_itemStoreDesc != null)
        {
            Debug.Assert(false, "이미 할당된 장착셀입니다");
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
            Debug.Assert(false, "이미 비워진 장착셀입니다");
            Debug.Break();
        }
        
        _itemStoreDesc = null;
    }


    public void Initialize(EquipmentCellDesc cellDesc)
    {
        Debug.Assert(cellDesc._owner != null, "EquipCell초기화시 owner가 null입니다");
        _ownerEquipmentBoard = cellDesc._owner;
    }

    public bool TryEquipItem(ItemStoreDesc storedDesc)
    {
        Debug.Assert(_ownerEquipmentBoard != null, "Equipcell Owner가 제대로 설정되지 않았습니다");

        //착용 부위가 다른템이다
        //이정도는 board 말고 cell 선에서 컷
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
