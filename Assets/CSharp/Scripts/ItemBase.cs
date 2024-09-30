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
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }


    public void OnPointerUp(PointerEventData eventData)
    {
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

            InventoryCell cellComponent = topObject.GetComponent<InventoryCell>();

            if (cellComponent == null) //마우스를 떼긴 했는데 '최상단 UI'인벤토리가 아니다 = 원위치
            {
                _myRectTransform.anchoredPosition = _myPosition;
                _myCanvas.overrideSorting = false;
                return;
            }

            InventoryTransitionDesc transitionDesc = new InventoryTransitionDesc();
            transitionDesc._from = _itemStoreDesc._owner;
            transitionDesc._itemInfo = _itemStoreDesc._info;
            transitionDesc._fromIndex = _itemStoreDesc._storedIndex;
            cellComponent.TryMoveItemToInventoryBoard(transitionDesc);

            _myRectTransform.anchoredPosition = _myPosition;
            _myCanvas.overrideSorting = false;
            return;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        _myRectTransform.anchoredPosition += eventData.delta;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _myPosition = _myRectTransform.anchoredPosition;
        _myCanvas.overrideSorting = true;

        /*---------------------------------------------------------------------------
         |TODO|  _myCanvas.overrideSorting = true; 이후 체계적인 렌더순서 관리 필요
        ---------------------------------------------------------------------------*/
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



    private RectTransform _myRectTransform = null;
    private Canvas _myCanvas = null;
    private GraphicRaycaster _myGraphicRaycaster = null;

    /*-----------------
    기능 실행마다 바뀔 변수들
     ----------------*/
    private Vector2 _myPosition = Vector2.zero;
    private ItemStoreDesc _itemStoreDesc;
    private InventoryBoard _inventoryBoard = null;


}
