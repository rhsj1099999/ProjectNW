using System;
using System.Collections.Generic;
using UnityEngine;
using static BuffAsset_ActiveStat;

public abstract class BuffAsset_StatBase : BuffAssetBase
{
    public enum BuffApplyType
    {
        //0. Set (������ ������Ų��)
        Set, //�� ���� ������ ���� ������ ���õȴ�

        //1. ����� ����
        Plus,
        Minus,

        //2. �ۼ������� ����
        PercentagePlus,
        PercentageMinus,

        //3. ������
        Multiply,
        Devide,

        End = 2048,
    }

    [Serializable]
    public class ApplyDescBase
    {
        public BuffApplyType _applyType = BuffApplyType.End;
        public int _amout = 0;
    }
}