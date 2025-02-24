using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class UIManager : SubManager<UIManager>
{
    public enum LayerOrder
    {
        Default,
        First,
        PlayerHUD,
        EnemyInBattle,
        InventorySomethingElse,
        Dialog,
        End,
    }

    

    [SerializeField] private List<GameObject> _uiSortingObject = new List<GameObject>();
    [SerializeField] private GameObject _uiSortingObjectPrefab = null;
    
    
    [SerializeField] private Canvas _canvas_2D = null;
    public Canvas Get2DCanvs(){return _canvas_2D; }

    [SerializeField] private EventSystem _eventSystem = null;
    [SerializeField] private int _consumeInputUICount = 0;

    private HUDScript _currHUD = null;
    public HUDScript _CurrHUD => _currHUD;


    public void SetHUD(GameObject hudObject, HUDScript component)
    {
        if (_currHUD != null)
        {
            //Debug.Assert(false, "�̹� ��尡 �ִµ���?");
            //Debug.Break();
            //Destroy(_currHUD.gameObject);

            return;
        }

        GameObject sortingObject = _uiSortingObject[(int)LayerOrder.PlayerHUD];

        hudObject.transform.SetParent(sortingObject.transform, false);

        _currHUD = component;
    }

    public void DestroyHUD(HUDScript component)
    {
        if (_currHUD != component)
        {
            Debug.Assert(false, "���� ������ �Ժη� �ı��Ϸ��Ѵ�");
            Debug.Break();
        }

        Destroy(_currHUD.gameObject);
        _currHUD = null;
    }

    public override void SubManagerInit()
    {
        SingletonAwake();

        if (_uiSortingObjectPrefab == null)
        {
            Debug.Assert(false, "UI�� �����ϱ����� �ӽ� ������Ʈ�� �����׽��ϴ� �ݵ�� �����ϼ���");
            Debug.Break();
        }

        if (_canvas_2D == null)
        {
            Debug.Assert(false, "MainCanvas�� �ݵ�� �����ؾ��Ѵ�");
        }


        if (_consumeInputUICount <= 0)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        ReadySortingLayer();
    }

    private void ReadySortingLayer()
    {
        for (int i = 0; i < (int)LayerOrder.End; i++)
        {
            GameObject newGameObject = Instantiate(_uiSortingObjectPrefab, _canvas_2D.transform);
            newGameObject.name = "UISortingLayer" + ((LayerOrder)i).ToString();
            _uiSortingObject.Add(newGameObject);
        }
    }

    public bool IsConsumeInput()
    {
        return (_consumeInputUICount > 0);
    }

    public void IncreaseConsumeInput()
    {
        int prev = _consumeInputUICount++;

        if (prev <= 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void DecreaseConsumeInput()
    {
        int after = --_consumeInputUICount;

        if (_consumeInputUICount < 0)
        {
            _consumeInputUICount = 0;
            after = 0;
            Debug.Log("Warning!!!!!!!! ¦�� ���� �ʾƿ�. ���� Ȯ�εȰ� Close Button�� ���� UI�� ���ϴ�");
        }

        if (after == 0)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void TurnOnUI(GameObject uiInstance, LayerOrder order)
    {
        UIComponent uiComponent = uiInstance.GetComponent<UIComponent>();

        if (uiComponent == null)
        {
            Debug.Assert(false, "UIComponent�� ���µ� �Ѱ�/���⸦ �����Ϸ��մϴ�");
            Debug.Break();
        }

        if (uiComponent.GetIsConsumeInput() == true)
        {
            Transform parent = uiComponent.transform.parent;

            if (parent != _canvas_2D.transform) 
            {
                IncreaseConsumeInput();
            }
        }

        GameObject layerObject = _uiSortingObject[(int)order];

        uiInstance.transform.SetParent(layerObject.transform);
        uiInstance.transform.SetAsLastSibling();
        uiComponent.ShowUI();

        uiInstance.transform.rotation = Quaternion.identity;
        uiInstance.transform.position = Vector3.zero;
        uiInstance.transform.localScale = Vector3.one;

        ((RectTransform)uiInstance.transform).anchoredPosition = Vector2.zero;
        ((RectTransform)uiInstance.transform).anchoredPosition += new Vector2(50.0f, 0.0f);
    }

    public void TurnOffUI(GameObject uiInstance)
    {
        UIComponent uiComponent = uiInstance.GetComponent<UIComponent>();

        if (uiComponent == null)
        {
            Debug.Assert(false, "UIComponent�� ���µ� �Ѱ�/���⸦ �����Ϸ��մϴ�");
            Debug.Break();
        }

        if (uiComponent.GetIsConsumeInput() == true)
        {
            DecreaseConsumeInput();
        }

        uiComponent.HideUI();
    }


    public void SetMeFinalZOrder(GameObject caller, LayerOrder order)
    {
        GameObject targetObject = _uiSortingObject[(int)order];


        caller.gameObject.transform.SetParent(targetObject.transform);
        caller.gameObject.transform.SetAsLastSibling();
    }


    public void RayCastAll(ref List<RaycastResult> results, bool isReverse = false)
    {
        if (_canvas_2D == null)
        {
            return;
        }

        GameObject rootObject = _canvas_2D.gameObject;

        PointerEventData pointerEventData = new PointerEventData(_eventSystem);
        pointerEventData.position = Input.mousePosition;

        rootObject.GetComponent<GraphicRaycaster>().Raycast(pointerEventData, results);

        if (isReverse == true)
        {
            results.Reverse();
        }
    }

    public override void SubManagerUpdate()
    {
    }

    public override void SubManagerFixedUpdate()
    {
    }

    public override void SubManagerLateUpdate()
    {
    }

    public override void SubManagerStart()
    {
    }
}
