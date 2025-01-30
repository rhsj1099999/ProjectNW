using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using static LevelStatAsset;
using static UnityEditor.VersionControl.Asset;

[CreateAssetMenu(fileName = "LevelStatAsset", menuName = "Scriptable Object/Create_LevelStatAsset", order = (int)MyUtil.CustomToolOrder.CreateBuffs)]
public class LevelStatAsset : ScriptableObject
{
    //내 게임캐릭터는 이런 스탯들이 존재해요

    //-> 자꾸 '소모' 되는 것들
    public enum ActiveStat
    {
        Hp,
        Stamina,
        Mp,
        Sp,
    }

    //-> '계산에 사용' 되는 것들
    public enum PassiveStat
    {
        MaxHP,
        MaxStamina,
        MaxMp,
        MaxSp,
        Roughness,
        Strength,
        Invincible,
    }




    [Serializable]
    public class ActiveStatDesc
    {
        public ActiveStatDesc() { }
        public ActiveStatDesc(ActiveStatDesc other)
        {
            _level = other._level;
            _hp = other._hp;
            _stamina = other._stamina;
            _mp = other._mp;
            _sp = other._sp;

            InitDict();
        }

        public void InitDict()
        {
            _ActiveStats.Clear();
            _ActiveStats.Add(ActiveStat.Hp, _Hp);
            _ActiveStats.Add(ActiveStat.Stamina, _Stamina);
            _ActiveStats.Add(ActiveStat.Mp, _Mp);
            _ActiveStats.Add(ActiveStat.Sp, _Sp);
        }

        private Dictionary<ActiveStat, int> _activeStats = new Dictionary<ActiveStat, int>();
        public Dictionary<ActiveStat, int> _ActiveStats => _activeStats;
        
        [SerializeField] private int _level = 0;
        public int _Level => _level;

        [SerializeField] private int _hp = 100;
        public int _Hp => _hp;

        [SerializeField] private int _stamina = 100;
        public int _Stamina => _stamina;

        [SerializeField] private int _mp = 100;
        public int _Mp => _mp;

        [SerializeField] private int _sp = 100;
        public int _Sp => _sp;
    }


    [Serializable]
    public class PassiveStatDesc
    {
        public PassiveStatDesc() { }
        public PassiveStatDesc(PassiveStatDesc other) 
        {
            _level = other._level;
            _maxHP = other._maxHP;
            _maxStamina = other._maxStamina;
            _maxMp = other._maxMp;
            _maxSp = other._maxSp;
            _roughness = other._roughness;
            _strength = other._strength;
            _invincible = other._invincible;

            InitDict();
        }

        public void InitDict() 
        {
            _PassiveStats.Clear();
            _PassiveStats.Add(PassiveStat.MaxHP, _MaxHP);
            _PassiveStats.Add(PassiveStat.MaxStamina, _MaxStamina);
            _PassiveStats.Add(PassiveStat.MaxMp, _MaxMp);
            _PassiveStats.Add(PassiveStat.MaxSp, _MaxSp);
            _PassiveStats.Add(PassiveStat.Roughness, _Roughness);
            _PassiveStats.Add(PassiveStat.Strength, _Strength);
            _PassiveStats.Add(PassiveStat.Invincible, _Invincible);
        }

        private Dictionary<PassiveStat, int> _passiveStats = new Dictionary<PassiveStat, int>();
        public Dictionary<PassiveStat, int> _PassiveStats => _passiveStats;

        [SerializeField] private int _level = 0;
        public int _Level => _level;

        [SerializeField] private int _maxHP = 100;
        public int _MaxHP => _maxHP;

        [SerializeField] private int _maxStamina = 100;
        public int _MaxStamina => _maxStamina;

        [SerializeField] private int _maxMp = 100;
        public int _MaxMp => _maxMp;

        [SerializeField] private int _maxSp = 100;
        public int _MaxSp => _maxSp;

        [SerializeField] private int _roughness = 1;  //강인도(자세를 유지하려는 힘)
        public int _Roughness => _roughness;  //강인도(자세를 유지하려는 힘)

        [SerializeField] private int _strength = 1;
        public int _Strength => _strength;

        [SerializeField] private int _invincible = 0;
        public int _Invincible => _invincible;
    }

    [SerializeField] private ActiveStatDesc _activeStatDesc = new ActiveStatDesc();
    public ActiveStatDesc _ActiveStatDesc => _activeStatDesc;

    [SerializeField] private PassiveStatDesc _passiveStatDesc = new PassiveStatDesc();
    public PassiveStatDesc _PassiveStatDesc => _passiveStatDesc;

    private void Awake()
    {
        //_activeStatDesc._ActiveStats.Clear();
        //_activeStatDesc._ActiveStats.Add(ActiveStat.Hp,         _activeStatDesc._Hp);
        //_activeStatDesc._ActiveStats.Add(ActiveStat.Stamina,    _activeStatDesc._Stamina);
        //_activeStatDesc._ActiveStats.Add(ActiveStat.Mp,         _activeStatDesc._Mp);
        //_activeStatDesc._ActiveStats.Add(ActiveStat.Sp,         _activeStatDesc._Sp);

        //_passiveStatDesc._PassiveStats.Clear();
        //_passiveStatDesc._PassiveStats.Add(PassiveStat.MaxHP,       _passiveStatDesc._MaxHP);
        //_passiveStatDesc._PassiveStats.Add(PassiveStat.MaxStamina,  _passiveStatDesc._MaxStamina);
        //_passiveStatDesc._PassiveStats.Add(PassiveStat.MaxMp,       _passiveStatDesc._MaxMp);
        //_passiveStatDesc._PassiveStats.Add(PassiveStat.MaxSp,       _passiveStatDesc._MaxSp);
        //_passiveStatDesc._PassiveStats.Add(PassiveStat.Roughness,   _passiveStatDesc._Roughness);
        //_passiveStatDesc._PassiveStats.Add(PassiveStat.Strength,    _passiveStatDesc._Strength);
        //_passiveStatDesc._PassiveStats.Add(PassiveStat.Invincible,  _passiveStatDesc._Invincible);
    }

}