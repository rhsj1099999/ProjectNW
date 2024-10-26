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
            Debug.Assert(_conditionDesc._keyInputConditionTarget.Count != 0, "Ű��ǲ�ε� ������ ����");
        }

        switch (_conditionDesc._singleConditionType)
        {
            case ConditionType.MoveDesired:
                _ownerComponents._ownerInputController = owner.GetComponent<InputController>();
                Debug.Assert(_ownerComponents._ownerInputController != null, "Input������ �ִµ� �� ������Ʈ�� �����ϴ�");
                break;

            case ConditionType.AnimationEnd:
                _ownerComponents._ownerAnimator = owner.GetComponent<Animator>();
                Debug.Assert(_ownerComponents._ownerAnimator != null, "Animation������ �ִµ� �� ������Ʈ�� �����ϴ�");
                break;

            case ConditionType.InAir:
                _ownerComponents._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                Debug.Assert(_ownerComponents._ownerMoveScript != null, "GroundLoss������ �ִµ� �� ������Ʈ�� �����ϴ�");
                break;

            case ConditionType.KeyInput:
                _ownerComponents._ownerInputController = owner.GetComponent<InputController>();
                Debug.Assert(_ownerComponents._ownerInputController != null, "Jump������ �ִµ� �� ������Ʈ�� �����ϴ�");
                break;

            default:
                Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
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
                                Debug.Log("KeyState��ǥ���� �����ϴ�.");
                                break;
                        }

                        if (isSuccess == false) { return false; }
                    }

                    return isSuccess;
                }

            default:
                Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                break;
        }

        return false;
    }
}
