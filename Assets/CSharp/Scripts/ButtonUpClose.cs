using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonUpClose : MonoBehaviour, IPointerUpHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_parentGameObjectToReturn != null && _uiReturnTarget != null)
        {
            _uiReturnTarget.transform.SetParent(_parentGameObjectToReturn.transform);
        }

        if (_closeTargetGameObject != null) 
        {
            _closeTargetGameObject.GetComponent<UIComponent>().HideUI();
        }

        _closeEvents.Invoke();
    }


    [SerializeField] private GameObject _closeTargetGameObject = null;
    [SerializeField] private GameObject _parentGameObjectToReturn = null;
    [SerializeField] private GameObject _uiReturnTarget = null;
    [SerializeField] private UnityEvent _closeEvents = new UnityEvent();
}
