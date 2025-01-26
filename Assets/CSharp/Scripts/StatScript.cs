using System.Collections.Generic;
using UnityEngine;

public class StatScript : GameCharacterSubScript
{
    public int _level = 1;

    //내 게임캐릭터는 이런 스탯들이 존재해요
    public enum Stats
    {
        MaxHP,
        Hp,

        MaxStamina,
        Stamina,

        MaxMp,
        Mp,

        MaxSp,
        Sp,

        Roughness,

        Strength,

        Invincible,

        End,
    }


    public class RunningBuff
    {
        public BuffAsset _buffAsset = null;
        public float _timeACC = 0.0f;
    }


    private Dictionary<Stats, object> _statVars = new Dictionary<Stats, object>();
    private Dictionary<Stats, System.Type> _statTypes = new Dictionary<Stats, System.Type>();

    private Dictionary<Stats, List<RunningBuff>> _currApplyingBuffs = new Dictionary<Stats, List<RunningBuff>>();
    private Dictionary<Stats, List<RunningBuff>> _currNotApplyingBuffs = new Dictionary<Stats, List<RunningBuff>>();

    List<object> _stats = new List<object>();

    
    //체력
    public int _maxHP = 100;
    public int _hp = 100;       

    //스테
    public int _maxStamina = 100;
    public int _stamina = 100;

    //Mp
    public int _maxMp = 100;
    public int _mp = 100;

    //Sp
    public int _maxSp = 100;
    public int _sp = 100;


    public int _roughness = 1;  //강인도(자세를 유지하려는 힘)

    public int _strength = 1;

    public bool _invincible = false; //무적입니까






    public void ApplyBuff(BuffAsset buff)
    {
        List<BuffAsset.BuffApplyWork> buffWorks = buff._BuffWorks;

        foreach (var buffWork in buffWorks)
        {
            ReadAndApply(buffWork);
        }
    }


    private void ReadAndApply(BuffAsset.BuffApplyWork buffWork)
    {
        BuffAsset.BuffApplyWork.BuffApplyType applyType = buffWork._buffApplyType;

        

        switch (applyType)
        {
            case BuffAsset.BuffApplyWork.BuffApplyType.Plus:
                break;
            case BuffAsset.BuffApplyWork.BuffApplyType.Multiply:
                break;
            case BuffAsset.BuffApplyWork.BuffApplyType.Percentage:
                break;
            case BuffAsset.BuffApplyWork.BuffApplyType.Set:
                break;
            default:
                Debug.Assert(false, "버프 적용타입이 대응되지 않습니다" + applyType);
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

        _stats.Add(_maxHP);
        _stats.Add(_hp);

        _stats.Add(_maxStamina);
        _stats.Add(_stamina);

        _stats.Add(_maxMp);
        _stats.Add(_mp);

        _stats.Add(_maxSp);
        _stats.Add(_sp);

        _stats.Add(_roughness);
        _stats.Add(_strength);
        _stats.Add(_invincible);
    }

    public override void SubScriptStart() { }
}
