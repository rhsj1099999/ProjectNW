using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static BuffAsset_PassiveStat;
using static BuffAsset_StatBase;
using static LevelStatAsset;
using static LevelStatInfoManager;
using static StatScript;

[CreateAssetMenu(fileName = "BuffAsset_RegenStat", menuName = "Scriptable Object/Create_BuffAsset_RegenStat", order = (int)MyUtil.CustomToolOrder.CreateBuffs)]
public class BuffAsset_RegenStat : BuffAsset_StatBase
{
    [Serializable]
    public class ApplyDesc_RegenStat : ApplyDescBase
    {
        public RegenStat _targetStat = RegenStat.End;
    }


    [SerializeField] private List<ApplyDesc_RegenStat> _buffList = new List<ApplyDesc_RegenStat>();
    public List<ApplyDesc_RegenStat> _BuffList => _buffList;


    public override void DoWork(StatScript usingThisBuffStatScript)
    {
        HashSet<RegenStat> reCachingTargets = new HashSet<RegenStat>();

        foreach (ApplyDesc_RegenStat buffWork in _buffList)
        {
            //수치 변화 계산
            reCachingTargets.Add(buffWork._targetStat);
            usingThisBuffStatScript.ReadAndApplyPassiveStatBuff(buffWork);
        }

        usingThisBuffStatScript.ReCacheBuffAmoints(reCachingTargets);
    }
}