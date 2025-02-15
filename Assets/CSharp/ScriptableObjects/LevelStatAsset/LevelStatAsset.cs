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

        PosturePercent, //자세 유지 = 높을수록 위험함



        End = 2048, //4
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
        IsInvincible,
        IsParry,
        IsGuard,
        IsInvincible_HP,        //피가 깎이진 않지만, 자세가 무너질 수 있다.
        IsInvincible_Stance,    //자세가 무너지진 않지만, 피가 깎인다.
        AttackSpeedPercentage,

        HPRegen,
        StaminaRegen,
        MPRegen,
        SPRegen,

        MoveSpeed,


        PostruePercentPhase1, //자세 1차 무너짐
        PostureRecovery,

        End = 2048,
    }




    [Serializable]
    public class ActiveStatDesc
    {
        public ActiveStatDesc() { }
        public ActiveStatDesc(ActiveStatDesc other)
        {
            _hp = other._hp;
            _stamina = other._stamina;
            _mp = other._mp;
            _sp = other._sp;
            _posturePercentage = other._posturePercentage;

            InitDict();
        }

        public void InitDict()
        {
            _ActiveStats.Clear();
            _ActiveStats.Add(ActiveStat.Hp, _hp);
            _ActiveStats.Add(ActiveStat.Stamina, _stamina);
            _ActiveStats.Add(ActiveStat.Mp, _mp);
            _ActiveStats.Add(ActiveStat.Sp, _sp);
            _ActiveStats.Add(ActiveStat.PosturePercent, _posturePercentage);
        }

        private Dictionary<ActiveStat, int> _activeStats = new Dictionary<ActiveStat, int>();
        public Dictionary<ActiveStat, int> _ActiveStats => _activeStats;

        [SerializeField] private int _hp = 100;
        [SerializeField] private int _stamina = 100;
        [SerializeField] private int _mp = 100;
        [SerializeField] private int _sp = 100;
        [SerializeField] private int _posturePercentage = 100;
    }


    [Serializable]
    public class PassiveStatDesc
    {
        public PassiveStatDesc() { }
        public PassiveStatDesc(PassiveStatDesc other) 
        {
            _maxHP = other._maxHP;
            _maxStamina = other._maxStamina;
            _maxMp = other._maxMp;
            _maxSp = other._maxSp;
            _roughness = other._roughness;
            _strength = other._strength;
            _isInvincible = other._isInvincible;
            _isParry = other._isParry;
            _isGuard = other._isGuard;
            _isInvincible_HP = other._isInvincible_HP;
            _isInvincible_Stance = other._isInvincible_Stance;
            _attackSpeedPercentage = other._attackSpeedPercentage;

            _hpRegen = other._hpRegen;
            _staminaRegen = other._staminaRegen;
            _mpRegen = other._mpRegen;
            _spRegen = other._spRegen;

            _moveSpeed = other._moveSpeed;

            _posturePercentagePhase1 = other._posturePercentagePhase1;
            _postureRecovery = other._postureRecovery;


            InitDict();
        }

        public void InitDict() 
        {
            _PassiveStats.Clear();
            _PassiveStats.Add(PassiveStat.MaxHP, _maxHP);
            _PassiveStats.Add(PassiveStat.MaxStamina, _maxStamina);
            _PassiveStats.Add(PassiveStat.MaxMp, _maxMp);
            _PassiveStats.Add(PassiveStat.MaxSp, _maxSp);
            _PassiveStats.Add(PassiveStat.Roughness, _roughness);
            _PassiveStats.Add(PassiveStat.Strength, _strength);

            _PassiveStats.Add(PassiveStat.IsInvincible, _isInvincible);
            _PassiveStats.Add(PassiveStat.IsParry, _isParry);
            _PassiveStats.Add(PassiveStat.IsGuard, _isGuard);
            _PassiveStats.Add(PassiveStat.IsInvincible_HP, _isInvincible_HP);
            _PassiveStats.Add(PassiveStat.IsInvincible_Stance, _isInvincible_Stance);
            _PassiveStats.Add(PassiveStat.AttackSpeedPercentage, _attackSpeedPercentage);

            _PassiveStats.Add(PassiveStat.HPRegen, _hpRegen);
            _PassiveStats.Add(PassiveStat.StaminaRegen, _staminaRegen);
            _PassiveStats.Add(PassiveStat.MPRegen, _mpRegen);
            _PassiveStats.Add(PassiveStat.SPRegen, _spRegen);

            _PassiveStats.Add(PassiveStat.MoveSpeed, _moveSpeed);

            _PassiveStats.Add(PassiveStat.PostruePercentPhase1, _posturePercentagePhase1);
            _PassiveStats.Add(PassiveStat.PostureRecovery, _postureRecovery);
        }

        private Dictionary<PassiveStat, int> _passiveStats = new Dictionary<PassiveStat, int>();
        public Dictionary<PassiveStat, int> _PassiveStats => _passiveStats;



        [SerializeField] private int _maxHP = 100;
        [SerializeField] private int _maxStamina = 100;
        [SerializeField] private int _maxMp = 100;
        [SerializeField] private int _maxSp = 100;
        [SerializeField] private int _roughness = 1;  //강인도(자세를 유지하려는 힘)
        [SerializeField] private int _strength = 1;
        [SerializeField] private int _isInvincible = 0;
        [SerializeField] private int _isParry = 0;
        [SerializeField] private int _isGuard = 0;
        [SerializeField] private int _isInvincible_HP = 0;
        [SerializeField] private int _isInvincible_Stance = 0;
        [SerializeField] private int _attackSpeedPercentage = 100;


        [SerializeField] private int _hpRegen = 1;
        [SerializeField] private int _staminaRegen = 1;
        [SerializeField] private int _mpRegen = 1;
        [SerializeField] private int _spRegen = 1;

        [SerializeField] private int _moveSpeed = 5;

        [SerializeField] private int _posturePercentagePhase1 = 60;

        [SerializeField] private int _postureRecovery = 10;
    }


    [SerializeField] private CharacterType _characterType = CharacterType.Player;
    public CharacterType _CharacterType => _characterType;

    [SerializeField] private int _level = 0;
    public int _Level => _level;

    [SerializeField] private ActiveStatDesc _activeStatDesc = new ActiveStatDesc();
    public ActiveStatDesc _ActiveStatDesc => _activeStatDesc;

    [SerializeField] private PassiveStatDesc _passiveStatDesc = new PassiveStatDesc();
    public PassiveStatDesc _PassiveStatDesc => _passiveStatDesc;

    public void PartailAwake_InitDict()
    {
        _activeStatDesc.InitDict();
        _passiveStatDesc.InitDict();
    }
}