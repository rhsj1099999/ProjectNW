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
    /*-------------------------------------------------------
    �������� �κ��丮, ���â �� �����ɶ� �̰��� ����� �����Ѵ�.
    �� UI ��ü�� �����Ѵ�.
    �ʿ� �ѷ��� Item�� �̰��� �ƴѵ� �̰Ŷ� ������ �����Ŵ�
    -------------------------------------------------------*/
    private RectTransform _myRectTransform = null;
    private ItemStoreDesc _itemStoreDesc;

    /*-----------------
    ��� ���ึ�� �ٲ� ������
     ----------------*/
    private Vector2 _myOriginalPosition = Vector2.zero;
    private Vector2 _myOriginalSize = Vector2.zero;

    private bool _additionalRotating_Dynamic = false;
    private bool _isDragging = false;


    public void Initialize(ItemStoreDesc storeDesc)
    {
        {
            //�̹��� ������Ʈ ����

            //Debug.Assert(false, "�̰��� �����ؾ��մϴ�");
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
            _myRectTransform.position = _myOriginalPosition;
            _myRectTransform.sizeDelta = _myOriginalSize;
        }


        List<RaycastResult> uiRayCastResult = new List<RaycastResult>();

        UIManager.Instance.RayCastAll(ref uiRayCastResult);

        if (uiRayCastResult.Count <= 0)
        {

            //�ƹ��͵� ���°��̶�� �ٴڿ� �����°ɷ� ����
            _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
            StartCoroutine(DestroyCoroutine());

            {
                //������ ����

            }

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


        //�ֻ���� ItemBase�̴�.
        ItemBase itemBaseComponent = topObject.GetComponent<ItemBase>();
        if (itemBaseComponent != null)
        {
            /*------------------------------------------------
            |NOTI| ���� ���⿡ �����ڱ�, ��ź �������� �����ϰ� �ȴٸ�
            �������� �����ۿ� ������ ���۵����� ���⿡ �۾��ϼ���
            ------------------------------------------------*/
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
            |NOTI| ���ο� ItemStoreDesc �ʱ�ȭ ������尡 ������ ������ �����ϰ� �ִ� ������ ���մϴ�.
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
        _myRectTransform.anchoredPosition += eventData.delta;
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

            //���� ���콺�� ���δ�
            _myRectTransform.position = Input.mousePosition;

            //���콺�� ������ ���ư� ũ�� ����
            _myOriginalSize = _myRectTransform.sizeDelta;

            //���� ũ��� �������´�
            _myRectTransform.sizeDelta = new Vector2(_itemStoreDesc._itemAsset._SizeX * 20, _itemStoreDesc._itemAsset._SizeY * 20);

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
        float angle = (_itemStoreDesc._isRotated == true) ? 90.0f : -90.0f;

        _myRectTransform.RotateAround(_myRectTransform.position, axis, angle);
    }
}
