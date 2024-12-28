using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//��ũ���ͺ� ������Ʈ�� �����մϴ�
[Serializable]
public class InteractionUIDesc
{
    private string _uiName = "���ڿ���";
    private Sprite _uiSprite = null;
}

public class UIInteractionableScript : MonoBehaviour
{
    [SerializeField] private InteractionUIDesc _interactionUIDesc = null;
    [SerializeField] private GameObject _uiWorkScript = null;
    [SerializeField] private UICallScript _uiCallScript = null;

    public InteractionUIDesc GetInteractionUIDesc()
    {
        return _interactionUIDesc;
    }

    private void Awake()
    {
        if (_interactionUIDesc == null)
        {
            Debug.Log("�ݵ�� �ϳ��� �������־���մϴ�(Scriptable Object�� ����)");
            _interactionUIDesc = new InteractionUIDesc();
        }
    }

    public void UICall()
    {
        Debug.Log("UICall!");
        _uiCallScript.UICall();
        //_uiWorkScript ->call
    }

    public void UICall_Off()
    {
        Debug.Log("UICall!");
        _uiCallScript.UICall_Off();
    }
}
