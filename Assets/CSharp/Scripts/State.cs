using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum StateAction
{
    Move,
    Jump,
    Attack, //���� ���⿡ ���� �޶��� �� �ִ�.
}

[Serializable]
public struct StateLinkDesc
{
    public ConditionAsset _conditionAsset;
    public StateAsset _stateAsset;
}

[Serializable]
public struct StateDesc
{
    public string _stataName;
    public AnimationClip _stateAnimationClip;

    public List<StateAction> _EnterStateActions;
    public List<StateAction> _inStateActions;
    public List<StateAction> _ExitStateActions;

    public List<StateLinkDesc> _linkedStates;
}


public struct StateInitDesc
{
    public CharacterMoveScript2 _ownerCharacterComponent;
    public Animator _ownerAnimator;
    public InputController _ownerInputController;
}



public class State
{
    private StateDesc _stateDesc; //Copy From ScriptableObject
    private StateAsset _stateAssetCreateFrom = null;
    private Dictionary<State/*Another State*/, Condition> _linkedState = new Dictionary<State , Condition>();
    StateInitDesc _ownerActionComponent;


    public void Initialize(PlayerScript owner)
    {
        _ownerActionComponent = new StateInitDesc();

        InitPartial(ref _ownerActionComponent, _stateDesc._EnterStateActions, owner);
        InitPartial(ref _ownerActionComponent, _stateDesc._inStateActions, owner);
        InitPartial(ref _ownerActionComponent, _stateDesc._ExitStateActions, owner);
    }
    private void InitPartial(ref StateInitDesc _ownerActionComponent, List<StateAction> list, PlayerScript owner)
    {
        foreach (var actions in list)
        {
            switch (actions)
            {
                case StateAction.Move:
                    {
                        if (_ownerActionComponent._ownerCharacterComponent == null)
                        {
                            _ownerActionComponent._ownerCharacterComponent = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "Move�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponent<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "Input�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                case StateAction.Jump:
                    {
                        if (_ownerActionComponent._ownerCharacterComponent == null)
                        {
                            _ownerActionComponent._ownerCharacterComponent = owner.GetComponent<CharacterMoveScript2>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterComponent != null, "Input�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerInputController == null)
                        {
                            _ownerActionComponent._ownerInputController = owner.GetComponent<InputController>();
                            Debug.Assert(_ownerActionComponent._ownerInputController != null, "Input�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;
                case StateAction.Attack:
                    {
                        //���� �����ϰ��ִ� ���⿡ ������ �޽��ϴ�.
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
            Condition newCondition = new Condition(linked._conditionAsset);
            newCondition.Initialize(owner);

            foreach (var state in allStates)
            {
                if (state == this)
                {
                    continue; //�� �ڽ��̴�
                }

                if (state.GetStateAssetFrom() == linked._stateAsset)
                {
                    _linkedState.Add(state, newCondition);
                    break;
                }

                Debug.Assert(false, "���� ���·� �Ѿ�� �ϴ� ������ �ֽ��ϴ�");
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
        foreach(KeyValuePair<State, Condition> pair in _linkedState)
        {
            if(pair.Value.CheckCondition() == true) //�� ���·� �Ѿ �� �ִ� ������ �����ߴ�.
            {
                return pair.Key;
            }
        }
        return null;
    }


    public void DoActions(List<StateAction> actions)
    {
        foreach(var action in actions)
        {
            switch(action)
            {
                case StateAction.Move:
                    _ownerActionComponent._ownerCharacterComponent.CharacterRotate(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                    _ownerActionComponent._ownerCharacterComponent.CharacterMove(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                    break;

                case StateAction.Jump:
                    break;
                case StateAction.Attack:
                    break;
                default:
                    Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                    break;
            }
        }
    }
}
