using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "StateAsset", menuName = "Scriptable Object/CreateStateAsset", order = int.MinValue)]
public class StateAsset : ScriptableObject
{
    [SerializeField] public StateDesc _myState;
}
