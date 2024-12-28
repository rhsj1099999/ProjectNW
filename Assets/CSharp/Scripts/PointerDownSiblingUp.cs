using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDownSiblingUp : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject _targetObject = null;

    public void OnPointerDown(PointerEventData eventData)
    {
        _targetObject.transform.SetAsLastSibling();
    }

    private void Awake()
    {
        if (_targetObject == null)
        {
            Debug.Assert(false, "Ÿ���� �����������ʾҴ�");
            Debug.Break();
            return;
        }
    }
}
