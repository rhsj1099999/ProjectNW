using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "StateAsset", menuName = "Scriptable Object/CreateStateAsset", order = int.MinValue)]
public class StateAsset : ScriptableObject
{
    [SerializeField] public StateDesc _myState = null;

    private void OnValidate()
    {
        if (_myState._isLoopState == true)
        {
            if (_myState._breakLoopStateCondition == null)
            {
                _myState._breakLoopStateCondition = new List<ConditionDesc>();
            }
        }
        else
        {
            if (_myState._breakLoopStateCondition != null)
            {
                _myState._breakLoopStateCondition = null;
            }
        }
    }
}
