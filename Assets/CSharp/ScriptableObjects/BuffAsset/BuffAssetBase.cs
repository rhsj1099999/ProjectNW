using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static LevelStatAsset;
using static LevelStatInfoManager;
using static StatScript;


public abstract class BuffAssetBase : ScriptableObject
{
    //public enum BuffApplyType
    //{
    //    //0. Set (강제로 고정시킨다)
    //    Set, //이 값이 있으면 이후 값들이 무시된다

    //    //1. 상수값 증가
    //    Plus,
    //    Minus,

    //    //2. 퍼센테이지 증가
    //    PercentagePlus,
    //    PercentageMinus,

    //    //3. 곱증가
    //    Multiply,
    //    Devide,

    //    End = 2048,
    //}

    //[Serializable]
    //public class BuffApplyWork_Skill
    //{
    //    public DamagingProcessDelegateType _delegateTiming = DamagingProcessDelegateType.End;
    //    public BuffAction _buffActionType = BuffAction.None;
    //}

    //[Serializable]
    //public class BuffApplyWork_Passive
    //{
    //    public PassiveStat _buffTarget_Passive = PassiveStat.End;
    //    public BuffApplyType _applyType = BuffApplyType.Plus;
    //    public float _amount = 0.0f;
    //}

    //[Serializable]
    //public class BuffApplyWork_Active
    //{
    //    public ActiveStat _buffTarget_Active = ActiveStat.End;
    //    public BuffApplyType _applyType = BuffApplyType.Plus;
    //    public float _amount = 0.0f;
    //}

    //[Serializable]
    //public class BuffApplyWork_Regen
    //{
    //    public RegenStat _buffTarget_Regen = RegenStat.End;
    //    public BuffApplyType _applyType = BuffApplyType.Plus;
    //    public float _amount = 0.0f;
    //}




    //public enum BuffType
    //{
    //    Skill,
    //    PassiveStat,
    //    ActiveStat,
    //    RegenStat,
    //    End,
    //}







    //[SerializeField] private int _buffKey = 0;
    //public int _BuffKey => _buffKey;
    public int _buffKey = 0;


    [SerializeField] private string _buffName = "";
    public string _BuffName => _buffName;






    //--------------------------------------------------------------
    //--------------------------------------------------------------
    //
    //
    //최대 중첩 가능 개수
    [SerializeField] private int _maxCount = 1;
    public int _MaxCount => _maxCount;

    //동일 버프 중첩시 지속시간 초기화 여부(duration이 존재할 때)
    [SerializeField] private bool _refresh = true;
    public bool _Refresh => _refresh;

    //지속시간 만료시 한번에 삭제 여부(duration이 존재할 때)
    [SerializeField] private bool _durationExpireOnce = true;
    public bool _DurationExpireOnce => _durationExpireOnce;

    //이건 없어져야 하는데...
    [SerializeField] private bool _specialAction_OnlyOne = false;
    public bool _SpecialAction_OnlyOne => _specialAction_OnlyOne;
    //
    //
    //--------------------------------------------------------------
    //--------------------------------------------------------------


    [SerializeField] private bool _isTemporary = false;
    public bool _IsTemporary => _isTemporary;

    [SerializeField] private bool _isDebuff = false;
    public bool _IsDebuff => _isDebuff;

    [SerializeField] private float _duration = 100.0f;
    public float _Duration => _duration;

    [SerializeField] private Sprite _buffUIImage = null;
    public Sprite _BuffUIImage => _buffUIImage;

    public abstract void DoWork(StatScript usingThisBuffStatScript);
}