using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "SubAnimationStateMachineAsset", menuName = "Scriptable Object/CreateSubAnimationStateMachine", order = int.MinValue)]
public class SubAnimationStateMachine : ScriptableObject
{
    public enum CalculateLogic
    {
        MoveDesiredDirection,
        END,
    }

    public CalculateLogic _calculateLogic = CalculateLogic.END;

    public List<AnimationClip> _animations = new List<AnimationClip>();
}
