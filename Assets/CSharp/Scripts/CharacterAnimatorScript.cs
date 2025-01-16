using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;
using static BodyPartBlendingWork;
using static CharacterScript;



/*----------------------------------------------------
|TODO| �� ������Ʈ�� �� ������ ���� ������ �� �����ϴٰ�
���� ���� ������ �޴´ٸ�? ������ ���� �Ѳ����� ó���ϴ� 
������ �ʿ��ϴ�.
----------------------------------------------------*/



/*----------------------------------------------------
|TODO| 

���Ͱ� �÷��̾ ���̸� nullref ���� = ���ʹ� AimScript�� ���

Aim�� �ߴٰ� Ǯ�� ���̸� nullref ���� = �׿����� null�� trnasform�� �޾ƿͼ�

Lock On �޴ٰ� �ٷ� Aim�ϸ� �̵��ӵ� �� ����
----------------------------------------------------*/






public class BodyPartBlendingWork
{
    public enum BodyPartWorkType
    {
        ActiveNextLayer,
        ActiveAllLayer,
        DeActiveAllLayer,
        TurnOffAllLayer,
        ChangeAnimation,
        ChangeAnimation_ZeroFrame,
        AnimationPlayHold,
        AttatchObjet,
        DestroyWeapon,
        WaitCoroutineUnlock,
        UnLockCoroutine,
        SwitchHand,
        WorkComplete,
        OwnerChangeGrabFocusType,
        OwnerIncreaeWeaponIndex,
        End,
    }

    public BodyPartBlendingWork(BodyPartWorkType type)
    {
        _workType = type;
    }

    public WeaponGrabFocus _targetGrabFocusType = WeaponGrabFocus.Normal;
    public BodyPartWorkType _workType = BodyPartWorkType.End;
    public AnimationClip _animationClip = null;
    public GameObject _attachObjectPrefab = null;
    public GameObject _deleteObjectTarget = null;
    public CoroutineLock _coroutineLock = null;
    public Transform _weaponEquipTransform = null;
}



public class AnimatorBlendingDesc
{
    public AnimatorBlendingDesc(AnimatorLayerTypes type, Animator ownerAnimator)
    {
        _myPart = type;
        _ownerAnimator = ownerAnimator;
    }

    public AnimatorLayerTypes _myPart = AnimatorLayerTypes.FullBody;

    public Animator _ownerAnimator = null;

    public float _blendTarget = 0.0f;
    public float _blendTarget_Sub = 0.0f;
    public float _transitionSpeed = 7.0f;

    public bool _isUsingFirstLayer = false;
}




public class CharacterAnimatorScript : GameCharacterSubScript
{
    private Avatar _gameBasicAvatar = null;
    private GameObject _gameBasicCharacter = null;

    protected Animator _animator = null;
    protected GameObject _characterModelObject = null;

    protected AnimatorOverrideController _overrideController = null;
    protected AnimationClip _currAnimClip = null;

    private StateAsset _currStateAsset = null;
    private int _currAnimIndex = -1;


    protected List<bool> _bodyCoroutineStarted = new List<bool>();
    protected List<bool> _currentBusyAnimatorLayer = new List<bool>();
    protected List<Coroutine> _currentBodyCoroutine = new List<Coroutine>();
    protected List<Action_LayerType> _bodyPartDelegates = new List<Action_LayerType>();


    [SerializeField] protected int _currentBusyAnimatorLayer_BitShift = 0;
    [SerializeField] protected List<AnimatorLayerTypes> _usingBodyPart = new List<AnimatorLayerTypes>();
    protected List<AnimatorBlendingDesc> _partBlendingDesc = new List<AnimatorBlendingDesc>();
    protected List<LinkedList<BodyPartBlendingWork>> _bodyPartWorks = new List<LinkedList<BodyPartBlendingWork>>();

    [SerializeField] protected AnimationClip _ifWeaponIsLight = null;










    public int GetBusyLayer() { return _currentBusyAnimatorLayer_BitShift; }
    public Animator GetCurrActivatedAnimator() { return _animator; }
    public GameObject GetCurrActivatedModelObject() { return _characterModelObject; }
    public AnimationClip GetCurrAnimationClip() { return _currAnimClip; }
    public Rig GetCharacterRig() { return _characterRig; }
    public RigBuilder GetCharacterRigBuilder() { return _characterRigBuilder; }
    public float GetCharacterHeight() { return 2.0f; }

    public AnimatorStateInfo? GetCurrentStateAnimationInfo()
    {
        int targetLayer = (_partBlendingDesc[(int)AnimatorLayerTypes.FullBody]._isUsingFirstLayer == true)
        ? (int)AnimatorLayerTypes.FullBody
        : (int)AnimatorLayerTypes.FullBody + 1;

        return _animator.GetCurrentAnimatorStateInfo(targetLayer);
    }

    public bool GetIsInTransition(AnimatorLayerTypes type)
    {
        int targetLayer = (_partBlendingDesc[(int)type]._isUsingFirstLayer == true)
            ? (int)type
            : (int)type + 1;

        return _animator.IsInTransition(targetLayer);
    }


    protected RigBuilder _characterRigBuilder = null;
    protected Rig _characterRig = null;

    private void OnDestroy()
    {
        TimeScaler.Instance?.RemoveTimeChangeDelegate(TimeChanged);

        if (_playableGraph.IsValid() == true)
        {
            _playableGraph.Stop();
            _playableGraph.Destroy();
        }
    }

    private void TimeChanged(float timeScale)
    {
        _layerMixer.SetSpeed(timeScale);
    }


    private void LateUpdate()
    {
        if (_currStateAsset._myState._isSubAnimationStateMachineExist == false)
        {
            return;
        }

        int animationIndex = _owner.GCST<StateContoller>().SubAnimationStateIndex(_currStateAsset);

        if (animationIndex == _currAnimIndex)
        {
            return;
        }

        AnimationClip nextAnimation = _currStateAsset._myState._subAnimationStateMachine._animations[animationIndex];
        FullBodyAnimationChange(nextAnimation);
        _currAnimIndex = animationIndex;
    }

    private PlayableGraph _playableGraph;
    private AnimatorControllerPlayable _controllerPlayable;
    private AnimationPlayableOutput _playableOutput;
    private AnimationLayerMixerPlayable _layerMixer;
    private AvatarMask _currentAdditiveAvatarMask = null;

    private Coroutine _clipPlayableDetachCoroutine = null;

    private IEnumerator ClipPlayableDetachCoroutine(float time)
    {
        float timeACC = 0.0f;

        while (true)
        {
            timeACC += Time.deltaTime;

            if (timeACC >= time)
            {
                _clipPlayableDetachCoroutine = null;
                AnimationClipPlayable? oldPlayable = (AnimationClipPlayable)_layerMixer.GetInput(1);

                if (oldPlayable == null) 
                {
                    Debug.Assert(false, "ClipPlayable�� �Ҿ���Ƚ��ϴ�");
                    Debug.Break();
                }

                _layerMixer.DisconnectInput(1);
                oldPlayable.Value.Destroy();

                break;
            }

            yield return null;
        }
    }

    public override void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(CharacterAnimatorScript);

        _animator = GetComponentInChildren<Animator>();
        _gameBasicAvatar = _animator.avatar;

        Debug.Assert(_animator != null, "Animator�� ����");
        _characterModelObject = _animator.gameObject;
        _gameBasicCharacter = Instantiate(_characterModelObject, transform);
        _gameBasicCharacter.SetActive(false);

        _characterRig = GetComponentInChildren<Rig>();
        _characterRigBuilder = GetComponentInChildren<RigBuilder>();


        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;

        for (int i = 0; i < (int)AnimatorLayerTypes.End; i++)
        {
            _partBlendingDesc.Add(null);
            _currentBusyAnimatorLayer.Add(false);
            _bodyPartWorks.Add(new LinkedList<BodyPartBlendingWork>());
            _bodyCoroutineStarted.Add(false);
            _currentBodyCoroutine.Add(null);
        }

        foreach (var type in _usingBodyPart)
        {
            if (_partBlendingDesc[(int)type] != null)
            {
                continue;
            }

            _partBlendingDesc[(int)type] = new AnimatorBlendingDesc(type, _animator);
        }


        if (_partBlendingDesc[(int)AnimatorLayerTypes.FullBody] == null)
        {
            Debug.Assert(false, "FullBody�� �ݵ�� ����ؾ� �Ѵ�");
            Debug.Break();
            _partBlendingDesc[(int)AnimatorLayerTypes.FullBody] = new AnimatorBlendingDesc(AnimatorLayerTypes.FullBody, _animator);
        }

        if (_gameBasicAvatar == null)
        {
            Debug.Assert(false, "ĳ���� �ʱⰪ �ƹ�Ÿ�Դϴ�. �ݵ�� �������־���մϴ�");
            Debug.Break();
        }

        _playableGraph = PlayableGraph.Create("AvatarMaskChanger");
        _playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", _animator);
        _controllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, _animator.runtimeAnimatorController);


        _layerMixer = AnimationLayerMixerPlayable.Create(_playableGraph, 2);
        _layerMixer.ConnectInput(0, _controllerPlayable, 0);

        _layerMixer.SetLayerAdditive(1, true);
        _layerMixer.SetInputWeight(0, 1.0f);
        _playableOutput.SetSourcePlayable(_layerMixer);
        _playableGraph.Play();

        TimeScaler.Instance.AddTimeChangeDelegate(TimeChanged);
    }



    public void RunAdditivaAnimationClip(AvatarMask additiveAvatarMask, AnimationClip targetAnimationClip, bool isLoop, float weight)
    {
        _currentAdditiveAvatarMask = additiveAvatarMask;

        _layerMixer.SetInputWeight(1, weight);

        if (_currentAdditiveAvatarMask != additiveAvatarMask)
        {
            _layerMixer.SetLayerMaskFromAvatarMask(1, additiveAvatarMask);
            if (_layerMixer.IsLayerAdditive(1) == false)
            {
                Debug.Assert(false);
            }
            
        }

        if (_clipPlayableDetachCoroutine != null)
        {
            StopCoroutine(_clipPlayableDetachCoroutine);
            _clipPlayableDetachCoroutine = null;
        }

        if (isLoop == false)
        {
            _clipPlayableDetachCoroutine = StartCoroutine(ClipPlayableDetachCoroutine(targetAnimationClip.length));
        }

        AnimationClipPlayable oldPlayable = (AnimationClipPlayable)_layerMixer.GetInput(1);

        if (oldPlayable.IsValid() == true)
        {
            if (oldPlayable.GetAnimationClip() == targetAnimationClip)
            {
                oldPlayable.SetTime(0.0f);
                return;
            }

            _layerMixer.DisconnectInput(1);
            oldPlayable.Destroy();
        }

        AnimationClipPlayable newPlayable = AnimationClipPlayable.Create(_playableGraph, targetAnimationClip);


        bool temp = newPlayable.GetApplyFootIK();

        _layerMixer.ConnectInput(1, newPlayable, 0);

        _layerMixer.SetInputWeight(1, 1.0f);

        _layerMixer.SetLayerMaskFromAvatarMask(1, additiveAvatarMask);
    }

    public bool IsLayerConnected(int layerIndex)
    {
        if (_layerMixer.GetInputCount() <= layerIndex)
        {
            return false;
        }

        return _layerMixer.GetInput(layerIndex).IsValid();
    }

    public void StopAdditivaAnimationClip()
    {
        if (IsLayerConnected(1) == false)
        {
            return;
        }

        AnimationClipPlayable? oldPlayable = (AnimationClipPlayable)_layerMixer.GetInput(1);
        _layerMixer.DisconnectInput(1);
        oldPlayable.Value.Destroy();
    }


    public override void SubScriptStart()
    {
        _owner.GCST<CharacterColliderScript>().InitModelCollider(_characterModelObject);
    }


    public void ResetCharacterModel()
    {
        ModelChange(_gameBasicCharacter);
    }



    public bool IsSameSkeleton(Avatar newModelAvatar)
    {
        return (newModelAvatar == _gameBasicAvatar);
    }

    public GameObject ModelChange(GameObject modelObject)
    {
        GameObject newModel = Instantiate(modelObject);
        newModel.SetActive(true);
        SkinnedMeshRenderer[] meshRenderers = newModel.GetComponents<SkinnedMeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = false;
        }

        newModel.transform.SetParent(transform);
        newModel.transform.localPosition = Vector3.zero;
        newModel.transform.rotation = _characterModelObject.transform.rotation;

        Animator newAnimator = newModel.GetComponentInChildren<Animator>();
        newAnimator.runtimeAnimatorController = new AnimatorOverrideController(newAnimator.runtimeAnimatorController);

        SyncAnimatorState(_animator, newAnimator);

        StartCoroutine(WaitNextFrameCoroutine(newModel, meshRenderers));

        return newModel;
    }


    void SyncAnimatorState(Animator sourceAnimator, Animator targetAnimator)
    {
        //0. Playable ����ȭ
        {
            float firstLayerWeight = _layerMixer.GetInputWeight(0);
            float secondLayerWeight = _layerMixer.GetInputWeight(1);

            _layerMixer.DisconnectInput(0);

            AnimatorControllerPlayable oldPlayable = _controllerPlayable;
            _controllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, targetAnimator.runtimeAnimatorController);

            _layerMixer.ConnectInput(0, _controllerPlayable, 0);

            _layerMixer.SetInputWeight(0, firstLayerWeight);
            _layerMixer.SetInputWeight(1, secondLayerWeight);

            _playableOutput.SetTarget(targetAnimator);

            oldPlayable.Destroy();
        }

        //1. ���� ����ȭ
        {
            foreach (AnimatorControllerParameter parameter in sourceAnimator.parameters)
            {
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        bool boolValue = sourceAnimator.GetBool(parameter.name);
                        targetAnimator.SetBool(parameter.name, boolValue);
                        break;

                    case AnimatorControllerParameterType.Float:
                        float floatValue = sourceAnimator.GetFloat(parameter.name);
                        targetAnimator.SetFloat(parameter.name, floatValue);
                        break;

                    case AnimatorControllerParameterType.Int:
                        int intValue = sourceAnimator.GetInteger(parameter.name);
                        targetAnimator.SetInteger(parameter.name, intValue);
                        break;

                    case AnimatorControllerParameterType.Trigger:
                        if (sourceAnimator.GetBool(parameter.name))
                        {
                            targetAnimator.SetTrigger(parameter.name);
                        }
                        break;
                }
            }
        }

        //2. ��Ÿ�� Ŭ�� ����ȭ
        {
            AnimatorOverrideController sourceOverrideController = sourceAnimator.runtimeAnimatorController as AnimatorOverrideController;
            AnimatorOverrideController targeteOverrideController = targetAnimator.runtimeAnimatorController as AnimatorOverrideController;


            List<KeyValuePair<AnimationClip, AnimationClip>> paris = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            sourceOverrideController.GetOverrides(paris);

            foreach (var clipPair in paris)
            {
                if (clipPair.Value == null)
                {
                    continue;
                }
                targeteOverrideController[clipPair.Key] = clipPair.Value;
            }
        }

        //3. Weight ����ȭ
        {
            int index = 0;

            foreach (AnimatorBlendingDesc desc in _partBlendingDesc)
            {
                desc._ownerAnimator = targetAnimator;

                for (int i = 0; i < 2; i++)
                {
                    AnimatorStateInfo stateInfo = sourceAnimator.GetCurrentAnimatorStateInfo(index);
                    targetAnimator.Play(stateInfo.shortNameHash, index, stateInfo.normalizedTime);
                    targetAnimator.SetLayerWeight(index, _animator.GetLayerWeight(index));

                    index++;
                }
            }
        }




    }

    private IEnumerator WaitNextFrameCoroutine(GameObject newModel, SkinnedMeshRenderer[] meshRenderers)
    {
        yield return new WaitForEndOfFrame();

        GameObject destroyThis = _characterModelObject;

        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = true;
        }

        _characterModelObject = newModel;
        _characterRig = _characterModelObject.GetComponentInChildren<Rig>();
        _characterRigBuilder = _characterModelObject.GetComponentInChildren<RigBuilder>();

        _animator = newModel.GetComponentInChildren<Animator>();
        _overrideController = _animator.runtimeAnimatorController as AnimatorOverrideController;

        _owner.GetComponentInChildren<AimScript2>().SetRigging(_characterRigBuilder, _characterRig);
        _owner.MoveWeapons(newModel);

        CharacterColliderScript ownerCharacterColliderScript = GetComponentInParent<CharacterColliderScript>();
        ownerCharacterColliderScript.InitModelCollider(newModel);



        Destroy(destroyThis);


    }







    public void StateChanged(StateAsset nextState)
    {
        _currStateAsset = nextState;

        AnimationClip nextAnimation = nextState._myState._stateAnimationClip;

        if (nextState._myState._isSubAnimationStateMachineExist == true)
        {
            //�ִϸ��̼��� �ٽ� �����ؾ��Ѵ�.

            int animationIndex = _owner.GCST<StateContoller>().SubAnimationStateIndex(nextState);

            _currAnimIndex = animationIndex;

            nextAnimation = nextState._myState._subAnimationStateMachine._animations[animationIndex];
        }

        FullBodyAnimationChange(nextAnimation);
    }

    private void FullBodyAnimationChange(AnimationClip nextAnimation)
    {
        _currAnimClip = nextAnimation;
        //FullBody Animation ����
        {
            /*-------------------------------------------------------------------
            |NOTI| FullBody�� Ư���մϴ�. ���� ���ϰ��ִٸ� ����ϰ� �����ؾ� �մϴ�.
            -------------------------------------------------------------------*/

            AnimatorLayerTypes fullBodyLayer = AnimatorLayerTypes.FullBody;
            int fullBodyLayerIndex = (int)fullBodyLayer;

            if (_currentBodyCoroutine[(int)AnimatorLayerTypes.FullBody] != null)
            {
                StopCoroutine(_currentBodyCoroutine[(int)AnimatorLayerTypes.FullBody]);
                _bodyPartWorks[fullBodyLayerIndex].Clear();
            }

            _bodyPartWorks[fullBodyLayerIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                _bodyPartWorks[fullBodyLayerIndex].Last.Value._animationClip = nextAnimation;
            }
            _bodyPartWorks[fullBodyLayerIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

            StartProceduralWork(fullBodyLayer, _bodyPartWorks[fullBodyLayerIndex].First.Value);
        }
    }

    protected void UpdateBusyAnimatorLayers(int willBusyLayers_BitShift, bool isOn)
    {
        if (isOn == true)
        {
            _currentBusyAnimatorLayer_BitShift = (_currentBusyAnimatorLayer_BitShift | willBusyLayers_BitShift);
        }
        else
        {
            _currentBusyAnimatorLayer_BitShift = (_currentBusyAnimatorLayer_BitShift & ~willBusyLayers_BitShift);
        }
    }

    public void WeaponLayerChange(WeaponGrabFocus grapFocusType, StateAsset currState, bool isUsingRightWeapon, bool isEnter)
    {
        bool isAttackState = isEnter;

        AnimatorBlendingDesc leftHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.LeftHand];
        int leftHandMainLayer = (leftHandBlendingDesc._isUsingFirstLayer == true)
            ? (int)AnimatorLayerTypes.LeftHand * 2
            : (int)AnimatorLayerTypes.LeftHand * 2 + 1;
        AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
        int rightHandMainLayer = (rightHandBlendingDesc._isUsingFirstLayer == true)
            ? (int)AnimatorLayerTypes.RightHand * 2
            : (int)AnimatorLayerTypes.RightHand * 2 + 1;

        bool isMirrored = (isUsingRightWeapon == false);

        switch (grapFocusType)
        {
            case WeaponGrabFocus.Normal:
                {
                    //���� �ִϸ��̼��̴�
                    if (isAttackState == true)
                    {
                        int usingHandLayer = (isUsingRightWeapon == true)
                            ? rightHandMainLayer
                            : leftHandMainLayer;

                        int oppositeHandLayer = (isUsingRightWeapon == true)
                            ? leftHandMainLayer
                            : rightHandMainLayer;


                        _animator.SetLayerWeight(usingHandLayer, 0.0f); //�޼��� �ݵ�� ���󰡾��ؼ� 0.0f

                        WeaponScript oppositeWeaponScript = _owner.GetCurrentWeaponScript(!isUsingRightWeapon);

                        if (oppositeWeaponScript == null) //�޼չ��⸦ ������� �ʴٸ�
                        {
                            _animator.SetLayerWeight(oppositeHandLayer, 0.0f); //�޼� ���⸦ ������� �ʴٸ� ����� ���󰡾��ؼ� ���̾ ��������.
                        }
                        else
                        {
                            if (oppositeWeaponScript._handlingIdleAnimation_OneHand == null)
                            {
                                //_overrideController[MyUtil._motionChangingAnimationNames[oppositeHandLayer]] = _tempWeaponHandling_NoPlay;
                            }
                            _animator.SetLayerWeight(oppositeHandLayer, 1.0f);
                        }

                        _animator.SetBool("IsMirroring", isMirrored);
                    }
                    //���� �ִϸ��̼��� �ƴϴ�
                    else
                    {

                        WeaponScript leftHandWeaponScript = _owner.GetCurrentWeaponScript(false);
                        WeaponScript rightHandWeaponScript = _owner.GetCurrentWeaponScript(true);

                        //�޼� ���⸦ ������� �ʰų� �޼� ������ ���� �ִϸ��̼��� ����
                        float leftHandLayerWeight = (leftHandWeaponScript == null || leftHandWeaponScript._handlingIdleAnimation_OneHand == null) //
                            ? 0.0f
                            : 1.0f;
                        _animator.SetLayerWeight(leftHandMainLayer, leftHandLayerWeight);

                        //������ ���⸦ ������� �ʰų� ������ ������ ���� �ִϸ��̼��� ����
                        float rightHandLayerWeight = (rightHandWeaponScript == null || rightHandWeaponScript._handlingIdleAnimation_OneHand == null)
                            ? 0.0f
                            : 1.0f;
                        _animator.SetLayerWeight(rightHandMainLayer, rightHandLayerWeight);

                        _animator.SetBool("IsMirroring", false);
                    }
                }
                break;

            case WeaponGrabFocus.RightHandFocused:
            case WeaponGrabFocus.LeftHandFocused:
                {
                    float layerWeight = (isAttackState == true)
                        ? 0.0f
                        : 1.0f;

                    _animator.SetLayerWeight(rightHandMainLayer, layerWeight);
                    _animator.SetLayerWeight(leftHandMainLayer, layerWeight);

                    _animator.SetBool("IsMirroring", isMirrored);

                    if (isAttackState == true)
                    {
                        _animator.SetBool("IsMirroring", isMirrored);
                    }

                    if (currState._myState._isAttackState == true)
                    {
                        if (isAttackState == false)
                        {
                            _animator.SetBool("IsMirroring", isMirrored);
                        }
                    }
                    else
                    {
                        _animator.SetBool("IsMirroring", false);
                    }
                }
                break;

            case WeaponGrabFocus.DualGrab:
                {
                    Debug.Assert(false, "�ּ� �������� �߰��ƽ��ϱ�?");
                }
                break;
        }
    }



    public void CalculateBodyWorkType_ChangeWeapon(WeaponGrabFocus ownerWeaponGrabFocusType, bool isRightWeaponTrigger, int layerLockResult, bool forcedGo = false)
    {
        HashSet<int> lockCompares = new HashSet<int>();

        AnimatorLayerTypes oppositePartType = (isRightWeaponTrigger == true)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;
        int oppositePartIndex = (int)oppositePartType;

        AnimatorLayerTypes targetPartType = (isRightWeaponTrigger == true)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;
        int targetPartIndex = (int)targetPartType;

        //�ݴ�� Work ����
        {
            WeaponScript currentOppositeWeaponScript = _owner.GetCurrentWeaponScript(oppositePartType);

            if (ownerWeaponGrabFocusType == WeaponGrabFocus.RightHandFocused || ownerWeaponGrabFocusType == WeaponGrabFocus.LeftHandFocused) //���⸦ ������� ����־���.
            {
                if (currentOppositeWeaponScript != null) //�ݴ�տ� ���Ⱑ �ִ�.
                {
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                    {
                        _bodyPartWorks[(int)oppositePartType].Last.Value._animationClip = currentOppositeWeaponScript.GetOneHandHandlingAnimation(oppositePartType);
                    }

                    //���̾� ����Ʈ �ٲٱ�
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                }
                else
                {
                    //�ݴ�տ� ������� �����鼭 ������� ���Ⱑ ����.
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
                }

                lockCompares.Add(oppositePartIndex);
            }
        }

        //Target Work ����
        {
            WeaponScript currTargetWeaponScript = _owner.GetCurrentWeaponScript(targetPartType);

            if (currTargetWeaponScript != null)
            {
                //���� ����ֱ� �ִϸ��̼����� �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                {
                    _bodyPartWorks[(int)targetPartType].Last.Value._animationClip = currTargetWeaponScript.GetPutawayAnimation(targetPartType);
                }

                //LayerWeight �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                //�� ����Ҷ����� ����ϱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
                //��������ϱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DestroyWeapon));
            }

            GameObject nextWeaponPrefab = _owner.GetNextWeaponPrefab(targetPartType);
            WeaponScript nextWeaponScript = _owner.GetNextWeaponScript(targetPartType);

            if (nextWeaponScript != null)
            {
                //�ٲٷ��� ���� ���Ⱑ �ִ�

                //���Ⲩ���� ZeroFrame ���� �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
                {
                    _bodyPartWorks[(int)targetPartType].Last.Value._animationClip = nextWeaponScript.GetDrawAnimation(targetPartType);
                }

                //Layer Wright �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

                //��������ֱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AttatchObjet));
                {
                    _bodyPartWorks[(int)targetPartType].Last.Value._attachObjectPrefab = nextWeaponPrefab;
                }

                //���Ⲩ���� �ִϸ��̼����� �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                {
                    _bodyPartWorks[(int)targetPartType].Last.Value._animationClip = nextWeaponScript.GetDrawAnimation(targetPartType);
                }

                //Layer Wright �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                //�� ����Ҷ����� ��ٸ���
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
            }
            else
            {
                //�ٲٷ��� ���� ���Ⱑ ����

                //���̾� ��������
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
            }

            lockCompares.Add(targetPartIndex);
        }

        _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.OwnerIncreaeWeaponIndex));

        int usingLockResult = 0;
        foreach (int index in lockCompares)
        {
            usingLockResult = usingLockResult | (1 << index);
        }

        if (forcedGo == false && 
            (usingLockResult != layerLockResult))
        {
            Debug.Assert(false, "����Ϸ��� ������ Lock�� ��ġ���� �ʽ��ϴ�. �ִϸ��̼� ������ �����ϼ���");
            Debug.Break();
            return;
        }

        _bodyPartWorks[targetPartIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.OwnerChangeGrabFocusType));
        {
            _bodyPartWorks[targetPartIndex].Last.Value._targetGrabFocusType = WeaponGrabFocus.Normal;
        }

        UpdateBusyAnimatorLayers(usingLockResult, true);

        foreach (int index in lockCompares)
        {
            StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        }
    }

    public void CalculateBodyWorkType_ChangeFocus(bool isRightHandTrigger, int layerLockResult)
    {
        HashSet<int> lockCompares = new HashSet<int>();

        AnimatorLayerTypes oppositePartType = (isRightHandTrigger == true)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;
        int oppositeBodyIndex = (int)oppositePartType;

        AnimatorLayerTypes targetPartType = (isRightHandTrigger == true)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;
        int targetBodyIndex = (int)targetPartType;

        GameObject targetWeapon = _owner.GetCurrentWeaponPrefab(targetPartType);

        WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();

        GameObject oppositeWeapon = _owner.GetCurrentWeaponPrefab(oppositePartType);

        //�ݴ� �տ� ���⸦ ����ִ�.
        if (oppositeWeapon != null)
        {
            //�ݴ���� ���� ����ֱ� �ִϸ��̼����� �ٲ۴�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                WeaponScript oppositeWeaponScript = oppositeWeapon.GetComponent<WeaponScript>();
                _bodyPartWorks[(int)oppositeBodyIndex].Last.Value._animationClip = oppositeWeaponScript.GetPutawayAnimation(oppositePartType);
            }

            //Layer Weight�� ���� �ø���.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
            //�ݴ���� ���� ����ֱ� �ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
            //�ݴ���� ���⸦ �����Ѵ�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DestroyWeapon));

            lockCompares.Add(oppositeBodyIndex);
        }

        //�� Behave���� �ܰ踦 �Ѿ�� �ڷ�ƾ ���� �����Ͽ� �˷��ش�.
        CoroutineLock waitOppsiteWeaponPutAwayLock = new CoroutineLock();
        _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.UnLockCoroutine));
        {
            _bodyPartWorks[oppositeBodyIndex].Last.Value._coroutineLock = waitOppsiteWeaponPutAwayLock;
        }

        //�ݴ���� �ִϸ��̼��� �ٲ۴�
        _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        {
            _bodyPartWorks[(int)oppositeBodyIndex].Last.Value._animationClip = targetWeaponScript.GetTwoHandHandlingAnimation(targetPartType);
        }

        //�ݴ���� LayerWeight�� �ø���.
        _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        lockCompares.Add(oppositeBodyIndex);


        //�ش� ���⸦ ������� ��� �ִϸ��̼��� �����Ѵ�. �ش� ���� �ִϸ��̼��� �ٲ۴�
        {
            //�ݴ���� �̺�Ʈ�� ���������� ���� ����Ѵ�
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.WaitCoroutineUnlock));
            {
                _bodyPartWorks[targetBodyIndex].Last.Value._coroutineLock = waitOppsiteWeaponPutAwayLock;
            }
            //�ش� ���� �ִϸ��̼��� �ٲ۴�
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                _bodyPartWorks[(int)targetBodyIndex].Last.Value._animationClip = targetWeaponScript.GetTwoHandHandlingAnimation(targetPartType);
            }
            //�ش� ���� Layer Weight�� �ٲ۴�.
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

            lockCompares.Add(targetBodyIndex);
        }

        _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.OwnerChangeGrabFocusType));
        {
            _bodyPartWorks[(int)targetBodyIndex].Last.Value._targetGrabFocusType = (isRightHandTrigger == true)
                ? WeaponGrabFocus.RightHandFocused
                : WeaponGrabFocus.LeftHandFocused;
        }

        int usingLockResult = 0;
        foreach (int index in lockCompares)
        {
            usingLockResult = usingLockResult | (1 << index);
        }

        if (usingLockResult != layerLockResult)
        {
            Debug.Assert(false, "����Ϸ��� ������ Lock�� ��ġ���� �ʽ��ϴ�. �ִϸ��̼� ������ �����ϼ���");
            Debug.Break();
            return;
        }

        UpdateBusyAnimatorLayers(usingLockResult, true);

        foreach (int index in lockCompares)
        {
            StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        }
    }

    public void CalculateBodyWorkType_ChangeFocus_ReleaseMode(bool isRightHandTrigger, int layerLockResult)
    {
        HashSet<int> lockCompares = new HashSet<int>();

        AnimatorLayerTypes oppositePartType = (isRightHandTrigger == true)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;
        int oppositeBodyIndex = (int)oppositePartType;

        AnimatorLayerTypes targetPartType = (isRightHandTrigger == true)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;
        int targetBodyIndex = (int)targetPartType;


        GameObject oppositeWeaponPrefab = _owner.GetCurrentWeaponPrefab(oppositePartType);

        //�ݴ� �տ� ������� ��� ��, ���⸦ ����־���.
        if (oppositeWeaponPrefab != null)
        {
            //�ݴ���� ���� ������ Zero Frame �ִϸ��̼����� �ٲ۴�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
            {
                WeaponScript oppositeWeaponScript = oppositeWeaponPrefab.GetComponent<WeaponScript>();
                _bodyPartWorks[oppositeBodyIndex].Last.Value._animationClip = oppositeWeaponScript.GetDrawAnimation(oppositePartType);
            }

            //�ݴ�� Layer Weight�� ���� �ø���.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
            //���⸦ ����ش�
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AttatchObjet));
            {
                _bodyPartWorks[oppositeBodyIndex].Last.Value._attachObjectPrefab = oppositeWeaponPrefab;
            }

            //�ݴ���� ���� ������ �ִϸ��̼����� �ٲ۴�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                WeaponScript oppositeWeaponScript = oppositeWeaponPrefab.GetComponent<WeaponScript>();
                _bodyPartWorks[oppositeBodyIndex].Last.Value._animationClip = oppositeWeaponScript.GetDrawAnimation(oppositePartType);
            }

            //�ݴ�� Layer Weight�� ���� �ø���.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

            //�ݴ���� ���� ������ �ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
        }
        else
        {
            //�׳� ��������
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
        }

        lockCompares.Add(oppositeBodyIndex);

        GameObject targetWeapon = _owner.GetCurrentWeaponPrefab(targetPartType);

        WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();

        AnimationClip oneHandHadlingAnimation = targetWeaponScript.GetOneHandHandlingAnimation(targetPartType);

        if (oneHandHadlingAnimation != null)
        {
            //�ڼ� �ִϸ��̼��� �ֽ��ϴ�.

            //�ش� ���� �ִϸ��̼��� �ٲ۴�
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                _bodyPartWorks[targetBodyIndex].Last.Value._animationClip = oneHandHadlingAnimation;
            }
            //�ش� ���� LayerWeight�� �ٲ۴�
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
        }
        else
        {
            //�ڼ� �ִϸ��̼��� �����ϴ�.
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
        }

        _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.OwnerChangeGrabFocusType));
        {
            _bodyPartWorks[(int)targetBodyIndex].Last.Value._targetGrabFocusType = WeaponGrabFocus.Normal;
        }

        lockCompares.Add(targetBodyIndex);

        int usingLockResult = 0;
        foreach (int index in lockCompares)
        {
            usingLockResult = usingLockResult | (1 << index);
        }

        if (usingLockResult != layerLockResult)
        {
            Debug.Assert(false, "����Ϸ��� ������ Lock�� ��ġ���� �ʽ��ϴ�. �ִϸ��̼� ������ �����ϼ���");
            Debug.Break();
            return;
        }

        UpdateBusyAnimatorLayers(usingLockResult, true);

        foreach (int index in lockCompares)
        {
            StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        }
    }

    public void CalculateBodyWorkType_UseItem_Drink(WeaponGrabFocus ownerWeaponGrabFocusType, ItemAsset usingItemInfo, int layerLockResult)
    {
        Debug.Assert(false, "�̰��� �����ؾ��մϴ�");
        Debug.Break();

        //HashSet<int> lockCompares = new HashSet<int>();

        //int rightHandIndex = (int)AnimatorLayerTypes.RightHand;

        //Transform rightHandWeaponOriginalTransform = null;

        //WeaponScript weaponScript = _owner.GetCurrentWeaponScript(AnimatorLayerTypes.RightHand);


        ////���⸦ �����տ� ������� ����ֽ��ϱ�?
        //if (ownerWeaponGrabFocusType == WeaponGrabFocus.RightHandFocused)
        //{
        //    //�����տ� ������� ��� �־��ٸ� ��� �޼տ� ����ش�.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.SwitchHand));
        //    {
        //        WeaponSocketScript.SideType targetSide = WeaponSocketScript.SideType.Left;

        //        //���� ����ִ� Ʈ������ ĳ��
        //        rightHandWeaponOriginalTransform = weaponScript._socketTranform;

        //        GameObject ownerModelObject = _characterModelObject;

        //        //�ݴ�� ���� ã��
        //        {
        //            Debug.Assert(ownerModelObject != null, "���⸦ ���̷��µ� ���� ����� �ȵȴ�");

        //            WeaponSocketScript[] weaponSockets = ownerModelObject.GetComponentsInChildren<WeaponSocketScript>();

        //            Debug.Assert(weaponSockets.Length > 0, "���⸦ ���̷��µ� �𵨿� ������ ����");

        //            ItemInfo.WeaponType targetType = weaponScript._weaponType;

        //            foreach (var socketComponent in weaponSockets)
        //            {
        //                if (socketComponent._sideType != targetSide)
        //                {
        //                    continue;
        //                }

        //                foreach (var type in socketComponent._equippableWeaponTypes)
        //                {
        //                    if (type == targetType)
        //                    {
        //                        _bodyPartWorks[rightHandIndex].Last.Value._weaponEquipTransform = socketComponent.gameObject.transform;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //else if (weaponScript != null) //������� ������� �ʾҽ��ϴ�. �ٵ� �����տ� ���⸦ ��� �߽��ϴ�.
        //{
        //    //�ݴ���� ���� ����ֱ� �ִϸ��̼����� �ٲ۴�.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //    {
        //        _bodyPartWorks[rightHandIndex].Last.Value._animationClip = weaponScript.GetPutawayAnimation(true);
        //    }

        //    //Layer Weight�� ���� �ø���.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
        //    //�ݴ���� ���� ����ֱ� �ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
        //    //�ݴ���� ���⸦ �����Ѵ�.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DestroyWeapon));
        //}

        //CoroutineLock waitForRightHandReady = new CoroutineLock();
        //_bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.UnLockCoroutine));
        //{
        //    _bodyPartWorks[rightHandIndex].Last.Value._coroutineLock = waitForRightHandReady;
        //}
        //lockCompares.Add(rightHandIndex);

        ////�������� ����ϴ� �����鿡�� �ϰ��� �����Ѵ�.
        //{
        //    List<AnimatorLayerTypes> mustUsingLayers = usingItemInfo._usingItemMustNotBusyLayers;

        //    foreach (AnimatorLayerTypes type in mustUsingLayers)
        //    {
        //        int typeIndex = (int)type;
        //        if (lockCompares.Contains(typeIndex) == false)
        //        {
        //            _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.WaitCoroutineUnlock));
        //            {
        //                _bodyPartWorks[typeIndex].Last.Value._coroutineLock = waitForRightHandReady;
        //            }
        //        }


        //        //�������� ����ϴ� �ִϸ��̼����� �ٲ۴�.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //        {
        //            _bodyPartWorks[typeIndex].Last.Value._animationClip = usingItemInfo._usingItemAnimation;
        //        }

        //        //Layer Weight�� ���� �ø���.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        //        //�ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));

        //        //Layer Weight�� ��� ��������.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.TurnOffAllLayer));

        //        lockCompares.Add(typeIndex);
        //    }

        //}

        //GameObject rightWeaponPrefab = _owner.GetCurrentWeaponPrefab(AnimatorLayerTypes.RightHand);

        //if (rightWeaponPrefab != null) //�����տ� ���� �վ����ϴ�.
        //{
        //    WeaponScript rightWeaponScript = rightWeaponPrefab.GetComponent<WeaponScript>();

        //    if (ownerWeaponGrabFocusType == WeaponGrabFocus.RightHandFocused)
        //    {
        //        //������� ��� �ִϸ��̼����� �ٲ۴�.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetTwoHandHandlingAnimation(true);
        //        }

        //        //Layer Weight�� ���� �ø���.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        //        //�Ʊ� �ݴ�տ� ������ ���⸦ �ٽ� �ݴ�� ���.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.SwitchHand));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._weaponEquipTransform = rightHandWeaponOriginalTransform;
        //        }
        //    }
        //    else
        //    {
        //        //���⸦ ������ Zero Frame Animation ���� �ٲ۴�
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetDrawAnimation(true);
        //        }

        //        //Layer Weight�� ���� �ø���.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        //        //���⸦ ����ش�
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AttatchObjet));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._attachObjectPrefab = rightWeaponPrefab;
        //        }

        //        //���⸦ ������ Zero Frame Animation ���� �ٲ۴�
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetDrawAnimation(true);
        //        }

        //        //Layer Weight�� ���� �ø���.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
        //    }

        //    lockCompares.Add(rightHandIndex);
        //}
        //else
        //{
        //    //�׳� ��������
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));

        //    lockCompares.Add(rightHandIndex);
        //}

        //int usingLockResult = 0;
        //foreach (int index in lockCompares)
        //{
        //    usingLockResult = usingLockResult | (1 << index);
        //}

        //if (usingLockResult != layerLockResult)
        //{
        //    Debug.Assert(false, "����Ϸ��� ������ Lock�� ��ġ���� �ʽ��ϴ�. �ִϸ��̼� ������ �����ϼ���");
        //    Debug.Break();
        //    return;
        //}

        //UpdateBusyAnimatorLayers(usingLockResult, true);

        //foreach (int index in lockCompares)
        //{
        //    StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        //}
    }
    
    public List<bool> GetCurrentBusyAnimatorLayer()
    {
        return _currentBusyAnimatorLayer;
    }

    public int GetCurrentBusyAnimatorLayer_BitShift()
    {
        return _currentBusyAnimatorLayer_BitShift;
    }

    public void WeaponChange_Animation(AnimationClip nextAnimation, AnimatorLayerTypes targetBody, bool isZeroFrame)
    {
        AnimatorBlendingDesc targetPart = _partBlendingDesc[(int)targetBody];

        targetPart._isUsingFirstLayer = !(targetPart._isUsingFirstLayer);

        int layerStartIndex = (int)targetBody * 2;

        int layerIndex = (targetPart._isUsingFirstLayer == true)
            ? layerStartIndex
            : layerStartIndex + 1;

        string nextNodeName = (targetPart._isUsingFirstLayer == true)
            ? "State1"
            : "State2";

        AnimationClip targetclip = (isZeroFrame == true)
            ? ResourceDataManager.Instance.GetZeroFrameAnimation(nextAnimation.name)
            : nextAnimation;

        _overrideController[MyUtil._motionChangingAnimationNames[layerIndex]] = targetclip;

        _animator.Play(nextNodeName, layerIndex, 0.0f);
    }

    #region Coroutines

    protected IEnumerator SwitchHandCoroutine(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        _owner.WeaponSwitchHand(layerType, work);
        return null;
    }

    protected IEnumerator DestroyWeaponCoroutine(AnimatorLayerTypes layerType)
    {
        _owner.DestroyWeapon(layerType);
        return null;
    }

    protected IEnumerator WaitCoroutineLock(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        while (work._coroutineLock._isEnd == false)
        {
            yield return null;
        }
    }

    protected IEnumerator UnlockCoroutineLock(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        work._coroutineLock._isEnd = true;
        return null;
    }

    protected IEnumerator AnimationPlayHoldCoroutine(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        AnimatorBlendingDesc blendingDesc = _partBlendingDesc[(int)layerType];

        while (true)
        {
            int layerIndex = (blendingDesc._isUsingFirstLayer == true)
                ? (int)layerType * 2
                : (int)layerType * 2 + 1;

            float normalizedTime = _animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;

            if (normalizedTime >= 1.0f)
            {
                break;
            }

            yield return null;
        }
    }

    protected IEnumerator OwnerIncreaseWeaponIndex(AnimatorLayerTypes targetBody)
    {
        _owner.IncreaseWeaponIndex(targetBody);
        return null;
    }

    protected IEnumerator WeaponChange_AnimationCoroutine(AnimationClip nextAnimation, AnimatorLayerTypes targetBody, bool isZeroFrame)
    {
        WeaponChange_Animation(nextAnimation, targetBody, isZeroFrame);
        return null;
    }

    protected IEnumerator CreateWeaponModelAndEquipCoroutine(AnimatorLayerTypes layerType, GameObject nextWeaponPrefab)
    {
        CreateWeaponModelAndEquip(layerType, nextWeaponPrefab);
        return null;
    }

    protected IEnumerator OwnerChangeWeaponGrabFocusType(WeaponGrabFocus targetType)
    {
        _owner.ChangeGrabFocusType(targetType);
        return null;
    }

    protected void CreateWeaponModelAndEquip(AnimatorLayerTypes layerType, GameObject nextWeaponPrefab)
    {
        _owner.CreateWeaponModelAndEquip(layerType, nextWeaponPrefab);
    }

    protected IEnumerator ChangeNextLayerWeightSubCoroutine_ActiveNextLayer(AnimatorLayerTypes layerType)
    {
        AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];

        if (targetBlendingDesc == null)
        {
            Debug.Assert(false, "������� �ʴ� ��Ʈ�� ���� �Լ��� ȣ���߽��ϴ�");
            Debug.Break();
            yield break;
        }

        int targetHandMainLayerIndex = (int)targetBlendingDesc._myPart * 2;
        int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

        while (true)
        {
            float mainLayerBlendDelta = 0.0f;
            float subLayerBlendDelta = 0.0f;

            mainLayerBlendDelta = (targetBlendingDesc._isUsingFirstLayer == true)
                ? targetBlendingDesc._transitionSpeed * Time.deltaTime
                : targetBlendingDesc._transitionSpeed * Time.deltaTime * -1.0f;

            subLayerBlendDelta = -1.0f * mainLayerBlendDelta;

            targetBlendingDesc._blendTarget += mainLayerBlendDelta;
            targetBlendingDesc._blendTarget_Sub += subLayerBlendDelta;

            targetBlendingDesc._blendTarget = Mathf.Clamp(targetBlendingDesc._blendTarget, 0.0f, 1.0f);
            targetBlendingDesc._blendTarget_Sub = Mathf.Clamp(targetBlendingDesc._blendTarget_Sub, 0.0f, 1.0f);

            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, targetBlendingDesc._blendTarget);
            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, targetBlendingDesc._blendTarget_Sub);

            float target = (targetBlendingDesc._isUsingFirstLayer == true)
                ? targetBlendingDesc._blendTarget
                : targetBlendingDesc._blendTarget_Sub;

            if (target >= 1.0f || target <= 0.0f)
            {
                break;
            }

            yield return null;
        }
    }

    protected IEnumerator ChangeNextLayerWeightSubCoroutine_ActiveAllLayer(AnimatorLayerTypes layerType)
    {
        AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];

        int targetHandMainLayerIndex = (int)targetBlendingDesc._myPart * 2;
        int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

        while (true)
        {
            float mainLayerBlendDelta = 0.0f;
            float subLayerBlendDelta = 0.0f;

            mainLayerBlendDelta = (targetBlendingDesc._isUsingFirstLayer == true)
                ? targetBlendingDesc._transitionSpeed * Time.deltaTime
                : targetBlendingDesc._transitionSpeed * Time.deltaTime * -1.0f;

            subLayerBlendDelta = -1.0f * mainLayerBlendDelta;

            targetBlendingDesc._blendTarget += mainLayerBlendDelta;
            targetBlendingDesc._blendTarget_Sub += subLayerBlendDelta;

            targetBlendingDesc._blendTarget = Mathf.Clamp(targetBlendingDesc._blendTarget, 0.0f, 1.0f);
            targetBlendingDesc._blendTarget_Sub = Mathf.Clamp(targetBlendingDesc._blendTarget_Sub, 0.0f, 1.0f);

            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex,targetBlendingDesc. _blendTarget);
            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, targetBlendingDesc._blendTarget_Sub);

            float target = (targetBlendingDesc._isUsingFirstLayer == true)
                ? targetBlendingDesc._blendTarget
                : targetBlendingDesc._blendTarget_Sub;

            if (target >= 1.0f)
            {
                break;
            }

            yield return null;
        }
    }

    protected IEnumerator ChangeNextLayerWeightSubCoroutine_DeActiveAllLayer(AnimatorLayerTypes layerType)
    {
        AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];

        int targetHandMainLayerIndex = (int)targetBlendingDesc._myPart * 2;
        int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;
        
        while (true)
        {
            float mainLayerBlendDelta = 0.0f;
            float subLayerBlendDelta = 0.0f;

            mainLayerBlendDelta = targetBlendingDesc._transitionSpeed * Time.deltaTime * -1.0f;
            subLayerBlendDelta = mainLayerBlendDelta;

            targetBlendingDesc._blendTarget += mainLayerBlendDelta;
            targetBlendingDesc._blendTarget_Sub += subLayerBlendDelta;

            targetBlendingDesc._blendTarget = Mathf.Clamp(targetBlendingDesc._blendTarget, 0.0f, 1.0f);
            targetBlendingDesc._blendTarget_Sub = Mathf.Clamp(targetBlendingDesc._blendTarget_Sub, 0.0f, 1.0f);

            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, targetBlendingDesc._blendTarget);
            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, targetBlendingDesc._blendTarget_Sub);

            float maxTarget = Mathf.Max(targetBlendingDesc._blendTarget, targetBlendingDesc._blendTarget_Sub);

            if (maxTarget <= 0.0f)
            {
                break;
            }

            yield return null;
        }
    }

    protected IEnumerator ChangeNextLayerWeightSubCoroutine_TurnOffAllLayer(AnimatorLayerTypes layerType)
    {
        AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];

        int targetHandMainLayerIndex = (int)targetBlendingDesc._myPart * 2;
        int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

        targetBlendingDesc._blendTarget = 0.0f;
        targetBlendingDesc._blendTarget_Sub = 0.0f;

        targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, targetBlendingDesc._blendTarget);
        targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, targetBlendingDesc._blendTarget_Sub);

        return null;
    }

    #endregion Coroutines


    #region Coroutine StartFunc

    protected void StartNextCoroutine(AnimatorLayerTypes layerType)
    {
        LinkedList<BodyPartBlendingWork> target = _bodyPartWorks[(int)layerType];

        target.RemoveFirst();

        if (target.Count <= 0)
        {
            UpdateBusyAnimatorLayers(1 << (int)layerType, false);
            return;
        }

        StartProceduralWork(layerType, target.First.Value);
    }

    protected Coroutine StartCoroutine_CallBackAction(IEnumerator coroutine, Action_LayerType callBack, AnimatorLayerTypes layerType)
    {
        return StartCoroutine(CoroutineWrapper(coroutine, callBack, layerType));
    }

    protected Coroutine StartCoroutine_CallBackAction(System.Func<IEnumerator> coroutine, Action_LayerType callBack, AnimatorLayerTypes layerType)
    {
        return StartCoroutine(CoroutineWrapper(coroutine, callBack, layerType));
    }

    protected IEnumerator CoroutineWrapper(IEnumerator coroutine, Action_LayerType callBack, AnimatorLayerTypes layerType)
    {
        yield return coroutine;

        callBack.Invoke(layerType);
    }

    protected IEnumerator CoroutineWrapper(System.Func<IEnumerator> coroutine, Action_LayerType callBack, AnimatorLayerTypes layerType)
    {
        yield return coroutine;

        callBack.Invoke(layerType);
    }

    #endregion Start CoroutineFunc

    protected Coroutine StartProceduralWork(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        Coroutine startedCoroutine = null;

        switch (work._workType)
        {
            case BodyPartWorkType.ActiveNextLayer:
                {
                    AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        ChangeNextLayerWeightSubCoroutine_ActiveNextLayer(layerType),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.ActiveAllLayer:
                {
                    AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        ChangeNextLayerWeightSubCoroutine_ActiveAllLayer(layerType),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.DeActiveAllLayer:
                {
                    AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        ChangeNextLayerWeightSubCoroutine_DeActiveAllLayer(layerType),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.TurnOffAllLayer:
                {
                    AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        ChangeNextLayerWeightSubCoroutine_TurnOffAllLayer(layerType),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.ChangeAnimation:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        WeaponChange_AnimationCoroutine(work._animationClip, layerType, false),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.ChangeAnimation_ZeroFrame:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        WeaponChange_AnimationCoroutine(work._animationClip, layerType, true),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.AnimationPlayHold:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        AnimationPlayHoldCoroutine(layerType, work),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.AttatchObjet:
                {
                    bool isRightHand = (layerType == AnimatorLayerTypes.RightHand);
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        CreateWeaponModelAndEquipCoroutine(layerType, work._attachObjectPrefab),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.DestroyWeapon:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        DestroyWeaponCoroutine(layerType),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.WaitCoroutineUnlock:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        WaitCoroutineLock(layerType, work),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.UnLockCoroutine:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        UnlockCoroutineLock(layerType, work),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.SwitchHand:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        SwitchHandCoroutine(layerType, work),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.OwnerChangeGrabFocusType:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        OwnerChangeWeaponGrabFocusType(work._targetGrabFocusType),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.OwnerIncreaeWeaponIndex:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        OwnerIncreaseWeaponIndex(layerType),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.End:
                {
                    startedCoroutine = null;
                }
                break;

            default:
                Debug.Assert(false, "���̽��� �߰��ƽ��ϱ�?");
                Debug.Break();
                break;

        }

        if (startedCoroutine == null)
        {
            Debug.Assert(false, "�ڷ�ƾ ���ۿ� �����߽��ϴ�.");
            Debug.Break();
            return null;
        }

        int layerTypeIndex = (int)layerType;

        _bodyCoroutineStarted[layerTypeIndex] = true;
        _currentBodyCoroutine[layerTypeIndex] = startedCoroutine;

        return startedCoroutine;
    }


    public float GetStateChangingPercentage()
    {
        float fullBodySubLayerWeight = _partBlendingDesc[(int)AnimatorLayerTypes.FullBody]._blendTarget_Sub;

        if (_partBlendingDesc[(int)AnimatorLayerTypes.FullBody]._isUsingFirstLayer == true)
        {
            fullBodySubLayerWeight = 1.0f - fullBodySubLayerWeight;
        }

        fullBodySubLayerWeight = Mathf.Clamp(fullBodySubLayerWeight, 0.0f, 1.0f);

        if (_bodyPartWorks[(int)AnimatorLayerTypes.FullBody].Count > 0 && _bodyPartWorks[(int)AnimatorLayerTypes.FullBody].First.Value._workType == BodyPartWorkType.ChangeAnimation)
        {
            fullBodySubLayerWeight = Mathf.Clamp(fullBodySubLayerWeight, 0.01f, 0.99f);
        }

        return fullBodySubLayerWeight;
    }

    #region BlendTree...������ �غ���
    //private PlayableGraph _playableGraph;
    //private AnimationLayerMixerPlayable _layerMixer;
    //private Vector2 _blendTreeWeight = Vector2.zero;
    //private AnimationPlayableOutput _playableOutput;
    //private AnimatorControllerPlayable _controllerPlayable;

    //private AnimationMixerPlayable _mixerPlayable_Main;
    //private SubBlendTreeAsset_2D _currBlendTreeAsset_Main = null;

    //private AnimationMixerPlayable _mixerPlayable_Sub;
    //private SubBlendTreeAsset_2D _currBlendTreeAsset_Sub = null;


    //private void Update()
    //{

    //}


    //private void OnDestroy()
    //{
    //    //if (_playableGraph.IsValid() == true)
    //    //{
    //    //    _playableGraph.Stop();
    //    //    _playableGraph.Destroy();
    //    //}
    //}


    ////private void InitFullbodyBlendTree()
    ////{
    ////    _playableGraph = PlayableGraph.Create("BlendTreeGraph");
    ////    _playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Output", _animator);
    ////    _layerMixer = AnimationLayerMixerPlayable.Create(_playableGraph, 3/*2���� ���̾ ����մϴ�. ��� ����*/);
    ////    _controllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, _animator.runtimeAnimatorController);

    ////    _mixerPlayable_Main = AnimationMixerPlayable.Create(_playableGraph, 4/*4���� �ִϸ��̼� ����մϴ�. ��� ����*/);
    ////    _mixerPlayable_Sub = AnimationMixerPlayable.Create(_playableGraph, 4/*4���� �ִϸ��̼� ����մϴ�. ��� ����*/);

    ////    _playableGraph.Connect(_controllerPlayable, 0, _layerMixer, 0); // Layer 0
    ////    _playableGraph.Connect(_mixerPlayable_Main, 0, _layerMixer, 1); // Layer 0
    ////    _playableGraph.Connect(_mixerPlayable_Sub, 0, _layerMixer, 2); // Layer 1

    ////    //_layerMixer.SetInputWeight((int)AnimatorLayerTypes.FullBody * 2, 0.0f);
    ////    //_layerMixer.SetInputWeight((int)AnimatorLayerTypes.FullBody * 2 + 1, 0.0f);
    ////    _layerMixer.SetInputWeight(0, 1.0f);
    ////    _layerMixer.SetInputWeight(1, 0.0f);
    ////    _layerMixer.SetInputWeight(2, 0.0f);


    ////    _playableOutput.SetSourcePlayable(_layerMixer);
    ////    _playableGraph.Stop();
    ////}


    ////private float CalculateBodyLayerWeight(AnimatorLayerTypes type)
    ////{
    ////    if (_partBlendingDesc[(int)type]._isUsingFirstLayer == true)
    ////    {
    ////        return _partBlendingDesc[(int)type]._blendTarget;
    ////    }
    ////    return _partBlendingDesc[(int)type]._blendTarget_Sub;
    ////}

    ////private int CalculateBodyLayerInder(AnimatorLayerTypes type)
    ////{
    ////    int currBodyTargetget = (_partBlendingDesc[(int)type]._isUsingFirstLayer == true)
    ////        ? (int)type
    ////        : (int)type + 1;

    ////    return currBodyTargetget;
    ////}


    ////private AnimationMixerPlayable? CalculateBodyLayerMixer(AnimatorLayerTypes type)
    ////{
    ////    AnimationMixerPlayable? currMixer = (_partBlendingDesc[(int)type]._isUsingFirstLayer == true)
    ////        ? _mixerPlayable_Main
    ////        : _mixerPlayable_Sub;

    ////    return currMixer;
    ////}


    ////private SubBlendTreeAsset_2D CalculateCurrBlendTree(AnimatorLayerTypes type)
    ////{
    ////    if (_partBlendingDesc[(int)type]._isUsingFirstLayer == true)
    ////    {
    ////        return _currBlendTreeAsset_Main;
    ////    }

    ////    return _currBlendTreeAsset_Sub;
    ////}


    ////public void ReadyBlendTree(SubBlendTreeAsset_2D blendTreeAsset)
    ////{
    ////    int currFullbodyTarget = CalculateBodyLayerInder(AnimatorLayerTypes.FullBody);
    ////    AnimationMixerPlayable? currMixer = CalculateBodyLayerMixer(AnimatorLayerTypes.FullBody);

    ////    if (currFullbodyTarget == 0)
    ////    {
    ////        _currBlendTreeAsset_Main = blendTreeAsset;
    ////    }
    ////    else
    ////    {
    ////        _currBlendTreeAsset_Sub = blendTreeAsset;
    ////    }

    ////    var playable_YUP = AnimationClipPlayable.Create(_playableGraph, blendTreeAsset._animation_YUP);
    ////    var playable_XUP = AnimationClipPlayable.Create(_playableGraph, blendTreeAsset._animation_XUP);
    ////    var playable_YDOWN = AnimationClipPlayable.Create(_playableGraph, blendTreeAsset._animation_YDOWN);
    ////    var playable_XDOWN = AnimationClipPlayable.Create(_playableGraph, blendTreeAsset._animation_XDOWN);

    ////    _playableGraph.Connect(playable_YUP, 0, currMixer.Value, 0);
    ////    _playableGraph.Connect(playable_XUP, 0, currMixer.Value, 1);
    ////    _playableGraph.Connect(playable_YDOWN, 0, currMixer.Value, 2);
    ////    _playableGraph.Connect(playable_XDOWN, 0, currMixer.Value, 3);

    ////    _playableGraph.Play();
    ////}


    ////public void ClearBlendTree()
    ////{
    ////    int currFullbodyTarget = CalculateBodyLayerInder(AnimatorLayerTypes.FullBody);
    ////    AnimationMixerPlayable? currMixer = CalculateBodyLayerMixer(AnimatorLayerTypes.FullBody);

    ////    if (_partBlendingDesc[(int)AnimatorLayerTypes.FullBody]._isUsingFirstLayer == true)
    ////    {
    ////        _currBlendTreeAsset_Main = null;
    ////    }
    ////    else
    ////    {
    ////        _currBlendTreeAsset_Sub = null;
    ////    }

    ////    if (currMixer.Value.IsValid() == false)
    ////    {
    ////        return;
    ////    }

    ////    for (int i = 0; i < 4; i++)
    ////    {
    ////        var oldPlayable = (AnimationClipPlayable)currMixer.Value.GetInput(i);
    ////        if (oldPlayable.IsValid() == false)
    ////        {
    ////            continue;
    ////        }
    ////        _playableGraph.Disconnect(currMixer.Value, i);
    ////        oldPlayable.Destroy();
    ////    }
    ////}


    //private void LateUpdate()
    //{
    //    //int currFullbodyTarget = CalculateBodyLayerInder(AnimatorLayerTypes.FullBody);
    //    //AnimationMixerPlayable? currMixer = CalculateBodyLayerMixer(AnimatorLayerTypes.FullBody);
    //    //SubBlendTreeAsset_2D currAsset = CalculateCurrBlendTree(AnimatorLayerTypes.FullBody);

    //    //if (currAsset != null)
    //    //{
    //    //    float total = Mathf.Abs(_blendTreeWeight.x) + Mathf.Abs(_blendTreeWeight.y);
    //    //    float xRatio = Mathf.Abs(_blendTreeWeight.x) / total;
    //    //    float yRatio = Mathf.Abs(_blendTreeWeight.y) / total;

    //    //    if (_blendTreeWeight.y >= 0.0f)
    //    //    {
    //    //        currMixer.Value.SetInputWeight(0, yRatio); // Idle
    //    //        currMixer.Value.SetInputWeight(2, 0.0f);  // Run
    //    //    }
    //    //    else
    //    //    {
    //    //        currMixer.Value.SetInputWeight(0, 0.0f); // Idle
    //    //        currMixer.Value.SetInputWeight(2, yRatio);  // Run
    //    //    }

    //    //    if (_blendTreeWeight.x >= 0.0f)
    //    //    {
    //    //        currMixer.Value.SetInputWeight(1, xRatio); // Walk
    //    //        currMixer.Value.SetInputWeight(3, 0.0f);  // Run
    //    //    }
    //    //    else
    //    //    {
    //    //        currMixer.Value.SetInputWeight(1, 0.0f); // Walk
    //    //        currMixer.Value.SetInputWeight(3, xRatio);  // Run
    //    //    }


    //    //    float currWeight = CalculateBodyLayerWeight(AnimatorLayerTypes.FullBody);
    //    //    _layerMixer.SetInputWeight(currFullbodyTarget + 1, currWeight);
    //    //}
    //}


    ////public void SetBlendTreeWeight(Vector2 weight)
    ////{
    ////    _blendTreeWeight = weight;
    ////}
    #endregion BlendTree...������ �غ���
}


//public void WeaponLayerChange_EnterAttack(WeaponGrabFocus grapFocusType, StateAsset currState, bool isUsingRightWeapon)
//{
//    bool isAttackState = true;

//    AnimatorBlendingDesc leftHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.LeftHand];
//    int leftHandMainLayer = (leftHandBlendingDesc._isUsingFirstLayer == true)
//        ? (int)AnimatorLayerTypes.LeftHand * 2
//        : (int)AnimatorLayerTypes.LeftHand * 2 + 1;
//    AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
//    int rightHandMainLayer = (rightHandBlendingDesc._isUsingFirstLayer == true)
//        ? (int)AnimatorLayerTypes.RightHand * 2
//        : (int)AnimatorLayerTypes.RightHand * 2 + 1;

//    bool isMirrored = (isUsingRightWeapon == false);

//    switch (grapFocusType)
//    {
//        case WeaponGrabFocus.Normal:
//            {
//                //���� �ִϸ��̼��̴�
//                if (isAttackState == true)
//                {
//                    int usingHandLayer = (isUsingRightWeapon == true)
//                        ? rightHandMainLayer
//                        : leftHandMainLayer;

//                    int oppositeHandLayer = (isUsingRightWeapon == true)
//                        ? leftHandMainLayer
//                        : rightHandMainLayer;


//                    _animator.SetLayerWeight(usingHandLayer, 0.0f); //�޼��� �ݵ�� ���󰡾��ؼ� 0.0f

//                    WeaponScript oppositeWeaponScript = _owner.GetCurrentWeaponScript(!isUsingRightWeapon);

//                    if (oppositeWeaponScript == null) //�޼չ��⸦ ������� �ʴٸ�
//                    {
//                        _animator.SetLayerWeight(oppositeHandLayer, 0.0f); //�޼� ���⸦ ������� �ʴٸ� ����� ���󰡾��ؼ� ���̾ ��������.
//                    }
//                    else
//                    {
//                        if (oppositeWeaponScript._handlingIdleAnimation_OneHand == null)
//                        {
//                            //_overrideController[MyUtil._motionChangingAnimationNames[oppositeHandLayer]] = _tempWeaponHandling_NoPlay;
//                        }
//                        _animator.SetLayerWeight(oppositeHandLayer, 1.0f);
//                    }

//                    _animator.SetBool("IsMirroring", isMirrored);
//                }
//                //���� �ִϸ��̼��� �ƴϴ�
//                else
//                {

//                    WeaponScript leftHandWeaponScript = _owner.GetCurrentWeaponScript(false);
//                    WeaponScript rightHandWeaponScript = _owner.GetCurrentWeaponScript(true);

//                    //�޼� ���⸦ ������� �ʰų� �޼� ������ ���� �ִϸ��̼��� ����
//                    float leftHandLayerWeight = (leftHandWeaponScript == null || leftHandWeaponScript._handlingIdleAnimation_OneHand == null) //
//                        ? 0.0f
//                        : 1.0f;
//                    _animator.SetLayerWeight(leftHandMainLayer, leftHandLayerWeight);

//                    //������ ���⸦ ������� �ʰų� ������ ������ ���� �ִϸ��̼��� ����
//                    float rightHandLayerWeight = (rightHandWeaponScript == null || rightHandWeaponScript._handlingIdleAnimation_OneHand == null)
//                        ? 0.0f
//                        : 1.0f;
//                    _animator.SetLayerWeight(rightHandMainLayer, rightHandLayerWeight);

//                    _animator.SetBool("IsMirroring", false);
//                }
//            }
//            break;

//        case WeaponGrabFocus.RightHandFocused:
//        case WeaponGrabFocus.LeftHandFocused:
//            {
//                float layerWeight = (isAttackState == true)
//                    ? 0.0f
//                    : 1.0f;

//                _animator.SetLayerWeight(rightHandMainLayer, layerWeight);
//                _animator.SetLayerWeight(leftHandMainLayer, layerWeight);

//                _animator.SetBool("IsMirroring", isMirrored);


//                if (currState._myState._isAttackState == true)
//                {
//                }
//                else
//                {
//                    _animator.SetBool("IsMirroring", false);
//                }
//            }
//            break;

//        case WeaponGrabFocus.DualGrab:
//            {
//                Debug.Assert(false, "�ּ� �������� �߰��ƽ��ϱ�?");
//            }
//            break;
//    }
//}

//public void WeaponLayerChange_ExitAttack(WeaponGrabFocus grapFocusType, StateAsset currState, bool isUsingRightWeapon)
//{
//    bool isAttackState = false;

//    AnimatorBlendingDesc leftHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.LeftHand];
//    int leftHandMainLayer = (leftHandBlendingDesc._isUsingFirstLayer == true)
//        ? (int)AnimatorLayerTypes.LeftHand * 2
//        : (int)AnimatorLayerTypes.LeftHand * 2 + 1;
//    AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
//    int rightHandMainLayer = (rightHandBlendingDesc._isUsingFirstLayer == true)
//        ? (int)AnimatorLayerTypes.RightHand * 2
//        : (int)AnimatorLayerTypes.RightHand * 2 + 1;

//    bool isMirrored = (isUsingRightWeapon == false);

//    switch (grapFocusType)
//    {
//        case WeaponGrabFocus.Normal:
//            {
//                //���� �ִϸ��̼��̴�
//                if (isAttackState == true)
//                {
//                    int usingHandLayer = (isUsingRightWeapon == true)
//                        ? rightHandMainLayer
//                        : leftHandMainLayer;

//                    int oppositeHandLayer = (isUsingRightWeapon == true)
//                        ? leftHandMainLayer
//                        : rightHandMainLayer;


//                    _animator.SetLayerWeight(usingHandLayer, 0.0f); //�޼��� �ݵ�� ���󰡾��ؼ� 0.0f

//                    WeaponScript oppositeWeaponScript = _owner.GetCurrentWeaponScript(!isUsingRightWeapon);

//                    if (oppositeWeaponScript == null) //�޼չ��⸦ ������� �ʴٸ�
//                    {
//                        _animator.SetLayerWeight(oppositeHandLayer, 0.0f); //�޼� ���⸦ ������� �ʴٸ� ����� ���󰡾��ؼ� ���̾ ��������.
//                    }
//                    else
//                    {
//                        if (oppositeWeaponScript.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null)
//                        {
//                            //_overrideController[MyUtil._motionChangingAnimationNames[oppositeHandLayer]] = _tempWeaponHandling_NoPlay;
//                        }
//                        _animator.SetLayerWeight(oppositeHandLayer, 1.0f);
//                    }

//                    _animator.SetBool("IsMirroring", isMirrored);
//                }
//                //���� �ִϸ��̼��� �ƴϴ�
//                else
//                {
//                    WeaponScript leftHandWeaponScript = _owner.GetCurrentWeaponScript(false);
//                    WeaponScript rightHandWeaponScript = _owner.GetCurrentWeaponScript(true);

//                    //�޼� ���⸦ ������� �ʰų� �޼� ������ ���� �ִϸ��̼��� ����
//                    float leftHandLayerWeight = (leftHandWeaponScript == null || leftHandWeaponScript._handlingIdleAnimation_OneHand == null) //
//                        ? 0.0f
//                        : 1.0f;
//                    _animator.SetLayerWeight(leftHandMainLayer, leftHandLayerWeight);

//                    //������ ���⸦ ������� �ʰų� ������ ������ ���� �ִϸ��̼��� ����
//                    float rightHandLayerWeight = (rightHandWeaponScript == null || rightHandWeaponScript._handlingIdleAnimation_OneHand == null)
//                        ? 0.0f
//                        : 1.0f;
//                    _animator.SetLayerWeight(rightHandMainLayer, rightHandLayerWeight);

//                    _animator.SetBool("IsMirroring", false);
//                }
//            }
//            break;

//        case WeaponGrabFocus.RightHandFocused:
//        case WeaponGrabFocus.LeftHandFocused:
//            {
//                float layerWeight = (isAttackState == true)
//                    ? 0.0f
//                    : 1.0f;

//                _animator.SetLayerWeight(rightHandMainLayer, layerWeight);
//                _animator.SetLayerWeight(leftHandMainLayer, layerWeight);

//                if (currState._myState._isAttackState == true)
//                {
//                    _animator.SetBool("IsMirroring", isMirrored);
//                }
//                else
//                {
//                    _animator.SetBool("IsMirroring", false);
//                }
//            }
//            break;

//        case WeaponGrabFocus.DualGrab:
//            {
//                Debug.Assert(false, "�ּ� �������� �߰��ƽ��ϱ�?");
//            }
//            break;
//    }
//}
