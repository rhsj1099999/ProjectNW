using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static BuffAsset_StatBase;
using static LevelStatAsset;
using static LevelStatInfoManager;
using static StatScript;

[CreateAssetMenu(fileName = "BuffAsset_PassiveStat", menuName = "Scriptable Object/Create_BuffAsset_PassiveStat", order = (int)MyUtil.CustomToolOrder.CreateBuffs)]
public class BuffAsset_PassiveStat : BuffAsset_StatBase
{
    [Serializable]
    public class ApplyDesc_PassiveStat : ApplyDescBase
    {
        public PassiveStat _targetStat = PassiveStat.End;
    }


    [SerializeField] private List<ApplyDesc_PassiveStat> _buffList = new List<ApplyDesc_PassiveStat>();
    public List<ApplyDesc_PassiveStat> _BuffList => _buffList;


    public override void DoWork(StatScript usingThisBuffStatScript)
    {
        HashSet<PassiveStat> reCachingTargets = new HashSet<PassiveStat>();

        foreach (ApplyDesc_PassiveStat buffWork in _buffList)
        {
            //수치 변화 계산
            reCachingTargets.Add(buffWork._targetStat);
            usingThisBuffStatScript.ReadAndApplyPassiveStatBuff(buffWork);
        }

        usingThisBuffStatScript.ReCacheBuffAmoints(reCachingTargets);
    }
}