using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;


public class DraggableUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData) {}
    public void OnPointerUp(PointerEventData eventData) {}

    public void OnDrag(PointerEventData eventData)
    {
        if (_targetRectTransform != null) 
        {
            Vector3 delta = new Vector3(eventData.delta.x, eventData.delta.y, 0.0f);
            _targetRectTransform.position += delta;
        }
    }

    [SerializeField] private RectTransform _targetRectTransform = null;

    /*-----------------
    기능 실행마다 바뀔 변수들
     ----------------*/
}
