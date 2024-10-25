using System;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "TestAsset", menuName = "Scriptable Object/CreateTestAsset", order = int.MinValue)]
public class TestAsset : ScriptableObject
{
    [SerializeField] private int testVar1;
    [SerializeField] private List<int> testVar7;



    [SerializeField] private int testVar6;
    [SerializeField] private int testVar2;
    [SerializeField] private int testVar3;
    [SerializeField] private int testVar5;
    [SerializeField] private int testVar4;
}
