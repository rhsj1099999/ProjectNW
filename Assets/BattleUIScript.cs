using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUIScript : UIComponent
{
    [SerializeField] private List<StatBarScript> _statBarScriptsForInit = new List<StatBarScript>();
    [SerializeField] private GameObject _followingWorldAnchor = null;
    [SerializeField] private StatScript _statScript = null;

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


    }

    private void Start()
    {
        foreach (StatBarScript statBarScript in _statBarScriptsForInit)
        {
            statBarScript.StatLinking(_statScript);
        }
    }

    private void LateUpdate()
    {
        if (_isShow == false) 
        {
            //최근에 피격되지않음. 걍 무시
            return; 
        }

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_followingWorldAnchor.transform.position);
        _myRectTransform.position = new Vector2(screenPosition.x, screenPosition.y);
    }

}
