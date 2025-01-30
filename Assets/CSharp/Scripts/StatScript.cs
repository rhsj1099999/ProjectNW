using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Build.Content;
using UnityEngine;
using static BuffAsset;
using static BuffAsset.BuffApplyWork;
using static LevelStatAsset;

public class StatScript : GameCharacterSubScript
{
    public class RunningBuff
    {
        public BuffAsset _buffAsset = null;
        public float _timeACC = 0.0f;
    }


    private int _currLevel = 1;


    /*---------------------------------------------------
    |NOTI| 기본빵 스텟은 매니저와 레벨을 통해 바로 알 수 있다
    ---------------------------------------------------*/
    private ActiveStatDesc _currActiveStat = null; //런타임 스텟입니다.
    private PassiveStatDesc _currPassiveStat = null; //런타임 스텟입니다.

    private Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>> _passiveStatDeltaEquation = new Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>>();
    private Dictionary<PassiveStat, int> _passiveStatDeltaCalculated = new Dictionary<PassiveStat, int>();

    private HashSet<BuffAsset> _stateBuffs = new HashSet<BuffAsset>();
    private HashSet<BuffAsset> _stateDeBuffs = new HashSet<BuffAsset>();

    private HashSet<BuffAsset> _buffs = new HashSet<BuffAsset>();
    private HashSet<BuffAsset> _deBuffs = new HashSet<BuffAsset>();


    public void StateChanged(StateAsset nextState)
    {
        {
            /*-------------------------------------------------------------------
            상태에 의해 걸렸던 버프들을 모두 취소한다.
            그럼에도 불구하고 현재 걸린 버프에 의해서 해당 이로운/해로운 효과가 남아있다면
            받을 수 있어야 한다
            -------------------------------------------------------------------*/
        }

        foreach (BuffAsset stateBuff in _stateBuffs) 
        {

        }
        _stateBuffs.Clear();
    }




    public int GetActiveStat(ActiveStat type)
    {
        return _currActiveStat._ActiveStats[type];
    }

    public int GetPassiveStat(PassiveStat type)
    {
        return _currPassiveStat._PassiveStats[type];
    }



    public void ChangePassiveStat(PassiveStat type, int amount)
    {
        //1. 기본 스텟을 가져와본다
        int currLevelPassiveStat = LevelStatInfoManager.Instance.GetLevelStatAsset(_currLevel)._PassiveStatDesc._PassiveStats[type];

        //2. amount 를 적용한다 -> 그럼 변동된 기본스텟이다.
        currLevelPassiveStat += amount;

        //3. 변동된 기본 스텟에, DeltaCalculated를 적용한다.
        currLevelPassiveStat +=_passiveStatDeltaCalculated[type];

        _currPassiveStat._PassiveStats[type] = currLevelPassiveStat;
    }


    public void ChangeActiveStat(ActiveStat type, int amount)
    {
        int nextVal = (_currActiveStat._ActiveStats[type] + amount);

        //이후 Passive Stat에 의한 계산을 실행한다
        {
            //예를 들어 체력에 amount 만큼 늘렸을때, max HP만큼은 안넘어가게
        }

        _currActiveStat._ActiveStats[type] = nextVal;
    }





    public void ApplyBuff(BuffAsset buff, bool isState = false)
    {
        HashSet<BuffAsset> target = null;

        if (isState == true)
        {
            target = (buff._IsDebuff == true)
            ? _stateDeBuffs
            : _stateBuffs;
        }
        else
        {
            target = (buff._IsDebuff == true)
            ? _deBuffs
            : _buffs;
        }


        if (target.Contains(buff) == true)
        {
            /*-------------------------------------------
            |TODO| 잭스 공속버프는 중첩되고 하나씩 사라져요
            -------------------------------------------*/
            Debug.Log("동일 버프는 중첩이 안돼요");
            return;
        }

        List<BuffApplyWork> buffWorks = buff._BuffWorks;

        foreach (var buffWork in buffWorks)
        {
            ReadAndApply(buffWork, false);
        }

        target.Add(buff);
    }




    public void RemoveBuff(BuffAsset buff, bool isState = false)
    {
        HashSet<BuffAsset> target = null;

        if (isState == true)
        {
            target = (buff._IsDebuff == true)
            ? _stateDeBuffs
            : _stateBuffs;
        }
        else
        {
            target = (buff._IsDebuff == true)
            ? _deBuffs
            : _buffs;
        }


        if (target.Contains(buff) == false)
        {
            Debug.Log("이미 취소됐습니다");
            return;
        }

        List<BuffApplyWork> buffWorks = buff._BuffWorks;

        foreach (var buffWork in buffWorks)
        {
            ReadAndApply(buffWork, true);
        }

        target.Remove(buff);
    }





    private void ReadAndApply(BuffApplyWork buffWork, bool isDeApply)
    {
        BuffApplyType applyType = buffWork._buffApplyType;

        switch (applyType)
        {
            case BuffApplyType.Plus:
                break;
            case BuffApplyType.Minus:
                break;


            case BuffApplyType.PercentagePlus:
                break;
            case BuffApplyType.PercentageMinus:
                break;


            case BuffApplyType.Multiply:
                break;
            case BuffApplyType.Devide:
                break;


            case BuffApplyType.Set:
                break;


            default:
                break;
        }
    }













    public int CalculateStatDamage()
    {
        return 1;
    }

    public int CalculateStatDamagingStamina()
    {
        return 1;
    }

    public int CalculatePower()
    {
        return 1;
    }






    public override void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(StatScript);

        //----------------------------
        //레벨 세팅
        //---------------------------
        // 1. 현재 레벨에 대한 테이블 값을 가져오고 덮어씁니다.

        LevelStatAsset statAsset = LevelStatInfoManager.Instance.GetLevelStatAsset(_currLevel);

        _currActiveStat = new ActiveStatDesc(statAsset._ActiveStatDesc);
        _currPassiveStat = new PassiveStatDesc(statAsset._PassiveStatDesc);
    }

    public override void SubScriptStart() 
    {

    }
}
