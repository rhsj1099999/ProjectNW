using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum StateActionType
{
    Move,
    Attack, //���� ���⿡ ���� �޶��� �� �ִ�.
    SaveLatestVelocity,
    Jump,
    ForcedMove,
    ResetLatestVelocity,
}

[Serializable]
public struct ConditionAssetPair
{
    public ConditionAsset _condition;
    public bool _goal;
}

[Serializable]
public struct ConditionSet
{
    public Condition _condition;
    public bool _goal;
}

[Serializable]
public struct StateLinkDesc
{
    public List<ConditionAssetPair> _multiConditionAsset; //MultiCondition
    public StateAsset _stateAsset;
}

[Serializable]
public struct StateDesc
{
    public string _stataName;
    public AnimationClip _stateAnimationClip;

    public List<StateActionType> _EnterStateActionTypes;
    public List<StateActionType> _inStateActionTypes;
    public List<StateActionType> _ExitStateActionTypes;

    public List<StateLinkDesc> _linkedStates;
}


public struct StateInitDesc
{
    public CharacterMoveScript2 _ownerMoveScript;
    public CharacterController _ownerCharacterController;
    public InputController _ownerInputController;
    public Animator _ownerAnimator;
}



public class State
{
    private StateDesc _stateDesc; //Copy From ScriptableObject
    private StateAsset _stateAssetCreateFrom = null;
    private Dictionary<State/*Another State*/, List<ConditionSet>> _linkedState = new Dictionary<State , List<ConditionSet>>();
    StateInitDesc _ownerActionComponent;


    public void Initialize(PlayerScript owner)
    {
        _ownerActionComponent = new StateInitDesc();

        InitPartial(ref _ownerActionComponent, _stateDesc._EnterStateActionTypes, owner);
        InitPartial(ref _ownerActionComponent, _stateDesc._inStateActionTypes, owner);
        InitPartial(ref _ownerActionComponent, _stateDesc._ExitStateActionTypes, owner);
    }

    private void InitPartial(ref StateInitDesc _ownerActionComponent, List<StateActionType> list, PlayerScript owner)
    {
        foreach (var actions in list)
        {
            switch (actions)
            {
                case StateActionType.Move:
                    {
                        if (_ownerActionComponent._ownerMoveScript == null)
                        {
                            _ownerActionComponent._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerMoveScript != null, "Move�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponent<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "Input�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.Attack:
                    {
                        //���� �����ϰ��ִ� ���⿡ ������ �޽��ϴ�.
                    }
                    break;

                case StateActionType.SaveLatestVelocity:
                    {
                        if (_ownerActionComponent._ownerMoveScript == null)
                        {
                            _ownerActionComponent._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerMoveScript != null, "SaveLatestVelocity�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.Jump:
                    {
                        if (_ownerActionComponent._ownerMoveScript == null)
                        {
                            _ownerActionComponent._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerMoveScript != null, "Jump�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.ForcedMove:
                    {
                        if (_ownerActionComponent._ownerMoveScript == null)
                        {
                            _ownerActionComponent._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerMoveScript != null, "ForcedMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerCharacterController == null)
                        {
                            _ownerActionComponent._ownerCharacterController = owner.GetComponent<CharacterController>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterController != null, "ForcedMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateActionType.ResetLatestVelocity:
                    {
                        if (_ownerActionComponent._ownerMoveScript == null)
                        {
                            _ownerActionComponent._ownerMoveScript = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerMoveScript != null, "ResetLatestVelocity�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                default:
                    Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                    break;
            }
        }
    }


    public void LinkingStates(/*��Ʈ�ѷ��� ����ϴ� ������Ʈ��*/ref List<State> allStates /*��ųʸ��� �ٲټ���*/, PlayerScript owner)
    {
        foreach (var linked in _stateDesc._linkedStates)
        {
            State targetState = null;
            foreach (var state in allStates)
            {
                if (state == this)
                {
                    continue; //�� �ڽ��̴�
                }

                if (state.GetStateAssetFrom() == linked._stateAsset)
                {
                    targetState = state;
                    break;
                }
                //Debug.Assert(false, "���� ���·� �Ѿ�� �ϴ� ������ �ֽ��ϴ�");
            }

            if (targetState == null) //����� ���°� �÷��̾�(Ȥ������)�� ������� �ʴ� ���´�
            {
                return;
            }

            
            if (_linkedState.ContainsKey(targetState) == false)
            {
                _linkedState.Add(targetState, new List<ConditionSet>());
            }

            List<ConditionSet> exists = _linkedState[targetState];

            foreach (var condition in linked._multiConditionAsset)
            {
                Condition newCondition = new Condition(condition._condition);
                newCondition.Initialize(owner);

                ConditionSet newConditionSet = new ConditionSet();
                newConditionSet._condition = newCondition;
                newConditionSet._goal = condition._goal;
                exists.Add(newConditionSet);
            }
        }
    }


    public StateDesc? GetStateDesc() //����ü Ŀ������ �����ϱ� ������ �ݽô�
    {
        return _stateDesc;
    }

    public StateAsset GetStateAssetFrom()
    {
        return _stateAssetCreateFrom;
    }

    public State(StateAsset stateAsset)
    {
        _stateDesc = stateAsset._myState; //���� �Ϸ�
        _stateAssetCreateFrom = stateAsset;
    }

    public State CheckChangeState()
    {
        foreach(KeyValuePair<State, List<ConditionSet>> pair in _linkedState)
        {
            bool isSuccess = true;

            foreach (ConditionSet? condition in pair.Value) 
            {
                if (condition.Value._condition.CheckCondition(condition.Value._goal) == false)
                {
                    isSuccess = false;
                    break; //��Ƽ����ǿ��� �ϳ��� �೵��.
                }
            }

            if (isSuccess == true)
            {
                return pair.Key; //���δ� �����ߴ�.
            }
        }

        return null; //�����Ѱ� �ϳ��� ����.
    }


    public void DoActions(List<StateActionType> actions)
    {
        foreach(var action in actions)
        {
            switch(action)
            {
                case StateActionType.Move:
                    _ownerActionComponent._ownerMoveScript.CharacterRotate(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                    _ownerActionComponent._ownerMoveScript.CharacterMove(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                    break;

                case StateActionType.Attack:
                    break;

                case StateActionType.SaveLatestVelocity:
                    break;

                case StateActionType.Jump:
                    _ownerActionComponent._ownerMoveScript.DoJump();
                    break;

                case StateActionType.ForcedMove:
                    Vector3 planeVelocity = _ownerActionComponent._ownerMoveScript.GetLatestVelocity();
                    planeVelocity.y = 0.0f;
                    _ownerActionComponent._ownerMoveScript.CharacterForcedMove(planeVelocity, 1.0f);
                    break;

                case StateActionType.ResetLatestVelocity:
                    //_ownerActionComponent._ownerMoveScript.ResetLatestVelocity();
                    break;

                default:
                    Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                    break;
            }
        }
    }
}
