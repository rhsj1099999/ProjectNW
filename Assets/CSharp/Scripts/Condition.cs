using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Condition
{
    public class ConditionComponentDesc
    {
        public CharacterController _ownerCharacterComponent;
        public Animator _ownerAnimator;
        public InputController _ownerInputController;
        public CharacterMoveScript2 _ownerMoveScript;
    }
    public Condition(ConditionDesc descRef)
    {
        _conditionDesc = descRef;
    }

    private ConditionDesc _conditionDesc; //Copy From ScriptableObject

    private ConditionComponentDesc _ownerComponents;

    public ConditionDesc GetConditionDesc() { return _conditionDesc; }

    public void Initialize(PlayerScript owner)
    {
        _ownerComponents = new ConditionComponentDesc();

        if (_conditionDesc._singleConditionType == ConditionType.KeyInput)
        {
            Debug.Assert(_conditionDesc._keyInputConditionTarget.Count != 0, "키인풋인데 정보가 없다");
        }

        switch (_conditionDesc._singleConditionType)
        {
            case ConditionType.MoveDesired:
                _ownerComponents._ownerInputController = owner.GetComponent<InputController>();
                Debug.Assert(_ownerComponents._ownerInputController != null, "Input조건이 있는데 이 컴포넌트가 없습니다");
                break;

            case ConditionType.AnimationEnd:
                _ownerComponents._ownerAnimator = owner.GetComponent<Animator>();
                Debug.Assert(_ownerComponents._ownerAnimator != null, "Animation조건이 있는데 이 컴포넌트가 없습니다");
                break;

            case ConditionType.InAir:
                _ownerComponents._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                Debug.Assert(_ownerComponents._ownerMoveScript != null, "GroundLoss조건이 있는데 이 컴포넌트가 없습니다");
                break;

            case ConditionType.KeyInput:
                _ownerComponents._ownerInputController = owner.GetComponent<InputController>();
                Debug.Assert(_ownerComponents._ownerInputController != null, "Jump조건이 있는데 이 컴포넌트가 없습니다");
                break;

            default:
                Debug.Assert(false, "데이터가 추가됐습니까?");
                break;
        }
    }


    public bool CheckCondition()
    {
        bool ret = false;

        switch (_conditionDesc._singleConditionType)
        {
            case ConditionType.MoveDesired:
                {
                    Vector3 desiredMoved = _ownerComponents._ownerInputController._pr_directionByInput;
                    if (desiredMoved != Vector3.zero)
                    {
                        ret = true;
                    }

                    return (ret == _conditionDesc._componentConditionGoal);
                }

            case ConditionType.AnimationEnd:
                {
                    return false;
                }

            case ConditionType.InAir:
                {
                    if (_ownerComponents._ownerMoveScript.GetIsInAir() == true)
                    {
                        ret = true;
                    }

                    return (ret == _conditionDesc._componentConditionGoal);
                }

            case ConditionType.KeyInput:
                {
                    bool isSuccess = true;

                    for (int i = 0; i < _conditionDesc._keyInputConditionTarget.Count; ++i)
                    {
                        switch (_conditionDesc._keyInputConditionTarget[i]._targetState)
                        {
                            case KeyInputConditionDesc.KeyPressType.Pressed:
                                if (Input.GetKeyDown(_conditionDesc._keyInputConditionTarget[i]._targetKeyCode) != _conditionDesc._keyInputConditionTarget[i]._keyInpuyGoal)
                                {
                                    isSuccess = false;
                                }
                                break;

                            case KeyInputConditionDesc.KeyPressType.Hold:
                                if (Input.GetKey(_conditionDesc._keyInputConditionTarget[i]._targetKeyCode) != _conditionDesc._keyInputConditionTarget[i]._keyInpuyGoal)
                                {
                                    isSuccess = false;
                                }
                                break;

                            case KeyInputConditionDesc.KeyPressType.Released:
                                if (Input.GetKeyUp(_conditionDesc._keyInputConditionTarget[i]._targetKeyCode) != _conditionDesc._keyInputConditionTarget[i]._keyInpuyGoal)
                                {
                                    isSuccess = false;
                                }
                                break;

                            default:
                                Debug.Log("KeyState목표값이 없습니다.");
                                break;
                        }

                        if (isSuccess == false) { return false; }
                    }

                    return isSuccess;
                }

            default:
                Debug.Assert(false, "데이터가 추가됐습니까?");
                break;
        }

        return false;
    }
}
