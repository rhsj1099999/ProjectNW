using System.Collections.Generic;
using System;

class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
{
    public int Compare(T x, T y)
    {
        return y.CompareTo(x);
    }
}

public static class MyUtil
{
    public const int deltaRoughness_lvl0 = 3;
    public const int deltaRoughness_lvl1 = 6;
    public const int deltaRoughness_lvl2 = 9;
    public static readonly string[] _motionChangingAnimationNames =
    {
        "Layer0",
        "Layer1",
        "Layer2",
        "Layer3",
        "Layer4",
        "Layer5",
        "Layer6",
        "Layer7",
        "Layer8",
        "Layer9",
        "Layer10",
        "Layer11",
        "Layer12",
        "Layer13",
        "Layer14",
        "Layer15",
        "Layer16",
        "Layer17",
        "Layer18",
        "Layer19",
    };

    public static float FloatMod(float a, float b)
    {
        return (a % b + b) % b;
    }

    public static void GuardAndDamageTypeConverter(DamageDesc.DamageType damageType, RepresentStateType curGuardType, RepresentStateType guardStateLvl)
    {
        guardStateLvl = 0;

        //가드 자세의 레벨이 더 크다.
            //무조건 막는자세로 간다.

        //가드 자세의 레벨과 데미지 타입이 같다.
            //막는다 혹은 밀려난다.

        //데미지 타입이 더 크다.
            //밀려난다. 혹은 가드가 부서진다.
    }
    
}
