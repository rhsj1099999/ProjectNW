using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct InventoryTransitionDesc
{
    public InventoryBoard _from;
    public int _fromIndex;
    public ItemInfo _itemInfo;
}

public struct InventoryCellDesc
{
    public InventoryBoard _owner;
}

public class InventoryCell : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

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

    public void TryMoveItemDropOnBoard(ItemStoreDesc storedDesc, ItemBase caller)
    {
        Debug.Assert(_owner != null, "Cell의 오너는 널일 수 없다.");

        int startX = -1;
        int startY = -1;
        if (_owner.CheckItemDragDrop(storedDesc, ref startX, ref startY, caller) == false)
        {
            return; //해당 마우스 포지션으로는 아이템을 넣을 수 없다.
        }

        storedDesc._owner.DeleteItemUseForDragItem(storedDesc);         //삭제하고
        _owner.AddItemUsingForcedIndex(storedDesc, startX, startY, caller.GetRotated());     //넣는다
    }


    public void TryMoveItemDropOnItem(ItemStoreDesc storedDesc, ItemBase caller)
    {
        Debug.Assert(_owner != null, "Cell의 오너는 널일 수 없다.");

        /*
        //1. 완벽한 자기 자신 위에 포개어 넣은경우
            //그냥 실수거나
            //회전을 시도한 경우다
        //2. 동일 종류 위에 포개어 넣은 경우
            //합치려고 한다 - 스택여분만큼 채워준다. 나머지는 원래대로 되돌려 놓는다.
        */
        //

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

        if (itemBaseComponent.getStoredDesc()._info._itemKey != storedDesc._info._itemKey)
        {
            /*---------------------------------------------------------------------------
            |TODO|  나중에 아이템 스왑하거나 룬 끼우기 할거라면 여기를 수정해야한다.
            ---------------------------------------------------------------------------*/
            //동일종류가 아님.
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



    private Color _originalColor = Color.white;


    private InventoryBoard _owner = null;
    private Image _imageComponent = null;
}
