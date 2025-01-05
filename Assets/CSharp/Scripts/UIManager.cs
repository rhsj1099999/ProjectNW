using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class UIManager : SubManager
{
    [SerializeField] private GameObject _mainCanvas = null;
    public GameObject GetMainCanvasObject(){return _mainCanvas;}
    [SerializeField] private EventSystem _eventSystem = null;
    [SerializeField] private int _consumeInputUICount = 0;

    private static UIManager _instance = null;
    
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject newGameObject = new GameObject("UIManager");
                _instance = newGameObject.AddComponent<UIManager>();
                DontDestroyOnLoad(newGameObject);
            }

            return _instance;
        }
    }

    public override void SubManagerAwake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;

        if (_mainCanvas == null)
        {
            Debug.Assert(false, "MainCanvas는 반드시 존재해야한다");
        }
    }

    public bool IsConsumeInput()
    {
        return (_consumeInputUICount > 0);
    }

    public void IncreaseConsumeInput()
    {
        ++_consumeInputUICount;
    }

    public void DecreaseConsumeInput()
    {
        --_consumeInputUICount;
    }

    public void TurnOnUI(GameObject uiInstance)
    {
        UIComponent uiComponent = uiInstance.GetComponent<UIComponent>();

        if (uiComponent == null)
        {
            Debug.Assert(false, "UIComponent가 없는데 켜고/끄기를 제어하려합니다");
            Debug.Break();
        }

        if (uiComponent.GetIsConsumeInput() == true)
        {
            Transform parent = uiComponent.transform.parent;

            if (parent != _mainCanvas.transform) 
            {
                IncreaseConsumeInput();
            }
        }

        uiComponent.ShowUI();

        uiInstance.transform.SetParent(_mainCanvas.transform);
        uiInstance.transform.SetAsLastSibling();

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
            Debug.Assert(false, "UIComponent가 없는데 켜고/끄기를 제어하려합니다");
            Debug.Break();
        }

        if (uiComponent.GetIsConsumeInput() == true)
        {
            Transform parent = uiComponent.transform.parent;

            if (parent == _mainCanvas.transform)
            {
                DecreaseConsumeInput();
            }
        }

        uiComponent.HideUI();
    }


    public void SetMeFinalZOrder(GameObject caller)
    {
        caller.gameObject.transform.SetParent(_mainCanvas.transform);
        caller.gameObject.transform.SetAsLastSibling();
    }


    public void RayCastAll(ref List<RaycastResult> results, bool isReverse = false)
    {
        if (_mainCanvas == null)
        {
            return;
        }

        GameObject rootObject = _mainCanvas;

        PointerEventData pointerEventData = new PointerEventData(_eventSystem);
        pointerEventData.position = Input.mousePosition;

        rootObject.GetComponent<GraphicRaycaster>().Raycast(pointerEventData, results);

        if (isReverse == true)
        {
            results.Reverse();
        }
    }
}
