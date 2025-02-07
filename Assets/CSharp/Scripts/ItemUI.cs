using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using Unity.VisualScripting;

/*-------------------------------------------------------
|NOTI| �ڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡ�
Canvas Scaler �� �۵����Դϴ�.

UI ��ġ ����� 
    AnchoredPosition
    Position

�� ��Ȯ�� ������ �ϼ���!
�ڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡڡ�
-------------------------------------------------------*/



public class ItemUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IBeginDragHandler
{
    private RectTransform _myRectTransform = null;
    private ItemStoreDescBase _itemStoreDesc;
    [SerializeField] private ItemCountUI _itemCountUIObject = null;

    /*-----------------
    ��� ���ึ�� �ٲ� ������
     ----------------*/
    private Vector2 _myOriginalPosition = Vector2.zero;
    private Quaternion _myOriginalRotation = Quaternion.identity;
    private Vector2 _myOriginalSize = Vector2.zero;
    private Transform _beforeDragTransform = null;

    private bool _additionalRotating_Dynamic = false;
    private bool _isDragging = false;
    private bool _isInitted = false;

    public virtual void OverlapItemWork(ItemUI itemUI) {}

    public void Initialize(ItemStoreDescBase storeDesc)
    {
        {
            //�̹��� ������Ʈ ����
            if (storeDesc._itemAsset._ItemImage == null)
            {
                Debug.Assert(false, "ǥ�õ� UI�� �ݵ�� �������ּ���");
                Debug.Break();
            }

            ItemImageScript itemImageScript = GetComponent<ItemImageScript>();
            itemImageScript.Init(storeDesc._itemAsset._ItemImage);
        }

        if (_itemCountUIObject == null)
        {
            Debug.Assert(false, "�ݵ�� CountUI Object�� �����ϼ���");
            Debug.Break();
        }

        storeDesc.AddCountDeleAction(_itemCountUIObject.SetCount);
        _itemCountUIObject.SetCount(storeDesc._Count);

        _myRectTransform = GetComponent<RectTransform>();
        _itemStoreDesc = storeDesc;
        _isInitted = true;
    }

    private void Start()
    {
        if (_isInitted == false)
        {
            Debug.Assert(false, "Init�� ȣ����� �ʾҽ��ϴ�");
            Debug.Break();
        }
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
            Debug.Log("�̷��� �ȵȴ�");
            Debug.Break();
            EventSystem.current.SetSelectedGameObject(null);
        }

        Destroy(gameObject);
    }


    public void OnPointerDown(PointerEventData eventData) {}

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isDragging == false)
        {
            //Ŭ�������� �巡���� ���ߴ�;
            return;
        }

        //�巡�� ����
        {
            _isDragging = false;
            _myRectTransform.rotation = _myOriginalRotation;
            _myRectTransform.position = _myOriginalPosition;
            _myRectTransform.sizeDelta = _myOriginalSize;
            transform.SetParent(_beforeDragTransform);
        }


        List<RaycastResult> uiRayCastResult = new List<RaycastResult>();

        UIManager.Instance.RayCastAll(ref uiRayCastResult);

        if (uiRayCastResult.Count <= 0)
        {
            if (_itemStoreDesc._itemAsset._FieldExistAble == true)
            {
                //������ ����
                UIComponent myUIComponent = _itemStoreDesc._owner.GetComponentInParent<UIComponent>();
                GameObject ownerCharacter = myUIComponent.GetUIControllingComponent().gameObject;
                ItemInfoManager.Instance.DropItemToField(ownerCharacter.transform, _itemStoreDesc);
            }

            _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
            StartCoroutine(DestroyCoroutine());

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
            return;
        }


        //�ֻ���� ItemUI�̴�.
        ItemUI itemBaseComponent = topObject.GetComponent<ItemUI>();
        if (itemBaseComponent != null)
        {
            bool isDelete = false;
            itemBaseComponent._itemStoreDesc.OverlapItem(_itemStoreDesc, ref isDelete);
            if (isDelete == true) 
            {
                _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
                StartCoroutine(DestroyCoroutine());
            }
            return;
        }


        //�ֻ���� BoardUICellBase �̴�
        BoardUICellBase cellComponent = topObject.GetComponent<BoardUICellBase>();
        if (cellComponent != null)
        {
            int startX = -1;
            int startY = -1;

            if (cellComponent.TryMoveItemDropOnCell(_itemStoreDesc, ref startX, ref startY, _additionalRotating_Dynamic) == false)
            {
                return;
            }

            /*--------------------------------------------------------------------------------------------------------
            |NOTI| ���ο� ItemStoreDescBase �ʱ�ȭ ������尡 ������ ������ �����ϰ� �ִ� ������ ���մϴ�.
            ���� owner �� Equip�ϼ����ְ�, Inventory �ϼ��� �־� ���ϼ��� ���ؼ��Դϴ�.
            --------------------------------------------------------------------------------------------------------*/


            //�����Ҷ� _owner = ������ �־��� ���� �����̳�
            _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
            _itemStoreDesc._isRotated = _additionalRotating_Dynamic;
            cellComponent.GetOwner().AddItemUsingForcedIndex(_itemStoreDesc, startX, startY, cellComponent);
            StartCoroutine(DestroyCoroutine());

            return;
        }

        return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 delta = new Vector3(eventData.delta.x, eventData.delta.y, 0.0f);
        //_myRectTransform.anchoredPosition += eventData.delta;
        _myRectTransform.position += delta;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //�巡�� �غ�
        {
            //�巡���� Ȱ��ȭ
            _isDragging = true;

            //�׷� �η��̼� �ʱ�ȭ
            _additionalRotating_Dynamic = _itemStoreDesc._isRotated;

            //���콺�� ������ ���� ���ư��� ����
            _myOriginalPosition = _myRectTransform.position;

            _myOriginalRotation = _myRectTransform.rotation;

            //���� ���콺�� ���δ�
            _myRectTransform.position = Input.mousePosition;

            //���콺�� ������ ���ư� ũ�� ����
            _myOriginalSize = _myRectTransform.sizeDelta;

            //���� ũ��� �������´�
            _myRectTransform.sizeDelta = new Vector2(_itemStoreDesc._itemAsset._SizeX * 20, _itemStoreDesc._itemAsset._SizeY * 20);

            _beforeDragTransform = transform.parent;

            //���� ���� �������� �׷���
            UIManager.Instance.SetMeFinalZOrder(gameObject);
        }

        int sizeX = (_itemStoreDesc._isRotated == false) ? _itemStoreDesc._itemAsset._SizeX * 20: _itemStoreDesc._itemAsset._SizeY * 20;
        int sizeY = (_itemStoreDesc._isRotated == false) ? _itemStoreDesc._itemAsset._SizeY * 20 : _itemStoreDesc._itemAsset._SizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //��������
    }

    private void RotateInGrab()
    {
        _additionalRotating_Dynamic = !_additionalRotating_Dynamic;

        _myRectTransform.position = Input.mousePosition;

        int sizeX = (_additionalRotating_Dynamic == false) ? _itemStoreDesc._itemAsset._SizeX * 20 : _itemStoreDesc._itemAsset._SizeY * 20;
        int sizeY = (_additionalRotating_Dynamic == false) ? _itemStoreDesc._itemAsset._SizeY * 20 : _itemStoreDesc._itemAsset._SizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //��������


        Vector3 axis = new Vector3(0.0f, 0.0f, 1.0f);
        float angle = (_additionalRotating_Dynamic == true) ? 90.0f : -90.0f;

        _myRectTransform.RotateAround(_myRectTransform.position, axis, angle);
    }
}
