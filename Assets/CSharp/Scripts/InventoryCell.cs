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
        Debug.Assert(_imageComponent != null, "�̹��� ������Ʈ�� �־���ϴ� ��ũ��Ʈ");
        
        _originalColor = _imageComponent.color;
    }

    public void Initialize(ref InventoryCellDesc desc)
    {
        _owner = desc._owner;
    }

    public bool TryMoveItemDropOnBoard(ItemStoreDesc storedDesc, ItemBase caller)
    {
        Debug.Assert(_owner != null, "Cell�� ���ʴ� ���� �� ����.");

        int startX = -1;
        int startY = -1;

        //�ش� ���콺 ���������δ� �������� ���� �� ����.
        if (_owner.CheckItemDragDrop(storedDesc, ref startX, ref startY, caller) == false)
        {
            return false; 
        }

        //�����ϰ�
        storedDesc._owner.DeleteOnMe(storedDesc);

        //�ִ´�
        _owner.AddItemUsingForcedIndex(storedDesc, startX, startY, caller.GetRotated());

        return true;
    }


    public void TryMoveItemDropOnItem(ItemStoreDesc storedDesc, ItemBase caller)
    {
        Debug.Assert(_owner != null, "Cell�� ���ʴ� ���� �� ����.");

        GameObject itemUI = _owner.getItem(storedDesc._storedIndex);

        ItemBase itemBaseComponent = itemUI.GetComponent<ItemBase>();

        if (itemBaseComponent == caller) 
        {
            if (false /*ȸ���� �ٸ��ϱ�? ->ȸ���� �ٸ�*/)
            {
                //ȸ���� �����ϴ� �۾���
            }

            return;
        }

        //���������� �ƴ�.
        if (itemBaseComponent.getStoredDesc()._info._itemKey != storedDesc._info._itemKey)
        {
            /*---------------------------------------------------------------------------
            |TODO|  ���߿� ������ �����ϰų� �� ����� �ҰŶ�� ���⸦ �����ؾ��Ѵ�.
            ---------------------------------------------------------------------------*/
            return;
        }

        //���� ������. ->���� �������� üũ, ���� �����Ѹ�ŭ�� ���� �ű��
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
