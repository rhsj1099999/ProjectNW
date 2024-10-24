using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Conditions
{
    KeyInput, //방향키가 입력이 됐나?
    AnimationEnd,
    GroundLoss,
}

[Serializable]
public struct ConditionPair
{
    public Conditions _condition;
    public bool _goal;
}

[Serializable]
public struct ConditionDesc
{
    public List<ConditionPair> _conditions;
}

public struct ConditionInitDesc
{
    public CharacterController _ownerCharacterComponent;
    public Animator _ownerAnimator;
    public InputController _ownerInputController;
}

public class Condition
{
    private ConditionDesc _conditionDesc; //Copy From ScriptableObject
    private ConditionInitDesc _ownerChecker;

    public void Initialize(PlayerScript owner)
    {
        _ownerChecker = new ConditionInitDesc();

        foreach (var conditions in _conditionDesc._conditions)
        {
            switch (conditions._condition)
            {
                case Conditions.KeyInput:
                    _ownerChecker._ownerInputController = owner.GetComponent<InputController>();
                    Debug.Assert(_ownerChecker._ownerInputController != null, "Input조건이 있는데 이 컴포넌트가 없습니다");
                    break;

                case Conditions.AnimationEnd:
                    _ownerChecker._ownerAnimator = owner.GetComponent<Animator>();
                    Debug.Assert(_ownerChecker._ownerAnimator != null, "Animation조건이 있는데 이 컴포넌트가 없습니다");
                    break;

                case Conditions.GroundLoss:
                    _ownerChecker._ownerCharacterComponent = owner.GetComponent<CharacterController>();
                    Debug.Assert(_ownerChecker._ownerCharacterComponent != null, "GroundLoss조건이 있는데 이 컴포넌트가 없습니다");
                    break;

                default:
                    Debug.Assert(false, "데이터가 추가됐습니까?");
                    break;
            }
        }
    }

    public ConditionDesc? GetConditionDesc() //구조체 커질수도 있으니까 참조로 줍시다
    {
        return _conditionDesc;
    }

    public Condition(ConditionAsset conditionAsset)
    {
        _conditionDesc = conditionAsset._myCondition; //복사 완료
    }

    public bool CheckCondition()
    {
        foreach (var actions in _conditionDesc._conditions)
        {
            bool ret = false;

            switch (actions._condition)
            {
                case Conditions.KeyInput:
                    {

                        Vector3 desiredMoved = _ownerChecker._ownerInputController._pr_directionByInput;
                        if (desiredMoved != Vector3.zero) 
                        {
                            ret = true;
                        }

                        return (ret == actions._goal);
                    }

                case Conditions.AnimationEnd:
                    {
                        //_ownerChecker._ownerAnimator -> DoSomething
                        return false;
                    }



                case Conditions.GroundLoss:
                    {
                        if (_ownerChecker._ownerCharacterComponent.isGrounded == false)
                        {
                            ret = true;
                        }

                        return (ret == actions._goal);
                    }



                default:
                    Debug.Assert(false, "데이터가 추가됐습니까?");
                    break;
            }
        }

        return false;

    }
}
