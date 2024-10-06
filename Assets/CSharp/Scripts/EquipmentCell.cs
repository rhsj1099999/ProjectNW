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
        Debug.Assert(cellDesc._owner != null, "EquipCell초기화시 owner가 null입니다");
        _ownerEquipmentBoard = cellDesc._owner;
    }


    private void Awake()
    {
        Debug.Assert(_equipType != EquipType.None, "장착셀에 None이 설정돼있으면 안된다");
    }


    public void TryEquipItem(ItemStoreDesc storedDesc)
    {
        Debug.Assert(_ownerEquipmentBoard != null, "Equipcell Owner가 제대로 설정되지 않았습니다");

        if (storedDesc._info._equipType != _equipType)
        {
            //착용 부위가 다른템이다
            //이정도는 board 말고 cell 선에서 컷
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
    //    Debug.Assert(_equippedItemUI != null, "장착하지 않았는데 장착해제합니다??");
    //    Destroy(_equippedItemUI);
    //    _equippedItemUI = null;

    //    UnEquipMesh(storeDesc); //기존 메쉬를 해제한다
    //}






    //public void EquipItem(ItemStoreDesc storedDesc)
    //{
        //if (_equippedItemUI != null) 
        //{
        //    return;
        //    //이미 해당칸에 장착하고 있으면 그냥 종료
        //    /*---------------------------------------------------------------------------
        //    |TODO|  나중에 아이템 스왑기능 구현시 이곳을 수정해야한다.
        //    ---------------------------------------------------------------------------*/
        //}

        //if (storedDesc._info._equipType != _equipType)
        //{
        //    //착용 부위가 다른템이다
        //    return;
        //}

        ////전에 있던 객체에서 아이템 삭제
        //{
        //    storedDesc._owner.DeleteOnMe(storedDesc);
        //}

        //나 자신(Cell)에게 정보 저장
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

        ////메쉬 장착
        //{
        //    EquipItemMesh(storedDesc);
        //}
    //}





}
