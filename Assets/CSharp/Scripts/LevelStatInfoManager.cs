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
    /*-------------------------------------------------------
    |NOTI| 이거 뭐하는 변수냐? = 버프 적용, 해제할때 할 일들
    -------------------------------------------------------*/

    public enum BuffApplyFSM 
    {
        End,
    }





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



    public List<BuffAssetBase> _buffs_Init = new List<BuffAssetBase>();
    Dictionary<int, BuffAssetBase> _buffs = new Dictionary<int, BuffAssetBase>();
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

    public BuffAssetBase GetBuff(int buffKey)
    {
        if (_buffs.ContainsKey(buffKey) == false)
        {
            Debug.Assert(false, "없는 BuffKey을 요청했습니다");
            Debug.Break();
            return null;
        }

        return _buffs[buffKey];
    }

    public BuffAssetBase GetBuff(string buffName)
    {
        if (_buffNames.ContainsKey(buffName) == false)
        {
            Debug.Assert(false, "없는 BuffName을 요청했습니다" + buffName);
            Debug.Break();
            return null;
        }

        return _buffs[GetBuffKey(buffName)];
    }


    private void ReadyBuffs()
    {
        int key = 0;
        foreach (BuffAssetBase buff in _buffs_Init)
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
        DamageReflection_SpikeArmor,
        AatroxSword_DrainHP,
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

        //라이오 위빙 버프_가시갑옷
        {
            AddBuffAction(BuffAction.DamageReflection_SpikeArmor, new BuffActionClass_DamageReflectionSpikeArmor());
        }

        //아트록스 흡혈
        {
            AddBuffAction(BuffAction.AatroxSword_DrainHP, new BuffActionClass_AatroxDrainBuff());
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

        public int _count = 0;

        protected int _myKey = 0;
        protected DamagingProcessDelegateType _myTiming = DamagingProcessDelegateType.End;

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
            StatScript ownerStatScript = victim.GCST<StatScript>();

            //이펙트 생성
            {
                Vector3 ChestPosition = victim.GCST<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBoneTransform(HumanBodyBones.Chest).transform.position;
                EffectManager.Instance.CreateEffect("HitSparkBeidouDamageACC", Vector3.up, ChestPosition);
            }

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

            Debug.Log("북두 뎀 누산 버프 || 누산된 데미지 : " + _damageACC);

            if (beforeACCLvl != afterACCLvl)
            {
                Debug.Log("북두 뎀 누산 버프 || 레벨 증가 : " + afterACCLvl);
                victim.GCST<StateContoller>().OverrideAnimationClip(afterACCLvl - 1);
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

            BuffAssetBase beidouDamageAccBuff = Instance.GetBuff(Instance.GetBuffKey("BeidouEle_DamageACC"));

            if (beidouDamageAccBuff == null)
            {
                return;
            }

            RuntimeBuffAsset beidouDamageAccRuntimeBuff = callerStatScript.GetRuntimeBuffAsset(beidouDamageAccBuff);

            BuffActionClass_BeidouDamageACC currDamageAcc = (BuffActionClass_BeidouDamageACC)beidouDamageAccRuntimeBuff._buffActions[LevelStatInfoManager.BuffAction.BeidouElementalArt];

            damage._damage += (currDamageAcc._damageACC / 2.0f);
            damage._damagePower = currDamageAcc._damageACC * 3.0f;

            Debug.Log("데미지 방출 || 누산돼있던 데미지 : " + currDamageAcc._damageACC + " 데미지 합 : " + damage._damage);
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

            owner_victimStatScript.ApplyBuff(Instance.GetBuff("WrioEle_AttackSpeed"), 1);
            owner_victimStatScript.ApplyBuff(Instance.GetBuff("WrioEle_DamageReflect"), 1);
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
    public class BuffActionClass_DamageReflectionSpikeArmor : BuffActionClass
    {
        public BuffActionClass_DamageReflectionSpikeArmor() { }
        public BuffActionClass_DamageReflectionSpikeArmor(int buffKey, DamagingProcessDelegateType timing) : base(buffKey, timing) {}

        public override void BuffAction(DamageDesc damage, bool weakPoint, CharacterScript attacker, CharacterScript victim)
        {
            Debug.Log("대미지 반사버프");
        }

        public override BuffActionClass CopyMe() { return new BuffActionClass_DamageReflectionSpikeArmor(_myKey, _myTiming); }
    }
    public class BuffActionClass_AatroxDrainBuff : BuffActionClass
    {
        public BuffActionClass_AatroxDrainBuff() { }
        public BuffActionClass_AatroxDrainBuff(int buffKey, DamagingProcessDelegateType timing) : base(buffKey, timing) { }

        public override void BuffAction(DamageDesc damage, bool weakPoint, CharacterScript attacker, CharacterScript victim)
        {
            StatScript attackerStatScript = attacker.GCST<StatScript>();

            //이펙트 생성
            {
                Vector3 ChestPosition = victim.GCST<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBoneTransform(HumanBodyBones.Chest).transform.position;
                EffectManager.Instance.CreateEffect("HitSparkCritical", Vector3.up, ChestPosition);
            }

            int currHP = attackerStatScript.GetActiveStat(LevelStatAsset.ActiveStat.Hp);

            attackerStatScript.ChangeActiveStat(LevelStatAsset.ActiveStat.Hp, (int)(damage._damage * 2.0f));
        }

        public override BuffActionClass CopyMe() { return new BuffActionClass_AatroxDrainBuff(_myKey, _myTiming); }
    }
}
