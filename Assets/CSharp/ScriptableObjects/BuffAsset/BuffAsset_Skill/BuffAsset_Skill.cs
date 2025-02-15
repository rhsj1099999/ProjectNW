using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static LevelStatAsset;
using static LevelStatInfoManager;
using static StatScript;

[CreateAssetMenu(fileName = "BuffAsset_Skill", menuName = "Scriptable Object/Create_BuffAsset_Skill", order = (int)MyUtil.CustomToolOrder.CreateBuffs)]
public class BuffAsset_Skill : BuffAssetBase
{
    [Serializable]
    public class BuffApplyWork_Skill
    {
        public DamagingProcessDelegateType _delegateTiming = DamagingProcessDelegateType.End;
        public BuffAction _buffActionType = BuffAction.None;
    }


    [SerializeField] private List<BuffApplyWork_Skill> _buffTargets = new List<BuffApplyWork_Skill>();
    public List<BuffApplyWork_Skill> _BuffTargets => _buffTargets;


    public override void DoWork(StatScript usingThisBuffStatScript)
    {

    }
}