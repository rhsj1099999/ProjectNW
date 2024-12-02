using System;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "TestAsset", menuName = "Scriptable Object/CreateTestAsset", order = int.MinValue)]
public class TestAsset : ScriptableObject
{
    public enum TestEnum
    {
        Val0 = 0,
        Val_Inter_After_0 = 1,
        Val_Inter_After_1 = 2,
        Val_Inter_After = 3,
        Val_Inter = 4,
        Val1 = 5,
        Val2 = 6,
        Val3 = 7,
        Val4 = 8,
        Val5 = 9,
        Val6 = 10,
    }


    [SerializeField] private int testVar1;
    [SerializeField] private List<int> testVar7;
    [SerializeField] private TestEnum testVar8 = TestEnum.Val0;


    [SerializeField] private int testVar6;
    [SerializeField] private int testVar2;
    [SerializeField] private int testVar3;
    [SerializeField] private int testVar5;
    [SerializeField] private int testVar4;
}
