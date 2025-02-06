using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDScript : MonoBehaviour
{
    [SerializeField] private BuffDisplayScript _buffDisplay = null;
    public BuffDisplayScript _BuffDisplay => _buffDisplay;



    /*--------------------------------------------------------------------------------------------------
    |NOTI| HUD 가 초기화되는 순간, 동시에 연결해줄 StatBar들을 '미리' 지정해둡니다.
    이것 앞으로 어떤 HUD든, 그 HUD의 스타일이라고 생각합니다
    --------------------------------------------------------------------------------------------------*/
    [SerializeField] List<StatBarScript> _statBarScriptsForInit = new List<StatBarScript>();

    private StatScript _owner = null;


    public void HUDLinking(StatScript caller)
    {
        _owner = caller;

        UIManager.Instance.SetHUD(this);

        if (_buffDisplay == null)
        {
            Debug.Assert(false, "인스펙터에서 버프 표시기를 설정하세요");
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
