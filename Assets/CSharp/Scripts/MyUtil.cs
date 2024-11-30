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
}
