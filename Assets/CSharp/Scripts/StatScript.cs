using System.Collections.Generic;
using UnityEngine;
using static BuffAsset;
using static BuffAsset.BuffApplyWork;

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

        //스텟 이외에도 적용시킬 수 있습니다. 다만 이곳에 해당 타입을 명시해야합니다.
        //EX_갑옷 레벨, 갑옷 방어도 등등...

        End,
    }


    public class RunningBuff
    {
        public BuffAsset _buffAsset = null;
        public float _timeACC = 0.0f;
    }

    
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

    private Dictionary<Stats, Dictionary<BuffApplyType, List<BuffAsset>>> _allBuffs = new Dictionary<Stats, Dictionary<BuffApplyType, List<BuffAsset>>>();
    private List<BuffAsset> _stateBuffs = new List<BuffAsset>();

    private Dictionary<Stats, object> _statVars = new Dictionary<Stats, object>();
    private Dictionary<Stats, System.Type> _statTypes = new Dictionary<Stats, System.Type>();

    private Dictionary<Stats, List<RunningBuff>> _currApplyingBuffs = new Dictionary<Stats, List<RunningBuff>>();
    private Dictionary<Stats, List<RunningBuff>> _currNotApplyingBuffs = new Dictionary<Stats, List<RunningBuff>>();

    List<object> _stats = new List<object>();

    public void StateChanged(StateAsset nextState)
    {
        /*-------------------------------------------------------------------
        상태에 의해 걸렸던 버프들을 모두 취소한다.
        그럼에도 불구하고 현재 걸린 버프에 의해서 해당 이로운/해로운 효과가 남아있다면
        받을 수 있어야 한다
        -------------------------------------------------------------------*/

    }






    public void ApplyBuff(BuffAsset buff)
    {
        List<BuffApplyWork> buffWorks = buff._BuffWorks;

        foreach (var buffWork in buffWorks)
        {
            ReadAndApply(buffWork);
        }
    }



    public void ApplyStateBuff(BuffAsset buff)
    {
        List<BuffApplyWork> buffWorks = buff._BuffWorks;

        foreach (var buffWork in buffWorks)
        {
            ReadAndApply(buffWork);
        }
    }





    private void ReadAndApply(BuffAsset.BuffApplyWork buffWork)
    {
        BuffApplyType applyType = buffWork._buffApplyType;

        

        switch (applyType)
        {
            //case BuffApplyType.Plus:
            //    break;
            //case BuffApplyType.Multiply:
            //    break;
            //case BuffApplyType.Percentage:
            //    break;
            //case BuffApplyType.Set:
            //    break;
            //default:
            //    Debug.Assert(false, "버프 적용타입이 대응되지 않습니다" + applyType);
            //    break;
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
