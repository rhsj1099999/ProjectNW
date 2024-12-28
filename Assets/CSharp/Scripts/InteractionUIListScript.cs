using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.InputSystem;

public class InteractionUIListScript : MonoBehaviour
{
    public class UIListWrapper
    {
        public Collider _owner = null;
        public GameObject _uiObject = null;
        public InteractionUIDesc _desc = null;
        public UIInteractionableScript _uiWorker = null;
    }

    [SerializeField] private GameObject _eachUIPrefab = null;
    [SerializeField] private GameObject _focusedUI = null;
    [SerializeField] private RectTransform _myRectTransform = null;

    private List<UIListWrapper> _currCreated = new List<UIListWrapper>();

    private Dictionary<Collider, int> _currKeys = new Dictionary<Collider, int>();

    private int _currIndex = 0;

    private void Awake()
    {
        _myRectTransform = GetComponent<RectTransform>();

        RectTransform focusedUIRectTransform = (RectTransform)_focusedUI.transform;
        focusedUIRectTransform.position = _myRectTransform.position;

        RectTransform eachUIRectTransform = (RectTransform)_eachUIPrefab.transform;
        Rect pixelAdjustedRect = RectTransformUtility.PixelAdjustRect(eachUIRectTransform, eachUIRectTransform.GetComponentInParent<Canvas>());
        float height = pixelAdjustedRect.height;
        float width = pixelAdjustedRect.width;
        focusedUIRectTransform.sizeDelta = new Vector2(width, height);

        //focusedUIRectTransform.sizeDelta = new Vector2(focusedUIRectTransform.rect.width, focusedUIRectTransform.rect.height);
    }

    public void AddList(Collider collider, InteractionUIDesc desc, UIInteractionableScript uiWorker)
    {
        if (_currCreated.Count <= 0)
        {
            //최초다!
            _currIndex = 0;
            GameObject newUI = Instantiate(_eachUIPrefab, _myRectTransform);
            ((RectTransform)newUI.transform).position = _myRectTransform.position;

            UIListWrapper newWrapper = new UIListWrapper();
            newWrapper._owner = collider;
            newWrapper._uiObject = newUI;
            newWrapper._desc = desc;
            newWrapper._uiWorker = uiWorker;

            _currCreated.Add(newWrapper);

            UIManager.Instance.TurnOnUI(gameObject);

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
            UIListWrapper newWrapper = new UIListWrapper();
            newWrapper._owner = collider;
            newWrapper._uiObject = newUI;
            newWrapper._desc = desc;
            newWrapper._uiWorker = uiWorker;

            ((RectTransform)newUI.transform).position = lastPosition;
            _currCreated.Add(newWrapper);
        }

        _focusedUI.transform.position = _currCreated[_currIndex]._uiObject.transform.position;
        _focusedUI.transform.SetAsLastSibling();
    }

    public void RemoveList(Collider collider, InteractionUIDesc desc, UIInteractionableScript uiWorker)
    {
        //if (_currKeys.ContainsKey(collider) == false)
        //{
        //    Debug.Assert(false, "없는데 지우려하고있습니다");
        //    Debug.Break();
        //    return;
        //}

        //int key = _currKeys[collider];

        int index = 0;

        UIListWrapper deleteTarget = null;

        foreach (UIListWrapper wrapper in _currCreated)
        {
            if (wrapper._owner == collider)
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

        for (int i = index; i > 0; i--)
        {
            _currCreated[i]._uiObject.transform.position -= delta;
        }

        deleteTarget._uiWorker.UICall_Off();

        Destroy(deleteTarget._uiObject);

        _currCreated.Remove(deleteTarget);

        //_currKeys.Remove(collider);

        if (_currCreated.Count <= 0)
        {
            UIManager.Instance.TurnOffUI(gameObject);
            _focusedUI.SetActive(false);
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


        if (_currCreated.Count >= 0 &&
            Input.GetKeyDown(KeyCode.F) == true)
        {
            UIListWrapper currWrapper = _currCreated[_currIndex];
            currWrapper._uiWorker.UICall();
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
