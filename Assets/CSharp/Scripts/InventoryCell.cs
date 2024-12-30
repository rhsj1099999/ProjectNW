using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCellDesc
{
    public InventoryBoard _owner;
}

public class InventoryCell : MonoBehaviour
{
    private Color _originalColor = Color.white;
    private InventoryBoard _owner = null;
    private Image _imageComponent = null;

    private void Awake()
    {
        _imageComponent = GetComponent<Image>();
        Debug.Assert(_imageComponent != null, "이미지 컴포넌트가 있어야하는 스크립트");
        
        _originalColor = _imageComponent.color;
    }

    public void Initialize(ref InventoryCellDesc desc)
    {
        _owner = desc._owner;
    }

    public bool TryMoveItemDropOnBoard(ItemStoreDesc storedDesc, ItemBase caller)
    {
        Debug.Assert(_owner != null, "Cell의 오너는 널일 수 없다.");

        int startX = -1;
        int startY = -1;

        //해당 마우스 포지션으로는 아이템을 넣을 수 없다.
        if (_owner.CheckItemDragDrop(storedDesc, ref startX, ref startY, caller) == false)
        {
            return false; 
        }

        //삭제하고
        storedDesc._owner.DeleteOnMe(storedDesc);

        //넣는다
        _owner.AddItemUsingForcedIndex(storedDesc, startX, startY, caller.GetRotated());

        return true;
    }


    public void TryMoveItemDropOnItem(ItemStoreDesc storedDesc, ItemBase caller)
    {
        Debug.Assert(_owner != null, "Cell의 오너는 널일 수 없다.");

        GameObject itemUI = _owner.getItem(storedDesc._storedIndex);

        ItemBase itemBaseComponent = itemUI.GetComponent<ItemBase>();

        if (itemBaseComponent == caller) 
        {
            if (false /*회전이 다릅니까? ->회전이 다름*/)
            {
                //회전을 적용하는 작업들
            }

            return;
        }

        //동일종류가 아님.
        if (itemBaseComponent.getStoredDesc()._info._itemKey != storedDesc._info._itemKey)
        {
            /*---------------------------------------------------------------------------
            |TODO|  나중에 아이템 스왑하거나 룬 끼우기 할거라면 여기를 수정해야한다.
            ---------------------------------------------------------------------------*/
            return;
        }

        //동일 종류다. ->스택 가능한지 체크, 스택 가능한만큼만 수량 옮기기
    }

    public void TurnOn()
    {
        _imageComponent.color = Color.green;
    }

    public void TurnOff()
    {
        _imageComponent.color = _originalColor;
    }
}
