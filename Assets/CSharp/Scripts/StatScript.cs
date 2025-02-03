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

public class StatScript : GameCharacterSubScript
{
    public enum BuffTimingType
    {
        AnimationFrame,
        State,
        Normal,
    }

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
        public BuffAsset _fromAsset = null;
        public Coroutine _durationCoroutine = null;

        public BuffTimingType _buffTimingType = BuffTimingType.Normal;

        public int _count = 0;
        public float _timeACC = 0.0f;
    }

    /*---------------------------------------------------
    |NOTI| 기본빵 스텟은 매니저와 레벨을 통해 바로 알 수 있다
    ---------------------------------------------------*/
    private int _currLevel = 1;
    private ActiveStatDesc _currActiveStat = null; //런타임 스텟입니다.
    private PassiveStatDesc _currPassiveStat = null; //런타임 스텟입니다.
    private Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>> _passiveStatDeltaEquation = new Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>>();


    private Dictionary<BuffTimingType, Dictionary<BuffAsset, RuntimeBuffAsset>> _buffs = new Dictionary<BuffTimingType, Dictionary<BuffAsset, RuntimeBuffAsset>>();
    private Dictionary<BuffTimingType, Dictionary<BuffAsset, RuntimeBuffAsset>> _deBuffs = new Dictionary<BuffTimingType, Dictionary<BuffAsset, RuntimeBuffAsset>>();
    
    private Dictionary<DamagingProcessDelegateType, Dictionary<BuffAsset, BuffActionClass>> _buffActions = new Dictionary<DamagingProcessDelegateType, Dictionary<BuffAsset, BuffActionClass>>();
    public IReadOnlyDictionary<DamagingProcessDelegateType, Dictionary<BuffAsset, BuffActionClass>> _BuffActions => _buffActions;


    public override void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(StatScript);

        LevelStatAsset statAsset = LevelStatInfoManager.Instance.GetLevelStatAsset(_currLevel, _owner._CharacterType);

        _currActiveStat = new ActiveStatDesc(statAsset._ActiveStatDesc);
        _currPassiveStat = new PassiveStatDesc(statAsset._PassiveStatDesc);
    }
    public override void SubScriptStart() { }
    public void InvokeDamagingProcessDelegate(DamagingProcessDelegateType type, DamageDesc damage, bool isWeakPoint, CharacterScript attacker, CharacterScript victim)
    {
        Dictionary<BuffAsset, BuffActionClass> buffActions = null;
        _buffActions.TryGetValue(type, out buffActions);

        if (buffActions == null) 
        {
            return;
        }

        foreach (var action in buffActions) 
        {
            action.Value.GetAction().Invoke(damage, isWeakPoint, attacker, victim);
            IReadOnlyList<Func<DamageDesc, bool, CharacterScript, CharacterScript, IEnumerator >> coroutines = action.Value._Coroutines;
            foreach (var coroutine in coroutines)
            {
                StartCoroutine(coroutine(damage, isWeakPoint, attacker, victim));
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
                    RemoveBuff(wrapper._fromAsset, wrapper._buffTimingType);
                    break;
                }
                else
                {
                    wrapper._count -= 1;

                    if (wrapper._count <= 0)
                    {
                        if (wrapper._count < 0)
                        {
                            Debug.Assert(false, "음수가 나와선 안됩니다");
                            Debug.Break();
                        }

                        RemoveBuff(wrapper._fromAsset, wrapper._buffTimingType);
                        break;
                    }

                    //카운트 하나 뺏고, 누산값 초기화 했음
                    //능력치는 감소되긴 해야한다.
                    wrapper._timeACC = 0.0f;
                    BuffChangeStatCalculate(wrapper._fromAsset, true);
                }
            }

            yield return null;
        }
    }


    public void ApplyBuff(BuffAsset buff, BuffTimingType timingType)
    {
        Dictionary<BuffAsset, RuntimeBuffAsset> target = null;

        target = (buff._IsDebuff == true)
            ? _buffs.GetOrAdd(timingType)
            : _deBuffs.GetOrAdd(timingType);

        RuntimeBuffAsset runtimeBuffAsset = null;
        target.TryGetValue(buff, out runtimeBuffAsset);


        if (runtimeBuffAsset != null)
        {
            if (buff._SpecialAction_OnlyOne == true)
            {
                RemoveBuff(buff, timingType);

                runtimeBuffAsset = new RuntimeBuffAsset();
                runtimeBuffAsset._buffTimingType = timingType;
                runtimeBuffAsset._fromAsset = buff;
                runtimeBuffAsset._count = 1;
                runtimeBuffAsset._durationCoroutine = StartCoroutine(BuffRunningCoroutine(runtimeBuffAsset));
                target.Add(buff, runtimeBuffAsset);
                BuffChangeStatCalculate(buff, false);

                return;
            }

            int moreCount = runtimeBuffAsset._count + 1;
            if (buff._MaxCount >= moreCount)
            {
                runtimeBuffAsset._count++;
                BuffChangeStatCalculate(buff, false);
            }

            if (buff._Refresh == true)
            {
                runtimeBuffAsset._timeACC = 0.0f;
            }
        }
        else
        {
            runtimeBuffAsset = new RuntimeBuffAsset();
            runtimeBuffAsset._buffTimingType = timingType;
            runtimeBuffAsset._fromAsset = buff;
            runtimeBuffAsset._count = 1;
            runtimeBuffAsset._durationCoroutine = StartCoroutine(BuffRunningCoroutine(runtimeBuffAsset));
            target.Add(buff, runtimeBuffAsset);

            BuffChangeStatCalculate(buff, false);
        }
    }




    private void BuffChangeStatCalculate(BuffAsset buff, bool isDeApply)
    {
        List<BuffApplyWork> buffWorks = buff._BuffWorks;
        HashSet<PassiveStat> reCachingTargets = new HashSet<PassiveStat>();
        foreach (var buffWork in buffWorks)
        {
            //버프 액션 적용
            DamagingProcessDelegateType delegateTiming = buffWork._buffAction._delegateTiming;
            if (delegateTiming != DamagingProcessDelegateType.End)
            {
                if (isDeApply == true)
                {
                    Dictionary<BuffAsset, BuffActionClass> targetDict = null;
                    _buffActions.TryGetValue(delegateTiming, out targetDict);
                    if (targetDict == null)
                    {
                        Debug.Assert(false, "해당 델리게이터 타이밍이 추가된적이 없다????");
                        Debug.Break();
                    }

                    if (targetDict.ContainsKey(buff) == false)
                    {
                        Debug.Assert(false, "해당 버프액션이 추가된적이 없다????");
                        Debug.Break();
                    }

                    targetDict.Remove(buff);
                }
                else
                {
                    BuffActionClass instance = LevelStatInfoManager.Instance.GetBuffAction(buffWork._buffAction._buffActionType);
                    Dictionary<BuffAsset, BuffActionClass> buffAction = _buffActions.GetOrAdd(delegateTiming);
                    buffAction.Add(buff, instance);
                }
            }


            //수치 변화 계산
            PassiveStat targetType = buffWork._targetType;
            if (targetType != PassiveStat.None)
            {
                reCachingTargets.Add(targetType);
                ReadAndApply(buffWork, isDeApply);
            }
        }

        ReCacheBuffAmoints(reCachingTargets);
    }




    public void RemoveBuff(BuffAsset buff, BuffTimingType timingType)
    {
        Dictionary<BuffAsset, RuntimeBuffAsset> target = null;

        target = (buff._IsDebuff == true)
            ? _buffs.GetOrAdd(timingType)
            : _deBuffs.GetOrAdd(timingType);

        RuntimeBuffAsset existRuntimeBuffAsset = null;
        target.TryGetValue(buff, out existRuntimeBuffAsset);

        if (existRuntimeBuffAsset == null)
        {
            Debug.Assert(false, "이미 취소됐습니다?");
            Debug.Break();
            return;
        }

        if (buff._Duration > 0.0f)
        {
            StopCoroutine(existRuntimeBuffAsset._durationCoroutine);
        }

        BuffChangeStatCalculate(buff, true);

        target.Remove(buff);
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
    
    private void ReadAndApply(BuffApplyWork buffWork, bool isDeApply)
    {
        SortedDictionary<BuffApplyType, int> currAppliedBuffs = _passiveStatDeltaEquation.GetOrAdd(buffWork._targetType);

        BuffApplyType applyType = buffWork._buffApplyType;

        if (currAppliedBuffs.ContainsKey(applyType) == false)
        {
            currAppliedBuffs.Add(applyType, 0);
        }

        if (isDeApply == true)
        {
            currAppliedBuffs[applyType] -= (int)buffWork._amount;
        }
        else
        {
            currAppliedBuffs[applyType] += (int)buffWork._amount;
        }

        if (currAppliedBuffs[applyType] < 0)
        {
            Debug.Assert(false, "음수가 나와선 안됩니다");
            Debug.Break();
        }
        
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
    public void ChangeActiveStat(ActiveStat type, int amount)
    {
        int nextVal = (_currActiveStat._ActiveStats[type] + amount);

        AfterCalculateActiveStat(type);

        _currActiveStat._ActiveStats[type] = nextVal;
    }
}
