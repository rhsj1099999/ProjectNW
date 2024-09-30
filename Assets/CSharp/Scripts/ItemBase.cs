using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;


public class ItemBase : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IBeginDragHandler
{
    public void Initialize(InventoryBoard inventoryBoard, ItemStoreDesc storeDesc)
    {
        if (inventoryBoard == null)
        {
            Debug.Assert(inventoryBoard != null, "인벤토리 보드가 널이다");
        }
        _inventoryBoard = inventoryBoard;
        _itemStoreDesc = storeDesc;
    }

    void Awake()
    {
        _myRectTransform = GetComponent<RectTransform>();
        _myCanvas = GetComponent<Canvas>();
        _myGraphicRaycaster = GetComponent<GraphicRaycaster>();

        Debug.Assert(_myRectTransform != null, "Rect Transform 은 없을 수 없다");
        Debug.Assert(_myCanvas != null, "canvas 없을 수 없다");
        Debug.Assert(_myGraphicRaycaster != null, "graphicRaycaster 은 없을 수 없다");
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) == true && _isDragging == true) 
        {
            RotateInGrab();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
        _myCanvas.overrideSorting = false;
        _myRectTransform.anchoredPosition = _myPosition;

        //다른 인벤토리에 전이가 가능한가?
        {
            List<RaycastResult> uiRayCastResult = new List<RaycastResult>();

            RayCastManager.Instance.RayCastAll(ref uiRayCastResult, false);

            if (uiRayCastResult.Count <= 1) //마우스를 땠을때 아무것도 없다. = 바닥에 뿌리기
            {//1 = 아이템 유아이는 제외함... |TODO| 1이란 숫자를 제외할 수 있는 방법
                _myRectTransform.anchoredPosition = _myPosition;
                _myCanvas.overrideSorting = false;
                return; //일단은 원위치로
            }

            GameObject topObject = GetTopOfRaycastExceptMe(ref uiRayCastResult);

            if (topObject == null)
            {
                return;
            }
            /*---------------------------------------------------------------------------
             |TODO|  자신 위에 포개어 놨다는 인터페이스로 하나로 묶는 구조를 생각해볼것.
            ---------------------------------------------------------------------------*/
            //1. Top Object가 아이템이거나. ItemBase로 확인

            //2. Top Object가 인벤토리이다. cellComponent로 확인

            InventoryCell cellComponent = topObject.GetComponent<InventoryCell>();
            if (cellComponent != null) //최상단이 CellComponent이다
            {
                cellComponent.TryMoveItemDropOnBoard(_itemStoreDesc, this);
                return;
            }

            ItemBase itemBaseComponent = topObject.GetComponent<ItemBase>();
            if (itemBaseComponent != null) //최상단이 ItemBase이다.
            {
                //GameObject cell = _inventoryBoard.getCell(_itemStoreDesc._storedIndex);
                //cellComponent = cell.GetComponent<InventoryCell>();
                //cellComponent.TryMoveItemDropOnItem(_itemStoreDesc, this);
                _myRectTransform.anchoredPosition = _myPosition;
                return;
            }


            return;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        _myRectTransform.anchoredPosition += eventData.delta;
        _mouseCatchingPosition = _myRectTransform.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;

        /*---------------------------------------------------------------------------
         |TODO|  _myCanvas.overrideSorting = true; 호출이후 체계적인 렌더순서 관리 필요
        ---------------------------------------------------------------------------*/
        _myPosition = _myRectTransform.anchoredPosition;
        _myCanvas.overrideSorting = true;

        {
            //인벤토리에 회전된 상태로 저장돼 있었다면?
            _isRotated = _itemStoreDesc._isRotated;
        }

        /*---------------------------------------------------------------------------
         |Noti| 조작상 오해의 소지가 있어서, 그냥 아이템의 특정 포지션을 마우스에 붙이는 방식으로
        //_myRectTransform.anchoredPosition += eventData.delta; 원래는 실시간으로 순수 이동만 더했다
        ---------------------------------------------------------------------------*/

        _myRectTransform.position = Input.mousePosition;

        int sizeX = (_isRotated == false) ? _itemStoreDesc._info._sizeX * 20: _itemStoreDesc._info._sizeY * 20;
        int sizeY = (_isRotated == false) ? _itemStoreDesc._info._sizeY * 20 : _itemStoreDesc._info._sizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //오프셋임


        _mouseCatchingPosition = _myRectTransform.position;
    }

    private GameObject GetTopOfRaycastExceptMe(ref List<RaycastResult> result)
    {
        GameObject target = null;

        foreach (RaycastResult raycastResult in result.AsEnumerable().Reverse())
        {
            if (raycastResult.gameObject != this.gameObject)
            {
                return raycastResult.gameObject;
            }
        }

        return target;
    }

    private void RotateInGrab()
    {
        _isRotated = !_isRotated;


        {
            //mouse catching position 갱신
            _myRectTransform.position = Input.mousePosition;

            int sizeX = (_isRotated == false) ? _itemStoreDesc._info._sizeX * 20 : _itemStoreDesc._info._sizeY * 20;
            int sizeY = (_isRotated == false) ? _itemStoreDesc._info._sizeY * 20 : _itemStoreDesc._info._sizeX * 20;

            _myRectTransform.position = Input.mousePosition;
            _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
            _myRectTransform.position += new Vector3(-10, 10, 0); //오프셋임


            _mouseCatchingPosition = _myRectTransform.position;
        }


        Vector3 axis = new Vector3(0.0f, 0.0f, 1.0f);
        float angle = (_isRotated == true) ? 90.0f : -90.0f;

        _myRectTransform.RotateAround(_mouseCatchingPosition, axis, angle);
    }


    public bool GetRotated()
    {
        return _isRotated;
    }


    public ItemStoreDesc getStoredDesc()
    {
        return _itemStoreDesc;
    }

    private RectTransform _myRectTransform = null;
    private Canvas _myCanvas = null;
    private GraphicRaycaster _myGraphicRaycaster = null;

    /*-----------------
    기능 실행마다 바뀔 변수들
     ----------------*/
    private Vector2 _mouseCatchingPosition = Vector2.zero;
    private Vector2 _myPosition = Vector2.zero;
    private ItemStoreDesc _itemStoreDesc;
    private InventoryBoard _inventoryBoard = null;
    private bool _isDragging = false;
    private bool _isRotated = false;
}
