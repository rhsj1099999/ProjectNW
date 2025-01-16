using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*-------------------------------------------
 * |Obsoleted|-------------------------------
-------------------------------------------*/

public class ItemCatcher : MonoBehaviour
{
}

//public struct ItemCatchingDesc
//{
//    public ItemAsset _info;
//    public InventoryBoard _from;
//    public Vector2 _position;
//}

//public class ItemCatcher : MonoBehaviour, IPointerUpHandler
//{
//    void Awake()
//    {
//        if (_instance != null && _instance != this)
//        {
//            Destroy(_instance.gameObject);
//            return;
//        }
//        else
//        {
//            _instance = this;
//            DontDestroyOnLoad(_instance.gameObject);
//        }

//        //_graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
//        //_eventSystem = GetComponentInParent<EventSystem>();
//        //_myRectTransform = GetComponentInParent<RectTransform>();

//        //Debug.Assert((_graphicRaycaster != null && _eventSystem != null && _myRectTransform != null), "그래픽레이캐스터, 이벤트시스템, rectTransform은 null이여선 안된다");
//    }


//    public void CatchItemStart(ItemCatchingDesc desc)
//    {
//        if (_isCatching == true) //이미 잡고있었다.
//        {
//            Debug.Assert(_isCatching == false, "잡고있었는데 잡으라는 호출이 들어왔다.");
//        }

//        _catchingDesc = desc;
//        //_isCatching = true;
//    }

//    public static ItemCatcher Instance
//    {
//        get
//        {
//            if (_instance == null)
//            {
//                _instance = new ItemCatcher();
//                DontDestroyOnLoad(_instance.gameObject);
//            }
//            return _instance;
//        }
//    }

//    public void OnPointerUp(PointerEventData eventData)
//    {
//        //레이캐스팅 후 인벤토리 넣기 시도
//        _isCatching = false;
//    }




//    private bool _isCatching = false;
//    private ItemCatchingDesc _catchingDesc;

//    //private EventSystem _eventSystem = null;
//    //private GraphicRaycaster _graphicRaycaster = null;
//    //private RectTransform _myRectTransform = null;

//    private static ItemCatcher _instance;


//}
