using System.Collections.Generic;
using System;
using static StateGraphAsset;
using UnityEngine;

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
    public const int deltaRoughness_lvl3 = 12;
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



    public enum CustomToolOrder
    {
        NoneFirst = 0,
        CreateItemAsset = 1,
        CreateItemSubInfo = 2,
        CreateBuffs = 3,
        AnimationFrameDataAsset = 4,
    };




    public static float FloatMod(float a, float b)
    {
        return (a % b + b) % b;
    }

    public static Quaternion LootAtPercentageRotation(Transform transform, Vector3 targetPosition, float percentage)
    {
        Quaternion currentRotation = transform.rotation;

        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

        Quaternion halfwayRotation = Quaternion.Lerp(currentRotation, targetRotation, percentage);
        
        return halfwayRotation;
    }

    public static U GetOrAdd<T, U>(this Dictionary<T, U> dictionary, T key) where U : new()
    {
        if (dictionary.ContainsKey(key) == false)
        {
            dictionary.Add(key, new U());
        }
        return dictionary[key];
    }


    public static U GetOrAdd<T, U>(this SortedDictionary<T, U> dictionary, T key) where U : new()
    {
        if (dictionary.ContainsKey(key) == false)
        {
            dictionary.Add(key, new U());
        }
        return dictionary[key];
    }

    public static int CalculateConditionWeight(List<ConditionAssetWrapper> conditions)
    {
        int retWeight = 0;

        foreach (ConditionAssetWrapper condition in conditions)
        {
            ConditionDesc conditionDesc = condition._conditionAsset._conditionDesc;
            //기본적으로 조건이 하나 걸려있으면 가중치 +1입니다.
            //콤보 키, KeyInput경우에는 키가 어려울수록 가중치가 더들어갑니다.
            switch (conditionDesc._singleConditionType)
            {
                default:
                    retWeight++;
                    break;

                case ConditionType.KeyInput:
                    {
                        //총 키 개수 ... ver 1
                        List<KeyInputConditionDesc> keys = conditionDesc._keyInputConditionTarget;
                        retWeight += keys.Count;
                    }
                    break;

                case ConditionType.ComboKeyCommand:
                    {
                        //조합키들 총 개수 + 콤보개수 ... ver 1
                        List<ComboKeyCommandDesc> comboKeys = conditionDesc._commandInputConditionTarget;
                        foreach (ComboKeyCommandDesc command in comboKeys)
                        {
                            retWeight += command._targetCommandKeys.Count;
                        }
                        retWeight += conditionDesc._commandInputConditionTarget.Count;
                    }
                    break;
            }
        }

        return retWeight;
    }




    public static int CalculateConditionWeight(List<ConditionDesc> conditions)
    {
        int retWeight = 0;

        foreach (ConditionDesc condition in conditions)
        {
            //기본적으로 조건이 하나 걸려있으면 가중치 +1입니다.
            //콤보 키, KeyInput경우에는 키가 어려울수록 가중치가 더들어갑니다.
            switch (condition._singleConditionType)
            {
                default:
                    retWeight++;
                    break;

                case ConditionType.KeyInput:
                    {
                        //총 키 개수 ... ver 1
                        List<KeyInputConditionDesc> keys = condition._keyInputConditionTarget;
                        retWeight += keys.Count;
                    }
                    break;

                case ConditionType.ComboKeyCommand:
                    {
                        //조합키들 총 개수 + 콤보개수 ... ver 1
                        List<ComboKeyCommandDesc> comboKeys = condition._commandInputConditionTarget;
                        foreach (ComboKeyCommandDesc command in comboKeys)
                        {
                            retWeight += command._targetCommandKeys.Count;
                        }
                        retWeight += condition._commandInputConditionTarget.Count;
                    }
                    break;
            }
        }

        return retWeight;
    }
}
