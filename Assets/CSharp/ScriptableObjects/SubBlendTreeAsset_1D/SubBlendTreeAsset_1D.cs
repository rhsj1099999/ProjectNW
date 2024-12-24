using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using SerializableDictionary.Scripts;
using static StateContoller;

[CreateAssetMenu(fileName = "SubBlendTreeAsset_1D", menuName = "Scriptable Object/CreateSubBlendTree_1D", order = int.MinValue)]
public class SubBlendTreeAsset_1D : ScriptableObject
{
    public enum BlendTreeLogic_1D
    {
        SpeedValue,
        END,
    }

    //public class AnimationClipWrapper
    //{

    //}
    //[SerializeField] private List<AnimationClip> _animations = new List<AnimationClip>();
}
