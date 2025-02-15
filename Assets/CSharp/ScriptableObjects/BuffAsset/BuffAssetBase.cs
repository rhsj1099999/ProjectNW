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
    //    //0. Set (������ ������Ų��)
    //    Set, //�� ���� ������ ���� ������ ���õȴ�

    //    //1. ����� ����
    //    Plus,
    //    Minus,

    //    //2. �ۼ������� ����
    //    PercentagePlus,
    //    PercentageMinus,

    //    //3. ������
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
    //�ִ� ��ø ���� ����
    [SerializeField] private int _maxCount = 1;
    public int _MaxCount => _maxCount;

    //���� ���� ��ø�� ���ӽð� �ʱ�ȭ ����(duration�� ������ ��)
    [SerializeField] private bool _refresh = true;
    public bool _Refresh => _refresh;

    //���ӽð� ����� �ѹ��� ���� ����(duration�� ������ ��)
    [SerializeField] private bool _durationExpireOnce = true;
    public bool _DurationExpireOnce => _durationExpireOnce;

    //�̰� �������� �ϴµ�...
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