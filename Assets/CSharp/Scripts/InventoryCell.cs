using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ItemUI;

public class InventoryCell : BoardUICellBase
{
    private Color _originalColor = Color.white;
    private Image _imageComponent = null;

    [SerializeField] private int _cellIndex = -1;
    public int _CellIndex => _cellIndex;

    private void Awake()
    {
        _imageComponent = GetComponent<Image>();
        Debug.Assert(_imageComponent != null, "이미지 컴포넌트가 있어야하는 스크립트");
        
        _originalColor = _imageComponent.color;
    }



    public override bool TryMoveItemDropOnCell(ItemStoreDescBase storedDesc, ref int startX, ref int startY, bool grabRotation)
    {
        Debug.Assert(_owner != null, "Cell의 오너는 널일 수 없다.");

        //해당 마우스 포지션으로는 아이템을 넣을 수 없다.
        if (_owner.CheckItemDragDrop(storedDesc, ref startX, ref startY, grabRotation, this) == false)
        {
            return false; 
        }

        return true;
    }


    public void SetCellIndex(int index)
    {
        _cellIndex = index;
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
