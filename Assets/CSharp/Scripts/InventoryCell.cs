using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ItemUI;

public class InventoryCell : BoardUICellBase
{
    private Color _originalColor = Color.white;
    private Image _imageComponent = null;

    private void Awake()
    {
        _imageComponent = GetComponent<Image>();
        Debug.Assert(_imageComponent != null, "�̹��� ������Ʈ�� �־���ϴ� ��ũ��Ʈ");
        
        _originalColor = _imageComponent.color;
    }



    public override bool TryMoveItemDropOnCell(ItemStoreDescBase storedDesc, ref int startX, ref int startY, bool grabRotation)
    {
        Debug.Assert(_owner != null, "Cell�� ���ʴ� ���� �� ����.");

        //�ش� ���콺 ���������δ� �������� ���� �� ����.
        if (_owner.CheckItemDragDrop(storedDesc, ref startX, ref startY, grabRotation, this) == false)
        {
            return false; 
        }

        return true;
    }





    public void TurnOn()
    {
        _imageComponent.color = Color.green;
    }

    public void TurnOff()
    {
        _imageComponent.color = _originalColor;
    }





    public void TryMoveItemDropOnItem(ItemStoreDescBase storedDesc, ItemUI caller)
    {
        //Debug.Assert(_owner != null, "Cell�� ���ʴ� ���� �� ����.");

        //GameObject itemUI = _owner.getItem(storedDesc._storedIndex);

        //ItemUI itemBaseComponent = itemUI.GetComponent<ItemUI>();

        //if (itemBaseComponent == caller) 
        //{
        //    if (false /*ȸ���� �ٸ��ϱ�? ->ȸ���� �ٸ�*/)
        //    {
        //        //ȸ���� �����ϴ� �۾���
        //    }

        //    return;
        //}

        ////���������� �ƴ�.
        //if (itemBaseComponent.getStoredDesc()._itemAsset._ItemKey != storedDesc._itemAsset._ItemKey)
        //{
        //    /*---------------------------------------------------------------------------
        //    |TODO|  ���߿� ������ �����ϰų� �� ����� �ҰŶ�� ���⸦ �����ؾ��Ѵ�.
        //    ---------------------------------------------------------------------------*/
        //    return;
        //}

        ////���� ������. ->���� �������� üũ, ���� �����Ѹ�ŭ�� ���� �ű��
    }
}
