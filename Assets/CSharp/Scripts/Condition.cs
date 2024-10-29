using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Condition
{
    public class ConditionComponentDesc
    {
        public PlayerScript _owner;
        public CharacterController _ownerCharacterComponent;
        public Animator _ownerAnimator;
        public InputController _ownerInputController;
        public CharacterMoveScript2 _ownerMoveScript;
        public WeaponScript _ownerCurrWeapon;
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

        _ownerComponents._owner = owner;

        //�����˻�
        switch (_conditionDesc._singleConditionType)
        {
            case ConditionType.KeyInput:
                Debug.Assert(_conditionDesc._keyInputConditionTarget.Count != 0, "Ű��ǲ�ε� ������ ����");
                break;
            case ConditionType.EquipWeaponByType:
                Debug.Assert(_conditionDesc._weaponTypeGoal != ItemInfo.WeaponType.NotWeapon, "WeaponType�ε� Goal�� None�̴�. ������ �ƴҼ��� �ִ�");
                break;
            case ConditionType.AnimationFrameUp:
                Debug.Assert(_conditionDesc._animationFrameUpGoal > float.Epsilon, "AnimSecondType�ε� Goal�� 0.0�̴�. ������ �ƴҼ��� �ִ�");
                break;

            case ConditionType.AnimationFrameUnder:
                Debug.Assert(_conditionDesc._animationFrameUnderGoal > float.Epsilon, "AnimSecondType�ε� Goal�� 0.0�̴�. ������ �ƴҼ��� �ִ�");
                break;

            default:
                break;
        }

        switch (_conditionDesc._singleConditionType)
        {
            case ConditionType.MoveDesired:
                _ownerComponents._ownerInputController = owner.GetComponent<InputController>();
                Debug.Assert(_ownerComponents._ownerInputController != null, "Input������ �ִµ� �� ������Ʈ�� �����ϴ�");
                break;

            case ConditionType.AnimationEnd:
                _ownerComponents._ownerAnimator = owner.GetComponentInChildren<Animator>();
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

            case ConditionType.EquipWeaponByType:
                //|TODO| �ǽð����� ���� ���� ������ �� �� �ִ� �κ��丮 ������Ʈ�� �˾ƾ��մϴ�.
                _ownerComponents._ownerCurrWeapon = owner.GetComponent<WeaponScript>();
                Debug.Assert(_ownerComponents._ownerCurrWeapon != null, "Jump������ �ִµ� �� ������Ʈ�� �����ϴ�");
                break;

            case ConditionType.AnimationFrameUp:
                //|TODO| ������ owner���Լ� ���� ����ð��� �޾ƿɴϴ�. ���� Animator�� �̵��ϸ� �̰��� �۾��� �ʿ��մϴ�.
                break;

            case ConditionType.AnimationFrameUnder:
                //|TODO| ������ owner���Լ� ���� ����ð��� �޾ƿɴϴ�. ���� Animator�� �̵��ϸ� �̰��� �۾��� �ʿ��մϴ�.
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
                    if (_ownerComponents._owner.GetCurrAnimationLoopCount() >= 1)
                    {
                        return true;
                    }
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
                            case KeyPressType.Pressed:
                                if (Input.GetKeyDown(_conditionDesc._keyInputConditionTarget[i]._targetKeyCode) != _conditionDesc._keyInputConditionTarget[i]._keyInpuyGoal)
                                {
                                    isSuccess = false;
                                }
                                break;

                            case KeyPressType.Hold:
                                if (Input.GetKey(_conditionDesc._keyInputConditionTarget[i]._targetKeyCode) != _conditionDesc._keyInputConditionTarget[i]._keyInpuyGoal ||
                                    CustomKeyManager.Instance.GetKeyInputDesc(_conditionDesc._keyInputConditionTarget[i]._targetKeyCode)._holdedSecond < _conditionDesc._keyInputConditionTarget[i]._keyHoldGoal)
                                {
                                    isSuccess = false;
                                }
                                break;

                            case KeyPressType.Released:
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

            case ConditionType.EquipWeaponByType:
                {
                    ItemInfo ownerCurrWeapon = _ownerComponents._owner.GetWeaponItem();

                    if (ownerCurrWeapon == null)
                    { return false; } //���⸦ �������� �ʽ��ϴ�.

                    if (ownerCurrWeapon._weaponType == _conditionDesc._weaponTypeGoal)
                    { return false; } //�����ִ� ���Ⱑ ��ǥ���� �ٸ��ϴ�.

                    return true;
                }

            case ConditionType.AnimationFrameUp:
                {
                    //|TODO| ������ owner���Լ� ���� ����ð��� �޾ƿɴϴ�. ���� Animator�� �̵��ϸ� �̰��� �۾��� �ʿ��մϴ�.
                    if (_ownerComponents._owner.GetCurrAnimationClipFrame() < _conditionDesc._animationFrameUpGoal)
                    {return false;}

                    return true;
                }

            case ConditionType.AnimationFrameUnder:
                {
                    //|TODO| ������ owner���Լ� ���� ����ð��� �޾ƿɴϴ�. ���� Animator�� �̵��ϸ� �̰��� �۾��� �ʿ��մϴ�.
                    if (_ownerComponents._owner.GetCurrAnimationClipFrame() > _conditionDesc._animationFrameUnderGoal)
                    { return false; }

                    return true;
                }

            default:
                Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                break;
        }

        return false;
    }
}
