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
            Debug.Assert(inventoryBoard != null, "�κ��丮 ���尡 ���̴�");
        }
        _inventoryBoard = inventoryBoard;
        _itemStoreDesc = storeDesc;
    }

    void Awake()
    {
        _myRectTransform = GetComponent<RectTransform>();
        _myCanvas = GetComponent<Canvas>();
        _myGraphicRaycaster = GetComponent<GraphicRaycaster>();

        Debug.Assert(_myRectTransform != null, "Rect Transform �� ���� �� ����");
        Debug.Assert(_myCanvas != null, "canvas ���� �� ����");
        Debug.Assert(_myGraphicRaycaster != null, "graphicRaycaster �� ���� �� ����");
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
        //�ٸ� �κ��丮�� ���̰� �����Ѱ�?
        {
            List<RaycastResult> uiRayCastResult = new List<RaycastResult>();

            RayCastManager.Instance.RayCastAll(ref uiRayCastResult, false);

            if (uiRayCastResult.Count <= 1) //���콺�� ������ �ƹ��͵� ����. = �ٴڿ� �Ѹ���
            {//1 = ������ �����̴� ������... |TODO| 1�̶� ���ڸ� ������ �� �ִ� ���
                _myRectTransform.anchoredPosition = _myPosition;
                _myCanvas.overrideSorting = false;
                return; //�ϴ��� ����ġ��
            }

            GameObject topObject = GetTopOfRaycastExceptMe(ref uiRayCastResult);

            if (topObject == null)
            {
                return;
            }

            InventoryCell cellComponent = topObject.GetComponent<InventoryCell>();

            if (cellComponent == null) //���콺�� ���� �ߴµ� '�ֻ�� UI'�κ��丮�� �ƴϴ� = ����ġ
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
         |TODO|  _myCanvas.overrideSorting = true; ���� ü������ �������� ���� �ʿ�
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
    ��� ���ึ�� �ٲ� ������
     ----------------*/
    private Vector2 _myPosition = Vector2.zero;
    private ItemStoreDesc _itemStoreDesc;
    private InventoryBoard _inventoryBoard = null;


}
