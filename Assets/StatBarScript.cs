using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static LevelStatAsset;

public class StatBarScript : MonoBehaviour
{
    [SerializeField] private Image _statBarImageComponent = null;
    [SerializeField] private TextMeshProUGUI _statBarTextComponent = null;


    [SerializeField] private ActiveStat _targetActiveStatType = ActiveStat.End;
    private PassiveStat _targetPassiveStatType = PassiveStat.End;
    private StatScript _myStatTarget = null;


    private void Awake()
    {
        if (_statBarImageComponent == null)
        {
            Debug.Assert(false, "UI ImageComponent를 설정하세요");
            Debug.Break();
        }

        if (_statBarTextComponent == null)
        {
            Debug.Assert(false, "Text Component를 설정하세요");
            Debug.Break();
        }


        if (_targetActiveStatType == ActiveStat.End)
        {
            Debug.Assert(false, "감시할 패시브 스텟이 설정돼있어야합니다");
            Debug.Break();
        }

        //스탯링킹, 색깔 연동단계

        

        switch (_targetActiveStatType)
        {
            case ActiveStat.Hp:
                _targetPassiveStatType = PassiveStat.MaxHP;
                _statBarImageComponent.color = new Color(152.0f / 255.0f, 0, 0);
                break;

            case ActiveStat.Stamina:
                _targetPassiveStatType = PassiveStat.MaxStamina;
                _statBarImageComponent.color = new Color(3.0f / 255.0f, 152.0f / 255.0f, 0);
                break;

            case ActiveStat.Mp:
                _targetPassiveStatType = PassiveStat.MaxMp;
                _statBarImageComponent.color = new Color(0, 20.0f / 255.0f, 152.0f / 255.0f);
                break;

            case ActiveStat.Sp:
                _targetPassiveStatType = PassiveStat.MaxSp;
                _statBarImageComponent.color = new Color(183.0f / 255.0f, 181.0f / 255.0f, 0);
                break;

            case ActiveStat.PosturePercent:
                _targetPassiveStatType = PassiveStat.End;
                _statBarImageComponent.color = new Color(150.0f / 112.0f, 0.0f / 255.0f, 0);
                break;

            default:
                Debug.Assert(false, "대응이 되지 않습니다");
                Debug.Break();
                break;
        }
    }

    public void StatLinking(StatScript owner)
    {
        _myStatTarget = owner;

        //델리게이터 바인딩...
        {
            int currVar = _myStatTarget.GetActiveStat(_targetActiveStatType);
            
            _myStatTarget._activeStatChangeDelegates[_targetActiveStatType] += IfStatBarChanged;

            IfStatBarChanged(currVar);
        }
    }

    public void IfStatBarChanged(int nextVar)
    {
        int maxVal = (_targetActiveStatType != ActiveStat.PosturePercent)
        ? _myStatTarget.GetPassiveStat(_targetPassiveStatType)
        : 100;

        float ratio = (float)nextVar / (float)maxVal;

        _statBarImageComponent.fillAmount = ratio;
        _statBarTextComponent.text = nextVar.ToString() + "/" + maxVal.ToString();
    }
}
