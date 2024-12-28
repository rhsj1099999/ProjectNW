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
            Debug.Assert(false, "타겟이 설정ㄷㅚ지않았다");
            Debug.Break();
            return;
        }
    }
}
