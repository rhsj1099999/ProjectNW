using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComponent : MonoBehaviour
{
    [SerializeField] private GameObject _parentGameObjectToReturn = null;
    [SerializeField] private bool _isConsumeInputUI = false;

    private Canvas _canvas = null;

    public GameObject GetReturnObject() { return _parentGameObjectToReturn; }
    

    private void Awake()
    {
        if (_parentGameObjectToReturn == null)
        {
            Debug.Assert(false, "UI가 꺼졌을때 돌아갈 오브젝트를 설정하세여");
            Debug.Break();
            return;
        }

        _canvas = GetComponent<Canvas>();
        
        if( _canvas != null )
        {
            _canvas.enabled = false;
        }
    }

    public void HideUI()
    {
        if (_canvas != null)
        {
            _canvas.enabled = false;
        }

        transform.SetParent(_parentGameObjectToReturn.transform);
    }

    public void ShowUI()
    {
        if (_canvas != null)
        {
            _canvas.enabled = true;
        }
    }

    public bool GetIsConsumeInput()
    {
        return _isConsumeInputUI;
    }


    public GameObject GetParentObjectToReturn()
    {
        return _parentGameObjectToReturn;
    }

}
