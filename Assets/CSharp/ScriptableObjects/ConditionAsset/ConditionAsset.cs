using System;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "ConditionAsset", menuName = "Scriptable Object/CreateConditionAsset", order = int.MinValue + 1)]
public class ConditionAsset : ScriptableObject
{
    [SerializeField] public ConditionDesc _myCondition;
}
