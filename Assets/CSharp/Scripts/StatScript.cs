using System.Collections.Generic;
using UnityEngine;
using static BuffAsset;
using static BuffAsset.BuffApplyWork;

public class StatScript : GameCharacterSubScript
{
    public int _level = 1;

    //�� ����ĳ���ʹ� �̷� ���ȵ��� �����ؿ�
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

        //���� �̿ܿ��� �����ų �� �ֽ��ϴ�. �ٸ� �̰��� �ش� Ÿ���� ����ؾ��մϴ�.
        //EX_���� ����, ���� �� ���...

        End,
    }


    public class RunningBuff
    {
        public BuffAsset _buffAsset = null;
        public float _timeACC = 0.0f;
    }

    
    //ü��
    public int _maxHP = 100;
    public int _hp = 100;       

    //����
    public int _maxStamina = 100;
    public int _stamina = 100;

    //Mp
    public int _maxMp = 100;
    public int _mp = 100;

    //Sp
    public int _maxSp = 100;
    public int _sp = 100;


    public int _roughness = 1;  //���ε�(�ڼ��� �����Ϸ��� ��)

    public int _strength = 1;

    public bool _invincible = false; //�����Դϱ�

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
        ���¿� ���� �ɷȴ� �������� ��� ����Ѵ�.
        �׷����� �ұ��ϰ� ���� �ɸ� ������ ���ؼ� �ش� �̷ο�/�طο� ȿ���� �����ִٸ�
        ���� �� �־�� �Ѵ�
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
            //    Debug.Assert(false, "���� ����Ÿ���� �������� �ʽ��ϴ�" + applyType);
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
