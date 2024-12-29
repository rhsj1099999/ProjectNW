using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using Unity.VisualScripting;

public class ItemBase : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IBeginDragHandler
{
    private RectTransform _myRectTransform = null;

    /*-----------------
    ��� ���ึ�� �ٲ� ������
     ----------------*/
    private Vector2 _mouseCatchingPosition = Vector2.zero;
    private Vector2 _myPosition = Vector2.zero;
    private ItemStoreDesc _itemStoreDesc;
    private IMoveItemStore _itemStoreAbleInstance = null;
    private bool _isDragging = false;
    private bool _isRotated = false;
    private GameObject _returnParent = null;

    public void Initialize(IMoveItemStore inventoryBoard, ItemStoreDesc storeDesc)
    {
        if (inventoryBoard == null)
        {
            Debug.Assert(inventoryBoard != null, "�κ��丮 ���尡 ���̴�");
        }
        _itemStoreAbleInstance = inventoryBoard;
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
        _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
        gameObject.SetActive(false);

        yield return new WaitForNextFrameUnit();
        yield return new WaitForNextFrameUnit();

        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            Debug.Log("�̷��� �ȵȴ�");
            Debug.Break();
            EventSystem.current.SetSelectedGameObject(null);
        }
        gameObject.SetActive(true);
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

        transform.SetParent(_returnParent.transform);

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

        //�ֻ���� CellComponent�̴�
        InventoryCell cellComponent = topObject.GetComponent<InventoryCell>();
        if (cellComponent != null) 
        {
            if (cellComponent.TryMoveItemDropOnBoard(_itemStoreDesc, this) == true)
            {
                StartCoroutine(DestroyCoroutine());
            }
            return;
        }

        //�ֻ���� EquipmentCell�̴�.
        EquipmentCell equipmentCellComponent = topObject.GetComponent<EquipmentCell>();
        if (equipmentCellComponent != null) 
        {
            if (equipmentCellComponent.TryEquipItem(_itemStoreDesc) == true)
            {
                StartCoroutine(DestroyCoroutine());
            }
            return;
        }

        //�ֻ���� ItemBase�̴�.
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

        _myPosition = _myRectTransform.anchoredPosition;

        _returnParent = transform.parent.gameObject;

        UIManager.Instance.SetMeFinalZOrder(gameObject);

        _isRotated = _itemStoreDesc._isRotated;

        _myRectTransform.position = Input.mousePosition;

        int sizeX = (_isRotated == false) ? _itemStoreDesc._info._sizeX * 20: _itemStoreDesc._info._sizeY * 20;
        int sizeY = (_isRotated == false) ? _itemStoreDesc._info._sizeY * 20 : _itemStoreDesc._info._sizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //��������

        _mouseCatchingPosition = _myRectTransform.position;
    }

    private void RotateInGrab()
    {
        _isRotated = !_isRotated;

        //mouse catching position ����
        _myRectTransform.position = Input.mousePosition;

        int sizeX = (_isRotated == false) ? _itemStoreDesc._info._sizeX * 20 : _itemStoreDesc._info._sizeY * 20;
        int sizeY = (_isRotated == false) ? _itemStoreDesc._info._sizeY * 20 : _itemStoreDesc._info._sizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //��������


        _mouseCatchingPosition = _myRectTransform.position;

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
}
