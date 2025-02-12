using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Build.Content;
using UnityEngine;
using static BuffAsset;
using static BuffAsset.BuffApplyWork;
using static LevelStatAsset;
using static LevelStatInfoManager;
using static StatScript;

public class StatScript : GameCharacterSubScript
{
    public enum DamagingProcessDelegateType
    {
        Before_InvincibleCheck,
        After_InvincibleCheck,

        Before_GuardCheck,
        After_GuardCheck,

        Before_BuffCheck,
        After_BuffCheck,

        Before_ApplyDamage,
        After_ApplyDamage,

        Before_AttackerBuffCheck,
        After_AttackerBuffCheck,

        End,
    }

    public class RuntimeBuffAsset
    {
        public RuntimeBuffAsset(BuffAsset fromAsset, int count, StatScript myOwner)
        {
            _myOwner = myOwner;
            _fromAsset = fromAsset;
            _count = count;

            List<BuffApplyWork> buffWorks = _fromAsset._BuffWorks;
            foreach (BuffApplyWork work in buffWorks)
            {
                BuffAction type = work._buffAction._buffActionType;
                if (work._buffAction._buffActionType != BuffAction.None)
                {
                    DamagingProcessDelegateType delegateType = work._buffAction._delegateTiming;
                    HashSet<RuntimeBuffAsset> existHashSet = _myOwner._buffActions.GetOrAdd(delegateType);

                    if (existHashSet.Contains(this) == true)
                    {
                        Debug.Assert(false, "버프가 만들어지는 타이밍인데 이미 존재하고 있으면 안됩니다");
                        Debug.Break();
                    }

                    existHashSet.Add(this);
                    _buffActions.Add(type, LevelStatInfoManager.Instance.GetBuffAction(type));
                }
            }
        }

        public StatScript _myOwner = null;
        public BuffAsset _fromAsset = null;
        public Coroutine _durationCoroutine = null;
        public Dictionary<BuffAction, BuffActionClass> _buffActions = new Dictionary<BuffAction, BuffActionClass>();
        public float _timeACC = 0.0f;

        public void AddDelegate(Action<int> action)
        {
            _countDelegates += action;
        }
        private Action<int> _countDelegates = null;

        private int _count = 0;
        public int _Count => _count;
        public void SetCount(int nextCount)
        {
            _count = nextCount;

            if (nextCount <= 0)
            {
                if (nextCount < 0)
                {
                    Debug.Assert(false, "음수가 나와선 안된다");
                    Debug.Break();
                }


                //버프 액션 적용
                List<BuffApplyWork> buffWorks = _fromAsset._BuffWorks;
                foreach (BuffApplyWork work in buffWorks)
                {
                    if (work._buffAction._buffActionType != BuffAction.None)
                    {
                        DamagingProcessDelegateType delegateType = work._buffAction._delegateTiming;
                        HashSet<RuntimeBuffAsset> existHashSet = _myOwner._buffActions.GetOrAdd(delegateType);

                        if (existHashSet.Contains(this) == false)
                        {
                            Debug.Assert(false, "버프액션이 제거될 타이밍인데 없으면 안된다");
                            Debug.Break();
                        }

                        existHashSet.Remove(this);
                    }
                }
            }

            //델리게이터 호출
            {
                _countDelegates?.Invoke(nextCount);
            }
        }
    }

    /*---------------------------------------------------
    |NOTI| 기본빵 스텟은 매니저와 레벨을 통해 바로 알 수 있다
    ---------------------------------------------------*/
    private int _currLevel = 1;

    private ActiveStatDesc _currActiveStat = null; //런타임 스텟입니다.
    private PassiveStatDesc _currPassiveStat = null; //런타임 스텟입니다.
    private Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>> _passiveStatDeltaEquation = new Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>>();
    public Dictionary<ActiveStat, Action<int>> _activeStatChangeDelegates = new Dictionary<ActiveStat, Action<int>>();
    public Dictionary<PassiveStat, Action<int>> _passiveStatChangeDelegates = new Dictionary<PassiveStat, Action<int>>();

    private Dictionary<BuffAsset, RuntimeBuffAsset> _buffs = new Dictionary<BuffAsset, RuntimeBuffAsset>();
    private Dictionary<BuffAsset, RuntimeBuffAsset> _deBuffs = new Dictionary<BuffAsset, RuntimeBuffAsset>();
    private Dictionary<PassiveStat, float> _activeStatRegenCalculator = new Dictionary<PassiveStat, float>();


    public RuntimeBuffAsset GetRuntimeBuffAsset(BuffAsset buff)
    {
        Dictionary<BuffAsset, RuntimeBuffAsset> targetDict = (buff._IsDebuff == true)
            ? _deBuffs
            : _buffs;

        RuntimeBuffAsset ret = null;
        targetDict.TryGetValue(buff, out ret);

        if (ret == null)
        {
            Debug.Assert(false, "해당 버프가 걸린적이 없습니다");
            Debug.Break();
        }

        return ret;
    }

    private Dictionary<DamagingProcessDelegateType, HashSet<RuntimeBuffAsset>> _buffActions = new Dictionary<DamagingProcessDelegateType, HashSet<RuntimeBuffAsset>>();
    public IReadOnlyDictionary<DamagingProcessDelegateType, HashSet<RuntimeBuffAsset>> _BuffActions => _buffActions;




    public void StatScriptUpdate()
    {
        //평상시 뭐해야합니까? -> 리젠 행동을 해야합니다
        ActiveStatRegen();
    }


    private void ActiveStatRegen()
    {
        ActiveStat currActiveStatType = ActiveStat.End;

        for (int i = (int)PassiveStat.HPRegen; i <= (int)PassiveStat.SPRegen; i++)
        {
            PassiveStat currActiveStatRegenType = (PassiveStat)i;

            switch (currActiveStatRegenType)
            {
                case PassiveStat.HPRegen:
                    currActiveStatType = ActiveStat.Hp;
                    break;

                case PassiveStat.StaminaRegen:
                    currActiveStatType = ActiveStat.Stamina;
                    break;

                case PassiveStat.MPRegen:
                    currActiveStatType = ActiveStat.Mp;
                    break;

                case PassiveStat.SPRegen:
                    currActiveStatType = ActiveStat.Sp;
                    break;

                default:
                    Debug.Assert(false, "대응이 되지 않습니다");
                    Debug.Break();
                    break;
            }

            _activeStatRegenCalculator[currActiveStatRegenType] += Time.deltaTime * GetPassiveStat(currActiveStatRegenType);

            int ready = (int)_activeStatRegenCalculator[currActiveStatRegenType];

            if (ready == 0)
            {
                continue;
            }

            _activeStatRegenCalculator[currActiveStatRegenType] -= ready;
            ChangeActiveStat(currActiveStatType, ready);
        }
    }



    public override void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(StatScript);

        LevelStatAsset statAsset = LevelStatInfoManager.Instance.GetLevelStatAsset(_currLevel, _owner._CharacterType);

        _currActiveStat = new ActiveStatDesc(statAsset._ActiveStatDesc);
        _currPassiveStat = new PassiveStatDesc(statAsset._PassiveStatDesc);

        for (int i = 0; i < (int)ActiveStat.End; i++)
        {
            _activeStatChangeDelegates.Add((ActiveStat)i, null);
        }

        for (int i = 0; i < (int)PassiveStat.End; i++)
        {
            _passiveStatChangeDelegates.Add((PassiveStat)i, null);
        }

        for (int i = (int)PassiveStat.HPRegen; i <= (int)PassiveStat.SPRegen; i++)
        {
            _activeStatRegenCalculator.Add((PassiveStat)i, 0.0f);
        }
    }

    public void CharacterRevive()
    {
        for (int i = 0; i < (int)ActiveStat.End; i++)
        {
            int maxVal = 0;

            ActiveStat statType = (ActiveStat)i;

            switch (statType)
            {
                case ActiveStat.Hp:
                    maxVal = GetPassiveStat(PassiveStat.MaxHP);
                    break;
                case ActiveStat.Stamina:
                    maxVal = GetPassiveStat(PassiveStat.MaxStamina);
                    break;
                case ActiveStat.Mp:
                    maxVal = GetPassiveStat(PassiveStat.MaxMp);
                    break;
                case ActiveStat.Sp:
                    maxVal = GetPassiveStat(PassiveStat.MaxSp);
                    break;
            }

            ChangeActiveStat(statType, maxVal);
        }
    }


    public override void SubScriptStart() { }

    public void InvokeDamagingProcessDelegate(DamagingProcessDelegateType type, DamageDesc damage, bool isWeakPoint, CharacterScript attacker, CharacterScript victim)
    {
        HashSet<RuntimeBuffAsset> buffActions = null;
        _buffActions.TryGetValue(type, out buffActions);

        if (buffActions == null) 
        {
            return;
        }


        foreach (RuntimeBuffAsset runtimeBuffAsset in buffActions.ToList()) 
        {
            for (int i = 0; i < runtimeBuffAsset._Count; i++)
            {
                foreach (KeyValuePair<BuffAction, BuffActionClass> pair in runtimeBuffAsset._buffActions)
                {
                    pair.Value.GetAction().Invoke(damage, isWeakPoint, attacker, victim);
                    IReadOnlyList<Func<DamageDesc, bool, CharacterScript, CharacterScript, IEnumerator>> coroutines = pair.Value._Coroutines;
                    foreach (var coroutine in coroutines)
                    {
                        StartCoroutine(coroutine(damage, isWeakPoint, attacker, victim));
                    }
                }
            }
        }
    }

    private IEnumerator BuffRunningCoroutine(RuntimeBuffAsset wrapper)
    {
        while (true) 
        {
            wrapper._timeACC += Time.deltaTime;
            
            if (wrapper._fromAsset._Duration <= wrapper._timeACC) //버프시간이 만료됨. 
            {
                Debug.Log("버프시간만료 키 : " + wrapper._fromAsset._BuffName);


                if (wrapper._fromAsset._DurationExpireOnce == true)
                {
                    RemoveBuff(wrapper._fromAsset, wrapper._Count);
                    break;
                }
                else
                {
                    RemoveBuff(wrapper._fromAsset, 1);
                    wrapper._timeACC = 0.0f;
                }
            }

            yield return null;
        }
    }


    public void ApplyBuff(BuffAsset buff, int count)
    {
        Dictionary<BuffAsset, RuntimeBuffAsset> target = null;

        target = (buff._IsDebuff == true)
            ? _deBuffs
            : _buffs;

        RuntimeBuffAsset runtimeBuffAsset = null;
        target.TryGetValue(buff, out runtimeBuffAsset);


        if (runtimeBuffAsset != null)
        {
            //버프가 이미 있습니다. 시간 갱신, 수량 갱신등을 합니다.

            if (buff._SpecialAction_OnlyOne == true)
            {
                RemoveBuff(buff, runtimeBuffAsset._Count);

                runtimeBuffAsset = new RuntimeBuffAsset(buff, count, this);
                if (buff._Duration > 0.0f)
                {
                    runtimeBuffAsset._durationCoroutine = StartCoroutine(BuffRunningCoroutine(runtimeBuffAsset));
                }
                target.Add(buff, runtimeBuffAsset);

                BuffChangeStatCalculate(runtimeBuffAsset, runtimeBuffAsset._Count);

                if (_owner._CharacterType == CharacterType.Player)
                {
                    BuffDisplayScript script = UIManager.Instance._CurrHUD._BuffDisplay;
                    script.AddBuff(runtimeBuffAsset);
                }

                return;
            }

            if (buff._MaxCount > runtimeBuffAsset._Count)
            {
                int prevCount = runtimeBuffAsset._Count;
                int nextCount = runtimeBuffAsset._Count + count;
                nextCount = Math.Clamp(nextCount, 0, buff._MaxCount);
                runtimeBuffAsset.SetCount(nextCount);

                BuffChangeStatCalculate(runtimeBuffAsset, nextCount - prevCount);
            }


            if (buff._Refresh == true)
            {
                runtimeBuffAsset._timeACC = 0.0f;
            }
        }
        else
        {
            //버프가 없습니다. 처음 만들어집니다

            runtimeBuffAsset = new RuntimeBuffAsset(buff, count, this);

            BuffChangeStatCalculate(runtimeBuffAsset, runtimeBuffAsset._Count);

            if (buff._IsTemporary == true)
            {
                //관리대상이 아니다.
                return;
            }

            if (buff._Duration >= 0.0f)
            {
                runtimeBuffAsset._durationCoroutine = StartCoroutine(BuffRunningCoroutine(runtimeBuffAsset));
            }

            target.Add(buff, runtimeBuffAsset);


            if (_owner._CharacterType == CharacterType.Player)
            {
                BuffDisplayScript script = UIManager.Instance._CurrHUD._BuffDisplay;
                script.AddBuff(runtimeBuffAsset);
            }
        }
    }

    public void RemoveBuff(BuffAsset buff, int count)
    {
        Dictionary<BuffAsset, RuntimeBuffAsset> target = null;

        target = (buff._IsDebuff == true)
            ? _deBuffs
            : _buffs;

        RuntimeBuffAsset existRuntimeBuffAsset = null;
        target.TryGetValue(buff, out existRuntimeBuffAsset);

        if (existRuntimeBuffAsset == null)
        {
            Debug.Assert(false, "이미 취소됐습니다?");
            Debug.Break();
            return;
        }

        int prevCount = existRuntimeBuffAsset._Count;
        int nextCount = existRuntimeBuffAsset._Count - count;
        nextCount = Math.Clamp(nextCount, 0, buff._MaxCount);

        existRuntimeBuffAsset.SetCount(nextCount);

        BuffChangeStatCalculate(existRuntimeBuffAsset, nextCount - prevCount);

        if (existRuntimeBuffAsset._Count <= 0) 
        {
            if (buff._Duration > 0.0f)
            {
                StopCoroutine(existRuntimeBuffAsset._durationCoroutine);
            }

            target.Remove(buff);


            if (_owner._CharacterType == CharacterType.Player)
            {
                BuffDisplayScript script = UIManager.Instance._CurrHUD._BuffDisplay;
                script.RemoveBuff(existRuntimeBuffAsset);
            }
        }
    }



    private void BuffChangeStatCalculate(RuntimeBuffAsset runtimeBuffAsset, int deltaCount)
    {
        List<BuffApplyWork> buffWorks = runtimeBuffAsset._fromAsset._BuffWorks;
        HashSet<PassiveStat> reCachingTargets = new HashSet<PassiveStat>();
        foreach (var buffWork in buffWorks)
        {
            //수치 변화 계산
            PassiveStat targetType = buffWork._targetType;
            if (targetType != PassiveStat.End)
            {
                reCachingTargets.Add(targetType);
                ReadAndApply(buffWork, deltaCount);
            }
        }

        ReCacheBuffAmoints(reCachingTargets);

        foreach (var buffWork in buffWorks)
        {
            ActiveStat activeStatType = buffWork._targetActiveStatType;
            if (activeStatType != ActiveStat.End)
            {
                ChangeActiveStat(activeStatType, (int)buffWork._amount);
            }
        }
    }
    
    private void ReadAndApply(BuffApplyWork buffWork, int deltaCount)
    {
        SortedDictionary<BuffApplyType, int> currAppliedBuffs = _passiveStatDeltaEquation.GetOrAdd(buffWork._targetType);

        BuffApplyType applyType = buffWork._buffApplyType;

        if (currAppliedBuffs.ContainsKey(applyType) == false)
        {
            currAppliedBuffs.Add(applyType, 0);
        }

        int applyAmount = (applyType == BuffApplyType.Set)
            ? (int)buffWork._amount
            : (int)buffWork._amount * deltaCount;

        currAppliedBuffs[applyType] += applyAmount;

        if (currAppliedBuffs[applyType] < 0)
        {
            Debug.Assert(false, "음수가 나와선 안됩니다");
            Debug.Break();
        }
        
    }



    private void ReCacheBuffAmoints(HashSet<PassiveStat> types)
    {
        foreach (PassiveStat type in types)
        {
            int baseStat = LevelStatInfoManager.Instance.GetLevelStatAsset(_currLevel, _owner._CharacterType)._PassiveStatDesc._PassiveStats[type];
            int beforeVar = _currPassiveStat._PassiveStats[type];
            int nextVar = baseStat;

            SortedDictionary<BuffApplyType, int> currBuffs = _passiveStatDeltaEquation.GetOrAdd(type);

            foreach (KeyValuePair<BuffApplyType, int> buffAmoint in currBuffs)
            {
                BuffApplyType applyType = buffAmoint.Key;


                bool isSet = false;

                switch (applyType)
                {
                    case BuffApplyType.Set:
                        {
                            _currPassiveStat._PassiveStats[type] = buffAmoint.Value;
                            nextVar = buffAmoint.Value;
                            isSet = true;
                        }
                        break;


                    case BuffApplyType.Plus:
                        {
                            nextVar += buffAmoint.Value;
                        }
                        break;

                    case BuffApplyType.Minus:
                        {
                            nextVar -= buffAmoint.Value;
                        }
                        break;

                    case BuffApplyType.PercentagePlus:
                        {
                            nextVar += (int)((float)baseStat * ((float)buffAmoint.Value / 100.0f));
                        }
                        break;
                    case BuffApplyType.PercentageMinus:
                        {
                            nextVar -= (int)((float)baseStat * ((float)buffAmoint.Value / 100.0f));
                        }
                        break;

                    case BuffApplyType.Multiply:
                        {
                            nextVar *= buffAmoint.Value;
                        }
                        break;

                    case BuffApplyType.Devide:
                        {
                            nextVar /= buffAmoint.Value;
                        }
                        break;

                    default:
                        Debug.Assert(false, "ApplyType과 일치하지 않습니다");
                        Debug.Break();
                        break;
                }

                if (isSet == true)
                {
                    break;
                }
            }

            _currPassiveStat._PassiveStats[type] = nextVar;
            /*--------------------------------------------------
            |NOTI| Passive Stat이 변경됐습니다.
            --------------------------------------------------*/
            _passiveStatChangeDelegates[type]?.Invoke(nextVar);

            {
                //스탯에 변경되면 뭘 해야합니까?
                //ex 최대체력이 변경되면 현재체력을 늘려야합니다.
                switch (type)
                {
                    case PassiveStat.MaxHP:
                        break;
                    case PassiveStat.MaxStamina:
                        break;
                    case PassiveStat.MaxMp:
                        break;
                    case PassiveStat.MaxSp:
                        break;
                    case PassiveStat.Roughness:
                        break;
                    case PassiveStat.Strength:
                        break;
                    case PassiveStat.IsInvincible:
                        break;
                    default:
                        break;
                }
            }
        }
    }


    public void ChangeActiveStat(ActiveStat type, int amount)
    {
        int nextVal = (_currActiveStat._ActiveStats[type] + amount);

        AfterCalculateActiveStat(type);


        int maxVar = 0;

        switch (type)
        {
            case ActiveStat.Hp:
                maxVar = GetPassiveStat(PassiveStat.MaxHP);
                break;
            case ActiveStat.Stamina:
                maxVar = GetPassiveStat(PassiveStat.MaxStamina);
                break;
            case ActiveStat.Mp:
                maxVar = GetPassiveStat(PassiveStat.MaxMp);
                break;
            case ActiveStat.Sp:
                maxVar = GetPassiveStat(PassiveStat.MaxSp);
                break;

            default:
                Debug.Assert(false, "대응이 되지 않습니다");
                Debug.Break();
                break;
        }

        nextVal = Math.Clamp(nextVal, 0, maxVar);

        _currActiveStat._ActiveStats[type] = nextVal;


        /*--------------------------------------------------
        |NOTI| Active Stat이 변경됐습니다.
        --------------------------------------------------*/
        _activeStatChangeDelegates[type]?.Invoke(nextVal);
    }


    public int GetActiveStat(ActiveStat type)
    {
        return _currActiveStat._ActiveStats[type];
    }
    public int GetPassiveStat(PassiveStat type)
    {
        return _currPassiveStat._PassiveStats[type];
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

    private void AfterCalculateActiveStat(ActiveStat type)
    {

    }
}
