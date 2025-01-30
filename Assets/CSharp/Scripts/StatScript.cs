using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Build.Content;
using UnityEngine;
using static BuffAsset;
using static BuffAsset.BuffApplyWork;
using static LevelStatAsset;

public class StatScript : GameCharacterSubScript
{
    public class RunningBuff
    {
        public BuffAsset _buffAsset = null;
        public float _timeACC = 0.0f;
    }


    private int _currLevel = 1;


    /*---------------------------------------------------
    |NOTI| �⺻�� ������ �Ŵ����� ������ ���� �ٷ� �� �� �ִ�
    ---------------------------------------------------*/
    private ActiveStatDesc _currActiveStat = null; //��Ÿ�� �����Դϴ�.
    private PassiveStatDesc _currPassiveStat = null; //��Ÿ�� �����Դϴ�.

    private Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>> _passiveStatDeltaEquation = new Dictionary<PassiveStat, SortedDictionary<BuffApplyType, int>>();
    private Dictionary<PassiveStat, int> _passiveStatDeltaCalculated = new Dictionary<PassiveStat, int>();

    private HashSet<BuffAsset> _stateBuffs = new HashSet<BuffAsset>();
    private HashSet<BuffAsset> _stateDeBuffs = new HashSet<BuffAsset>();

    private HashSet<BuffAsset> _buffs = new HashSet<BuffAsset>();
    private HashSet<BuffAsset> _deBuffs = new HashSet<BuffAsset>();


    public void StateChanged(StateAsset nextState)
    {
        {
            /*-------------------------------------------------------------------
            ���¿� ���� �ɷȴ� �������� ��� ����Ѵ�.
            �׷����� �ұ��ϰ� ���� �ɸ� ������ ���ؼ� �ش� �̷ο�/�طο� ȿ���� �����ִٸ�
            ���� �� �־�� �Ѵ�
            -------------------------------------------------------------------*/
        }

        foreach (BuffAsset stateBuff in _stateBuffs) 
        {

        }
        _stateBuffs.Clear();
    }




    public int GetActiveStat(ActiveStat type)
    {
        return _currActiveStat._ActiveStats[type];
    }

    public int GetPassiveStat(PassiveStat type)
    {
        return _currPassiveStat._PassiveStats[type];
    }



    public void ChangePassiveStat(PassiveStat type, int amount)
    {
        //1. �⺻ ������ �����ͺ���
        int currLevelPassiveStat = LevelStatInfoManager.Instance.GetLevelStatAsset(_currLevel)._PassiveStatDesc._PassiveStats[type];

        //2. amount �� �����Ѵ� -> �׷� ������ �⺻�����̴�.
        currLevelPassiveStat += amount;

        //3. ������ �⺻ ���ݿ�, DeltaCalculated�� �����Ѵ�.
        currLevelPassiveStat +=_passiveStatDeltaCalculated[type];

        _currPassiveStat._PassiveStats[type] = currLevelPassiveStat;
    }


    public void ChangeActiveStat(ActiveStat type, int amount)
    {
        int nextVal = (_currActiveStat._ActiveStats[type] + amount);

        //���� Passive Stat�� ���� ����� �����Ѵ�
        {
            //���� ��� ü�¿� amount ��ŭ �÷�����, max HP��ŭ�� �ȳѾ��
        }

        _currActiveStat._ActiveStats[type] = nextVal;
    }





    public void ApplyBuff(BuffAsset buff, bool isState = false)
    {
        HashSet<BuffAsset> target = null;

        if (isState == true)
        {
            target = (buff._IsDebuff == true)
            ? _stateDeBuffs
            : _stateBuffs;
        }
        else
        {
            target = (buff._IsDebuff == true)
            ? _deBuffs
            : _buffs;
        }


        if (target.Contains(buff) == true)
        {
            /*-------------------------------------------
            |TODO| �轺 ���ӹ����� ��ø�ǰ� �ϳ��� �������
            -------------------------------------------*/
            Debug.Log("���� ������ ��ø�� �ȵſ�");
            return;
        }

        List<BuffApplyWork> buffWorks = buff._BuffWorks;

        foreach (var buffWork in buffWorks)
        {
            ReadAndApply(buffWork, false);
        }

        target.Add(buff);
    }




    public void RemoveBuff(BuffAsset buff, bool isState = false)
    {
        HashSet<BuffAsset> target = null;

        if (isState == true)
        {
            target = (buff._IsDebuff == true)
            ? _stateDeBuffs
            : _stateBuffs;
        }
        else
        {
            target = (buff._IsDebuff == true)
            ? _deBuffs
            : _buffs;
        }


        if (target.Contains(buff) == false)
        {
            Debug.Log("�̹� ��ҵƽ��ϴ�");
            return;
        }

        List<BuffApplyWork> buffWorks = buff._BuffWorks;

        foreach (var buffWork in buffWorks)
        {
            ReadAndApply(buffWork, true);
        }

        target.Remove(buff);
    }





    private void ReadAndApply(BuffApplyWork buffWork, bool isDeApply)
    {
        BuffApplyType applyType = buffWork._buffApplyType;

        switch (applyType)
        {
            case BuffApplyType.Plus:
                break;
            case BuffApplyType.Minus:
                break;


            case BuffApplyType.PercentagePlus:
                break;
            case BuffApplyType.PercentageMinus:
                break;


            case BuffApplyType.Multiply:
                break;
            case BuffApplyType.Devide:
                break;


            case BuffApplyType.Set:
                break;


            default:
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

        //----------------------------
        //���� ����
        //---------------------------
        // 1. ���� ������ ���� ���̺� ���� �������� ����ϴ�.

        LevelStatAsset statAsset = LevelStatInfoManager.Instance.GetLevelStatAsset(_currLevel);

        _currActiveStat = new ActiveStatDesc(statAsset._ActiveStatDesc);
        _currPassiveStat = new PassiveStatDesc(statAsset._PassiveStatDesc);
    }

    public override void SubScriptStart() 
    {

    }
}
