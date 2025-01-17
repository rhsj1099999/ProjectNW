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
    아이템을 인벤토리, 장비창 등 생성될때 이것을 사용해 생성한다.
    즉 UI 객체로 동작한다.
    맵에 뿌려진 Item은 이것은 아닌데 이거랑 연관이 깊을거다
    -------------------------------------------------------*/
    private RectTransform _myRectTransform = null;
    private ItemStoreDesc _itemStoreDesc;

    /*-----------------
    기능 실행마다 바뀔 변수들
     ----------------*/
    private Vector2 _myOriginalPosition = Vector2.zero;
    private Vector2 _myOriginalSize = Vector2.zero;

    private bool _additionalRotating_Dynamic = false;
    private bool _isDragging = false;


    public void Initialize(ItemStoreDesc storeDesc)
    {
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


    public void OnPointerDown(PointerEventData eventData) {}

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isDragging == false)
        {
            //클릭만했지 드래깅을 안했다;
            return;
        }

        //드래깅 종료
        {
            _isDragging = false;
            _myRectTransform.position = _myOriginalPosition;
            _myRectTransform.sizeDelta = _myOriginalSize;
        }


        List<RaycastResult> uiRayCastResult = new List<RaycastResult>();

        UIManager.Instance.RayCastAll(ref uiRayCastResult);

        if (uiRayCastResult.Count <= 0)
        {

            //아무것도 없는곳이라면 바닥에 버리는걸로 판정
            _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
            StartCoroutine(DestroyCoroutine());

            {
                //아이템 생성

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


        //최상단이 ItemBase이다.
        ItemBase itemBaseComponent = topObject.GetComponent<ItemBase>();
        if (itemBaseComponent != null)
        {
            /*------------------------------------------------
            |NOTI| 만약 무기에 보석박기, 삽탄 컨텐츠를 구현하게 된다면
            아이템을 아이템에 포개서 시작됨으로 여기에 작업하세요
            ------------------------------------------------*/
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
            |NOTI| 새로운 ItemStoreDesc 초기화 오버헤드가 있지만 무조건 삭제하고 넣는 구조를 택합니다.
            이전 owner 가 Equip일수도있고, Inventory 일수도 있어 통일성을 위해서입니다.
            --------------------------------------------------------------------------------------------------------*/


            //삭제할때 _owner = 이전에 있었던 저장 컨테이너
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
        //드래깅 준비
        {
            //드래깅모드 활성화
            _isDragging = true;

            //그랩 로레이션 초기화
            _additionalRotating_Dynamic = _itemStoreDesc._isRotated;

            //마우스를 놨을때 어디로 돌아갈지 세팅
            _myOriginalPosition = _myRectTransform.position;

            //나를 마우스에 붙인다
            _myRectTransform.position = Input.mousePosition;

            //마우스를 놨을때 돌아갈 크기 세팅
            _myOriginalSize = _myRectTransform.sizeDelta;

            //원래 크기로 돌려놓는다
            _myRectTransform.sizeDelta = new Vector2(_itemStoreDesc._itemAsset._SizeX * 20, _itemStoreDesc._itemAsset._SizeY * 20);

            //나를 제일 마지막에 그려라
            UIManager.Instance.SetMeFinalZOrder(gameObject);
        }

        int sizeX = (_itemStoreDesc._isRotated == false) ? _itemStoreDesc._itemAsset._SizeX * 20: _itemStoreDesc._itemAsset._SizeY * 20;
        int sizeY = (_itemStoreDesc._isRotated == false) ? _itemStoreDesc._itemAsset._SizeY * 20 : _itemStoreDesc._itemAsset._SizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //오프셋임
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


        Vector3 axis = new Vector3(0.0f, 0.0f, 1.0f);
        float angle = (_itemStoreDesc._isRotated == true) ? 90.0f : -90.0f;

        _myRectTransform.RotateAround(_myRectTransform.position, axis, angle);
    }
}
