using System.Collections;
using System.Collections.Generic;
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

    public void Initialize(EquipmentCellDesc cellDesc)
    {
        Debug.Assert(cellDesc._owner != null, "EquipCell�ʱ�ȭ�� owner�� null�Դϴ�");
        _ownerEquipmentBoard = cellDesc._owner;
    }


    private void Awake()
    {
        Debug.Assert(_equipType != EquipType.None, "�������� None�� ������������ �ȵȴ�");
    }


    public void TryEquipItem(ItemStoreDesc storedDesc)
    {
        Debug.Assert(_ownerEquipmentBoard != null, "Equipcell Owner�� ����� �������� �ʾҽ��ϴ�");

        if (storedDesc._info._equipType != _equipType)
        {
            //���� ������ �ٸ����̴�
            //�������� board ���� cell ������ ��
            return;
        }

        _ownerEquipmentBoard.EquipItem(storedDesc, this.gameObject);
    }

    public EquipType GetCellType()
    {
        return _equipType;
    }




    //public void DeleteOnMe(ItemStoreDesc storeDesc) // : IMoveItemStore
    //{
    //    Debug.Assert(_equippedItemUI != null, "�������� �ʾҴµ� ���������մϴ�??");
    //    Destroy(_equippedItemUI);
    //    _equippedItemUI = null;

    //    UnEquipMesh(storeDesc); //���� �޽��� �����Ѵ�
    //}






    //public void EquipItem(ItemStoreDesc storedDesc)
    //{
        //if (_equippedItemUI != null) 
        //{
        //    return;
        //    //�̹� �ش�ĭ�� �����ϰ� ������ �׳� ����
        //    /*---------------------------------------------------------------------------
        //    |TODO|  ���߿� ������ ���ұ�� ������ �̰��� �����ؾ��Ѵ�.
        //    ---------------------------------------------------------------------------*/
        //}

        //if (storedDesc._info._equipType != _equipType)
        //{
        //    //���� ������ �ٸ����̴�
        //    return;
        //}

        ////���� �ִ� ��ü���� ������ ����
        //{
        //    storedDesc._owner.DeleteOnMe(storedDesc);
        //}

        //�� �ڽ�(Cell)���� ���� ����
        //{
        //    GameObject equipmentUIObject = Instantiate(_equipmentUIObjectPrefab, this.gameObject.transform);

        //    RectTransform equipmentUIRectTransform = equipmentUIObject.GetComponent<RectTransform>();

        //    RectTransform myRectTransform = GetComponent<RectTransform>();

        //    equipmentUIRectTransform.sizeDelta = new Vector2(myRectTransform.rect.width, myRectTransform.rect.height);

        //    equipmentUIRectTransform.position = myRectTransform.position;

        //    ItemBase itemBaseComponent = equipmentUIObject.GetComponent<ItemBase>();

        //    storedDesc._owner = this;

        //    itemBaseComponent.Initialize(this, storedDesc);

        //    _equippedItemUI = equipmentUIObject;
        //}

        ////�޽� ����
        //{
        //    EquipItemMesh(storedDesc);
        //}
    //}





}
