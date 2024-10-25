using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public enum ConditionType
{
    MoveDesired, //����Ű�� �Է��� �Ƴ�?
    AnimationEnd,
    InAir,
    KeyInput,
}

[Serializable]
public struct ConditionPair
{
    public ConditionType _conditionType;
}

[Serializable]
public struct KeyInputConditionDesc
{
    public enum KeyPressType
    {
        Pressed,
        Hold,
        Released,
    };

    public KeyCode _targetKeyCode;
    public KeyPressType _targetState;
    public bool _goal;
}

[Serializable]
public struct ConditionDesc
{
    public ConditionPair _singleCondition; //SingleCondition
    public List<KeyInputConditionDesc> _keyInputConditionTarget;
}

public struct ConditionInitDesc
{
    public CharacterController _ownerCharacterComponent;
    public Animator _ownerAnimator;
    public InputController _ownerInputController;
    public CharacterMoveScript2 _ownerMoveScript;
}



public class Condition
{
    public Condition(ConditionAsset conditionAsset)
    {
        _conditionDesc = conditionAsset._myCondition; //���� �Ϸ�
    }

    private ConditionDesc _conditionDesc; //Copy From ScriptableObject
    private ConditionInitDesc _ownerChecker;
    

    public void Initialize(PlayerScript owner)
    {
        _ownerChecker = new ConditionInitDesc();


        switch (_conditionDesc._singleCondition._conditionType)
        {
            case ConditionType.MoveDesired:
                _ownerChecker._ownerInputController = owner.GetComponent<InputController>();
                Debug.Assert(_ownerChecker._ownerInputController != null, "Input������ �ִµ� �� ������Ʈ�� �����ϴ�");
                break;

            case ConditionType.AnimationEnd:
                _ownerChecker._ownerAnimator = owner.GetComponent<Animator>();
                Debug.Assert(_ownerChecker._ownerAnimator != null, "Animation������ �ִµ� �� ������Ʈ�� �����ϴ�");
                break;

            case ConditionType.InAir:
                _ownerChecker._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                Debug.Assert(_ownerChecker._ownerMoveScript != null, "GroundLoss������ �ִµ� �� ������Ʈ�� �����ϴ�");
                break;

            case ConditionType.KeyInput:
                _ownerChecker._ownerInputController = owner.GetComponent<InputController>();
                Debug.Assert(_ownerChecker._ownerInputController != null, "Jump������ �ִµ� �� ������Ʈ�� �����ϴ�");
                break;

            default:
                Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                break;
        }
    }

    public ConditionDesc? GetConditionDesc() //����ü Ŀ������ �����ϱ� ������ �ݽô�
    {
        return _conditionDesc;
    }

    public bool CheckCondition(bool goal)
    {
        bool ret = false;

        switch (_conditionDesc._singleCondition._conditionType)
        {
            case ConditionType.MoveDesired:
                {
                    Vector3 desiredMoved = _ownerChecker._ownerInputController._pr_directionByInput;
                    if (desiredMoved != Vector3.zero)
                    {
                        ret = true;
                    }

                    return (ret == goal);
                }

            case ConditionType.AnimationEnd:
                {
                    return false;
                }

            case ConditionType.InAir:
                {
                    if (_ownerChecker._ownerMoveScript.GetIsInAir() == true)
                    {
                        ret = true;
                    }

                    return (ret == goal);
                }

            case ConditionType.KeyInput:
                {
                    bool isSuccess = true;
                    for (int i = 0; i < _conditionDesc._keyInputConditionTarget.Count; ++i)
                    {
                        switch (_conditionDesc._keyInputConditionTarget[i]._targetState)
                        {
                            case KeyInputConditionDesc.KeyPressType.Pressed:
                                if (Input.GetKeyDown(_conditionDesc._keyInputConditionTarget[i]._targetKeyCode) != _conditionDesc._keyInputConditionTarget[i]._goal)
                                {
                                    isSuccess = false;
                                }
                                break;

                            case KeyInputConditionDesc.KeyPressType.Hold:
                                if (Input.GetKey(_conditionDesc._keyInputConditionTarget[i]._targetKeyCode) != _conditionDesc._keyInputConditionTarget[i]._goal)
                                {
                                    isSuccess = false;
                                }
                                break;

                            case KeyInputConditionDesc.KeyPressType.Released:
                                if (Input.GetKeyUp(_conditionDesc._keyInputConditionTarget[i]._targetKeyCode) != _conditionDesc._keyInputConditionTarget[i]._goal)
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
