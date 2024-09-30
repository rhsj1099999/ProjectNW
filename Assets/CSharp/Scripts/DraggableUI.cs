using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;


public class DraggableUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private void Awake()
    {
    }
    void Start()
    {
        
    }
    void Update()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }
    public void OnPointerUp(PointerEventData eventData)
    {

    }
    public void OnDrag(PointerEventData eventData)
    {
        if (_targetRectTransform != null) 
        {
            _targetRectTransform.anchoredPosition += eventData.delta;
        }
    }

    [SerializeField] private RectTransform _targetRectTransform = null;

    /*-----------------
    기능 실행마다 바뀔 변수들
     ----------------*/
}
