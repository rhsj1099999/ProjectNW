using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InventoryTransitionDesc
{
    public InventoryBoard _from;
    public int _fromIndex;
    public ItemInfo _itemInfo;
}

public struct InventoryCellDesc
{
    public InventoryBoard _owner;
}

public class InventoryCell : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void Awake()
    {
        //Debug.Assert(_owner != null, "인벤Cell 의 Owner 는 null일 수 없다");
    }

    public void Initialize(ref InventoryCellDesc desc)
    {
        _owner = desc._owner;
    }

    public void TryMoveItemToInventoryBoard(InventoryTransitionDesc transitionDesc)
    {
        Debug.Assert(_owner != null, "Cell의 오너는 널일 수 없다.");

        //드래그 해서 인벤토리 위에 넣으면 이 함수가 호출된다.
        //다음에 대해서 생각해봐야한다.
        //1. 이미 동일 종류 아이템 키 위에 포개놓은 경우
        //1.1 스택가능하다
        //최대스택이 여분이 있다 ->수량을 증가시킨다
        //최대스택이 여분이 없다 ->아무것도 안한다 (혹은 알아서 자리찾기 -> 이건 기획적요소)
        //1.2 스택가능하지 않다
        // ||여유공간 체크 후 넣기||
        //2.


        if (true)
        {
            //체크 -> 된다 2개의 로직을 연이어서 실행
            transitionDesc._from.DeleteItemUseForDragItem(transitionDesc);  //삭제하고
            _owner.AddItemDragDrop(transitionDesc);                            //넣는다
        }


    }

    private InventoryBoard _owner = null;
}
