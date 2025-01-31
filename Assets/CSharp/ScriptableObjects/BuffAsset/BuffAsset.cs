using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static LevelStatAsset;

[CreateAssetMenu(fileName = "BuffAsset", menuName = "Scriptable Object/Create_BuffAsset", order = (int)MyUtil.CustomToolOrder.CreateBuffs)]
public class BuffAsset : ScriptableObject
{
    /*------------------------------------------------------------
    |NOTI| 동일 이름의 버프린데 레벨만 다르다 => 그래도 다른 버프키다
    ------------------------------------------------------------*/


    [Serializable]
    public class BuffApplyWork
    {
        public enum BuffApplyType
        {
            //0. Set (강제로 고정시킨다)
            Set, //이 값이 있으면 이후 값들이 무시된다

            //1. 상수값 증가
            Plus,
            Minus,

            //2. 퍼센테이지 증가
            PercentagePlus,
            PercentageMinus,

            //3. 곱증가
            Multiply,
            Devide,

            None,
        }

        public PassiveStat _targetType = PassiveStat.MaxHP;
        public BuffApplyType _buffApplyType = BuffApplyType.Plus;
        public float _amount = 0.0f;
    }





    [SerializeField] private string _buffName = "";
    public string _BuffName => _buffName;

    //[SerializeField] private int _buffKey = 0;
    //public int _BuffKey => _buffKey;
    public int _buffKey = 0;

    [SerializeField] private List<BuffApplyWork> _buffWorks = new List<BuffApplyWork>();
    public List<BuffApplyWork> _BuffWorks => _buffWorks;

    [SerializeField] private bool _isDebuff = false;
    public bool _IsDebuff => _isDebuff;

    [SerializeField] private float _duration = 100.0f;
    public float _Duration => _duration;

    [SerializeField] private Image _buffUIImage = null;
    public Image _BuffUIImage => _buffUIImage;
}