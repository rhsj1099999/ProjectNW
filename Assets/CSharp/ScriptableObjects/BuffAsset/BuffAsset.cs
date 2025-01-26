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
            IgnoreNothing,          //�ƹ��͵� ���Ѵ�. ��ø�����ϴ�.
            CompareAndSelectHigh,   //������ ������� ���ϰ� ���ٸ� �����Ѵ�.
            OnlyMe,                 //���� ���Ե� �̰͸� �����Ѵ� (������ �ִ��͵鵵 �� �����Ѵ�)
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