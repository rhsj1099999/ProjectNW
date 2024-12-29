using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonUpClose : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] private UnityEvent _actions;

    public void OnPointerUp(PointerEventData eventData)
    {
        UIManager.Instance.TurnOffUI(_closeTargetGameObject);
        _actions.Invoke();
    }

    [SerializeField] private GameObject _closeTargetGameObject = null;
}
