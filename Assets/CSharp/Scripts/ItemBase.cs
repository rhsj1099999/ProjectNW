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
    기능 실행마다 바뀔 변수들
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
            Debug.Assert(inventoryBoard != null, "인벤토리 보드가 널이다");
        }
        _itemStoreAbleInstance = inventoryBoard;
        _itemStoreDesc = storeDesc;
    }


    public GameObject GetReturnParent()
    {
        return _returnParent;
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

        if (_returnParent != null)
        {
            transform.SetParent(_returnParent.transform);
        }

        _returnParent = null;

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

        //최상단이 CellComponent이다
        InventoryCell cellComponent = topObject.GetComponent<InventoryCell>();
        if (cellComponent != null) 
        {
            if (cellComponent.TryMoveItemDropOnBoard(_itemStoreDesc, this) == true)
            {
                EquipmentBoard equipmentBoard = _itemStoreDesc._owner as EquipmentBoard;
                if (equipmentBoard != null &&
                    _itemStoreDesc._info._equipType == ItemInfo.EquipType.All)
                {
                    List<GameObject> restItemUIs = equipmentBoard.GetRestEquipmetItems(this.gameObject);

                    foreach (var item in restItemUIs)
                    {
                        Destroy(item.gameObject);
                    }
                }

                StartCoroutine(DestroyCoroutine());
            }
            return;
        }

        //최상단이 EquipmentCell이다.
        EquipmentCell equipmentCellComponent = topObject.GetComponent<EquipmentCell>();
        if (equipmentCellComponent != null) 
        {
            if (equipmentCellComponent.TryEquipItem(_itemStoreDesc) == true)
            {
                StartCoroutine(DestroyCoroutine());
            }
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

        _myPosition = _myRectTransform.anchoredPosition;

        _returnParent = transform.parent.gameObject;

        UIManager.Instance.SetMeFinalZOrder(gameObject);

        _isRotated = _itemStoreDesc._isRotated;

        _myRectTransform.position = Input.mousePosition;

        int sizeX = (_isRotated == false) ? _itemStoreDesc._info._sizeX * 20: _itemStoreDesc._info._sizeY * 20;
        int sizeY = (_isRotated == false) ? _itemStoreDesc._info._sizeY * 20 : _itemStoreDesc._info._sizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //오프셋임

        _mouseCatchingPosition = _myRectTransform.position;
    }

    private void RotateInGrab()
    {
        _isRotated = !_isRotated;

        //mouse catching position 갱신
        _myRectTransform.position = Input.mousePosition;

        int sizeX = (_isRotated == false) ? _itemStoreDesc._info._sizeX * 20 : _itemStoreDesc._info._sizeY * 20;
        int sizeY = (_isRotated == false) ? _itemStoreDesc._info._sizeY * 20 : _itemStoreDesc._info._sizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //오프셋임


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
