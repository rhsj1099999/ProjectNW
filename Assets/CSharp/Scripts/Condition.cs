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

        //오류검사
        switch (_conditionDesc._singleConditionType)
        {
            case ConditionType.KeyInput:
                Debug.Assert(_conditionDesc._keyInputConditionTarget.Count != 0, "키인풋인데 정보가 없다");
                break;
            case ConditionType.EquipWeaponByType:
                Debug.Assert(_conditionDesc._weaponTypeGoal != ItemInfo.WeaponType.NotWeapon, "WeaponType인데 Goal이 None이다. 오류는 아닐수도 있다");
                break;
            case ConditionType.AnimationFrameUp:
                Debug.Assert(_conditionDesc._animationFrameUpGoal > float.Epsilon, "AnimSecondType인데 Goal이 0.0이다. 오류는 아닐수도 있다");
                break;

            case ConditionType.AnimationFrameUnder:
                Debug.Assert(_conditionDesc._animationFrameUnderGoal > float.Epsilon, "AnimSecondType인데 Goal이 0.0이다. 오류는 아닐수도 있다");
                break;

            default:
                break;
        }

        switch (_conditionDesc._singleConditionType)
        {
            case ConditionType.MoveDesired:
                _ownerComponents._ownerInputController = owner.GetComponent<InputController>();
                Debug.Assert(_ownerComponents._ownerInputController != null, "Input조건이 있는데 이 컴포넌트가 없습니다");
                break;

            case ConditionType.AnimationEnd:
                _ownerComponents._ownerAnimator = owner.GetComponentInChildren<Animator>();
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

            case ConditionType.EquipWeaponByType:
                //|TODO| 실시간으로 무기 장착 정보를 알 수 있는 인벤토리 컴포넌트를 알아야합니다.
                _ownerComponents._ownerCurrWeapon = owner.GetComponent<WeaponScript>();
                Debug.Assert(_ownerComponents._ownerCurrWeapon != null, "Jump조건이 있는데 이 컴포넌트가 없습니다");
                break;

            case ConditionType.AnimationFrameUp:
                //|TODO| 지금은 owner에게서 직접 재생시간을 받아옵니다. 추후 Animator가 이동하면 이곳에 작업이 필요합니다.
                break;

            case ConditionType.AnimationFrameUnder:
                //|TODO| 지금은 owner에게서 직접 재생시간을 받아옵니다. 추후 Animator가 이동하면 이곳에 작업이 필요합니다.
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
                                Debug.Log("KeyState목표값이 없습니다.");
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
                    { return false; } //무기를 끼고있지 않습니다.

                    if (ownerCurrWeapon._weaponType == _conditionDesc._weaponTypeGoal)
                    { return false; } //끼고있는 무기가 목표값과 다릅니다.

                    return true;
                }

            case ConditionType.AnimationFrameUp:
                {
                    //|TODO| 지금은 owner에게서 직접 재생시간을 받아옵니다. 추후 Animator가 이동하면 이곳에 작업이 필요합니다.
                    if (_ownerComponents._owner.GetCurrAnimationClipFrame() < _conditionDesc._animationFrameUpGoal)
                    {return false;}

                    return true;
                }

            case ConditionType.AnimationFrameUnder:
                {
                    //|TODO| 지금은 owner에게서 직접 재생시간을 받아옵니다. 추후 Animator가 이동하면 이곳에 작업이 필요합니다.
                    if (_ownerComponents._owner.GetCurrAnimationClipFrame() > _conditionDesc._animationFrameUnderGoal)
                    { return false; }

                    return true;
                }

            default:
                Debug.Assert(false, "데이터가 추가됐습니까?");
                break;
        }

        return false;
    }
}
