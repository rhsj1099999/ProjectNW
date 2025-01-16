using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using Unity.VisualScripting;
using static ItemAsset;

public class ItemBase : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IBeginDragHandler
{
    private RectTransform _myRectTransform = null;

    /*-------------------------------------------------------
    아이템을 인벤토리, 장비창 등 생성될때 이것을 사용해 생성한다.
    -------------------------------------------------------*/
    private ItemStoreDesc _itemStoreDesc;

    /*-----------------
    기능 실행마다 바뀔 변수들
     ----------------*/
    private Vector2 _mouseCatchingPosition = Vector2.zero;
    private Vector2 _myPosition = Vector2.zero;
    private bool _isDragging = false;

    private bool _additionalRotating_Dynamic = false;

    public void Initialize(IMoveItemStore fromOwner, ItemStoreDesc storeDesc)
    {
        if (fromOwner == null)
        {
            Debug.Assert(fromOwner != null, "인벤토리 보드가 널이다");
        }

        {
            //이미지 컴포넌트 세팅

            //Debug.Assert(false, "이곳은 수정해야합니다");
            //Debug.Break();
            //if (info._sprite != null)
            //{
            //    itemUI.GetComponent<Image>().sprite = info._sprite;
            //}
        }

        _itemStoreDesc = storeDesc;
    }


    void Awake()
    {
        _myRectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) == true && _isDragging == true) 
        {
            RotateInGrab();
        }
    }

    public IEnumerator DestroyCoroutine()
    {
        transform.localScale = Vector3.zero;

        yield return new WaitForNextFrameUnit();
        yield return new WaitForNextFrameUnit();

        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            Debug.Log("이러면 안된다");
            Debug.Break();
            EventSystem.current.SetSelectedGameObject(null);
        }

        Destroy(gameObject);
    }


    public void OnPointerDown(PointerEventData eventData) 
    {
        UIManager.Instance.IncreaseConsumeInput();
    }



    public void OnPointerUp(PointerEventData eventData)
    {
        UIManager.Instance.DecreaseConsumeInput();

        _isDragging = false;

        _myRectTransform.anchoredPosition = _myPosition;

        List<RaycastResult> uiRayCastResult = new List<RaycastResult>();

        UIManager.Instance.RayCastAll(ref uiRayCastResult);

        if (uiRayCastResult.Count <= 0)
        {
            return;
        }

        GameObject topObject = uiRayCastResult.First().gameObject;

        if (topObject == gameObject) 
        {
            if (uiRayCastResult.Count <= 1)
            {
                return;
            }

            topObject = uiRayCastResult[1].gameObject;
        }

        if (topObject == null)
        {
            _myRectTransform.anchoredPosition = _myPosition;
            return;
        }

        //최상단이 InventoryCell 이다
        InventoryCell cellComponent = topObject.GetComponent<InventoryCell>();
        if (cellComponent != null) 
        {
            int startX = -1;
            int startY = -1;
            if (cellComponent.TryMoveItemDropOnCell(_itemStoreDesc, ref startX, ref startY, _additionalRotating_Dynamic) == false)
            {
                return;
            }

            /*--------------------------------------------------------------------------------------------------------
            |NOTI| 새로운 ItemStoreDesc 초기화 오버헤드가 있지만 무조건 삭제하고 넣는 구조를 택합니다.
            이전 owner 가 Equip일수도있고, Inventory 일수도 있습니다. 통일성을 위해서입니다.
            --------------------------------------------------------------------------------------------------------*/
            _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
            _itemStoreDesc._isRotated = _additionalRotating_Dynamic;
            cellComponent.GetOwner().AddItemUsingForcedIndex(_itemStoreDesc, startX, startY);
            StartCoroutine(DestroyCoroutine());

            return;
        }

        //최상단이 EquipmentCell이다.
        EquipmentCell equipmentCellComponent = topObject.GetComponent<EquipmentCell>();
        if (equipmentCellComponent != null) 
        {
            int startX = -1;
            int startY = -1;
            if (cellComponent.TryMoveItemDropOnCell(_itemStoreDesc, ref startX, ref startY, _additionalRotating_Dynamic) == false)
            {
                return;
            }

            _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
            _itemStoreDesc._isRotated = _additionalRotating_Dynamic;
            cellComponent.GetOwner().AddItemUsingForcedIndex(_itemStoreDesc, startX, startY); //장착코드가 될것
            StartCoroutine(DestroyCoroutine());
            return;
        }

        //최상단이 ItemBase이다.
        ItemBase itemBaseComponent = topObject.GetComponent<ItemBase>();
        if (itemBaseComponent != null) 
        {
            return;
        }

        return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _myRectTransform.anchoredPosition += eventData.delta;
        _mouseCatchingPosition = _myRectTransform.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _additionalRotating_Dynamic = _itemStoreDesc._isRotated;


        _myRectTransform.sizeDelta = new Vector2(_itemStoreDesc._itemAsset._SizeX * 20, _itemStoreDesc._itemAsset._SizeY * 20);

        _myPosition = _myRectTransform.anchoredPosition;

        UIManager.Instance.SetMeFinalZOrder(gameObject);

        _myRectTransform.position = Input.mousePosition;

        int sizeX = (_itemStoreDesc._isRotated == false) ? _itemStoreDesc._itemAsset._SizeX * 20: _itemStoreDesc._itemAsset._SizeY * 20;
        int sizeY = (_itemStoreDesc._isRotated == false) ? _itemStoreDesc._itemAsset._SizeY * 20 : _itemStoreDesc._itemAsset._SizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //오프셋임

        _mouseCatchingPosition = _myRectTransform.position;
    }

    private void RotateInGrab()
    {
        _additionalRotating_Dynamic = !_additionalRotating_Dynamic;

        _myRectTransform.position = Input.mousePosition;

        int sizeX = (_additionalRotating_Dynamic == false) ? _itemStoreDesc._itemAsset._SizeX * 20 : _itemStoreDesc._itemAsset._SizeY * 20;
        int sizeY = (_additionalRotating_Dynamic == false) ? _itemStoreDesc._itemAsset._SizeY * 20 : _itemStoreDesc._itemAsset._SizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //오프셋임


        _mouseCatchingPosition = _myRectTransform.position;

        Vector3 axis = new Vector3(0.0f, 0.0f, 1.0f);
        float angle = (_itemStoreDesc._isRotated == true) ? 90.0f : -90.0f;

        _myRectTransform.RotateAround(_mouseCatchingPosition, axis, angle);
    }


    //public bool GetRotated()
    //{
    //    return _itemStoreDesc._isRotated;
    //}


    //public ItemStoreDesc getStoredDesc()
    //{
    //    return _itemStoreDesc;
    //}
}
