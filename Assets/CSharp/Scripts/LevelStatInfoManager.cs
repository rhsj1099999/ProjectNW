using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static StatScript;

public class LevelStatInfoManager : SubManager<LevelStatInfoManager>
{
    public List<LevelStatAsset> _levelStats_Init = new List<LevelStatAsset>();
    private Dictionary<CharacterType, Dictionary<int, LevelStatAsset>> _levelStats = new Dictionary<CharacterType, Dictionary<int, LevelStatAsset>>();

    private void ReadyLevelStatData()
    {
        foreach (LevelStatAsset statAsset in _levelStats_Init)
        {
            CharacterType characterType = statAsset._CharacterType;

            Dictionary<int, LevelStatAsset> targetLevelAssets = _levelStats.GetOrAdd(characterType);

            int level_key = statAsset._Level;

            if (targetLevelAssets.ContainsKey(level_key) == true)
            {
                Debug.Assert(false, "�̹� �ش� ������ ���� �����Ͱ� �ִ�" + level_key);
                Debug.Break();
            }

            targetLevelAssets.Add(level_key, statAsset);
            statAsset.PartailAwake_InitDict();
        }
    }


    public LevelStatAsset GetLevelStatAsset(int level, CharacterType characterType)
    {
        

        Dictionary<int, LevelStatAsset> targetLevelAssets = null;
        _levelStats.TryGetValue(characterType, out targetLevelAssets);

        if (targetLevelAssets == null)
        {
            Debug.Assert(false, "�ش� ĳ����Ÿ�Կ� ���� ������ �����ϴ�");
            Debug.Break();
        }

        if (targetLevelAssets.ContainsKey(level) == false)
        {
            Debug.Assert(false, "���� ������ ���� �����͸� ��û�ߵ�/ ���� : " + level);
            Debug.Break();
        }

        return targetLevelAssets[level];
    }



    public List<BuffAsset> _buffs_Init = new List<BuffAsset>();
    Dictionary<int, BuffAsset> _buffs = new Dictionary<int, BuffAsset>();
    Dictionary<string, int> _buffNames = new Dictionary<string, int>();


    public int GetBuffKey(string buffName)
    {
        if (_buffNames.ContainsKey(buffName) == false)
        {
            Debug.Assert(false, "���� BuffName�� ��û�߽��ϴ�");
            Debug.Break();
            return -1;
        }

        return _buffNames[buffName];
    }

    public BuffAsset GetBuff(int buffKey)
    {
        if (_buffs.ContainsKey(buffKey) == false)
        {
            Debug.Assert(false, "���� BuffKey�� ��û�߽��ϴ�");
            Debug.Break();
            return null;
        }

        return _buffs[buffKey];
    }

    public BuffAsset GetBuff(string buffName)
    {
        if (_buffNames.ContainsKey(buffName) == false)
        {
            Debug.Assert(false, "���� BuffName�� ��û�߽��ϴ�");
            Debug.Break();
            return null;
        }

        return _buffs[GetBuffKey(buffName)];
    }


    private void ReadyBuffs()
    {
        int key = 0;
        foreach (BuffAsset buff in _buffs_Init)
        {
            if (_buffNames.ContainsKey(buff._BuffName) == true)
            {
                Debug.Assert(false, "�ߺ��Ǵ� �̸��� �ֽ��ϴ�" + buff._BuffName);
                Debug.Break();
            }

            buff._buffKey = key;

            _buffNames.Add(buff._BuffName, key);

            _buffs.Add(key, buff);

            key++;
        }
    }


    public enum BuffAction
    {
        BeidouElementalArt,
        BeidouElementalArtDamageReturn,
        WrioWeavingBuff,
        None,
    }

    Dictionary<BuffAction, BuffActionClass> _buffActions = new Dictionary<BuffAction, BuffActionClass>();
    public BuffActionClass GetBuffAction(BuffAction type)
    {
        if (_buffActions.ContainsKey(type) == false)
        {
            Debug.Assert(false, "���� ������ ��û�ߵ�");
            Debug.Break();
        }

        return _buffActions[type].CopyMe();
    }
    private void AddBuffAction(BuffAction type, BuffActionClass action)
    {
        if (_buffActions.ContainsKey(type) == true)
        {
            Debug.Assert(false, "�ش� Ÿ���� �̹� �ֽ��ϴ�");
            Debug.Break();
        }

        _buffActions.Add(type, action);
    }
    private void ReadyBuffAction()
    {
        //�ϵ� �ݰ� ����
        {
            AddBuffAction(BuffAction.BeidouElementalArt, new BuffActionClass_BeidouDamageACC());
        }

        //�ϵ� �ݰ� �������������
        {
            AddBuffAction(BuffAction.BeidouElementalArtDamageReturn, new BuffActionClass_BeidouElementalArtReturn());
        }

        //���̿� ���� ����
        {
            AddBuffAction(BuffAction.WrioWeavingBuff, new BuffActionClass_WrioWeaving());
        }
    }














    public override void SubManagerInit()
    {
        SingletonAwake();

        ReadyLevelStatData();

        ReadyBuffAction();
        ReadyBuffs();
    }

    public override void SubManagerFixedUpdate(){}
    public override void SubManagerLateUpdate(){}
    public override void SubManagerStart(){}
    public override void SubManagerUpdate() {}


    public abstract class BuffActionClass
    {
        public BuffActionClass() { }
        public BuffActionClass(int buffKey, DamagingProcessDelegateType timing)
        {
            _myKey = buffKey;
            _myTiming = timing;
        }

        protected DamagingProcessDelegateType _myTiming = DamagingProcessDelegateType.End;
        protected int _myKey = 0;

        protected List<Func<DamageDesc, bool, CharacterScript, CharacterScript, IEnumerator>> _coroutines = new List<Func<DamageDesc, bool, CharacterScript, CharacterScript, IEnumerator>>();
        public IReadOnlyList<Func<DamageDesc, bool, CharacterScript, CharacterScript, IEnumerator>> _Coroutines => _coroutines.AsReadOnly();

        public abstract void BuffAction(DamageDesc damage, bool weakPoint, CharacterScript attacker, CharacterScript victim);
        public abstract BuffActionClass CopyMe();
        public Action<DamageDesc, bool, CharacterScript, CharacterScript> GetAction() { return BuffAction; }
    }

    public class BuffActionClass_BeidouDamageACC : BuffActionClass
    {
        public BuffActionClass_BeidouDamageACC() { }
        public BuffActionClass_BeidouDamageACC(int buffKey, DamagingProcessDelegateType timing) : base(buffKey, timing) {}

        public int _damageACC = 0;
        public override void BuffAction(DamageDesc damage, bool weakPoint, CharacterScript attacker, CharacterScript victim)
        {
            CharacterScript ownerCharacterScript = victim.GetComponent<CharacterScript>();
            StatScript ownerStatScript = ownerCharacterScript.GCST<StatScript>();

            int ownerMaxHP = ownerStatScript.GetPassiveStat(LevelStatAsset.PassiveStat.MaxHP);

            int hpRisk = ownerMaxHP / 100;

            if (hpRisk <= 0)
            {
                hpRisk = 1;
            }

            int beforeACCLvl = _damageACC / hpRisk;

            _damageACC += (int)damage._damage;

            int afterACCLvl = _damageACC / hpRisk;

            if (afterACCLvl > 2)
            {
                afterACCLvl = 2;
            }

            Debug.Log("�ϵ� �� ���� ���� || ����� ������ : " + _damageACC);

            if (beforeACCLvl != afterACCLvl)
            {
                Debug.Log("�ϵ� �� ���� ���� || ���� ���� : " + afterACCLvl);
                ownerCharacterScript.GCST<StateContoller>().OverrideAnimationClip(afterACCLvl - 1);
            }

        }
        public override BuffActionClass CopyMe()
        {
            return new BuffActionClass_BeidouDamageACC(_myKey, _myTiming);
        }
    }

    public class BuffActionClass_BeidouElementalArtReturn : BuffActionClass
    {
        public BuffActionClass_BeidouElementalArtReturn() { }
        public BuffActionClass_BeidouElementalArtReturn(int buffKey, DamagingProcessDelegateType timing) : base(buffKey, timing) { }

        public override void BuffAction(DamageDesc damage, bool weakPoint, CharacterScript attacker, CharacterScript victim)
        {
            StatScript callerStatScript = attacker.GetComponent<StatScript>();
            BuffAsset beidouDamageAccBuff = Instance.GetBuff(Instance.GetBuffKey("�ϵο��ҽ�ų��������������"));

            Dictionary<BuffAsset, BuffActionClass> targetDict = callerStatScript._BuffActions[beidouDamageAccBuff._BuffWorks[0]._buffAction._delegateTiming];
            BuffActionClass_BeidouDamageACC beidouDamageAccBuffAction = targetDict[beidouDamageAccBuff] as BuffActionClass_BeidouDamageACC;

            damage._damage += beidouDamageAccBuffAction._damageACC;

            Debug.Log("������ ���� || ������ִ� ������ : " + beidouDamageAccBuffAction._damageACC + " ������ �� : " + damage._damage);

            callerStatScript.RemoveBuff(Instance.GetBuff("�ϵο��ҽ�ų��������������"), BuffTimingType.State);
        }

        public override BuffActionClass CopyMe() {return new BuffActionClass_BeidouElementalArtReturn(_myKey, _myTiming);}
    }

    public class BuffActionClass_WrioWeaving : BuffActionClass
    {
        public BuffActionClass_WrioWeaving() { }
        public BuffActionClass_WrioWeaving(int buffKey, DamagingProcessDelegateType timing) : base(buffKey, timing) 
        {
            _coroutines.Add(TimeSlowCoroutine);
        }

        bool _isTriggerd = false;

        public override void BuffAction(DamageDesc damage, bool weakPoint, CharacterScript attacker, CharacterScript victim)
        {
            if (_isTriggerd == true)
            {
                return;
            }

            _isTriggerd = true;

            StatScript owner_victimStatScript = victim.GCST<StatScript>();

            owner_victimStatScript.ApplyBuff(Instance.GetBuff("���̿��������ӹ���"), BuffTimingType.Normal);
        }

        private IEnumerator TimeSlowCoroutine(DamageDesc damage, bool weakPoint, CharacterScript attacker, CharacterScript victim)
        {
            float target = 0.5f;
            float timeScale = 0.25f;
            float timeACC = 0.0f;

            Time.timeScale = timeScale;

            while (true)
            {
                timeACC += Time.deltaTime * (1.0f / Time.timeScale);

                if (timeACC > target) 
                {
                    Time.timeScale = 1.0f;
                    break;
                }

                yield return null;
            }
        }

        public override BuffActionClass CopyMe() {return new BuffActionClass_WrioWeaving(_myKey, _myTiming);}
    }
}
