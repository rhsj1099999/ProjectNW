using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDScript : MonoBehaviour
{
    [SerializeField] private BuffDisplayScript _buffDisplay = null;
    public BuffDisplayScript _BuffDisplay => _buffDisplay;



    /*--------------------------------------------------------------------------------------------------
    |NOTI| HUD �� �ʱ�ȭ�Ǵ� ����, ���ÿ� �������� StatBar���� '�̸�' �����صӴϴ�.
    �̰� ������ � HUD��, �� HUD�� ��Ÿ���̶�� �����մϴ�
    --------------------------------------------------------------------------------------------------*/
    [SerializeField] List<StatBarScript> _statBarScriptsForInit = new List<StatBarScript>();

    private StatScript _owner = null;


    public void HUDLinking(StatScript caller)
    {
        _owner = caller;

        UIManager.Instance.SetHUD(this);

        if (_buffDisplay == null)
        {
            Debug.Assert(false, "�ν����Ϳ��� ���� ǥ�ñ⸦ �����ϼ���");
            Debug.Break();
        }

        foreach (StatBarScript statBarScript in _statBarScriptsForInit)
        {
            statBarScript.StatLinking(_owner);
        }
    }



    private void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.DestroyHUD(this);
        }
    }
}
