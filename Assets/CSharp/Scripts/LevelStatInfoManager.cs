using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

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
                Debug.Assert(false, "이미 해당 레벨에 대한 데이터가 있다" + level_key);
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
            Debug.Assert(false, "해당 캐릭터타입에 대한 정보가 없습니다");
            Debug.Break();
        }

        if (targetLevelAssets.ContainsKey(level) == false)
        {
            Debug.Assert(false, "없는 레벨에 대한 데이터를 요청했따/ 레벨 : " + level);
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
            Debug.Assert(false, "없는 BuffName을 요청했습니다");
            Debug.Break();
            return -1;
        }

        return _buffNames[buffName];
    }

    public BuffAsset GetBuff(int buffKey)
    {
        if (_buffs.ContainsKey(buffKey) == false)
        {
            Debug.Assert(false, "없는 BuffKey을 요청했습니다");
            Debug.Break();
            return null;
        }

        return _buffs[buffKey];
    }

    public BuffAsset GetBuff(string buffName)
    {
        if (_buffNames.ContainsKey(buffName) == false)
        {
            Debug.Assert(false, "없는 BuffName을 요청했습니다");
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
                Debug.Assert(false, "중복되는 이름이 있습니다" + buff._BuffName);
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
            Debug.Assert(false, "없는 버프를 요청했따");
            Debug.Break();
        }

        return _buffActions[type].CopyMe();
    }
    private void AddBuffAction(BuffAction type, BuffActionClass action)
    {
        if (_buffActions.ContainsKey(type) == true)
        {
            Debug.Assert(false, "해당 타입이 이미 있습니다");
            Debug.Break();
        }

        _buffActions.Add(type, action);
    }
    private void ReadyBuffAction()
    {
        //북두 반격 버프
        {
            AddBuffAction(BuffAction.BeidouElementalArt, new BuffActionClass_BeidouDamageACC());
        }

        //북두 반격 데미지방출버프
        {
            AddBuffAction(BuffAction.BeidouElementalArtDamageReturn, new BuffActionClass_BeidouElementalArtReturn());
        }

        //라이오 위빙 버프
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
        public List<IEnumerator> _funcs = new List<IEnumerator>();
        public abstract void BuffAction(DamageDesc damage, bool weakPoint, CharacterScript attacker, CharacterScript victim);
        public abstract BuffActionClass CopyMe();
        public Action<DamageDesc, bool, CharacterScript, CharacterScript> GetAction() { return BuffAction; }
    }

    public class BuffActionClass_BeidouDamageACC : BuffActionClass
    {
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

            Debug.Log("북두 뎀 누산 버프 || 누산된 데미지 : " + _damageACC);

            if (afterACCLvl <= 2 &&
                beforeACCLvl != afterACCLvl)
            {
                Debug.Log("북두 뎀 누산 버프 || 레벨 증가 : " + afterACCLvl);

                ownerCharacterScript.GCST<StateContoller>().OverrideAnimationClip(afterACCLvl - 1);
            }

        }
        public override BuffActionClass CopyMe()
        {
            return new BuffActionClass_BeidouDamageACC();
        }
    }

    public class BuffActionClass_BeidouElementalArtReturn : BuffActionClass
    {
        public override void BuffAction(DamageDesc damage, bool weakPoint, CharacterScript attacker, CharacterScript victim)
        {
            StatScript callerStatScript = attacker.GetComponent<StatScript>();
            int beidouDamageAccBuffKey = Instance.GetBuffKey("북두원소스킬데미지누적버프");
            BuffActionClass_BeidouDamageACC beidouDamageAccBuff = callerStatScript._DelegateHistory[beidouDamageAccBuffKey] as BuffActionClass_BeidouDamageACC;

            damage._damage += beidouDamageAccBuff._damageACC;

            Debug.Log("데미지 방출 || 누산돼있던 데미지 : " + beidouDamageAccBuff._damageACC + " 데미지 합 : " + damage._damage);

            callerStatScript.RemoveBuff(Instance.GetBuff("북두원소스킬데미지누적버프"), StatScript.BuffTimingType.State);
        }

        public override BuffActionClass CopyMe() {return new BuffActionClass_BeidouElementalArtReturn();}
    }

    public class BuffActionClass_WrioWeaving : BuffActionClass
    {
        bool _isTriggerd = false;

        public override void BuffAction(DamageDesc damage, bool weakPoint, CharacterScript attacker, CharacterScript victim)
        {
            if (_isTriggerd == true)
            {
                return;
            }

            _isTriggerd = true;

            StatScript owner_victimStatScript = victim.GCST<StatScript>();

            Debug.Log("위빙 성공");


            //이건 델리게이터다
            //호출즉시 해야할것들

            //1. 타임 슬립
            //StartCoroutine(TimeSlowCoroutine());


            //2. 공버프 부여, 공속 버프 부여
            //owner_victimStatScript.ApplyBuff(0, StatScript.BuffTimingType.Normal);
        }

        private IEnumerator TimeSlowCoroutine()
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

        public override BuffActionClass CopyMe() {return new BuffActionClass_WrioWeaving();}
    }
}
