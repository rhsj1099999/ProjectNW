using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonUpClose : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] private GameObject _closeTargetGameObject = null;

    public void OnPointerUp(PointerEventData eventData)
    {
        UIManager.Instance.TurnOffUI(_closeTargetGameObject);
    }
}
