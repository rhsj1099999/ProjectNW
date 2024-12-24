using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using SerializableDictionary.Scripts;
using static StateContoller;

[CreateAssetMenu(fileName = "SubBlendTreeAsset_2D", menuName = "Scriptable Object/CreateSubBlendTree_2D", order = int.MinValue)]
public class SubBlendTreeAsset_2D : ScriptableObject
{
    public enum BlendTreeLogic_2D
    {
        MoveDesiredDirection,
        END,
    }


    public BlendTreeLogic_2D _calculateLogic = BlendTreeLogic_2D.END; 

    public AnimationClip _animation_YUP = null;
    public AnimationClip _animation_XUP = null;
    public AnimationClip _animation_YDOWN = null;
    public AnimationClip _animation_XDOWN = null;
}
