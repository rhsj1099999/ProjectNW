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
            Debug.Assert(false, "ȭ�鿡 ǥ�õ� �� ���� ������ GameObject�� �����ϼ���");
            Debug.Break();
        }

        if (_statScript == null)
        {
            Debug.Assert(false, "����� StatScript�� �����ϼ���");
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
            //�ֱٿ� �ǰݵ�������. �� ����
            return; 
        }

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_followingWorldAnchor.transform.position);
        _myRectTransform.position = new Vector2(screenPosition.x, screenPosition.y);
    }

}
