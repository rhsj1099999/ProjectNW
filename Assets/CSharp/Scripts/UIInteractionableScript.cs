using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//스크립터블 오브젝트로 찍어내야합니다
[Serializable]
public class InteractionUIDesc
{
    private string _uiName = "상자열기";
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
            Debug.Log("반드시 하나는 설정돼있어야합니다(Scriptable Object로 찍어낼것)");
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
