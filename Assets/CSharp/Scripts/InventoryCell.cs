using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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



    public override bool TryMoveItemDropOnCell(ItemStoreDesc storedDesc, ref int startX, ref int startY, bool grabRotation)
    {
        Debug.Assert(_owner != null, "Cell�� ���ʴ� ���� �� ����.");

        //�ش� ���콺 ���������δ� �������� ���� �� ����.
        if (_owner.CheckItemDragDrop(storedDesc, ref startX, ref startY, grabRotation) == false)
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





    public void TryMoveItemDropOnItem(ItemStoreDesc storedDesc, ItemBase caller)
    {
        //Debug.Assert(_owner != null, "Cell�� ���ʴ� ���� �� ����.");

        //GameObject itemUI = _owner.getItem(storedDesc._storedIndex);

        //ItemBase itemBaseComponent = itemUI.GetComponent<ItemBase>();

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
