using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public struct ItemCatchingDesc
{
    public ItemInfo _info;
    public InventoryBoard _from;
    public Vector2 _position;
}

public class ItemCatcher : MonoBehaviour, IPointerUpHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_isCatching == true)
        {

        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(_instance.gameObject);
            return;
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(_instance.gameObject);
        }

        //_graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
        //_eventSystem = GetComponentInParent<EventSystem>();
        //_myRectTransform = GetComponentInParent<RectTransform>();

        //Debug.Assert((_graphicRaycaster != null && _eventSystem != null && _myRectTransform != null), "�׷��ȷ���ĳ����, �̺�Ʈ�ý���, rectTransform�� null�̿��� �ȵȴ�");
    }


    public void CatchItemStart(ItemCatchingDesc desc)
    {
        if (_isCatching == true) //�̹� ����־���.
        {
            Debug.Assert(_isCatching == false, "����־��µ� ������� ȣ���� ���Դ�.");
        }

        _catchingDesc = desc;
        //_isCatching = true;
    }

    public static ItemCatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ItemCatcher();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //����ĳ���� �� �κ��丮 �ֱ� �õ�
        _isCatching = false;
    }




    private bool _isCatching = false;
    private ItemCatchingDesc _catchingDesc;

    //private EventSystem _eventSystem = null;
    //private GraphicRaycaster _graphicRaycaster = null;
    //private RectTransform _myRectTransform = null;

    private static ItemCatcher _instance;


}
