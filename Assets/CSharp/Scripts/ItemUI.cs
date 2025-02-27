using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using Unity.VisualScripting;

/*-------------------------------------------------------
|NOTI| ★★★★★★★★★★★★★★★★★★★★★★★★★★★
Canvas Scaler 가 작동중입니다.

UI 위치 변경시 
    AnchoredPosition
    Position

의 정확한 구분을 하세여!
★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
-------------------------------------------------------*/



public class ItemUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IBeginDragHandler
{
    private RectTransform _myRectTransform = null;
    private ItemStoreDescBase _itemStoreDesc;
    [SerializeField] private ItemCountUI _itemCountUIObject = null;

    /*-----------------
    기능 실행마다 바뀔 변수들
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
            //이미지 컴포넌트 세팅
            if (storeDesc._itemAsset._ItemImage == null)
            {
                Debug.Assert(false, "표시될 UI를 반드시 세팅해주세요");
                Debug.Break();
            }

            ItemImageScript itemImageScript = GetComponent<ItemImageScript>();
            itemImageScript.Init(storeDesc._itemAsset._ItemImage);
        }

        if (_itemCountUIObject == null)
        {
            Debug.Assert(false, "반드시 CountUI Object를 선택하세여");
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
            Debug.Assert(false, "Init이 호출되지 않았습니다");
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
            Debug.Log("이러면 안된다");
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
            //클릭만했지 드래깅을 안했다
            return;
        } //중략...드래그한적이 없을때 단순 Return

        {
            //드래깅 종료
            _isDragging = false;
            _myRectTransform.rotation = _myOriginalRotation;
            _myRectTransform.position = _myOriginalPosition;
            transform.SetParent(_beforeDragTransform);
        } //중략...마우스에서 손을 뗐을때 변수 갱신

        List<RaycastResult> uiRayCastResult = new List<RaycastResult>();

        UIManager.Instance.RayCastAll(ref uiRayCastResult);

        if (uiRayCastResult.Count <= 0)
        {
            if (_itemStoreDesc._itemAsset._FieldExistAble == true)
            {
                //아이템 생성
                UIComponent myUIComponent = _itemStoreDesc._owner.GetComponentInParent<UIComponent>();
                GameObject ownerCharacter = myUIComponent.GetUIControllingComponent().gameObject;
                ItemInfoManager.Instance.DropItemToField(ownerCharacter.transform, _itemStoreDesc);
            }

            //아이템 삭제요청
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


        //최상단이 ItemUI이다.
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


        //최상단이 BoardUICellBase 이다
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
            |NOTI| 새로운 ItemStoreDescBase 초기화 오버헤드가 있지만 무조건 삭제하고 넣는 구조를 택합니다.
            이전 owner 가 Equip일수도있고, Inventory 일수도 있어 통일성을 위해서입니다.
            --------------------------------------------------------------------------------------------------------*/

            //삭제할때 _owner = 이전에 있었던 저장 컨테이너
            _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
            _itemStoreDesc._isRotated = _additionalRotating_Dynamic;
            cellComponent.GetOwner().AddItemUsingForcedIndex(_itemStoreDesc, startX, startY, cellComponent);
            StartCoroutine(DestroyCoroutine());
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 delta = new Vector3(eventData.delta.x, eventData.delta.y, 0.0f);
        //_myRectTransform.anchoredPosition += eventData.delta;
        _myRectTransform.position += delta;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //드래깅 준비
        {
            //드래깅모드 활성화
            _isDragging = true;

            //그랩 로레이션 초기화
            _additionalRotating_Dynamic = _itemStoreDesc._isRotated;

            //마우스를 놨을때 어디로 돌아갈지 세팅
            _myOriginalPosition = _myRectTransform.position;

            _myOriginalRotation = _myRectTransform.rotation;

            //나를 마우스에 붙인다
            _myRectTransform.position = Input.mousePosition;

            //마우스를 놨을때 돌아갈 크기 세팅
            //_myOriginalSize = _myRectTransform.sizeDelta;

            //원래 크기로 돌려놓는다
            //_myRectTransform.sizeDelta = new Vector2(_itemStoreDesc._itemAsset._SizeX * 20, _itemStoreDesc._itemAsset._SizeY * 20);

            _beforeDragTransform = transform.parent;

            //나를 제일 마지막에 그려라
            UIManager.Instance.SetMeFinalZOrder(gameObject, UIManager.LayerOrder.InventorySomethingElse);
        }

        int sizeX = (_itemStoreDesc._isRotated == false) ? _itemStoreDesc._itemAsset._SizeX * 20: _itemStoreDesc._itemAsset._SizeY * 20;
        int sizeY = (_itemStoreDesc._isRotated == false) ? _itemStoreDesc._itemAsset._SizeY * 20 : _itemStoreDesc._itemAsset._SizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.anchoredPosition += new Vector2(sizeX, -sizeY);
    }

    private void RotateInGrab()
    {
        _additionalRotating_Dynamic = !_additionalRotating_Dynamic;

        _myRectTransform.position = Input.mousePosition;

        int sizeX = (_additionalRotating_Dynamic == false) ? _itemStoreDesc._itemAsset._SizeX * 20 : _itemStoreDesc._itemAsset._SizeY * 20;
        int sizeY = (_additionalRotating_Dynamic == false) ? _itemStoreDesc._itemAsset._SizeY * 20 : _itemStoreDesc._itemAsset._SizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.anchoredPosition += new Vector2(sizeX, -sizeY);


        Vector3 axis = new Vector3(0.0f, 0.0f, 1.0f);
        float angle = (_additionalRotating_Dynamic == true) ? 90.0f : -90.0f;

        _myRectTransform.RotateAround(_myRectTransform.position, axis, angle);
    }
}
