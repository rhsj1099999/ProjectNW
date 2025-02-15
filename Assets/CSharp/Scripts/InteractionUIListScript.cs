using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.InputSystem;

public class InteractionUIListScript : GameUISubComponent
{
    public class UIListWrapper
    {
        public GameObject _uiObject = null;
        public UICallScript _uiCall = null;
    }

    [SerializeField] private GameObject _eachUIPrefab = null;
    [SerializeField] private GameObject _focusedUI = null;
    [SerializeField] private RectTransform _myRectTransform = null;

    private List<UIListWrapper> _currCreated = new List<UIListWrapper>();

    private int _currIndex = 0;

    public override void Init(UIComponent owner)
    {
        _owner = owner;

        _myRectTransform = GetComponent<RectTransform>();

        RectTransform focusedUIRectTransform = (RectTransform)_focusedUI.transform;
        focusedUIRectTransform.position = _myRectTransform.position;

        UIScaler listElementScaler = _eachUIPrefab.GetComponent<UIScaler>();
        
        focusedUIRectTransform.sizeDelta = listElementScaler.GetAnchoredSize();
    }

    public void AddList(UICallScript uiCallScript)
    {
        if (_currCreated.Count <= 0)
        {
            //최초다!
            _currIndex = 0;
            GameObject newUI = Instantiate(_eachUIPrefab, _myRectTransform);
            InteractionUIScript interactionUIScript = newUI.GetComponent<InteractionUIScript>();
            interactionUIScript.SetUIListData(uiCallScript._UIData._sprite, uiCallScript._UIData._message);



            ((RectTransform)newUI.transform).position = _myRectTransform.position;

            UIListWrapper newWrapper = new UIListWrapper();
            newWrapper._uiObject = newUI;
            newWrapper._uiCall = uiCallScript;

            _currCreated.Add(newWrapper);

            UIManager.Instance.TurnOnUI(gameObject, UIManager.LayerOrder.PlayerHUD);

            _focusedUI.SetActive(true);
        }
        else
        {
            Vector3[] representCorner = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                ((RectTransform)_currCreated.First()._uiObject.transform).GetWorldCorners(representCorner);
            }

            float dynamicHeight = representCorner[1].y - representCorner[0].y;


            foreach (UIListWrapper eachUI in _currCreated)
            {
                eachUI._uiObject.transform.position += new Vector3(0.0f, dynamicHeight / 2.0f, 0.0f);
            }

            Vector3 lastPosition = _currCreated.Last()._uiObject.transform.position;
            lastPosition += new Vector3(0.0f, -dynamicHeight, 0.0f);
            GameObject newUI = Instantiate(_eachUIPrefab, _myRectTransform);
            InteractionUIScript interactionUIScript = newUI.GetComponent<InteractionUIScript>();
            interactionUIScript.SetUIListData(uiCallScript._UIData._sprite, uiCallScript._UIData._message);

            UIListWrapper newWrapper = new UIListWrapper();
            newWrapper._uiObject = newUI;
            newWrapper._uiCall = uiCallScript;

            ((RectTransform)newUI.transform).position = lastPosition;
            _currCreated.Add(newWrapper);
        }

        _focusedUI.transform.position = _currCreated[_currIndex]._uiObject.transform.position;
        _focusedUI.transform.SetAsLastSibling();
    }

    public void RemoveList(UICallScript uiCallScript)
    {
        int index = 0;

        UIListWrapper deleteTarget = null;

        foreach (UIListWrapper wrapper in _currCreated)
        {
            if (wrapper._uiCall == uiCallScript)
            {
                deleteTarget = wrapper;
                break;
            }

            index++;
        }

        if (deleteTarget == null)
        {
            Debug.Assert(false, "못찾았습니까?");
            Debug.Break();
            return;
        }


        Vector3[] representWorldCorner = new Vector3[4];
        ((RectTransform)_currCreated.First()._uiObject.transform).GetWorldCorners(representWorldCorner);
        float height = representWorldCorner[1].y - representWorldCorner[0].y;
        Vector3 delta = new Vector3(0.0f, height / 2.0f, 0.0f);

        for (int i = index; i < _currCreated.Count; i++)
        {
            _currCreated[i]._uiObject.transform.position += delta;
        }

        for (int i = index; i >= 0; i--)
        {
            _currCreated[i]._uiObject.transform.position -= delta;
        }

        deleteTarget._uiCall.UICall_Off(this);
        _currCreated.Remove(deleteTarget);
        Destroy(deleteTarget._uiObject);


        if (_currCreated.Count <= 0)
        {
            UIManager.Instance.TurnOffUI(gameObject);
            _focusedUI.SetActive(false);
            _currIndex = 0;
            return;
        }

        if (index == _currIndex)
        {
            _currIndex--;
        }
        _currIndex = Math.Clamp(_currIndex, 0, _currCreated.Count - 1);
        _focusedUI.transform.position = _currCreated[_currIndex]._uiObject.transform.position;
    }

    private void Update()
    {
        if (_currCreated.Count > 1)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0f)
            {
                ChangeScroll(scroll);
            }
        }


        if (_currCreated.Count > 0 &&
            Input.GetKeyDown(KeyCode.F) == true)
        {
            UIListWrapper currWrapper = _currCreated[_currIndex];
            currWrapper._uiCall.UICall(this);
        }
    }

    private void ChangeScroll(float scroll)
    {
        if (scroll > 0f)
        {
            _currIndex--;
        }
        else if (scroll < 0f)
        {
            _currIndex++;
        }

        _currIndex = Math.Clamp(_currIndex, 0, _currCreated.Count - 1);
        _focusedUI.transform.position = _currCreated[_currIndex]._uiObject.transform.position;
    }

    public int GetCount()
    {
        return _currCreated.Count;
    }
}
