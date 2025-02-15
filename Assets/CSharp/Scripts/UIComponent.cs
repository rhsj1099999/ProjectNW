using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class UIComponent : MonoBehaviour
{
    [SerializeField] private bool _isConsumeInputUI = false;


    [SerializeField] private GameActorScript _uiControllingObject = null;
    public GameActorScript GetUIControllingComponent() { return _uiControllingObject; }
    protected RectTransform _myRectTransform = null;
    protected bool _isShow = false;
    public bool _IsShow => _isShow;

    protected virtual void Awake()
    {
        if (_uiControllingObject == null)
        {
            Debug.Assert(false, "UI가 꺼졌을때 돌아갈 오브젝트를 설정하세여");
            Debug.Break();
            return;
        }

        GameUISubComponent[] subComponents = GetComponentsInChildren<GameUISubComponent>();

        foreach (GameUISubComponent subComponent in subComponents)
        {
            subComponent.Init(this);
        }

        _myRectTransform = (RectTransform)transform;
    }

    public void HideUI()
    {
        transform.SetParent(_uiControllingObject.transform);
        _isShow = false;
    }

    public void ShowUI()
    {
        _myRectTransform.anchorMin = Vector2.zero; // (0,0)
        _myRectTransform.anchorMax = Vector2.one;  // (1,1)

        _myRectTransform.offsetMin = Vector2.zero;
        _myRectTransform.offsetMax = Vector2.zero;
        _isShow = true;
    }

    public bool GetIsConsumeInput()
    {
        return _isConsumeInputUI;
    }
}
