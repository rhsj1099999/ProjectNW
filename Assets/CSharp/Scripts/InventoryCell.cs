using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        //Debug.Assert(_owner != null, "�κ�Cell �� Owner �� null�� �� ����");
    }

    public void Initialize(ref InventoryCellDesc desc)
    {
        _owner = desc._owner;
    }

    public void TryMoveItemToInventoryBoard(InventoryTransitionDesc transitionDesc)
    {
        Debug.Assert(_owner != null, "Cell�� ���ʴ� ���� �� ����.");

        //�巡�� �ؼ� �κ��丮 ���� ������ �� �Լ��� ȣ��ȴ�.
        //������ ���ؼ� �����غ����Ѵ�.
        //1. �̹� ���� ���� ������ Ű ���� �������� ���
        //1.1 ���ð����ϴ�
        //�ִ뽺���� ������ �ִ� ->������ ������Ų��
        //�ִ뽺���� ������ ���� ->�ƹ��͵� ���Ѵ� (Ȥ�� �˾Ƽ� �ڸ�ã�� -> �̰� ��ȹ�����)
        //1.2 ���ð������� �ʴ�
        // ||�������� üũ �� �ֱ�||
        //2.


        if (true)
        {
            //üũ -> �ȴ� 2���� ������ ���̾ ����
            transitionDesc._from.DeleteItemUseForDragItem(transitionDesc);  //�����ϰ�
            _owner.AddItemDragDrop(transitionDesc);                            //�ִ´�
        }


    }

    private InventoryBoard _owner = null;
}
