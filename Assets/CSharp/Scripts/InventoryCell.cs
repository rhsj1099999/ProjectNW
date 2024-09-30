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
        Debug.Assert(_imageComponent != null, "�̹��� ������Ʈ�� �־���ϴ� ��ũ��Ʈ");
        
        _originalColor = _imageComponent.color;
    }

    public void Initialize(ref InventoryCellDesc desc)
    {
        _owner = desc._owner;
    }

    public void TryMoveItemDropOnBoard(ItemStoreDesc storedDesc, ItemBase caller)
    {
        Debug.Assert(_owner != null, "Cell�� ���ʴ� ���� �� ����.");

        int startX = -1;
        int startY = -1;
        if (_owner.CheckItemDragDrop(storedDesc, ref startX, ref startY, caller) == false)
        {
            return; //�ش� ���콺 ���������δ� �������� ���� �� ����.
        }

        storedDesc._owner.DeleteItemUseForDragItem(storedDesc);         //�����ϰ�
        _owner.AddItemUsingForcedIndex(storedDesc, startX, startY, caller.GetRotated());     //�ִ´�
    }


    public void TryMoveItemDropOnItem(ItemStoreDesc storedDesc, ItemBase caller)
    {
        Debug.Assert(_owner != null, "Cell�� ���ʴ� ���� �� ����.");

        /*
        //1. �Ϻ��� �ڱ� �ڽ� ���� ������ �������
            //�׳� �Ǽ��ų�
            //ȸ���� �õ��� ����
        //2. ���� ���� ���� ������ ���� ���
            //��ġ���� �Ѵ� - ���ÿ��и�ŭ ä���ش�. �������� ������� �ǵ��� ���´�.
        */
        //

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

        if (itemBaseComponent.getStoredDesc()._info._itemKey != storedDesc._info._itemKey)
        {
            /*---------------------------------------------------------------------------
            |TODO|  ���߿� ������ �����ϰų� �� ����� �ҰŶ�� ���⸦ �����ؾ��Ѵ�.
            ---------------------------------------------------------------------------*/
            //���������� �ƴ�.
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



    private Color _originalColor = Color.white;


    private InventoryBoard _owner = null;
    private Image _imageComponent = null;
}
