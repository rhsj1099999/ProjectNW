using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using UnityEngine;

public class BattleUIScript : UIComponent
{
    [SerializeField] private List<StatBarScript> _statBarScriptsForInit = new List<StatBarScript>();
    [SerializeField] private GameObject _followingWorldAnchor = null;
    [SerializeField] private StatScript _statScript = null;
    [SerializeField] private CanvasGroup _canvasGroup = null;
    private bool _isBackSpace = false;
    

    protected override void Awake()
    {
        base.Awake();

        if (_followingWorldAnchor == null)
        {
            Debug.Assert(false, "화면에 표시될 때 따라갈 포지션 GameObject를 설정하세요");
            Debug.Break();
        }

        if (_statScript == null)
        {
            Debug.Assert(false, "연결될 StatScript를 지정하세요");
            Debug.Break();
        }

        if (_canvasGroup == null)
        {
            Debug.Assert(false, "_canvasGroup를 연결해주세요");
            Debug.Break();
        }

        
    }

    private void Start()
    {
        foreach (StatBarScript statBarScript in _statBarScriptsForInit)
        {
            statBarScript.StatLinking(_statScript);
        }
    }

    private bool CalculateBackSpace()
    {
        Vector3 dir = _followingWorldAnchor.transform.position - Camera.main.transform.position;

        //float distance = Vector3.Distance(dir);

        //if (distance <= Camera.main.nearClipPlane)
        //{
        //    return true;
        //}

        float dotRet = Vector3.Dot(Camera.main.transform.forward, dir);

        if (dotRet <= 0.0f)
        {
            return true;
        }

        return false;
    }

    private void LateUpdate()
    {
        if (_isShow == false) 
        {
            //최근에 피격되지않음. 걍 무시
            return; 
        }

        bool isBackSpace = CalculateBackSpace();
        
        if (isBackSpace != _isBackSpace) 
        {
            _isBackSpace = isBackSpace;

            if (_isBackSpace == true)
            {
                _canvasGroup.alpha = 0.0f;
                return;
            }
            else
            {
                _canvasGroup.alpha = 1.0f;
            }
        }

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_followingWorldAnchor.transform.position);
        _myRectTransform.position = new Vector2(screenPosition.x, screenPosition.y);
    }

}
