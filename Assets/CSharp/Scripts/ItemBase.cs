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
            /*---------------------------------------------------------------------------
             |TODO|  �ڽ� ���� ������ ���ٴ� �������̽��� �ϳ��� ���� ������ �����غ���.
            ---------------------------------------------------------------------------*/
            //1. Top Object�� �������̰ų�. ItemBase�� Ȯ��

            //2. Top Object�� �κ��丮�̴�. cellComponent�� Ȯ��

            InventoryCell cellComponent = topObject.GetComponent<InventoryCell>();
            if (cellComponent != null) //�ֻ���� CellComponent�̴�
            {
                cellComponent.TryMoveItemDropOnBoard(_itemStoreDesc, this);
                return;
            }

            ItemBase itemBaseComponent = topObject.GetComponent<ItemBase>();
            if (itemBaseComponent != null) //�ֻ���� ItemBase�̴�.
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
         |TODO|  _myCanvas.overrideSorting = true; ȣ������ ü������ �������� ���� �ʿ�
        ---------------------------------------------------------------------------*/
        _myPosition = _myRectTransform.anchoredPosition;
        _myCanvas.overrideSorting = true;

        {
            //�κ��丮�� ȸ���� ���·� ����� �־��ٸ�?
            _isRotated = _itemStoreDesc._isRotated;
        }

        /*---------------------------------------------------------------------------
         |Noti| ���ۻ� ������ ������ �־, �׳� �������� Ư�� �������� ���콺�� ���̴� �������
        //_myRectTransform.anchoredPosition += eventData.delta; ������ �ǽð����� ���� �̵��� ���ߴ�
        ---------------------------------------------------------------------------*/

        _myRectTransform.position = Input.mousePosition;

        int sizeX = (_isRotated == false) ? _itemStoreDesc._info._sizeX * 20: _itemStoreDesc._info._sizeY * 20;
        int sizeY = (_isRotated == false) ? _itemStoreDesc._info._sizeY * 20 : _itemStoreDesc._info._sizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //��������


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
            //mouse catching position ����
            _myRectTransform.position = Input.mousePosition;

            int sizeX = (_isRotated == false) ? _itemStoreDesc._info._sizeX * 20 : _itemStoreDesc._info._sizeY * 20;
            int sizeY = (_isRotated == false) ? _itemStoreDesc._info._sizeY * 20 : _itemStoreDesc._info._sizeX * 20;

            _myRectTransform.position = Input.mousePosition;
            _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
            _myRectTransform.position += new Vector3(-10, 10, 0); //��������


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
    ��� ���ึ�� �ٲ� ������
     ----------------*/
    private Vector2 _mouseCatchingPosition = Vector2.zero;
    private Vector2 _myPosition = Vector2.zero;
    private ItemStoreDesc _itemStoreDesc;
    private InventoryBoard _inventoryBoard = null;
    private bool _isDragging = false;
    private bool _isRotated = false;
}
