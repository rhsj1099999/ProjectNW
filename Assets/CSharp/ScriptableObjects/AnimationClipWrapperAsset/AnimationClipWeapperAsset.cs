using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "AnimationClipWrapperAsset", menuName = "Scriptable Object/CreateAnimationClipWrapperAsset", order = int.MinValue)]
public class AnimationClipWrapperAsset : ScriptableObject
{
    [Serializable]
    public class FrameDataWrapper
    {
        public FrameDataType _frameDataType = FrameDataType.NextAttackMotion;
        public FrameData _dataAsset = new FrameData();
    }

    [Serializable]
    public class AnimationFrameDataWrapper
    {
        public AnimationClip _animationClip = null;
        public List<FrameDataWrapper> _dataAssetWrapper = new List<FrameDataWrapper>();
    }

    public List<AnimationFrameDataWrapper> _list = new List<AnimationFrameDataWrapper>();
}
