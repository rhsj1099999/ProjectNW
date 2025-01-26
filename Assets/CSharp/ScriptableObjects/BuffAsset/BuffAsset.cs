using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "BuffAsset", menuName = "Scriptable Object/Create_BuffAsset", order = (int)MyUtil.CustomToolOrder.CreateBuffs)]
public class BuffAsset : ScriptableObject
{
    [Serializable]
    public class BuffApplyWork
    {
        public enum BuffApplyType
        {
            Plus,
            Multiply,
            Percentage,
            Set,
            None,
        }


        public enum BuffNestType
        {
            IgnoreNothing,          //아무것도 안한다. 중첩가능하다.
            CompareAndSelectHigh,   //앞전의 버프들과 비교하고 높다면 적용한다.
            OnlyMe,                 //뭐가 들어왔든 이것만 적용한다 (기존에 있던것들도 다 무시한다)
        }

        public StatScript.Stats _targetVar = StatScript.Stats.Hp;
        public BuffApplyType _buffApplyType = BuffApplyType.Plus;
        public BuffNestType buffNestType = BuffNestType.IgnoreNothing;

        public float _amount = 0.0f;
        public float _duration = 0.0f;


    }





    [SerializeField] private string _buffName = "";
    public string _BuffName => _buffName;

    //[SerializeField] private int _buffKey = 0;
    //public int _BuffKey => _buffKey;
    public int _buffKey = 0;

    [SerializeField] private List<BuffApplyWork> _buffWorks = new List<BuffApplyWork>();
    public List<BuffApplyWork> _BuffWorks => _buffWorks;

}