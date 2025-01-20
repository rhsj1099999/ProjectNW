using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComponent : MonoBehaviour
{
    [SerializeField] private bool _isConsumeInputUI = false;

    private Canvas _canvas = null;

    [SerializeField] private GameActorScript _uiControllingObject = null;
    public GameActorScript GetUIControllingComponent() { return _uiControllingObject; }
    

    private void Awake()
    {
        if (_uiControllingObject == null)
        {
            Debug.Assert(false, "UI가 꺼졌을때 돌아갈 오브젝트를 설정하세여");
            Debug.Break();
            return;
        }

        _canvas = GetComponent<Canvas>();
        
        if( _canvas != null )
        {
            _canvas.enabled = false;
        }

        GameUISubComponent[] subComponents = GetComponentsInChildren<GameUISubComponent>();

        foreach (GameUISubComponent subComponent in subComponents)
        {
            subComponent.Init(this);
        }
    }

    public void HideUI()
    {
        if (_canvas != null)
        {
            _canvas.enabled = false;
        }

        transform.SetParent(_uiControllingObject.transform);
    }

    public void ShowUI()
    {
        if (_canvas != null)
        {
            _canvas.enabled = true;
        }
    }

    public bool GetIsConsumeInput()
    {
        return _isConsumeInputUI;
    }
}
