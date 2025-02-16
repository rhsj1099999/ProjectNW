using System;
using System.Collections.Generic;
using System.Linq;
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


    public override void DoWork(StatScript usingThisBuffStatScript, RuntimeBuffAsset runtimeBuffAsset, int deltaCount)
    {
        foreach (BuffApplyWork_Skill buff in _buffTargets)
        {
            BuffAction type = buff._buffActionType;
            if (buff._buffActionType != BuffAction.None)
            {
                DamagingProcessDelegateType delegateType = buff._delegateTiming;
                HashSet<RuntimeBuffAsset> existHashSet = null;
                usingThisBuffStatScript._buffActions.TryGetValue(delegateType, out existHashSet);

                if (runtimeBuffAsset._Count <= 0)
                {
                    runtimeBuffAsset._buffActions.Remove(type);
                    existHashSet.Remove(runtimeBuffAsset);
                }
                else 
                {
                    if (existHashSet == null)
                    {
                        usingThisBuffStatScript._buffActions.Add(delegateType, new HashSet<RuntimeBuffAsset>());
                    }

                    existHashSet = usingThisBuffStatScript._buffActions[delegateType];

                    if (existHashSet.Contains(runtimeBuffAsset) == false)
                    {
                        existHashSet.Add(runtimeBuffAsset);
                    }

                    if (runtimeBuffAsset._buffActions.ContainsKey(type) == false)
                    {
                        runtimeBuffAsset._buffActions.Add(type, LevelStatInfoManager.Instance.GetBuffAction(type));
                    }
                }
            }
        }
    }
}