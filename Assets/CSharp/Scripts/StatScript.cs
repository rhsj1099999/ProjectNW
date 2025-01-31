using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Build.Content;
using UnityEngine;
using static BuffAsset;
using static BuffAsset.BuffApplyWork;
using static LevelStatAsset;

public class StatScript : GameCharacterSubScript
{
    public class BuffWrapper
    {
        public BuffAsset _fromAsset = null; //�̰� �� �־���մϱ�?
        public float _timeACC = 0.0f;
        public float _duration = 0.0f; //�̰� �������¿� ���� �ʽ��ϱ�?
        public Coroutine _coroutine = null;
    }


    private int _currLevel = 1;

    /*---------------------------------------------------
    |NOTI| �⺻�� ������ �Ŵ����� ������ ���� �ٷ� �� �� �ִ�
    ---------------------------------------------------*/
    private ActiveStatDesc _currActiveStat = null; //��Ÿ�� �����Դϴ�.
    private PassiveStatDesc _currPassiveStat = null; //��Ÿ�� �����Դϴ�.

    private Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>> _passiveStatDeltaEquation = new Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>>();


    private HashSet<BuffAsset> _buffs = new HashSet<BuffAsset>();
    private HashSet<BuffAsset> _deBuffs = new HashSet<BuffAsset>();

    private Dictionary<BuffAsset, BuffWrapper> _buffCoroutines = new Dictionary<BuffAsset, BuffWrapper>();

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

        End,
    }

    private Dictionary<DamagingProcessDelegateType, Action<DamageDesc, bool, GameObject>> _damagingProcessDelegates = new Dictionary<DamagingProcessDelegateType, Action<DamageDesc, bool, GameObject>>();
    //public Dictionary<DamagingProcessDelegateType, Action<DamageDesc, bool, GameObject>> _DamagingProcessDelegates => _damagingProcessDelegates;
    public void InvokeDamagingProcessDelegate(DamagingProcessDelegateType type, DamageDesc damage, bool isWeakPoint, GameObject caller)
    {
        _damagingProcessDelegates[type]?.Invoke(damage, isWeakPoint, caller);
    }





    private IEnumerator BuffRunningCoroutine(BuffWrapper wrapper)
    {
        while (true) 
        {
            wrapper._timeACC += Time.deltaTime;
            
            if (wrapper._duration <= wrapper._timeACC) //�����ð��� �����. 
            {
                RemoveBuff(wrapper._fromAsset);
                break;
            }

            yield return null;
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



    private void AfterCalculateActiveStat(ActiveStat type)
    {

    }




    public void ChangeActiveStat(ActiveStat type, int amount)
    {
        int nextVal = (_currActiveStat._ActiveStats[type] + amount);

        AfterCalculateActiveStat(type);

        _currActiveStat._ActiveStats[type] = nextVal;
    }

    public void ApplyBuff(BuffAsset buff)
    {
        HashSet<BuffAsset> target = null;

        target = (buff._IsDebuff == true)
        ? _deBuffs
        : _buffs;


        if (target.Contains(buff) == true)
        {
            /*-------------------------------------------
            |TODO| �轺 ���ӹ����� ��ø�ǰ� �ϳ��� �������
            -------------------------------------------*/
            Debug.Log("���� ������ ��ø�� �ȵſ�");
            return;
        }

        List<BuffApplyWork> buffWorks = buff._BuffWorks;

        HashSet<PassiveStat> reCachingTargets = new HashSet<PassiveStat>();

        foreach (var buffWork in buffWorks)
        {
            reCachingTargets.Add(buffWork._targetType);
            ReadAndApply(buffWork, false);
        }

        ReCacheBuffAmoints(reCachingTargets);

        if (buff._Duration > 0.0f)
        {
            if (target.Contains(buff) == true)
            {
                BuffWrapper wrapperTarget = _buffCoroutines[buff];
                wrapperTarget._timeACC = 0.0f;
            }
            else
            {
                BuffWrapper wrapper = new BuffWrapper();
                wrapper._duration = buff._Duration;
                wrapper._fromAsset = buff;
                wrapper._coroutine = StartCoroutine(BuffRunningCoroutine(wrapper));
                _buffCoroutines.Add(buff, wrapper);
                target.Add(buff);
            }
        }
    }




    public void RemoveBuff(BuffAsset buff)
    {
        HashSet<BuffAsset> target = null;

        target = (buff._IsDebuff == true)
        ? _deBuffs
        : _buffs;

        if (target.Contains(buff) == false)
        {
            Debug.Assert(false, "�̹� ��ҵƽ��ϴ�?");
            Debug.Break();
            return;
        }

        target.Remove(buff);

        List<BuffApplyWork> buffWorks = buff._BuffWorks;

        HashSet<PassiveStat> reCachingTargets = new HashSet<PassiveStat>();

        foreach (var buffWork in buffWorks)
        {
            reCachingTargets.Add(buffWork._targetType);
            ReadAndApply(buffWork, true);
        }

        ReCacheBuffAmoints(reCachingTargets);

        if (buff._Duration > 0.0f)
        {
            BuffWrapper wrapperTarget = _buffCoroutines[buff];
            StopCoroutine(wrapperTarget._coroutine);
            _buffCoroutines.Remove(buff);
        }
    }


    private void ReCacheBuffAmoints(HashSet<PassiveStat> types)
    {
        foreach (PassiveStat type in types)
        {
            int baseStat = LevelStatInfoManager.Instance.GetLevelStatAsset(_currLevel)._PassiveStatDesc._PassiveStats[type];
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
                        Debug.Assert(false, "ApplyType�� ��ġ���� �ʽ��ϴ�");
                        Debug.Break();
                        break;
                }

                if (isSet == true) 
                {
                    break;
                }
            }

            _currPassiveStat._PassiveStats[type] = nextVar;

            //���ȿ� ����Ǹ� �� �ؾ��մϱ�?
            //ex �ִ�ü���� ����Ǹ� ����ü���� �÷����մϴ�.
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
            Debug.Assert(false, "������ ���ͼ� �ȵ˴ϴ�");
            Debug.Break();
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
        //���������� ����
        //---------------------------
        for (int i = 0; i < (int)DamagingProcessDelegateType.End; i++)
        {
            _damagingProcessDelegates.Add((DamagingProcessDelegateType)i, null);
        }

        //----------------------------
        //���� ����
        //---------------------------
        LevelStatAsset statAsset = LevelStatInfoManager.Instance.GetLevelStatAsset(_currLevel);

        _currActiveStat = new ActiveStatDesc(statAsset._ActiveStatDesc);
        _currPassiveStat = new PassiveStatDesc(statAsset._PassiveStatDesc);
    }

    public override void SubScriptStart() 
    {

    }
}
