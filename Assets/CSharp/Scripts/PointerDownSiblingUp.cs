using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDownSiblingUp : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Assert(_targetObject != null, "Ÿ���� �������� �ʾҴ�");
        _targetObject.transform.SetAsLastSibling();
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    [SerializeField] private GameObject _targetObject = null;
}
