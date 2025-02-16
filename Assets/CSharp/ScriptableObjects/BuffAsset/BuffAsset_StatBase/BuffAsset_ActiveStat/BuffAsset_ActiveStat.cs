using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static BuffAsset_StatBase;
using static LevelStatAsset;
using static LevelStatInfoManager;
using static StatScript;

[CreateAssetMenu(fileName = "BuffAsset_ActiveStat", menuName = "Scriptable Object/Create_BuffAsset_ActiveStat", order = (int)MyUtil.CustomToolOrder.CreateBuffs)]
public class BuffAsset_ActiveStat : BuffAsset_StatBase
{
    [Serializable]
    public class ApplyDesc_ActiveStat : ApplyDescBase
    {
        public ActiveStat _targetStat = ActiveStat.End;
    }


    [SerializeField] private List<ApplyDesc_ActiveStat> _buffList = new List<ApplyDesc_ActiveStat>();
    public List<ApplyDesc_ActiveStat> _BuffList => _buffList;


    public override void DoWork(StatScript usingThisBuffStatScript, RuntimeBuffAsset runtimeBuffAsset, int deltaCount)
    {
        foreach (ApplyDesc_ActiveStat buffWork in _buffList)
        {
            ActiveStat activeStatType = buffWork._targetStat;

            if (activeStatType != ActiveStat.End)
            {
                usingThisBuffStatScript.ChangeActiveStat(activeStatType, buffWork._amout * deltaCount);
            }
        }
    }
}