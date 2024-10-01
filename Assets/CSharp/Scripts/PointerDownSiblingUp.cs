using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDownSiblingUp : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Assert(_targetObject != null, "타겟이 설정돼지 않았다");
        _targetObject.transform.SetAsLastSibling();
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    [SerializeField] private GameObject _targetObject = null;
}
