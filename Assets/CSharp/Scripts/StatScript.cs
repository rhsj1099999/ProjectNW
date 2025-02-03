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
    public class RuntimeBuffAsset
    {
        public BuffAsset _fromAsset = null;
        //public float _duration = 0.0f; //�̰� �������¿� ���� �ʽ��ϱ�?


        public BuffTimingType _buffTimingType = BuffTimingType.Normal;
        public float _timeACC = 0.0f;
        public Coroutine _durationCoroutine = null;

        public int _count = 0;

    }


    private int _currLevel = 1;

    /*---------------------------------------------------
    |NOTI| �⺻�� ������ �Ŵ����� ������ ���� �ٷ� �� �� �ִ�
    ---------------------------------------------------*/
    private ActiveStatDesc _currActiveStat = null; //��Ÿ�� �����Դϴ�.
    private PassiveStatDesc _currPassiveStat = null; //��Ÿ�� �����Դϴ�.
    private Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>> _passiveStatDeltaEquation = new Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>>();



    public enum BuffTimingType
    {
        AnimationFrame,
        State,
        Normal,
    }

    private Dictionary<BuffTimingType, Dictionary<BuffAsset, RuntimeBuffAsset>> _buffs = new Dictionary<BuffTimingType, Dictionary<BuffAsset, RuntimeBuffAsset>>();
    private Dictionary<BuffTimingType, Dictionary<BuffAsset, RuntimeBuffAsset>> _deBuffs = new Dictionary<BuffTimingType, Dictionary<BuffAsset, RuntimeBuffAsset>>();
    
    private Dictionary<DamagingProcessDelegateType, Dictionary<BuffAsset, BuffActionClass>> _buffActions = new Dictionary<DamagingProcessDelegateType, Dictionary<BuffAsset, BuffActionClass>>();
    public IReadOnlyDictionary<DamagingProcessDelegateType, Dictionary<BuffAsset, BuffActionClass>> _BuffActions => _buffActions;

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
            
            if (wrapper._fromAsset._Duration <= wrapper._timeACC) //�����ð��� �����. 
            {
                Debug.Log("�����ð����� Ű : " + wrapper._fromAsset._BuffName);

                RemoveBuff(wrapper._fromAsset, wrapper._buffTimingType);
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





    public void ApplyBuff(BuffAsset buff, BuffTimingType timingType)
    {
        Dictionary<BuffAsset, RuntimeBuffAsset> target = null;

        target = (buff._IsDebuff == true)
            ? _buffs.GetOrAdd(timingType)
            : _deBuffs.GetOrAdd(timingType);


        if (target.ContainsKey(buff) == true &&
            true /*��ø�� ������� �ʴ� �������*/)
        {
            Debug.Log("���� ������ ��ø�� �ȵſ�");
            RemoveBuff(buff, timingType);
            //return;
        }

        List<BuffApplyWork> buffWorks = buff._BuffWorks;

        HashSet<PassiveStat> reCachingTargets = new HashSet<PassiveStat>();

        foreach (var buffWork in buffWorks)
        {
            //���� �׼� ����
            DamagingProcessDelegateType delegateTiming = buffWork._buffAction._delegateTiming;
            if (delegateTiming != DamagingProcessDelegateType.End)
            {
                BuffActionClass instance = LevelStatInfoManager.Instance.GetBuffAction(buffWork._buffAction._buffActionType);
                Dictionary<BuffAsset, BuffActionClass> buffAction = _buffActions.GetOrAdd(delegateTiming);
                buffAction.Add(buff, instance);
            }

            //��ġ ��ȭ ���
            PassiveStat targetType = buffWork._targetType;
            if (targetType != PassiveStat.None)
            {
                reCachingTargets.Add(targetType);
                ReadAndApply(buffWork, false);
            }
        }

        ReCacheBuffAmoints(reCachingTargets);

        
        RuntimeBuffAsset runtimeBuffAsset = null;
        target.TryGetValue(buff, out runtimeBuffAsset);

        if (runtimeBuffAsset == null)
        {
            runtimeBuffAsset = new RuntimeBuffAsset();
            runtimeBuffAsset._buffTimingType = timingType;
            runtimeBuffAsset._count = 1;
            runtimeBuffAsset._fromAsset = buff;
            runtimeBuffAsset._durationCoroutine = StartCoroutine(BuffRunningCoroutine(runtimeBuffAsset));
            target.Add(buff, runtimeBuffAsset);
        }
        else 
        {
            runtimeBuffAsset._timeACC = 0.0f;
        }
    }




    public void RemoveBuff(BuffAsset buff, BuffTimingType timingType)
    {
        Dictionary<BuffAsset, RuntimeBuffAsset> target = null;

        target = (buff._IsDebuff == true)
            ? _buffs.GetOrAdd(timingType)
            : _deBuffs.GetOrAdd(timingType);

        if (target.ContainsKey(buff) == false)
        {
            Debug.Assert(false, "�̹� ��ҵƽ��ϴ�?");
            Debug.Break();
            return;
        }

        RuntimeBuffAsset existRuntimeBuffAsset = target[buff];
        target.Remove(buff);

        List<BuffApplyWork> buffWorks = buff._BuffWorks;

        HashSet<PassiveStat> reCachingTargets = new HashSet<PassiveStat>();

        

        foreach (var buffWork in buffWorks)
        {
            //���� �׼� ����
            DamagingProcessDelegateType delegateTiming = buffWork._buffAction._delegateTiming;
            if (delegateTiming != DamagingProcessDelegateType.End)
            {
                Dictionary<BuffAsset, BuffActionClass> targetDict = null;
                _buffActions.TryGetValue(delegateTiming, out targetDict);
                if (targetDict == null) 
                {
                    Debug.Assert(false, "�ش� ���������� Ÿ�̹��� �߰������� ����????");
                    Debug.Break();
                }

                if (targetDict.ContainsKey(buff) == false)
                {
                    Debug.Assert(false, "�ش� �����׼��� �߰������� ����????");
                    Debug.Break();
                }

                targetDict.Remove(buff);
            }

            //��ġ��ȭ ���
            PassiveStat targetType = buffWork._targetType;
            if (targetType != PassiveStat.None)
            {
                reCachingTargets.Add(targetType);
                ReadAndApply(buffWork, true);
            }
        }

        ReCacheBuffAmoints(reCachingTargets);

        if (buff._Duration > 0.0f)
        {
            StopCoroutine(existRuntimeBuffAsset._durationCoroutine);
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
