using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "AnimationClipWrapperAsset", menuName = "Scriptable Object/CreateAnimationClipWrapperAsset", order = int.MinValue)]
public class AnimationClipWrapperAsset : ScriptableObject
{
    [SerializeField] public string _animationName = null;

    [SerializeField] public AnimationClip _animationClip = null;

    [SerializeField] public int _stateChangineFrameMin = -1;

    [SerializeField] public int _stateChangineFrameMax = -1;

    [SerializeField] public int _attackFrames = -1; //다단히트 가능성
}
