using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class UIManager : SubManager
{
    [SerializeField] private GameObject _mainCanvas = null;
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
        }
        else
        {
            _instance = this;
        }

        if (_mainCanvas == null)
        {
            Debug.Assert(false, "MainCanvas는 반드시 존재해야한다");
        }
    }

    public bool IsConsumeInput()
    {
        return (_consumeInputUICount > 0);
    }

    public void TurnOnUI(GameObject uiInstance)
    {
        UIComponent uiComponent = uiInstance.GetComponent<UIComponent>();

        if (uiComponent == null)
        {
            Debug.Assert(false, "UIComponent가 없는데 켜고/끄기를 제어하려합니다");
            Debug.Break();
        }

        uiComponent.ShowUI();

        uiInstance.transform.SetParent(_mainCanvas.transform);
        uiInstance.transform.SetAsLastSibling();

        if (uiInstance.name == "InteractionListUI")
        {
            uiInstance.transform.rotation = Quaternion.identity;
            uiInstance.transform.position = Vector3.zero;
            ((RectTransform)uiInstance.transform).anchoredPosition = Vector2.zero;
            ((RectTransform)uiInstance.transform).anchoredPosition += new Vector2(50.0f, 0.0f);
        }

        if (uiComponent.GetIsConsumeInput() == true)
        {
            ++_consumeInputUICount;
        }
    }

    public void TurnOffUI(GameObject uiInstance)
    {
        UIComponent uiComponent = uiInstance.GetComponent<UIComponent>();
        if (uiComponent == null)
        {
            Debug.Assert(false, "UIComponent가 없는데 켜고/끄기를 제어하려합니다");
            Debug.Break();
        }
        uiComponent.HideUI();

        if (uiComponent.GetIsConsumeInput() == true)
        {
            --_consumeInputUICount;
        }
    }

    public void RayCastAll(ref List<RaycastResult> results, bool isReverse = false)
    {
        if (_mainCanvas == null)
        {
            return;
        }

        GameObject rootObject = _mainCanvas;

        GraphicRaycaster[] raycasters = rootObject.GetComponentsInChildren<GraphicRaycaster>();

        Vector2 mousePosition = Input.mousePosition;
        PointerEventData pointerEventData = new PointerEventData(_eventSystem);
        pointerEventData.position = mousePosition;

        List<RaycastResult> eachResult = new List<RaycastResult>();

        if (isReverse == true)
        {
            foreach (var raycaster in raycasters.AsEnumerable().Reverse())
            {
                eachResult.Clear();
                raycaster.Raycast(pointerEventData, eachResult);
                eachResult.Reverse();

                foreach (var result in eachResult)
                {
                    results.Add(result);
                }
            }
        }
        else
        {
            foreach (var raycaster in raycasters)
            {
                eachResult.Clear();
                raycaster.Raycast(pointerEventData, eachResult);
                eachResult.Reverse();

                foreach (var result in eachResult)
                {
                    results.Add(result);
                }
            }
        }
    }
}
