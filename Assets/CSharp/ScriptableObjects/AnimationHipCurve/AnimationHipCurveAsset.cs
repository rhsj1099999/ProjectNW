using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationHipCurveAsset", menuName = "Scriptable Object/CreateAnimationHipCurveAsset", order = int.MinValue)]
public class AnimationHipCurveAsset : ScriptableObject
{
    public AnimationClip _clip = null;
    public AnimationCurve _animationHipCurveX = null;
    public AnimationCurve _animationHipCurveY = null;
    public AnimationCurve _animationHipCurveZ = null;
}




