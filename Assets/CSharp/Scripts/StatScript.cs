using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BuffAsset_PassiveStat;
using static BuffAsset_RegenStat;
using static BuffAsset_StatBase;
using static LevelStatAsset;
using static LevelStatInfoManager;

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
        public RuntimeBuffAsset(BuffAssetBase fromAsset, int count, StatScript myOwner)
        {
            _count = count;
            _myOwner = myOwner;
            _fromAsset = fromAsset;
        }

        public StatScript _myOwner = null;
        public BuffAssetBase _fromAsset = null;
        public Coroutine _durationCoroutine = null;
        public float _timeACC = 0.0f;
        public Dictionary<BuffAction, BuffActionClass> _buffActions = new Dictionary<BuffAction, BuffActionClass>();

        public void AddDelegate(Action<int> action) {_countDelegates += action;}
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
    private RegenStatDesc _currRegenStat = null; //런타임 스텟입니다.

    private Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>> _passiveStatDeltaEquation = new Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>>();
    private Dictionary<RegenStat, SortedDictionary<BuffApplyType, int>> _regenStatDeltaEquation = new Dictionary<RegenStat, SortedDictionary<BuffApplyType, int>>();

    private Dictionary<RegenStat, float> _activeStatRegenCalculator = new Dictionary<RegenStat, float>();

    public Dictionary<RegenStat, Action<int>> _regenStatChangeDelegates = new Dictionary<RegenStat, Action<int>>();
    public Dictionary<ActiveStat, Action<int>> _activeStatChangeDelegates = new Dictionary<ActiveStat, Action<int>>();
    public Dictionary<PassiveStat, Action<int>> _passiveStatChangeDelegates = new Dictionary<PassiveStat, Action<int>>();


    private Dictionary<BuffAssetBase, RuntimeBuffAsset> _buffs = new Dictionary<BuffAssetBase, RuntimeBuffAsset>();
    private Dictionary<BuffAssetBase, RuntimeBuffAsset> _deBuffs = new Dictionary<BuffAssetBase, RuntimeBuffAsset>();

    public Dictionary<DamagingProcessDelegateType, HashSet<RuntimeBuffAsset>> _buffActions = new Dictionary<DamagingProcessDelegateType, HashSet<RuntimeBuffAsset>>();


    public RuntimeBuffAsset GetRuntimeBuffAsset(BuffAssetBase buff)
    {
        Dictionary<BuffAssetBase, RuntimeBuffAsset> targetDict = (buff._IsDebuff == true)
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





    public override void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(StatScript);

        LevelStatAsset statAsset = LevelStatInfoManager.Instance.GetLevelStatAsset(_currLevel, _owner._CharacterType);

        _currActiveStat = new ActiveStatDesc(statAsset._ActiveStatDesc);
        _currPassiveStat = new PassiveStatDesc(statAsset._PassiveStatDesc);
        _currRegenStat = new RegenStatDesc(statAsset._RegenStatDesc);

        for (int i = 0; i < (int)ActiveStat.End; i++)
        {
            _activeStatChangeDelegates.Add((ActiveStat)i, null);
        }

        for (int i = 0; i < (int)PassiveStat.End; i++)
        {
            _passiveStatChangeDelegates.Add((PassiveStat)i, null);
        }

        for (int i = 0; i < (int)RegenStat.End; i++)
        {
           _regenStatChangeDelegates.Add((RegenStat)i, null);
        }





        for (int i = (int)RegenStat.HPRegen; i <= (int)PassiveStat.End; i++)
        {
            _activeStatRegenCalculator.Add((RegenStat)i, 0.0f);
        }
    }

    public void CharacterRevive()
    {
        for (int i = 0; i < (int)ActiveStat.End; i++)
        {
            int maxVal = 0;

            ActiveStat statType = (ActiveStat)i;

            if (statType == ActiveStat.PosturePercent)
            {
                maxVal = 0;
            }
            else
            {
                GetReletivePassiveStat(statType, ref maxVal);
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


    public void ApplyBuff(BuffAssetBase buff, int count)
    {
        Dictionary<BuffAssetBase, RuntimeBuffAsset> target = null;

        target = (buff._IsDebuff == true)
            ? _deBuffs
            : _buffs;

        RuntimeBuffAsset runtimeBuffAsset = null;

        target.TryGetValue(buff, out runtimeBuffAsset);

        int buffCount = 0;

        if (buff._SpecialAction_OnlyOne == true)
        {
            if (runtimeBuffAsset != null)
            {
                RemoveBuff(buff, runtimeBuffAsset._Count);
            }

            runtimeBuffAsset = new RuntimeBuffAsset(buff, count, this);

            target.Add(buff, runtimeBuffAsset);

            BuffChangeStatCalculate(runtimeBuffAsset, count);

            if (_owner._CharacterType == CharacterType.Player)
            {
                BuffDisplayScript script = UIManager.Instance._CurrHUD._BuffDisplay;
                script.AddBuff(runtimeBuffAsset);
            }

            if (buff._Duration > 0.0f)
            {
                runtimeBuffAsset._durationCoroutine = StartCoroutine(BuffRunningCoroutine(runtimeBuffAsset));
            }

            return;
        }

        if (runtimeBuffAsset == null)
        {
            runtimeBuffAsset = new RuntimeBuffAsset(buff, 0, this);

            if (buff._IsTemporary == false)
            {
                target.Add(buff, runtimeBuffAsset);
            }
        }

        if (buff._MaxCount > runtimeBuffAsset._Count)
        {
            int prevCount = runtimeBuffAsset._Count;
            int nextCount = runtimeBuffAsset._Count + count;
            nextCount = Math.Clamp(nextCount, 0, buff._MaxCount);

            runtimeBuffAsset.SetCount(nextCount);

            buffCount = nextCount - prevCount;
        }

        BuffChangeStatCalculate(runtimeBuffAsset, buffCount);

        if (buff._IsTemporary == true)
        {
            return;
        }

        if (buff._Duration >= 0.0f && runtimeBuffAsset._durationCoroutine == null)
        {
            runtimeBuffAsset._durationCoroutine = StartCoroutine(BuffRunningCoroutine(runtimeBuffAsset));
        }

        if (buff._Refresh == true)
        {
            runtimeBuffAsset._timeACC = 0.0f;
        }

        if (_owner._CharacterType == CharacterType.Player)
        {
            BuffDisplayScript script = UIManager.Instance._CurrHUD._BuffDisplay;
            script.AddBuff(runtimeBuffAsset);
        }
    }

    public void RemoveBuff(BuffAssetBase buff, int count)
    {
        if (buff == null)
        {
            return;
        }

        Dictionary<BuffAssetBase, RuntimeBuffAsset> target = null;

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
        runtimeBuffAsset._fromAsset.DoWork(this, runtimeBuffAsset, deltaCount);
    }

    public void ReadAndApplyPassiveStatBuff(ApplyDesc_PassiveStat buffWork, int deltaCount)
    {
        SortedDictionary<BuffApplyType, int> currAppliedBuffs = _passiveStatDeltaEquation.GetOrAdd(buffWork._targetStat);

        BuffApplyType applyType = buffWork._applyType;

        if (currAppliedBuffs.ContainsKey(applyType) == false)
        {
            currAppliedBuffs.Add(applyType, 0);
        }

        int applyAmount = buffWork._amout * deltaCount;
        
        currAppliedBuffs[applyType] += applyAmount;

        if (currAppliedBuffs[applyType] < 0)
        {
            Debug.Assert(false, "음수가 나와선 안됩니다");
            Debug.Break();
        }
    }



    public void ReadAndApplyRegenStatBuff(ApplyDesc_RegenStat buffWork, int deltaCount)
    {
        SortedDictionary<BuffApplyType, int> currAppliedBuffs = _regenStatDeltaEquation.GetOrAdd(buffWork._targetStat);

        BuffApplyType applyType = buffWork._applyType;

        if (currAppliedBuffs.ContainsKey(applyType) == false)
        {
            currAppliedBuffs.Add(applyType, 0);
        }

        int applyAmount = buffWork._amout * deltaCount;

        currAppliedBuffs[applyType] += applyAmount;

        if (currAppliedBuffs[applyType] < 0)
        {
            Debug.Assert(false, "음수가 나와선 안됩니다");
            Debug.Break();
        }
    }



    public void ReCacheBuffAmoints(HashSet<PassiveStat> types)
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

            int statOut = 0;
            GetReletiveActiveStat(type, ref statOut);
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


    public void ReCacheBuffAmoints(HashSet<RegenStat> types)
    {
        foreach (RegenStat type in types)
        {
            int baseStat = LevelStatInfoManager.Instance.GetLevelStatAsset(_currLevel, _owner._CharacterType)._RegenStatDesc._RegenStats[type];
            int beforeVar = _currRegenStat._RegenStats[type];
            int nextVar = baseStat;

            SortedDictionary<BuffApplyType, int> currBuffs = _regenStatDeltaEquation.GetOrAdd(type);

            foreach (KeyValuePair<BuffApplyType, int> buffAmoint in currBuffs)
            {
                BuffApplyType applyType = buffAmoint.Key;


                bool isSet = false;

                switch (applyType)
                {
                    case BuffApplyType.Set:
                        {
                            _currRegenStat._RegenStats[type] = buffAmoint.Value;
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

            _currRegenStat._RegenStats[type] = nextVar;
            /*--------------------------------------------------
            |NOTI| Passive Stat이 변경됐습니다.
            --------------------------------------------------*/
            _regenStatChangeDelegates[type]?.Invoke(nextVar);
        }
    }


    public void GetReletivePassiveStat(ActiveStat type, ref int statOut)
    {
        switch (type)
        {
            case ActiveStat.Hp:
                statOut = GetPassiveStat(PassiveStat.MaxHP);
                break;
            case ActiveStat.Stamina:
                statOut = GetPassiveStat(PassiveStat.MaxStamina);
                break;
            case ActiveStat.Mp:
                statOut = GetPassiveStat(PassiveStat.MaxMp);
                break;
            case ActiveStat.Sp:
                statOut = GetPassiveStat(PassiveStat.MaxSp);
                break;

            case ActiveStat.PosturePercent:
                statOut = 100;
                break;

            default:
                Debug.Assert(false, "대응이 되지 않습니다");
                Debug.Break();
                break;
        }
    }

    public void GetReletiveActiveStat(PassiveStat type, ref int statOut)
    {
        switch (type)
        {
            case PassiveStat.MaxHP:
                statOut = GetActiveStat(ActiveStat.Hp);
                break;
            case PassiveStat.MaxMp:
                statOut = GetActiveStat(ActiveStat.Mp);
                break;
            case PassiveStat.MaxStamina:
                statOut = GetActiveStat(ActiveStat.Stamina);
                break;
            case PassiveStat.MaxSp:
                statOut = GetActiveStat(ActiveStat.Sp);
                break;

            default:
                //Debug.Assert(false, "대응이 되지 않습니다");
                //Debug.Break();
                break;
        }
    }



    public void ChangeActiveStat(ActiveStat type, int amount)
    {
        int nextVal = (_currActiveStat._ActiveStats[type] + amount);

        AfterCalculateActiveStat(type);


        int maxVar = 0;

        GetReletivePassiveStat(type, ref maxVar);

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
    public int GetRegenStat(RegenStat type)
    {
        return _currRegenStat._RegenStats[type];
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









    public void StatScriptUpdate()
    {
        //평상시 뭐해야합니까? -> 리젠 행동을 해야합니다
        ActiveStatRegen();
    }


    private void ActiveStatRegen()
    {
        ActiveStat currActiveStatType = ActiveStat.End;

        for (int i = (int)RegenStat.HPRegen; i <= (int)RegenStat.PostureRecovery; i++)
        {
            RegenStat currActiveStatRegenType = (RegenStat)i;

            switch (currActiveStatRegenType)
            {
                case RegenStat.HPRegen:
                    currActiveStatType = ActiveStat.Hp;
                    break;

                case RegenStat.StaminaRegen:
                    currActiveStatType = ActiveStat.Stamina;
                    break;

                case RegenStat.MPRegen:
                    currActiveStatType = ActiveStat.Mp;
                    break;

                case RegenStat.SPRegen:
                    currActiveStatType = ActiveStat.Sp;
                    break;

                case RegenStat.PostureRecovery:
                    currActiveStatType = ActiveStat.PosturePercent;
                    break;

                default:
                    Debug.Assert(false, "대응이 되지 않습니다");
                    Debug.Break();
                    break;
            }

            _activeStatRegenCalculator[currActiveStatRegenType] += Time.deltaTime * GetRegenStat(currActiveStatRegenType);

            int ready = (int)_activeStatRegenCalculator[currActiveStatRegenType];

            if (ready == 0)
            {
                continue;
            }

            _activeStatRegenCalculator[currActiveStatRegenType] -= ready;
            ChangeActiveStat(currActiveStatType, ready);
        }
    }
}
