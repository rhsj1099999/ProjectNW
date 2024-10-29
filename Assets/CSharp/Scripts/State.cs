using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum StateActionType
{
    Move,
    Attack, //���� ���⿡ ���� �޶��� �� �ִ�.
    SaveLatestVelocity,
    Jump,
    ForcedMove,
    ResetLatestVelocity,
    RootMove,
    RotateWithoutInterpolate,
}

public enum ConditionType
{
    MoveDesired,
    AnimationEnd,
    InAir,
    KeyInput,
    EquipWeaponByType,
    AnimationFrameUp, //~~�� �̻����� ��� �ƽ��ϴ�. -> Animator�� �����ϴ� normalizedTime�� ���� �ʽ��ϴ�.
    AnimationFrameUnder, //~~�� ���Ϸ� ��� �ƽ��ϴ�. -> Animator�� �����ϴ� normalizedTime�� ���� �ʽ��ϴ�.
}

[Serializable]
public class KeyInputConditionDesc
{


    public KeyCode _targetKeyCode;
    public KeyPressType _targetState;
    public bool _keyInpuyGoal;
    public float _keyHoldGoal = 0.0f; //n�� �̻� �� Ű�� ������ �ٵ� ��� �����ϳ�
}

[Serializable]
public class ConditionDesc
{
    public ConditionType _singleConditionType;
    public bool _componentConditionGoal;
    public ItemInfo.WeaponType _weaponTypeGoal;
    public List<KeyInputConditionDesc> _keyInputConditionTarget;
    public float _animationFrameUpGoal;
    public float _animationFrameUnderGoal;
}

[Serializable]
public class StateLinkDesc
{
    public List<ConditionDesc> _multiConditionAsset; //MultiCondition
    public StateAsset _stateAsset;
}

[Serializable]
public class StateDesc
{
    public string _stataName;
    public AnimationClip _stateAnimationClip;

    public List<StateActionType> _EnterStateActionTypes;
    public List<StateActionType> _inStateActionTypes;
    public List<StateActionType> _ExitStateActionTypes;

    public List<StateLinkDesc> _linkedStates;
}

public class State
{
    public State(StateAsset stateAsset)
    {
        _stateDesc = stateAsset._myState; //���� �Ϸ�
        _stateAssetCreateFrom = stateAsset;
    }

    public class StateInitDesc
    {
        public PlayerScript _owner = null;
        public CharacterMoveScript2 _ownerMoveScript = null;
        public CharacterController _ownerCharacterController = null;
        public InputController _ownerInputController = null;
        public Animator _ownerAnimator = null;

        public AnimationCurve _animationHipCurveX = null;
        public AnimationCurve _animationHipCurveY = null;
        public AnimationCurve _animationHipCurveZ = null;

        public float _currStateSecond = 0.0f;
        public float _prevStateSecond = 0.0f;
        public float _prevReadedSecond = 0.0f;
    }


    private StateDesc _stateDesc; //Copy From ScriptableObject
    private StateAsset _stateAssetCreateFrom = null;
    private Dictionary<State/*Another State*/, List<Condition>> _linkedState = new Dictionary<State , List<Condition>>();
    private StateInitDesc _ownerActionComponent;
    private string _unityName_HipBone = "Hips";
    private string _unityName_HipBoneLocalPositionX = "RootT.x";
    private string _unityName_HipBoneLocalPositionY = "RootT.y";
    private string _unityName_HipBoneLocalPositionZ = "RootT.z";


    public StateDesc GetStateDesc() {return _stateDesc;}
    public StateAsset GetStateAssetFrom() {return _stateAssetCreateFrom;}
    


    public void Initialize(PlayerScript owner)
    {
        _ownerActionComponent = new StateInitDesc();

        _ownerActionComponent._owner = owner;

        InitPartial(ref _ownerActionComponent, _stateDesc._EnterStateActionTypes, owner);
        InitPartial(ref _ownerActionComponent, _stateDesc._inStateActionTypes, owner);
        InitPartial(ref _ownerActionComponent, _stateDesc._ExitStateActionTypes, owner);
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
                continue;
            }

            
            if (_linkedState.ContainsKey(targetState) == false)
            {
                _linkedState.Add(targetState, new List<Condition>());
            }

            List<Condition> existConditions = _linkedState[targetState];

            for (int i = 0; i < linked._multiConditionAsset.Count; i++)
            {
                Condition newCondition = new Condition(linked._multiConditionAsset[i]);
                newCondition.Initialize(owner);
                existConditions.Add(newCondition);
            }
        }
    }

    public State CheckChangeState()
    {
        foreach(KeyValuePair<State, List<Condition>> pair in _linkedState)
        {
            bool isSuccess = true;

            foreach (Condition condition in pair.Value) 
            {
                if (condition.CheckCondition() == false)
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
        _ownerActionComponent._currStateSecond += Time.deltaTime;

        foreach (var action in actions)
        {
            switch(action)
            {
                case StateActionType.Move:
                    {
                        _ownerActionComponent._ownerMoveScript.CharacterRotate(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                        _ownerActionComponent._ownerMoveScript.CharacterMove(_ownerActionComponent._ownerInputController._pr_directionByInput, 1.0f);
                    }

                    break;

                case StateActionType.Attack:
                    break;

                case StateActionType.SaveLatestVelocity:
                    break;

                case StateActionType.Jump:
                    {
                        _ownerActionComponent._ownerMoveScript.DoJump();
                    }
                    
                    break;

                case StateActionType.ForcedMove:
                    {
                        Vector3 planeVelocity = _ownerActionComponent._ownerMoveScript.GetLatestVelocity();
                        planeVelocity.y = 0.0f;
                        _ownerActionComponent._ownerMoveScript.CharacterForcedMove(planeVelocity, 1.0f);
                    }

                    break;

                case StateActionType.ResetLatestVelocity:
                    break;

                case StateActionType.RootMove:
                    {
                        float currentSecond = _ownerActionComponent._owner.GetCurrAnimationClipSecond();
                        //float currentSecond = _ownerActionComponent._currStateSecond;

                        Vector3 currentUnityLocalHip = new Vector3
                            (
                            _ownerActionComponent._animationHipCurveX.Evaluate(currentSecond),
                            _ownerActionComponent._animationHipCurveY.Evaluate(currentSecond),
                            _ownerActionComponent._animationHipCurveZ.Evaluate(currentSecond)
                            );

                        float prevSecond = _ownerActionComponent._prevReadedSecond;
                        //float prevSecond = _ownerActionComponent._prevStateSecond;

                        if (prevSecond > currentSecond)//�ִϸ��̼��� �ٲ��? ���Ű� �� ũ��
                        {
                            prevSecond = 0.0f;
                        }

                        Vector3 prevUnityLocalHip = new Vector3
                            (
                            _ownerActionComponent._animationHipCurveX.Evaluate(prevSecond),
                            _ownerActionComponent._animationHipCurveY.Evaluate(prevSecond),
                            _ownerActionComponent._animationHipCurveZ.Evaluate(prevSecond)
                            );

                        Vector3 deltaLocalHip = (currentUnityLocalHip - prevUnityLocalHip);
                        Vector3 worldDelta = _ownerActionComponent._ownerCharacterController.transform.localToWorldMatrix * deltaLocalHip;
                        _ownerActionComponent._ownerCharacterController.Move(worldDelta);

                        //_ownerActionComponent._prevStateSecond = currentSecond;
                        _ownerActionComponent._prevReadedSecond = currentSecond;
                    }
                    break;

                case StateActionType.RotateWithoutInterpolate:
                    {

                    }
                    break;

                default:
                    Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                    break;
            }
        }
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

                case StateActionType.RootMove:
                    {
                        if (_ownerActionComponent._ownerAnimator == null)
                        {
                            _ownerActionComponent._ownerAnimator = owner.GetComponentInChildren<Animator>();
                            Debug.Assert(_ownerActionComponent._ownerAnimator != null, "RootMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        if (_ownerActionComponent._ownerCharacterController == null)
                        {
                            _ownerActionComponent._ownerCharacterController = owner.GetComponentInChildren<CharacterController>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterController != null, "RootMove�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }

                        //�ִϸ��̼� Ŀ�� ã�Ƴ���
                        {
                            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(_stateDesc._stateAnimationClip);

                            bool curveXFind = false, curveYFind = false, curveZFind = false;

                            foreach (var binding in bindings)
                            {
                                if (curveXFind == true && curveYFind == true && curveZFind == true)
                                { break; } //�� ã�ҽ��ϴ�.

                                //if (binding.path == _unityName_HipBone && binding.propertyName == _unityName_HipBoneLocalPositionX)
                                //{_ownerActionComponent._animationHipCurveX = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveXFind = true; }

                                //if (binding.path == _unityName_HipBone && binding.propertyName == _unityName_HipBoneLocalPositionY)
                                //{_ownerActionComponent._animationHipCurveY = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveYFind = true; }

                                //if (binding.path == _unityName_HipBone && binding.propertyName == _unityName_HipBoneLocalPositionZ)
                                //{_ownerActionComponent._animationHipCurveZ = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveZFind = true; }

                                if (binding.propertyName == _unityName_HipBoneLocalPositionX)
                                { _ownerActionComponent._animationHipCurveX = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveXFind = true; }

                                if (binding.propertyName == _unityName_HipBoneLocalPositionY)
                                { _ownerActionComponent._animationHipCurveY = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveYFind = true; }

                                if (binding.propertyName == _unityName_HipBoneLocalPositionZ)
                                { _ownerActionComponent._animationHipCurveZ = AnimationUtility.GetEditorCurve(_stateDesc._stateAnimationClip, binding); curveZFind = true; }
                            }

                            Debug.Assert((curveXFind == true && curveYFind == true && curveZFind == true), "Ŀ�갡 �������� �ʽ��ϴ�");
                        }

                    }
                    break;

                case StateActionType.RotateWithoutInterpolate:
                    {
                        if (_ownerActionComponent._ownerCharacterController == null)
                        {
                            _ownerActionComponent._ownerCharacterController = owner.GetComponentInChildren<CharacterController>();
                            Debug.Assert(_ownerActionComponent._ownerCharacterController != null, "RotateWithoutInterpolate�ൿ�� �ִµ� �� ������Ʈ�� �����ϴ�");
                        }
                    }
                    break;

                default:
                    Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                    break;
            }
        }
    }
}
